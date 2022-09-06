using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using TShockAPI.Hooks;
using Terraria;
using TerrariaApi.Server;
using System.IO;
using Terraria.Localization;
using System.Diagnostics;
using Terraria.ID;
using System.Data;
using TShockAPI.DB;
using System.Collections;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using OTAPI;
using Newtonsoft.Json;
using Challenger.CProjs;
using Challenger.CNPCs;

namespace Challenger
{
    public partial class Challenger : TerrariaPlugin
    {
        private void SetChallengerFile(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }


        private void OnReload(ReloadEventArgs e)
        {
            config = Config.LoadConfig();
        }


        //怪物触碰玩家时触发事件
        private void PlayerSufferDamage(object sender, GetDataHandlers.PlayerDamageEventArgs e)
        {
            if (!config.enableChallenge_是否启用挑战模式)
                return;

            if (config.enableMonsterSucksBlood_是否启用怪物吸血)
            {
                if (e.PlayerDeathReason._sourceNPCIndex != -1)
                {   //怪物触碰玩家吸血事件
                    TouchedAndBeSucked(e);
                }
                else if (e.PlayerDeathReason._sourceProjectileType != -1)
                {   //怪物触碰玩家吸血事件
                    ProjAndBeSucked(e);
                }
            }

            if (e.PlayerDeathReason._sourceNPCIndex != -1)
            {
                //启用修改后的Cnpc的触碰伤害发生事件的其他效果，比如debuff,或者话痨
                int index = e.PlayerDeathReason._sourceNPCIndex;
                if (CMain.cNPCs[index] != null)
                {
                    CMain.cNPCs[index].OnHurtPlayers(Main.npc[index], e);
                }
            }
            else if (e.PlayerDeathReason._sourceProjectileType != -1)
            {
            }

            //死灵套效果
            NecroArmor(e);
            //忍者套效果
            NinjaArmorEffect(e);
            //水晶刺客效果
            CrystalAssassinArmorEffect(null, e);
            //甲虫治疗
            BeetleArmorEffect(null, e, null);
        }


        //修改射弹的ai
        private void OnProjAIUpdate(ProjectileAiUpdateEventArgs args)
        {
            if (!config.enableChallenge_是否启用挑战模式)
            {
                return;
            }

            if (CMain.cProjectiles[args.Projectile.whoAmI] != null)
            {
                //这里增加一个无效cproj的检查，有效排除死掉proj而又没来及kill cproj的情况
                if (!CMain.cProjectiles[args.Projectile.whoAmI].c_proj.active || CMain.cProjectiles[args.Projectile.whoAmI].c_proj.type != args.Projectile.type)
                {
                    CMain.cProjectiles[args.Projectile.whoAmI] = null;
                    return;
                }
                CMain.cProjectiles[args.Projectile.whoAmI].ProjectileAI(args.Projectile);
            }
            else
            {
                //在射弹活动的时候处理某些效果，不算做修改原有射弹，写入你想要的射弹id
                switch (args.Projectile.type)
                {
                    case 227:
                        if (args.Projectile.owner != 255 && CMain.cProjectiles[args.Projectile.whoAmI] == null)
                        {
                            CMain.cProjectiles[args.Projectile.whoAmI] = new CrystalLeafShot(args.Projectile);
                            //做个标记
                            CMain.cProjectiles[args.Projectile.whoAmI].c_ai[0] = 1;
                        }
                        break;
                    case 841://皮鞭4672
                    case 914://荆棘4913
                    case 952://骨5074
                    case 913://鞭炮4912
                    case 912://冷鞭4911
                        TikiArmorEffect(null, args);
                        SpookyArmorEffect(args);
                        break;
                    case 847://神圣4678
                    case 849://黑收4680
                    case 848://晨星4679
                    case 915://万花筒4914
                        TikiArmorEffect(null, args, 1);
                        SpookyArmorEffect(args, 1);
                        break;
                    default:
                        break;
                }
            }
        }


        //杀死射弹前
        private HookResult OnProjPreKilled(Projectile projectile)
        {
            if (config.enableChallenge_是否启用挑战模式)
            {
                //射弹杀死时的效果
                ShroomiteArmorEffect(projectile, null);
            }

            return HookResult.Continue;
        }


        //在Proj杀死时清除Cprojectile
        private void OnProjPostKilled(Projectile projectile)
        {
            switch (projectile.type)
            {
                case 125:  //红宝石法杖射弹
                case 227:  //叶绿水晶矢
                case 121:  //紫宝石法杖
                    CMain.cProjectiles[projectile.whoAmI] = null;
                    break;
                default:
                    break;
            }
        }


