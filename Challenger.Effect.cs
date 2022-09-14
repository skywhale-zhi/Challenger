using Challenger.CProjs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;

namespace Challenger
{
    public partial class Challenger : TerrariaPlugin
    {
        //接触敌怪吸血效果
        public void TouchedAndBeSucked(GetDataHandlers.PlayerDamageEventArgs e)
        {
            NPC npc = Main.npc[e.PlayerDeathReason._sourceNPCIndex];
            float healnpc;
            float healplayer;
            NPC tempnpc = NearestWeakestNPC(npc.Center, 2000 * 2000);
            int targetNpcIndex = e.PlayerDeathReason._sourceNPCIndex;
            //如果接触敌怪的周围没有boss，则给造成伤害的npc回血
            if (tempnpc != null && !tempnpc.boss || tempnpc == null)//虽然这里我判断了tempnpc == null但是根本不可能出现这种情况，为了下面使用else if
            {
                if (npc.lifeMax * 0.1f <= 600)
                    healnpc = (e.Damage * 0.7f + npc.lifeMax * 0.1f) * config.BloodAbsorptionRatio_吸血比率;
                else
                    healnpc = (e.Damage * 0.7f + 650) * config.BloodAbsorptionRatio_吸血比率;
            }
            //否则首选boss
            else
            {
                healnpc = (e.Damage * 0.3f + npc.lifeMax * 0.015f) * config.BloodAbsorptionRatio_吸血比率 * 2;
                targetNpcIndex = tempnpc.whoAmI;
            }

            Vector2 position = Main.player[e.Player.Index].Center;

            //若是血包被玩家接住，则回血，回血量 = （敌怪伤害 - 玩家防御）* （1 - 玩家伤害减免） * 0.6, 血包放在c_ai[0]里保存
            //排除下光女吴克的高额伤害
            healplayer = e.Damage - Main.player[e.Player.Index].statDefense >= 0 ? e.Damage - Main.player[e.Player.Index].statDefense : 0;
            healplayer = (1f - Main.player[e.Player.Index].endurance) * healplayer * 0.6f;
            if (healplayer > 500)
            {
                healplayer = 500;
            }
            else if (healplayer < 0)
            {
                healplayer = 0;
            }
            BloodBagProj.NewCProjectile(position, Vector2.Zero, targetNpcIndex, healnpc, 0, healplayer);
        }


        //被射弹射中敌怪吸血效果
        public void ProjAndBeSucked(GetDataHandlers.PlayerDamageEventArgs e)
        {
            Vector2 position = Main.player[e.Player.Index].Center;

            //如果周围有boss，则给boss回血
            NPC temp = NearestWeakestNPC(position, 2000 * 2000);
            float healnpc;
            float healplayer;
            if (temp != null && temp.boss)
            {
                healnpc = e.Damage * config.BloodAbsorptionRatio_吸血比率 * 2f + temp.lifeMax * 0.003f;//boss得到射弹伤害的特殊回血
            }
            else
            {
                healnpc = e.Damage * config.BloodAbsorptionRatio_吸血比率;  //若周围没有生物或boss,正常回血
            }
            //若是血包被玩家接住，则回血，回血量 = （敌怪伤害 - 玩家防御）* （1 - 玩家伤害减免）* 0.6, 血包放在c_ai[0]里保存
            //排除光女等
            healplayer = e.Damage - Main.player[e.Player.Index].statDefense >= 0 ? e.Damage - Main.player[e.Player.Index].statDefense : 0;
            healplayer = (1f - Main.player[e.Player.Index].endurance) * healplayer * 0.6f;
            if (healplayer > 500)
            {
                healplayer = 500;
            }
            else if (healplayer < 0)
            {
                healplayer = 0;
            }

            //爆炸弹加斗篷逃课的自残惩罚
            if (e.PlayerDeathReason._sourceProjectileType == 286)
            {
                healnpc *= 20;
                BloodBagProj.NewCProjectile(position, Vector2.Zero, 0, healnpc, 0, healplayer, 0, 1);
            }
            else
            {
                BloodBagProj.NewCProjectile(position, Vector2.Zero, 0, healnpc, 0, healplayer);
            }
        }


        #region 盔甲效果 方法内置对其他槽位的检查

        //忍者盔甲效果
        public void NinjaArmorEffect(GetDataHandlers.PlayerDamageEventArgs e)
        {
            Item[] items = Main.player[e.Player.Index].armor;
            if (items[0].type == 256 && items[1].type == 257 && items[2].type == 258)
            {
                int index = Projectile.NewProjectile(null, Main.player[e.Player.Index].Center, Vector2.Zero, 196, 0, 0);
                Main.projectile[index].timeLeft = 240;
                if (Main.rand.Next(4) == 0)
                {
                    //设置无敌帧
                    //Main.player[e.Player.Index].SetImmuneTimeForAllTypes(1800);
                    //克脑的闪避
                    //NetMessage.SendData(62, -1, -1, null, e.Player.Index, 4f, 0f, 0f, 0, 0, 0);
                    //忍者大师的闪避
                    NetMessage.SendData(62, -1, -1, null, e.Player.Index, 1f, 0f, 0f, 0, 0, 0);
                    //同步生命值
                    NetMessage.SendData(16, -1, -1, NetworkText.Empty, e.Player.Index);
                    SendPlayerText(e.Damage, Color.Green, Main.player[e.Player.Index].Center);
                    if (config.EnableConsumptionMode_启用话痨模式)
                        SendPlayerText("闪避锁血成功！", Color.White, Main.player[e.Player.Index].Center + new Vector2(Main.rand.Next(-60, 61), Main.rand.Next(61)));
                }
            }
        }


