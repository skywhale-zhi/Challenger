using Challenger.CNPCs;
using Challenger.CProjs;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using OTAPI;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

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


        //玩家受伤时触发事件
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

            //盔甲判断，用腿铠判断，数据量更小些，这里是预先判断，命中后进入方法再准确判断，目的减小服务器计算量(不至于不至于，我应该考虑更多网络问题)
            switch (Main.player[e.Player.Index].armor[2].type)
            {
                case 153:
                    NecroArmor(e, null);//死灵套效果
                    break;
                case 258:
                    NinjaArmorEffect(e);//忍者套效果
                    break;
                case 4984:
                    CrystalAssassinArmorEffect(null, e);//水晶刺客效果
                    break;
                case 2202:
                    BeetleArmorEffect(null, e, null);//甲虫治疗
                    break;
                default:
                    break;
            }
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
                //启用修改后的射弹cproj的ai
                CMain.cProjectiles[args.Projectile.whoAmI].ProjectileAI(args.Projectile);
            }
            else //或者不想修改射弹，只是在射弹活动的时候处理某些效果，不算做修改原有射弹ai，此处写入你想要的射弹id
            {
                switch (args.Projectile.type)
                {
                    case 227:
                        if (args.Projectile.owner != 255 && CMain.cProjectiles[args.Projectile.whoAmI] == null)
                        {
                            CMain.cProjectiles[args.Projectile.whoAmI] = new CrystalLeafShot(args.Projectile);
                            //做个标记（该死的射弹生成钩子，给的index索引竟然不是proj.whoami,只能在这里进行分配了）
                            CMain.cProjectiles[args.Projectile.whoAmI].Lable = 1;
                        }
                        break;
                    case 841://皮鞭4672
                    case 914://荆棘4913
                    case 952://骨5074
                    case 913://鞭炮4912
                    case 912://冷鞭4911
                        TikiArmorEffect(null, args);//提基套效果
                        SpookyArmorEffect(args);//阴森
                        break;
                    case 847://神圣4678
                    case 849://黑收4680
                    case 848://晨星4679
                    case 915://万花筒4914
                        TikiArmorEffect(null, args, 1); //提基
                        SpookyArmorEffect(args, 1);//阴森
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
                if (CMain.cProjectiles[projectile.whoAmI] != null)
                {
                    //这里增加一个无效cproj的检查，有效排除死掉proj而又没来及kill cproj的情况
                    if (!CMain.cProjectiles[projectile.whoAmI].c_proj.active || CMain.cProjectiles[projectile.whoAmI].c_proj.type != projectile.type)
                    {
                        CMain.cProjectiles[projectile.whoAmI] = null;
                        return HookResult.Continue;
                    }
                    //启用修改后的射弹cproj的PreProjectileKilled
                    CMain.cProjectiles[projectile.whoAmI].PreProjectileKilled(projectile);
                }

                //蘑菇套效果
                ShroomiteArmorEffect(projectile, null);
            }

            return HookResult.Continue;
        }


        //在Proj杀死时清除Cprojectile
        private void OnProjPostKilled(Projectile projectile)
        {
            switch (projectile.type)
            {
                case 125:  //红宝石法杖射弹 bloodbagproj.cs
                case 227:  //叶绿水晶矢 crystalleafshot.cs
                case 121:  //紫宝石法杖 beetleheal.cs
                case 346:  //圣诞彩球饰品 honey.cs
                case 597:  //闪电球 fossiarmorproj.cs
                case 371:  //蜘蛛蛋 spiderarmorproj.cs
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
                case NPCID.EaterofWorldsHead:
                    CMain.cNPCs[index] = new EaterofWorldsHead(Main.npc[index]);
                    break;
                case NPCID.EaterofWorldsBody:
                    CMain.cNPCs[index] = new EaterofWorldsBody(Main.npc[index]);
                    break;
                case NPCID.EaterofWorldsTail:
                    CMain.cNPCs[index] = new EaterofWorldsTail(Main.npc[index]);
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
                //运行修改npc的ai
                CMain.cNPCs[npc.whoAmI].NPCAI(npc);
            }
        }


        //杀死npc触发
        private void OnNpcKilled(NPC npc)
        {
            if (config.enableChallenge_是否启用挑战模式)
            {
                //执行改编后的NPC的OnKilled
                if (CMain.cNPCs[npc.whoAmI] != null)
                {
                    CMain.cNPCs[npc.whoAmI].OnKilled(npc);
                }


                #region 在npc杀死时清除Cnpc
                switch (npc.type)
                {
                    case NPCID.KingSlime: //史莱姆王
                    case NPCID.EyeofCthulhu: //克苏鲁之眼
                    case NPCID.ServantofCthulhu: //克苏鲁之仆
                    case NPCID.BrainofCthulhu: //克苏鲁之脑
                    case NPCID.Creeper: //飞眼怪
                    case NPCID.EaterofWorldsHead://世吞头
                    case NPCID.EaterofWorldsBody://世吞身
                    case NPCID.EaterofWorldsTail://世吞尾
                    case NPCID.Deerclops: //巨鹿
                    case NPCID.SkeletronHead: //骷髅王头
                    case NPCID.SkeletronHand: //骷髅王手
                    case NPCID.CursedSkull: //骷髅王之仆
                    case NPCID.QueenBee: //蜂后
                    case NPCID.WallofFleshEye: //血肉墙-眼
                    case NPCID.WallofFlesh: //血肉墙

                        CMain.cNPCs[npc.whoAmI] = null;//清除干净
                        break;
                    default:
                        break;
                }
                #endregion
            }
        }


        //在npc被击中时触发
        private void OnNpcStrike(NpcStrikeEventArgs args)
        {
            if (!config.enableChallenge_是否启用挑战模式)
            {
                return;
            }
            //盔甲判断，用腿铠判断，数据量更小些，这里是预先判断，命中后进入方法再准确判断，目的减小服务器计算量
            switch (args.Player.armor[2].type)
            {
                case 794:
                    CrimsonArmorEffect(args);//血腥套效果
                    break;
                case 100:
                case 958:
                    ShadowArmorEffect(args);//暗影套效果
                    break;
                case 125:
                    MeteorArmorEffect(args, null);//陨石套效果
                    break;
                case 3268:
                    ObsidianArmorEffect(args);//黑曜石套效果
                    break;
                case 153:
                    NecroArmor(null, args);//死灵
                    break;
                case 2372:
                    SpiderArmorEffect(args, null);//蜘蛛套
                    break;
                case 552:
                case 4901:
                    HallowedArmorEffect(args);//神圣
                    break;
                case 2202:
                    BeetleArmorEffect(null, null, args);//甲虫
                    break;
                case 1550:
                    ShroomiteArmorEffect(null, args);//蘑菇
                    break;
                default:
                    break;
            }


            //执行改编后的NPC的WhenHurtbyPlayer
            if (CMain.cNPCs[args.Npc.whoAmI] != null)
            {
                //这里增加一个无效cnpc的检查，有效排除死掉cnpc而又没来及kill cnpc的情况
                if (!CMain.cNPCs[args.Npc.whoAmI].c_npc.active)
                {
                    CMain.cNPCs[args.Npc.whoAmI] = null;
                    return;
                }
                //运行修改npc的WhenHurtbyPlayer
                CMain.cNPCs[args.Npc.whoAmI].WhenHurtByPlayer(args);
            }
        }


        //游戏更新
        private void OnGameUpdate(EventArgs args)
        {
            //忘了删
        }


        //拿持修改后的物品时添加提示词
        private void OnHoldItem(object sender, GetDataHandlers.PlayerSlotEventArgs e)
        {
            if (e.Slot == 58 && e.Stack != 0)
            {
                if (config.enableChallenge_是否启用挑战模式 && CMain.cPlayers[e.Player.Index] != null && CMain.cPlayers[e.Player.Index].tips)
                {
                    DisplayTips(e.Player, e.Type);
                }
            }
        }


        //玩家更新前触发，实现某些效果
        private HookResult OnPreUpdate(Player player, ref int i)
        {
            if (!config.enableChallenge_是否启用挑战模式)
            {
                return HookResult.Continue;
            }

            //盔甲判断，用腿铠判断，数据量更小些，这里是预先判断，命中后进入方法再准确判断，目的减小服务器计算量
            switch (player.armor[2].type)
            {
                case 233:
                    MoltenArmor(player, ref i);//熔岩
                    break;
                case 230:
                case 962:
                    JungleArmorEffect(player, ref i);//丛林
                    break;
                case 2363:
                    BeeArmorEffect(player, ref i);//蜜蜂
                    break;
                case 3376:
                    FossilArmorEffect(player, ref i);//化石
                    break;
                case 125:
                    MeteorArmorEffect(null, player); //陨石
                    break;
                case 2372:
                    SpiderArmorEffect(null, player);//蜘蛛
                    break;
                case 3778:
                    ForbiddenArmorEffect(player, ref i);//禁戒
                    break;
                case 4984:
                    CrystalAssassinArmorEffect(player, null);//水晶刺客
                    break;
                case 686:
                    FrostArmorEffect(player, ref i);//寒霜
                    break;
                case 1005:
                    ChlorophyteArmorEffect(player, ref i);//叶绿
                    break;
                case 1318:
                    TurtleArmorEffect(player, ref i);//乌龟
                    break;
                case 1161:
                    TikiArmorEffect(player, null);//提基
                    break;
                case 2202:
                    BeetleArmorEffect(player, null, null);//甲虫
                    break;
                case 1505:
                    SpectreArmorEffect(player);//幽魂
                    break;
                default:
                    //这里装着需要检查卸下后的效果，例如叶绿套卸掉后生命值-100，必须要在这里运行一遍检查是否取消效果
                    FossilArmorEffect(player, ref i);//化石
                    ChlorophyteArmorEffect(player, ref i);//叶绿
                    TurtleArmorEffect(player, ref i);//乌龟
                    TikiArmorEffect(player, null);//提基
                    BeetleArmorEffect(player, null, null);//甲虫
                    SpectreArmorEffect(player);//幽魂
                    break;

            }

            //饰品判断
            Item[] items = player.armor;
            for (int j = 3; j < 10; j++)
            {
                switch (items[j].type)
                {
                    case 3333:
                        HivePack(player);  //蜂巢背包
                        break;
                    case 3090:
                        RoyalGel(player);  //皇家凝胶
                        break;
                    default:
                        //这里装着需要检查卸下后的效果，例如叶绿套卸掉后生命值-100，必须要在这里运行一遍
                        break;
                }
            }

            return HookResult.Continue;
        }


        //进入服务器触发，同步cplayer
        private void OnServerjoin(JoinEventArgs args)
        {
            if (args == null || TShock.Players[args.Who] == null)
                return;
            if (CMain.cPlayers[args.Who] == null)
            {
                CMain.cPlayers[args.Who] = new CPlayer(args.Who);
            }
            if (config.enableChallenge_是否启用挑战模式)
            {
                TShock.Players[args.Who].SendMessage("世界已开启挑战模式，祝您好运！", new Color(255, 82, 165));
            }
            else
            {
                TShock.Players[args.Who].SendMessage("世界已关闭挑战模式，快乐游玩吧", new Color(82, 155, 119));
            }
        }


        //离开服务器触发, 同步cplayer
        private void OnServerLeave(LeaveEventArgs args)
        {
            if (args == null || TShock.Players[args.Who] == null)
                return;
            //玩家离开后清除身上所有血量修改
            if (CMain.cPlayers[args.Who].ExtraLife != 0)
            {
                Main.player[args.Who].statLifeMax -= CMain.cPlayers[args.Who].ExtraLife;
                NetMessage.SendData(16, -1, -1, NetworkText.Empty, args.Who);
            }
            //玩家离开后清除身上所有魔力修改
            if (CMain.cPlayers[args.Who].ExtraMana != 0)
            {
                Main.player[args.Who].statManaMax -= CMain.cPlayers[args.Who].ExtraMana;
                NetMessage.SendData(42, -1, -1, NetworkText.Empty, args.Who);
            }
            CMain.cPlayers[args.Who] = null;
        }


        //指令启用挑战模式
        private void EnableModel(CommandArgs args)
        {
            if (args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /cenable 来启用挑战模式，再次使用取消");
                return;
            }
            if (config.enableChallenge_是否启用挑战模式)
            {
                config.enableChallenge_是否启用挑战模式 = false;
                //如果取消挑战模式，重置cproj和cnpc,cplayer
                CMain.cProjectiles = new CProjectile[Main.maxProjectiles];
                CMain.cNPCs = new CNPC[Main.maxNPCs];
                //重置cplayer前恢复玩家血量魔力
                foreach (CPlayer p in CMain.cPlayers)
                {
                    if (p != null && p.ExtraLife != 0)
                    {
                        Main.player[p.c_index].statLifeMax -= p.ExtraLife;
                        NetMessage.SendData(16, -1, -1, NetworkText.Empty, p.c_index);
                    }
                    if (p != null && p.ExtraMana != 0)
                    {
                        Main.player[p.c_index].statManaMax -= p.ExtraMana;
                        NetMessage.SendData(42, -1, -1, NetworkText.Empty, p.c_index);
                    }
                }

                CMain.cPlayers = new CPlayer[Main.maxPlayers];

                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                TSPlayer.All.SendMessage($"挑战模式已取消，您觉得太难了？[操作来自：{args.Player.Name}]", new Color(82, 155, 119));
            }
            else
            {
                config.enableChallenge_是否启用挑战模式 = true;
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));

                //如果开启挑战模式，重新配置cnpc，cplayer, cproj就算了，太麻烦了，反正影响不大
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
                        case NPCID.EaterofWorldsHead:
                            CMain.cNPCs[n.whoAmI] = new EaterofWorldsHead(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.EaterofWorldsBody:
                            CMain.cNPCs[n.whoAmI] = new EaterofWorldsBody(Main.npc[n.whoAmI]);
                            break;
                        case NPCID.EaterofWorldsTail:
                            CMain.cNPCs[n.whoAmI] = new EaterofWorldsTail(Main.npc[n.whoAmI]);
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
                TSPlayer.All.SendMessage($"挑战模式启用，祝您愉快。[操作来自：{args.Player.Name}]", new Color(255, 82, 165));
            }
        }


        //提示指令，凡是对所有玩家发送的悬浮文字消息均不能用这个指令禁止（因为非常非常麻烦）
        private void EnableTips(CommandArgs args)
        {
            if (args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /tips 来启用内容提示，如各种物品装备的修改文字提示，再次使用取消");
                return;
            }
            if (!config.enableChallenge_是否启用挑战模式)
            {
                args.Player.SendInfoMessage("挑战模式已关闭，无法开启文字提示");
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