        //怪物生成设置CNPC
        private HookResult OnNpcSpawn(ref int index)
        {
            if (!config.enableChallenge_是否启用挑战模式)
            {
                return HookResult.Continue;
            }
            int type = Main.npc[index].type;

            switch (type)
            {
                case NPCID.KingSlime:
                    CMain.cNPCs[index] = new SlimeKing(Main.npc[index]);
                    break;
                case NPCID.EyeofCthulhu:
                    CMain.cNPCs[index] = new EyeofCthulhu(Main.npc[index]);
                    break;
                case NPCID.ServantofCthulhu:
                    CMain.cNPCs[index] = new EyeofCthulhu_DemonEye(Main.npc[index]);
                    break;
                case NPCID.BrainofCthulhu:
                    CMain.cNPCs[index] = new BrainofCthulhu(Main.npc[index]);
                    break;
                case NPCID.Creeper:
                    CMain.cNPCs[index] = new Creeper(Main.npc[index]);
                    break;
                case NPCID.Deerclops:
                    CMain.cNPCs[index] = new Deerclops(Main.npc[index]);
                    break;
                case NPCID.SkeletronHead:
                    CMain.cNPCs[index] = new Skeletron(Main.npc[index]);
                    break;
                case NPCID.SkeletronHand:
                    CMain.cNPCs[index] = new SkeletronHand(Main.npc[index]);
                    break;
                case NPCID.CursedSkull:
                    CMain.cNPCs[index] = new Skeletron_Surrand(Main.npc[index]);
                    break;
                case NPCID.QueenBee:
                    CMain.cNPCs[index] = new QueenBee(Main.npc[index]);
                    break;
                case NPCID.WallofFleshEye:
                    CMain.cNPCs[index] = new WallofFleshEye(Main.npc[index]);
                    break;
                case NPCID.WallofFlesh:
                    CMain.cNPCs[index] = new WallofFlesh(Main.npc[index]);
                    break;
                default:
                    break;
            }

            return HookResult.Continue;
        }


        //修改npc的ai
        private void OnNpcPostAI(NPC npc)
        {
            if (!config.enableChallenge_是否启用挑战模式)
            {
                return;
            }
            //执行改编后的NPC的ai
            if (CMain.cNPCs[npc.whoAmI] != null)
            {
                //这里增加一个无效cnpc的检查，有效排除死掉cnpc而又没来及kill cnpc的情况
                if (!CMain.cNPCs[npc.whoAmI].c_npc.active)
                {
                    CMain.cNPCs[npc.whoAmI] = null;
                    return;
                }

                CMain.cNPCs[npc.whoAmI].NPCAI(npc);
            }
        }


        //杀死npc触发
        private void OnNpcKilled(NPC npc)
        {
            if (config.enableChallenge_是否启用挑战模式)
            {
                //执行改编后的NPC的killed
                if (CMain.cNPCs[npc.whoAmI] != null)
                {
                    CMain.cNPCs[npc.whoAmI].OnKilled(npc);
                }
            }


            #region 在npc杀死时清除Cnpc
            switch (npc.type)
            {
                case NPCID.KingSlime: //史莱姆王
                case NPCID.EyeofCthulhu: //克苏鲁之眼
                case NPCID.ServantofCthulhu: //克苏鲁之仆
                case NPCID.BrainofCthulhu: //克苏鲁之脑
                case NPCID.Creeper: //飞眼怪
                case NPCID.Deerclops: //巨鹿
                case NPCID.SkeletronHead: //骷髅王头
                case NPCID.SkeletronHand: //骷髅王手
                case NPCID.CursedSkull: //骷髅王之仆
                case NPCID.QueenBee: //蜂后
                case NPCID.WallofFleshEye: //血肉墙-眼
                case NPCID.WallofFlesh: //血肉墙

                    CMain.cNPCs[npc.whoAmI] = null;
                    break;
                default:
                    break;
            }
            #endregion
        }


        //npc击中时触发
        private void OnNpcStrike(NpcStrikeEventArgs args)
        {
            if (!config.enableChallenge_是否启用挑战模式)
            {
                return;
            }
            //血腥套效果
            CrimsonArmorEffect(args);
            //暗影套效果
            ShadowArmorEffect(args);
            //陨石套效果
            MeteorArmorEffect(args);
            //黑曜石套效果
            ObsidianArmorEffect(args);
            //蜘蛛套
            SpiderArmorEffect(args);
            //神圣
            HallowedArmorEffect(args);
            //甲虫
            BeetleArmorEffect(null, null, args);
            //蘑菇
            ShroomiteArmorEffect(null, args);
        }



        //游戏更新
        private void OnGameUpdate(EventArgs args)
        {

        }


        //拿持修改后的物品时添加提示词
        private void OnHoldItem(object sender, GetDataHandlers.PlayerSlotEventArgs e)
        {
            if (e.Slot == 58 && e.Stack != 0)
            {
                if (config.enableChallenge_是否启用挑战模式)
                {
                    AddFunctionWords(e.Player, e.Type);
                }
            }
        }