        //化石套效果
        public void FossilArmorEffect(Player player, ref int i)
        {
            Item[] items = player.armor;
            bool flag = items[0].type == 3374 && items[1].type == 3375 && items[2].type == 3376 || items[10].type == 3374 && items[11].type == 3375 && items[12].type == 3376;
            if (flag && (Main.time - CMain.cPlayers[player.whoAmI].FossilArmorEffectTimer >= 240 || Main.time < CMain.cPlayers[player.whoAmI].FossilArmorEffectTimer))
            {
                TShock.Players[player.whoAmI].SetBuff(BuffID.BabyDinosaur, 241);
                //复原计时器
                CMain.cPlayers[player.whoAmI].FossilArmorEffectTimer = Main.time;
            }
            //头顶琥珀光球
            bool flag2 = items[0].type == 3374 && items[1].type == 3375 && items[2].type == 3376;
            if (flag2 && !CMain.cPlayers[player.whoAmI].FossilArmorEffectTemp)
            {
                FossiArmorProj f = FossiArmorProj.NewCProjectile(player.Center, Vector2.Zero, 0, 0, 1, 0, player.whoAmI);
                CMain.cPlayers[player.whoAmI].FossilArmorEffectProjIndex = f.c_index;
                CMain.cPlayers[player.whoAmI].FossilArmorEffectProj = true;
                CMain.cPlayers[player.whoAmI].FossilArmorEffectTemp = true;
            }
            if (!flag2 && CMain.cPlayers[player.whoAmI].FossilArmorEffectProj)
            {
                CMain.cProjectiles[CMain.cPlayers[player.whoAmI].FossilArmorEffectProjIndex].c_proj.Kill();
                CMain.cPlayers[player.whoAmI].FossilArmorEffectProj = false;
                CMain.cPlayers[player.whoAmI].FossilArmorEffectTemp = false;
            }
        }


        //血腥套效果
        public void CrimsonArmorEffect(NpcStrikeEventArgs args)
        {
            Item[] items = args.Player.armor;
            if (items[0].type == 792 && items[1].type == 793 && items[2].type == 794 && args.Critical && (Main.time - CMain.cPlayers[args.Player.whoAmI].CrimsonArmorEffectTimer >= 300 || Main.time < CMain.cPlayers[args.Player.whoAmI].CrimsonArmorEffectTimer))//冷却时间300
            {
                NPC[] npcs = NearAllHostileNPCs(args.Player.Center, 320 * 320);
                if (!npcs.Any())
                {
                    return;
                }
                int count = 0;
                foreach (NPC n in npcs)
                {
                    count += 1;
                    int healnum = 10 - count > 0 ? 10 - count : 0;
                    if (healnum == 0)
                    {
                        return;
                    }
                    Projectile.NewProjectile(null, n.Center, Vector2.Zero, 305, 0, 0);
                    HealPlayer(args.Player, healnum);
                }
                //复原计时器
                CMain.cPlayers[args.Player.whoAmI].CrimsonArmorEffectTimer = Main.time;
            }
        }


        //魔矿套效果
        public void ShadowArmorEffect(NpcStrikeEventArgs args)
        {
            Item[] items = args.Player.armor;
            NPC npc = args.Npc;
            if ((items[0].netID == 102 || items[0].netID == 956) && (items[1].netID == 101 || items[1].netID == 957) && (items[2].netID == 100 || items[2].netID == 958) && args.Critical && (Main.time - CMain.cPlayers[args.Player.whoAmI].ShadowArmorEffectTimer >= 60 || Main.time < CMain.cPlayers[args.Player.whoAmI].ShadowArmorEffectTimer))
            {
                int count = Main.rand.Next(2, 6);
                for (int i = 0; i < count; i++)
                {
                    int index = Projectile.NewProjectile(null, args.Player.Center, new Vector2((float)Math.Cos(Main.rand.NextDouble() * MathHelper.TwoPi), (float)Math.Sin(Main.rand.NextDouble() * MathHelper.TwoPi)), 307, 20, 2);
                    Main.projectile[index].scale *= 0.5f;
                }
                CMain.cPlayers[args.Player.whoAmI].ShadowArmorEffectTimer = Main.time;
            }
        }


        //陨石套效果
        public void MeteorArmorEffect(NpcStrikeEventArgs args, Player player)
        {
            if (args != null)
            {
                Item[] items = args.Player.armor;
                if (items[0].netID == 123 && items[1].netID == 124 && items[2].netID == 125 && args.Critical)
                {
                    args.Player.statMana += 5;
                    if (config.EnableConsumptionMode_启用话痨模式)
                    {
                        HealPlayerMana(args.Player, 5, false);
                        SendPlayerText(TShock.Players[args.Player.whoAmI], "陨石回魔 + 5", new Color(6, 0, 255), args.Player.Center);
                    }
                    else
                    {
                        HealPlayerMana(args.Player, 5);
                    }
                }
            }
            else
            {
                Item[] items = player.armor;
                if (items[0].netID == 123 && items[1].netID == 124 && items[2].netID == 125 && (Main.time - CMain.cPlayers[player.whoAmI].MeteorArmorEffectTimer >= 180 || Main.time < CMain.cPlayers[player.whoAmI].MeteorArmorEffectTimer))
                {
                    Projectile.NewProjectile(null, player.Center + new Vector2(Main.rand.Next(-860, 861), -600), Vector2.UnitY.RotateRandom(0.3) * 16, 723, 500, 0);
                    CMain.cPlayers[player.whoAmI].MeteorArmorEffectTimer = Main.time;
                }
            }
        }


        //丛林套效果
        public void JungleArmorEffect(Player player, ref int i)
        {
            Item[] items = player.armor;
            if ((items[0].type == 228 || items[0].type == 960) && (items[1].type == 229 || items[1].type == 961) && (items[2].type == 230 || items[2].type == 962))
            {
                if (Main.rand.Next(20) == 0)
                {
                    Projectile.NewProjectile(player.GetProjectileSource_Accessory(items[0]), player.Center, Vector2.One.RotatedByRandom(MathHelper.TwoPi), Main.rand.Next(569, 572), 13, 5);
                }
            }
        }


