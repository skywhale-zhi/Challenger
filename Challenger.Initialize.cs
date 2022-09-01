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
            if (!config.enableChallenge_是否启用挑战模式 || !config.enableMonsterSucksBlood_是否启用怪物吸血)
                return;

            if (e.PlayerDeathReason._sourceNPCIndex != -1)
            {   //怪物触碰玩家吸血事件
                TouchedAndBeSucked(e);
                //启用修改后的Cnpc的触碰伤害发生事件的其他效果
                int index = e.PlayerDeathReason._sourceNPCIndex;
                if (CMain.cNPCs[index] != null)
                {
                    CMain.cNPCs[index].OnHurtPlayers(Main.npc[index]);
                }
            }
            else if (e.PlayerDeathReason._sourceProjectileType != -1)
            {   //怪物触碰玩家吸血事件
                ProjAndBeSucked(e);
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
                if (!CMain.cProjectiles[args.Projectile.whoAmI].c_proj.active)
                {
                    CMain.cProjectiles[args.Projectile.whoAmI] = null;
                    return;
                }

                CMain.cProjectiles[args.Projectile.whoAmI].ProjectileAI(args.Projectile);
            }
        }


        //在Proj杀死时清除Cprojectile
        private void OnProjPostKilled(Projectile projectile)
        {
            switch (projectile.type)
            {
                case 125:  //红宝石法杖射弹
                case 156:   //真断钢剑射弹
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
            int type =  Main.npc[index].type;
            switch (type)
            {
                case 4: //克苏鲁之眼
                    CMain.cNPCs[index] = new EyeofCthulhu(Main.npc[index]);
                    break;
                case 5: //克苏普之仆
                    CMain.cNPCs[index] = new EyeofCthulhu_DemonEye(Main.npc[index]);
                    break;
                case 50: //史莱姆王
                    CMain.cNPCs[index] = new SlimeKing(Main.npc[index]);
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
            if(CMain.cNPCs[npc.whoAmI] != null)
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


        //在npc杀死时清除Cnpc
        private void OnNpcKilled(NPC npc)
        {
            switch (npc.type)
            {
                case 4: //克苏鲁之眼
                case 5: //克苏鲁之仆
                case 50: //史莱姆王
                    CMain.cNPCs[npc.whoAmI] = null;
                    break;
                default:
                    break;
            }
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
        }



        //游戏更新
        private void OnGameUpdate(EventArgs args)
        {

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
                //如果取消挑战模式，重置cproj和cnpc
                CMain.cProjectiles = new CProjectile[Main.maxProjectiles];
                CMain.cNPCs = new CNPC[Main.maxNPCs];

                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                args.Player.SendMessage($"挑战模式已取消，您觉得不够愉快？", Color.DeepPink);
            }
            else
            {
                config.enableChallenge_是否启用挑战模式 = true;
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                //如果开启挑战模式，重新配置npc，proj就算了，太麻烦了
                foreach(NPC n in Main.npc)
                {
                    switch (n.type)
                    {
                        case 4: //克苏鲁之眼
                            CMain.cNPCs[n.whoAmI] = new EyeofCthulhu(Main.npc[n.whoAmI]);
                            break;
                        case 5: //克苏鲁之仆
                            CMain.cNPCs[n.whoAmI] = new EyeofCthulhu_DemonEye(Main.npc[n.whoAmI]);
                            break;
                        case 50: //史莱姆王
                            CMain.cNPCs[n.whoAmI] = new SlimeKing(Main.npc[n.whoAmI]);
                            break;
                        default:
                            break;
                    }
                }
                args.Player.SendMessage($"挑战模式启用，祝您愉快。", Color.GreenYellow);
            }
        }
    }
}