        //玩家更新前触发
        private HookResult OnPreUpdate(Player player, ref int i)
        {
            if (config.enableChallenge_是否启用挑战模式)
            {
                MoltenArmor(player, ref i);
                JungleArmorEffect(player, ref i);
                BeeArmorEffect(player, ref i);
                FossilArmorEffect(player, ref i);
                ForbiddenArmorEffect(player, ref i);
                CrystalAssassinArmorEffect(player, null);
                FrostArmorEffect(player, ref i);
                ChlorophyteArmorEffect(player, ref i);
                TurtleArmorEffect(player, ref i);
                TikiArmorEffect(player, null);
                BeetleArmorEffect(player, null, null);
                SpectreArmorEffect(player);
                return HookResult.Continue;
            }
            return HookResult.Continue;
        }


        //进入服务器触发
        private void OnServerjoin(JoinEventArgs args)
        {
            if (args == null || TShock.Players[args.Who] == null)
                return;
            if (CMain.cPlayers[args.Who] == null)
            {
                CMain.cPlayers[args.Who] = new CPlayer(args.Who);
            }
        }


        //离开服务器触发
        private void OnServerLeave(LeaveEventArgs args)
        {
            if (args == null || TShock.Players[args.Who] == null)
                return;
            CMain.cPlayers[args.Who] = null;
        }


        //指令启用挑战模式
        private void EnableModel(CommandArgs args)
        {
            if (args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /enable 来启用挑战模式，再次使用取消");
                return;
            }
            if (config.enableChallenge_是否启用挑战模式)
            {
                config.enableChallenge_是否启用挑战模式 = false;
                //如果取消挑战模式，重置cproj和cnpc,cplayer
                CMain.cProjectiles = new CProjectile[Main.maxProjectiles];
                CMain.cNPCs = new CNPC[Main.maxNPCs];
                CMain.cPlayers = new CPlayer[Main.maxPlayers];

                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                TSPlayer.All.SendMessage($"挑战模式已取消，您觉得不够愉快？[操作来自：{args.Player.Name}]", Color.DeepPink);
            }
            else
            {
                config.enableChallenge_是否启用挑战模式 = true;
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));

                //如果开启挑战模式，重新配置npc，proj就算了，太麻烦了
                foreach (NPC n in Main.npc)
                {
                    switch (n.type)
                    {
                        case NPCID.KingSlime:
                            CMain.cNPCs[n.whoAmI] = new SlimeKing(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.EyeofCthulhu:
                            CMain.cNPCs[n.whoAmI] = new EyeofCthulhu(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.ServantofCthulhu:
                            CMain.cNPCs[n.whoAmI] = new EyeofCthulhu_DemonEye(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.BrainofCthulhu:
                            CMain.cNPCs[n.whoAmI] = new BrainofCthulhu(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.Creeper:
                            CMain.cNPCs[n.whoAmI] = new Creeper(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.Deerclops:
                            CMain.cNPCs[n.whoAmI] = new Deerclops(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.SkeletronHead:
                            CMain.cNPCs[n.whoAmI] = new Skeletron(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.SkeletronHand:
                            CMain.cNPCs[n.whoAmI] = new SkeletronHand(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.CursedSkull:
                            CMain.cNPCs[n.whoAmI] = new Skeletron_Surrand(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.QueenBee:
                            CMain.cNPCs[n.whoAmI] = new QueenBee(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.WallofFleshEye:
                            CMain.cNPCs[n.whoAmI] = new WallofFleshEye(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.WallofFlesh:
                            CMain.cNPCs[n.whoAmI] = new WallofFlesh(Main.npc[n.whoAmI]);
                            break;
                        default:
                            break;
                    }
                }
                //重置cplayer
                foreach (Player player in Main.player)
                {
                    if (player.active && TShock.Players[player.whoAmI].IsLoggedIn)
                    {
                        CMain.cPlayers[player.whoAmI] = new CPlayer(player.whoAmI);
                    }
                }
                TSPlayer.All.SendMessage($"挑战模式启用，祝您愉快。[操作来自：{args.Player.Name}]", Color.GreenYellow);
            }
        }


        //提示指令
        private void EnableTips(CommandArgs args)
        {
            if (args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /tips 来启用内容提示，如各种物品的强化文字提示，再次使用取消");
                return;
            }
            if (CMain.cPlayers[args.Player.Index].tips)
            {
                CMain.cPlayers[args.Player.Index].tips = false;
                args.Player.SendMessage("文字提示已取消", new Color(45, 187, 45));
            }
            else
            {
                CMain.cPlayers[args.Player.Index].tips = true;
                args.Player.SendMessage("文字提示已启用", new Color(45, 187, 45));
            }
        }

    }
}