        //蜜蜂套装效果
        public void BeeArmorEffect(Player player, ref int i)
        {
            Item[] items = player.armor;
            if (items[0].type == 2361 && items[1].type == 2362 && items[2].type == 2363 && (Main.time - CMain.cPlayers[player.whoAmI].BeeArmorEffectTimer >= 59 || Main.time < CMain.cPlayers[player.whoAmI].BeeArmorEffectTimer))
            {
                TShock.Players[player.whoAmI].SetBuff(BuffID.Honey, 60);
                CMain.cPlayers[player.whoAmI].BeeArmorEffectTimer = Main.time;
            }

            if (items[0].type == 2361 && items[1].type == 2362 && items[2].type == 2363 && Main.rand.Next(120) == 0)
            {
                Honey.NewCProjectile(player.Center, -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * 7, 0, 0, 1, 0, 0, player.whoAmI);
            }
        }


        //死灵套效果
        public void NecroArmor(GetDataHandlers.PlayerDamageEventArgs e, NpcStrikeEventArgs args)
        {
            if (e != null)
            {
                Item[] items = Main.player[e.Player.Index].armor;
                if ((items[0].type == 151 || items[0].type == 959) && items[1].type == 152 && items[2].type == 153)
                {
                    //if (items[3].type == 3245 || items[4].type == 3245 || items[5].type == 3245 || items[6].type == 3245 || items[7].type == 3245 || items[8].type == 3245 || items[9].type == 3245)
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 v = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 8 * i + MathHelper.TwoPi / 16) * 4;
                        Projectile.NewProjectile(null, Main.player[e.Player.Index].Center, v, 532, 18, 5);
                    }
                }
            }
            else
            {
                Item[] items = args.Player.armor;
                if ((items[0].type == 151 || items[0].type == 959) && items[1].type == 152 && items[2].type == 153 && Main.rand.Next(3) == 0)
                {
                    Projectile.NewProjectile(null, args.Player.Center, (args.Npc.Center + new Vector2(0, -10) - args.Player.Center) * 0.02f, ProjectileID.BoneArrow, 20, 2);
                }
            }
        }


        //黑曜石盗贼套效果
        public void ObsidianArmorEffect(NpcStrikeEventArgs args)
        {
            Item[] items = args.Player.armor;
            if (items[0].type == 3266 && items[1].type == 3267 && items[2].type == 3268)
            {
                if (args.Player.heldProj < 1 || args.Player.heldProj > Main.maxProjectileTypes)
                {
                    return;
                }
                if (ProjectileID.Sets.IsAWhip[Main.projectile[args.Player.heldProj].type] == true && args.Npc.CanBeChasedBy())
                {
                    int num = Main.rand.Next(100000);
                    if (num < 70000) { }
                    else if (num < 98000)
                        Item.NewItem(null, args.Npc.Center, new Vector2(args.Npc.width, args.Npc.height), 71, Main.rand.Next(99));
                    else if (num < 99500)
                        Item.NewItem(null, args.Npc.Center, new Vector2(args.Npc.width, args.Npc.height), 72, Main.rand.Next(20));
                    else if (num < 99990)
                        Item.NewItem(null, args.Npc.Center, new Vector2(args.Npc.width, args.Npc.height), 73, Main.rand.Next(3));
                    else if (num == 99999)
                        Item.NewItem(null, args.Npc.Center, new Vector2(args.Npc.width, args.Npc.height), 74, 1);
                }
            }
        }


        //狱炎套效果
        public void MoltenArmor(Player player, ref int i)
        {
            Item[] items = player.armor;
            if (items[0].type == 231 && items[1].type == 232 && items[2].type == 233 && (Main.time - CMain.cPlayers[player.whoAmI].MoltenArmorTimer >= 60 || Main.time < CMain.cPlayers[player.whoAmI].MoltenArmorTimer))
            {
                TShock.Players[player.whoAmI].SetBuff(BuffID.ObsidianSkin, 181);
                TShock.Players[player.whoAmI].SetBuff(BuffID.Inferno, 181);
                CMain.cPlayers[player.whoAmI].MoltenArmorTimer = Main.time;
            }
        }


        //蜘蛛套效果
        public void SpiderArmorEffect(NpcStrikeEventArgs args, Player player)
        {
            if (args != null)
            {
                Item[] items = args.Player.armor;
                if (items[0].type == 2370 && items[1].type == 2371 && items[2].type == 2372)
                {
                    try
                    {
                        if (ProjectileID.Sets.IsAWhip[Main.projectile[args.Player.heldProj].type] == true)
                        {
                            args.Npc.AddBuff(BuffID.Venom, 3600);
                            args.Npc.AddBuff(BuffID.Poisoned, 3600);
                        }
                    }
                    catch { }
                }
            }
            else
            {
                Item[] items = player.armor;
                if (items[0].type == 2370 && items[1].type == 2371 && items[2].type == 2372 && (Main.time - CMain.cPlayers[player.whoAmI].SpiderArmorEffectTimer >= 60 || Main.time < CMain.cPlayers[player.whoAmI].SpiderArmorEffectTimer))
                {
                    if (player.controlDown)
                    {
                        SpiderArmorProj.NewCProjectile(player.Center, Vector2.Zero, 0, 0, 1);
                        CMain.cPlayers[player.whoAmI].SpiderArmorEffectTimer = Main.time;
                    }
                }
            }
        }


        //水晶刺客套装效果
        public void CrystalAssassinArmorEffect(Player player, GetDataHandlers.PlayerDamageEventArgs e)
        {
            Item[] items;
            Vector2 position;
            int type;
            int playerIndex;
            if (player != null)
            {
                items = player.armor;
                position = player.Center;
                type = 90;
                playerIndex = player.whoAmI;
            }
            else
            {
                items = Main.player[e.Player.Index].armor;
                position = Main.player[e.Player.Index].Center;
                type = 94;
                playerIndex = e.Player.Index;
            }
            if (items[0].type == 4982 && items[1].type == 4983 && items[2].type == 4984 && (Main.time - CMain.cPlayers[playerIndex].CrystalAssassinArmorEffectTimer >= 50 || Main.time < CMain.cPlayers[playerIndex].CrystalAssassinArmorEffectTimer || e != null))
            {
                if (NearestHostileNPC(Main.player[playerIndex].Center, 500 * 500) == null && e == null)
                {
                    return;
                }
                for (int j = 0; j < 20; j++)
                {
                    Vector2 v = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 20 * j + MathHelper.TwoPi / 40) * 4;
                    int index = Projectile.NewProjectile(Main.player[playerIndex].GetProjectileSource_Item(items[0]), position, v, type, type == 94 ? 30 : 25, 5);
                    Main.projectile[index].timeLeft = 60 * 3;
                }
                CMain.cPlayers[playerIndex].CrystalAssassinArmorEffectTimer = Main.time;
            }
        }


        //禁戒套效果
        public void ForbiddenArmorEffect(Player player, ref int i)
        {
            Item[] items = player.armor;
            if (items[0].type == 3776 && items[1].type == 3777 && items[2].type == 3778)
            {
                if (Main.rand.Next(30) == 0)
                {
                    Projectile.NewProjectile(null, player.Center, Vector2.Zero, 659, 35, 0);
                }
            }
        }


        //寒霜套
        public void FrostArmorEffect(Player player, ref int i)
        {
            Item[] items = player.armor;
            if (items[0].type == 684 && items[1].type == 685 && items[2].type == 686)
            {
                if (Main.rand.Next(10) == 0)
                {
                    Vector2 position = player.Center + new Vector2((float)Math.Cos(Main.rand.NextDouble() * MathHelper.Pi), (float)Math.Cos(Main.rand.NextDouble() * MathHelper.Pi)) * Main.rand.Next(100);
                    Projectile.NewProjectile(null, position, Vector2.Zero, 344, 35, 0, 255, 0, Main.rand.Next(3));
                }
            }
        }


        //神圣
        public void HallowedArmorEffect(NpcStrikeEventArgs args)
        {
            Item[] items = args.Player.armor;
            if ((items[0].type == 559 || items[0].type == 553 || items[0].type == 558 || items[0].type == 4873) && items[1].type == 551 && items[2].type == 552)
            {
                double d = Main.rand.NextDouble();
                Vector2 p = args.Npc.Center + new Vector2((float)Math.Cos(d * MathHelper.TwoPi), (float)Math.Sin(d * MathHelper.TwoPi)) * 300;
                Vector2 v = (args.Npc.Center - p).SafeNormalize(Vector2.Zero) * 20;
                int index = Projectile.NewProjectile(null, p, v, 156, 56, 5);
                Main.projectile[index].timeLeft = 30;
                Main.projectile[index].penetrate = 2;
            }
            else if ((items[0].type == 4896 || items[0].type == 4897 || items[0].type == 4898 || items[0].type == 4899) && items[1].type == 4900 && items[2].type == 4901)
            {
                double d = Main.rand.NextDouble();
                Vector2 p = args.Npc.Center + new Vector2((float)Math.Cos(d * MathHelper.TwoPi), (float)Math.Sin(d * MathHelper.TwoPi)) * 300;
                Vector2 v = (args.Npc.Center - p).SafeNormalize(Vector2.Zero) * 20;
                int index = Projectile.NewProjectile(null, p, v, 157, 40, 5);
                Main.projectile[index].timeLeft = 17;
                Main.projectile[index].penetrate = 2;
            }
        }


        //叶绿
        public void ChlorophyteArmorEffect(Player player, ref int i)
        {
            Item[] items = player.armor;
            bool flag = (items[0].type == 1001 || items[0].type == 1002 || items[0].type == 1003) && items[1].type == 1004 && items[2].type == 1005;
            if (flag && !CMain.cPlayers[player.whoAmI].ChlorophyteArmorEffectTemp)
            {
                player.statLifeMax += 100;
                CMain.cPlayers[player.whoAmI].ExtraLife += 100;
                NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.whoAmI);
                CMain.cPlayers[player.whoAmI].ChlorophyteArmorEffectLife = true;
                CMain.cPlayers[player.whoAmI].ChlorophyteArmorEffectTemp = true;
                if (config.EnableConsumptionMode_启用话痨模式)
                {
                    SendPlayerText("生命值上限 + 100", new Color(0, 255, 255), player.Center);
                }
            }
            if (!flag && CMain.cPlayers[player.whoAmI].ChlorophyteArmorEffectLife)
            {
                player.statLifeMax -= 100;
                CMain.cPlayers[player.whoAmI].ExtraLife -= 100;
                NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.whoAmI);
                CMain.cPlayers[player.whoAmI].ChlorophyteArmorEffectLife = false;
                CMain.cPlayers[player.whoAmI].ChlorophyteArmorEffectTemp = false;
                if (config.EnableConsumptionMode_启用话痨模式)
                {
                    SendPlayerText("生命值上限 - 100", new Color(255, 0, 156), player.Center);
                }
            }
        }


        //海龟
        public void TurtleArmorEffect(Player player, ref int i)
        {
            Item[] items = player.armor;
            bool flag = items[0].type == 1316 && items[1].type == 1317 && items[2].type == 1318;
            if (flag && (Main.time - CMain.cPlayers[player.whoAmI].TurtleArmorEffectTimer >= 60 * 2.5 || Main.time < CMain.cPlayers[player.whoAmI].TurtleArmorEffectTimer))
            {
                int num = Main.rand.Next(15, 25);
                for (int j = 0; j < num; j++)
                {
                    Vector2 v = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / num * j + MathHelper.TwoPi / (num * 2)) * (Main.rand.NextFloat() * 3 + 9);
                    Projectile.NewProjectile(null, player.Center, v, 249, 60, 5);
                }
                CMain.cPlayers[player.whoAmI].TurtleArmorEffectTimer = Main.time;
            }
            if (flag && !CMain.cPlayers[player.whoAmI].TurtleArmorEffectTemp)
            {
                player.statLifeMax += 60;
                CMain.cPlayers[player.whoAmI].ExtraLife += 60;
                NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.whoAmI);
                CMain.cPlayers[player.whoAmI].TurtleArmorEffectLife = true;
                CMain.cPlayers[player.whoAmI].TurtleArmorEffectTemp = true;
                if (config.EnableConsumptionMode_启用话痨模式)
                {
                    SendPlayerText("生命值上限 + 60", new Color(0, 255, 255), player.Center);
                }
            }
            if (!flag && CMain.cPlayers[player.whoAmI].TurtleArmorEffectLife)
            {
                player.statLifeMax -= 60;
                CMain.cPlayers[player.whoAmI].ExtraLife -= 60;
                NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.whoAmI);
                CMain.cPlayers[player.whoAmI].TurtleArmorEffectLife = false;
                CMain.cPlayers[player.whoAmI].TurtleArmorEffectTemp = false;
                if (config.EnableConsumptionMode_启用话痨模式)
                {
                    SendPlayerText("生命值上限 - 60", new Color(255, 0, 156), player.Center);
                }
            }
        }


        //提基
        public void TikiArmorEffect(Player pl, ProjectileAiUpdateEventArgs args, int mode = 0)
        {
            if (args != null)
            {
                Item[] items = Main.player[args.Projectile.owner].armor;
                Player player = Main.player[args.Projectile.owner];
                bool flag = items[0].type == 1159 && items[1].type == 1160 && items[2].type == 1161;
                int type;
                //25~48 鞭炮，荆棘，骨，皮
                //万花筒，迪朗达尔，黑收40~70 ，晨星40~72
                int time1, time2;
                if (mode == 0)
                {
                    time1 = 25;
                    time2 = 48;
                }
                else
                {
                    time1 = 40;
                    time2 = 70;
                }
                if (Main.dayTime)
                {
                    type = 227;
                }
                else
                {
                    type = 228;
                }
                if (flag && args.Projectile.ai[0] >= time1 && args.Projectile.ai[0] <= time2 && args.Projectile.ai[0] % 4 == 0)
                {
                    List<Vector2> list = new List<Vector2>();
                    Projectile.FillWhipControlPoints(args.Projectile, list);
                    Vector2 Pos = list[list.Count - 2];
                    int index = Projectile.NewProjectile(null, Pos, (Pos - player.Center) * 0.004f, type, (int)(args.Projectile.damage * 0.2), 0);
                    Main.projectile[index].timeLeft = 180;
                }
            }
            else
            {
                Item[] items = pl.armor;
                bool flag = items[0].type == 1159 && items[1].type == 1160 && items[2].type == 1161;
                if (flag && !CMain.cPlayers[pl.whoAmI].TikiArmorEffectTemp)
                {
                    pl.statLifeMax += 20;
                    CMain.cPlayers[pl.whoAmI].ExtraLife += 20;
                    NetMessage.SendData(16, -1, -1, NetworkText.Empty, pl.whoAmI);
                    CMain.cPlayers[pl.whoAmI].TikiArmorEffectLife = true;
                    CMain.cPlayers[pl.whoAmI].TikiArmorEffectTemp = true;
                    if (config.EnableConsumptionMode_启用话痨模式)
                    {
                        SendPlayerText("生命值上限 + 20", new Color(0, 255, 255), pl.Center);
                    }
                }
                if (!flag && CMain.cPlayers[pl.whoAmI].TikiArmorEffectLife)
                {
                    pl.statLifeMax -= 20;
                    CMain.cPlayers[pl.whoAmI].ExtraLife -= 20;
                    NetMessage.SendData(16, -1, -1, NetworkText.Empty, pl.whoAmI);
                    CMain.cPlayers[pl.whoAmI].TikiArmorEffectLife = false;
                    CMain.cPlayers[pl.whoAmI].TikiArmorEffectTemp = false;
                    if (config.EnableConsumptionMode_启用话痨模式)
                    {
                        SendPlayerText("生命值上限 - 20", new Color(255, 0, 156), pl.Center);
                    }
                }
            }
        }


        //甲虫
        public void BeetleArmorEffect(Player player, GetDataHandlers.PlayerDamageEventArgs e, NpcStrikeEventArgs args)
        {
            if (player != null)
            {
                Item[] items = player.armor;
                bool flag = items[0].type == 2199 && (items[1].type == 2200 || items[1].type == 2201) && items[2].type == 2202;
                if (flag && !CMain.cPlayers[player.whoAmI].BeetleArmorEffectTemp)
                {
                    player.statLifeMax += 60;
                    CMain.cPlayers[player.whoAmI].ExtraLife += 60;
                    NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.whoAmI);
                    CMain.cPlayers[player.whoAmI].BeetleArmorEffectLife = true;
                    CMain.cPlayers[player.whoAmI].BeetleArmorEffectTemp = true;
                    if (config.EnableConsumptionMode_启用话痨模式)
                    {
                        SendPlayerText("生命值上限 + 60", new Color(0, 255, 255), player.Center);
                    }
                }
                if (!flag && CMain.cPlayers[player.whoAmI].BeetleArmorEffectLife)
                {
                    player.statLifeMax -= 60;
                    CMain.cPlayers[player.whoAmI].ExtraLife -= 60;
                    NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.whoAmI);
                    CMain.cPlayers[player.whoAmI].BeetleArmorEffectLife = false;
                    CMain.cPlayers[player.whoAmI].BeetleArmorEffectTemp = false;
                    if (config.EnableConsumptionMode_启用话痨模式)
                    {
                        SendPlayerText("生命值上限 - 60", new Color(255, 0, 156), player.Center);
                    }
                }
            }
            else if (e != null)
            {
                Item[] items = Main.player[e.Player.Index].armor;
                bool flag = items[0].type == 2199 && (items[1].type == 2200 || items[1].type == 2201) && items[2].type == 2202;
                if (flag)
                {
                    //制作吸血效果
                    Vector2 position = Main.player[e.Player.TPlayer.whoAmI].Center;
                    int heal = (int)(e.Damage * 0.3);
                    BeetleHeal.NewCProjectile(position, Vector2.Zero, 0, 0, 0, heal, 10 * 60, e.Player.TPlayer.whoAmI);
                }
            }
            else
            {
                Item[] items = args.Player.armor;
                bool flag = items[0].type == 2199 && (items[1].type == 2200 || items[1].type == 2201) && items[2].type == 2202;
                bool flag2 = false;
                for (int i = 3; i < 10; i++)
                {
                    if (items[i].type == 938 || items[i].type == 3998 || items[i].type == 3997)
                    {
                        flag2 = true;
                        break;
                    }
                }
                if (flag && flag2)
                {
                    double d = Main.rand.NextDouble();
                    Vector2 p = args.Npc.Center + new Vector2((float)Math.Cos(d * MathHelper.TwoPi), (float)Math.Sin(d * MathHelper.TwoPi)) * 250;
                    Vector2 v = (args.Npc.Center - p).SafeNormalize(Vector2.Zero) * 20;
                    int index = Projectile.NewProjectile(null, p, v, ProjectileID.PaladinsHammerFriendly, 170, 20);
                    Main.projectile[index].timeLeft = 30;
                }
            }
        }


        //蘑菇
        public void ShroomiteArmorEffect(Projectile projectile, NpcStrikeEventArgs args)
        {
            //射弹死亡时炸出蘑菇
            if (projectile != null)
            {
                Item[] items = Main.player[projectile.owner].armor;
                bool flag = (items[0].type == 1546 || items[0].type == 1547 || items[0].type == 1548) && items[1].type == 1549 && items[2].type == 1550;
                //排除容易卡的射弹
                bool flag2 = projectile.type == 90 || projectile.type == 92 || projectile.type == 640 || projectile.type == 631;
                if (!flag2 && flag && projectile.ranged && (projectile.Center - Main.player[projectile.owner].Center).LengthSquared() <= 921600)
                {
                    int num = Main.rand.Next(2, 5);
                    for (int j = 0; j < num; j++)
                    {
                        Vector2 v = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / num * j + Main.time % 3) * 20;
                        Projectile.NewProjectile(null, projectile.Center, v, ProjectileID.Mushroom, (int)(projectile.damage * 0.42f), 5);
                    }
                }
            }
            /*
            else//经过npc时释放蘑菇
            {
                Item[] items = args.Player.armor;
                bool flag = (items[0].type == 1546 || items[0].type == 1547 || items[0].type == 1548) && items[1].type == 1549 && items[2].type == 1550;
                if (flag)
                {
                    int num;
                    foreach (Projectile proj in Main.projectile)
                    {
                        bool flag2 = proj.type == 90 || proj.type == 92 || proj.type == 640 || proj.type == 631;
                        if (!flag2 && proj.ranged && proj.owner == args.Player.whoAmI && (proj.Center - args.Npc.Center).LengthSquared() <= args.Npc.width * args.Npc.height * 100 && proj.active)
                        {
                            num = Main.rand.Next(2, 4);
                            for (int j = 0; j < num; j++)
                            {
                                Vector2 v = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / num * j + Main.time % 3.14) * 70;
                                Projectile.NewProjectile(null, args.Npc.Center + v, Vector2.Zero, ProjectileID.Mushroom, (int)(proj.damage * 0.15f), 5);
                            }
                        }
                    }
                }
            }
            */
        }


        //幽魂
        public void SpectreArmorEffect(Player player)
        {
            Item[] items = player.armor;
            //修改生命上限
            bool flag = items[0].type == 1503 && items[1].type == 1504 && items[2].type == 1505;
            if (flag && !CMain.cPlayers[player.whoAmI].SpectreArmorEffectTemp)
            {
                player.statLifeMax += 40;
                CMain.cPlayers[player.whoAmI].ExtraLife += 40;
                NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.whoAmI);
                CMain.cPlayers[player.whoAmI].SpectreArmorEffectLife = true;
                CMain.cPlayers[player.whoAmI].SpectreArmorEffectTemp = true;
                if (config.EnableConsumptionMode_启用话痨模式)
                {
                    SendPlayerText("生命值上限 + 40", new Color(0, 255, 255), player.Center);
                }
            }
            if (!flag && CMain.cPlayers[player.whoAmI].SpectreArmorEffectLife)
            {
                player.statLifeMax -= 40;
                CMain.cPlayers[player.whoAmI].ExtraLife -= 40;
                NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.whoAmI);
                CMain.cPlayers[player.whoAmI].SpectreArmorEffectLife = false;
                CMain.cPlayers[player.whoAmI].SpectreArmorEffectTemp = false;
                if (config.EnableConsumptionMode_启用话痨模式)
                {
                    SendPlayerText("生命值上限 - 40", new Color(255, 0, 156), player.Center);
                }
            }
            //修改魔力上限
            flag = items[0].type == 2189 && items[1].type == 1504 && items[2].type == 1505;
            if (flag && !CMain.cPlayers[player.whoAmI].SpectreArmorEffectTemp2)
            {
                player.statManaMax += 80;
                CMain.cPlayers[player.whoAmI].ExtraMana += 80;
                NetMessage.SendData(42, -1, -1, NetworkText.Empty, player.whoAmI);
                CMain.cPlayers[player.whoAmI].SpectreArmorEffectMana = true;
                CMain.cPlayers[player.whoAmI].SpectreArmorEffectTemp2 = true;
                if (config.EnableConsumptionMode_启用话痨模式)
                {
                    SendPlayerText("魔力值上限 + 80", new Color(0, 255, 255), player.Center + new Vector2(0, 32));
                }
            }
            if (!flag && CMain.cPlayers[player.whoAmI].SpectreArmorEffectMana)
            {
                player.statManaMax -= 80;
                CMain.cPlayers[player.whoAmI].ExtraMana -= 80;
                NetMessage.SendData(42, -1, -1, NetworkText.Empty, player.whoAmI);
                CMain.cPlayers[player.whoAmI].SpectreArmorEffectMana = false;
                CMain.cPlayers[player.whoAmI].SpectreArmorEffectTemp2 = false;
                if (config.EnableConsumptionMode_启用话痨模式)
                {
                    SendPlayerText("魔力值上限 - 80", new Color(255, 0, 156), player.Center + new Vector2(0, 32));
                }
            }
            //发射幽魂射弹
            flag = (items[0].type == 1503 || items[0].type == 2189) && items[1].type == 1504 && items[2].type == 1505;
            if (flag)
            {
                NPC target = NearestHostileNPC(player.Center, 1000 * 1000);
                if (Main.rand.Next(12) == 0 && target != null)
                {
                    float k = (target.Center - player.Center).LengthSquared() / 1000000;
                    int damage = (int)(10 + (1 - k) * (1 - k) * 150);
                    Vector2 v = Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * 5f;
                    Projectile.NewProjectile(null, player.Center, v, 356, damage, 0);
                }
            }
        }


        //阴森
        public void SpookyArmorEffect(ProjectileAiUpdateEventArgs args, int mode = 0)
        {
            Item[] items = Main.player[args.Projectile.owner].armor;
            Player player = Main.player[args.Projectile.owner];
            bool flag = items[0].type == 1832 && items[1].type == 1833 && items[2].type == 1834;
            int time1, time2;
            if (mode == 0)
            {
                time1 = 25;
                time2 = 48;
            }
            else
            {
                time1 = 40;
                time2 = 70;
            }
            int type;
            //316蝙蝠321南瓜头323尖桩311糖果玉米312南瓜灯400鸡尾酒
            if (Main.dayTime)
            {
                type = 316;
            }
            else
            {
                type = 321;
            }
            if (flag && args.Projectile.ai[0] >= time1 && args.Projectile.ai[0] <= time2 && args.Projectile.ai[0] % 4 == 0)
            {
                List<Vector2> list = new List<Vector2>();
                Projectile.FillWhipControlPoints(args.Projectile, list);
                Vector2 Pos = list[list.Count - 2];
                int index = Projectile.NewProjectile(null, Pos, (Pos - player.Center) * 0.008f, type, (int)(args.Projectile.damage * 0.2), 0);
                Main.projectile[index].timeLeft = 300;
            }
        }

        #endregion


        #region 饰品效果 方法内不再次检查槽位，在方法调用前检查

        //蜂巢背包
        public void HivePack(Player player)
        {
            if (Main.rand.Next(120) == 0)
            {
                Honey.NewCProjectile(player.Center, -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * 7, 0, 1, 2, 0, 0, player.whoAmI);
            }
        }

        //皇家凝胶
        public void RoyalGel(Player player)
        {
            NPC npc = NearestHostileNPC(player.Center, 300 * 300);
            if (npc != null && (Main.time - CMain.cPlayers[player.whoAmI].RoyalGelTimer >= 60 || Main.time < CMain.cPlayers[player.whoAmI].RoyalGelTimer))
            {
                switch (npc.type)
                {
                    case 1:
                    case 16:
                    case 59:
                    case 71:
                    case 81:
                    case 138:
                    case 121:
                    case 122:
                    case 141:
                    case 147:
                    case 183:
                    case 184:
                    case 204:
                    case 225:
                    case 244:
                    case 302:
                    case 333:
                    case 335:
                    case 334:
                    case 336:
                    case 537:
                        Projectile.NewProjectile(null, player.Center, player.Center.DirectionTo(npc.Center) * testnum1, 22, 10, 2);
                        break;
                    default:
                        break;
                }
                CMain.cPlayers[player.whoAmI].RoyalGelTimer = Main.time;
            }
        }

        #endregion


        //给玩家显示修改过功能的item的文本介绍
        public void DisplayTips(TSPlayer tsplayer, short type)
        {
            switch (type)
            {
                #region 盔甲类

                case 256:
                case 257:
                case 258:
                    SendPlayerText(tsplayer, "【忍者套装】\n挑战模式奖励：受击时释放烟雾，有四分之一概率\n闪避非致命伤害", Color.Black, Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;
                case 3374:
                case 3375:
                case 3376:
                    SendPlayerText(tsplayer, "【化石套装】\n挑战模式奖励：在头上召唤一个琥珀光球，向敌人\n抛出闪电矢；召唤一只小恐龙跟着你！（你必须接\n受这个宠物！）", new Color(232, 205, 119), Main.player[tsplayer.Index].Center + new Vector2(0, -32));
                    break;
                case 792:
                case 793:
                case 794:
                    SendPlayerText(tsplayer, "【猩红套装】\n挑战模式奖励：暴击时从周围每个敌怪处吸取一定\n血量随着敌怪数目增多吸血量-1，冷却 5秒", new Color(209, 46, 93), Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;
                case 100:
                case 101:
                case 102:
                case 956:
                case 957:
                case 958:
                    SendPlayerText(tsplayer, "【暗影套装】\n挑战模式奖励：暴击时从玩家周围生成吞噬怪飞弹\n攻击周围敌人，冷却 1秒", new Color(95, 91, 207), Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;
                case 123:
                case 124:
                case 125:
                    SendPlayerText(tsplayer, "【陨石套装】\n挑战模式奖励：暴击时恢复些许魔力，间歇地降下\n落星攻击敌人", new Color(128, 15, 12), Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;
                case 228:
                case 229:
                case 230:
                case 960:
                case 961:
                case 962:
                    SendPlayerText(tsplayer, "【丛林套装】\n挑战模式奖励：间断从玩家周围生成伤害性的孢子", new Color(101, 151, 8), Main.player[tsplayer.Index].Center + new Vector2(0, -16));
                    break;
                case 151:
                case 152:
                case 153:
                case 959:
                    SendPlayerText(tsplayer, "【死灵套装】\n挑战模式奖励：受到伤害时，向四周飞溅骨头；偶\n尔发射骨箭", new Color(113, 113, 36), Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;
                case 2361:
                case 2362:
                case 2363:
                    SendPlayerText(tsplayer, "【蜜蜂套装】\n挑战模式奖励：给予永久的蜂蜜增益；不间断地向\n四周撒蜂糖罐，玩家接触后回血并给予15秒蜂蜜增\n益；对玩家自身的治疗量低于对其他玩家", new Color(232, 229, 74), Main.player[tsplayer.Index].Center + new Vector2(0, -32));
                    break;
                case 3266:
                case 3267:
                case 3268:
                    SendPlayerText(tsplayer, "【黑曜石套装】\n挑战模式奖励：用鞭子攻击时，有几率从敌人身上\n偷钱", new Color(90, 83, 160), Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;
                case 231:
                case 232:
                case 233:
                    SendPlayerText(tsplayer, "【狱炎套装】\n挑战模式奖励：免疫岩浆，给予永久的地狱火增益", new Color(255, 27, 0), Main.player[tsplayer.Index].Center + new Vector2(0, -16));
                    break;
                case 2370:
                case 2371:
                case 2372:
                    SendPlayerText(tsplayer, "【蜘蛛套装】\n挑战模式奖励：用鞭子攻击时，有低概率给予超长\n时间的中毒和剧毒减益，按“下”键生成一个毒牙\n药水瓶，砸中敌人爆炸", new Color(184, 79, 29), Main.player[tsplayer.Index].Center + new Vector2(0, -32));
                    break;
                case 4982:
                case 4983:
                case 4984:
                    SendPlayerText(tsplayer, "【水晶刺客套装】\n挑战模式奖励：当有敌人在附近时，自身释放出水\n晶碎片；若玩家被击中，释放出更强大的碎片", new Color(221, 83, 146), Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;
                case 3776:
                case 3777:
                case 3778:
                    SendPlayerText(tsplayer, "【禁戒套装】\n挑战模式奖励：释放自动寻的灵焰魂火攻击附近的\n敌人", new Color(222, 171, 26), Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;
                case 684:
                case 685:
                case 686:
                    SendPlayerText(tsplayer, "【寒霜套装】\n挑战模式奖励：在身边凝结伤害性的雪花", new Color(31, 193, 229), Main.player[tsplayer.Index].Center + new Vector2(0, -16));
                    break;
                case 559:
                case 553:
                case 558:
                case 4873:
                case 551:
                case 552:
                case 4896:
                case 4897:
                case 4898:
                case 4899:
                case 4900:
                case 4901:
                    SendPlayerText(tsplayer, "【神圣套装】\n挑战模式奖励：击中敌人时召唤光与暗剑气", new Color(179, 179, 203), Main.player[tsplayer.Index].Center + new Vector2(0, -16));
                    break;
                case 1001:
                case 1002:
                case 1003:
                case 1004:
                case 1005:
                    SendPlayerText(tsplayer, "【叶绿套装】\n挑战模式奖励：释放强大的叶绿水晶矢，增加 100\n血上限", new Color(103, 209, 0), Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;
                case 1316:
                case 1317:
                case 1318:
                    SendPlayerText(tsplayer, "【海龟套装】\n挑战模式奖励：增加60血上限，自动在附近释放爆\n炸碎片", new Color(169, 104, 69), Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;
                case 1159:
                case 1160:
                case 1161:
                    SendPlayerText(tsplayer, "【提基套装】\n挑战模式奖励：增加20血上限，在鞭子的轨迹上留\n下叶绿元素", Color.Green, Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;
                case 2199:
                case 2200:
                case 2201:
                case 2202:
                    SendPlayerText(tsplayer, "【甲虫套装】\n挑战模式奖励：增加60血上限，敌人的伤害的一部\n分会治疗周围的队友；当装备帕拉丁盾或其上级合\n成物时，帕拉丁之锤会辅助攻击敌人", new Color(101, 75, 120), Main.player[tsplayer.Index].Center + new Vector2(0, -32));
                    break;
                case 1546:
                case 1547:
                case 1548:
                case 1549:
                case 1550:
                    SendPlayerText(tsplayer, "【蘑菇套装】\n挑战模式奖励：射弹会不稳定地留下蘑菇", new Color(47, 36, 237), Main.player[tsplayer.Index].Center + new Vector2(0, -16));
                    break;
                case 1503:
                case 1504:
                case 1505:
                case 2189:
                    SendPlayerText(tsplayer, "【幽魂套装】\n挑战模式奖励：根据头饰选择增加40血上限或80魔\n力上限；溢出鬼魂攻击敌人离敌人越近鬼魂伤害越高", new Color(166, 169, 218), Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;
                case 1832:
                case 1833:
                case 1834:
                    SendPlayerText(tsplayer, "【阴森套装】\n挑战模式奖励：使用鞭子时，有概率甩出蝙蝠或南\n瓜头", new Color(85, 75, 126), Main.player[tsplayer.Index].Center + new Vector2(0, -24));
                    break;

                #endregion


                #region 饰品类

                case 3333://蜜蜂背包
                    SendPlayerText(tsplayer, "挑战模式奖励：不间断地向四周扔出毒蜂罐，爆炸\n后释放一只大蜜蜂", new Color(232, 229, 74), Main.player[tsplayer.Index].Center);
                    break;
                case 3090://皇家凝胶
                    SendPlayerText(tsplayer, "挑战模式奖励：自动攻击周围附近的史莱姆", new Color(0, 189, 238), Main.player[tsplayer.Index].Center + new Vector2(0, 0));
                    break;

                #endregion


                default:
                    break;
            }
        }
    }
}
