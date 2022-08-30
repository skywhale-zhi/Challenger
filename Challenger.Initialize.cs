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


        //怪物触碰玩家时吸血
        private void PlayerSufferDamage(object sender, GetDataHandlers.PlayerDamageEventArgs e)
        {
            if (!config.enableChallenge_是否启用挑战模式 || !config.enableMonsterSucksBlood_是否启用怪物吸血)
                return;

            if (e.PlayerDeathReason._sourceNPCIndex != -1)
            {
                TouchedAndBeSucked(e);
            }
            else if (e.PlayerDeathReason._sourceProjectileType != -1)
            {
                ProjAndBeSucked(e);


            }
        }


        //怪物血量调整
        private HookResult OnNpcSpawn(ref int index)
        {
            if (config.enableChallenge_是否启用挑战模式)
            {
                NPC npc = Main.npc[index];
                npc.life = npc.lifeMax = (int)(npc.lifeMax * 1.2f);
                npc.netUpdate = true;
            }
            return HookResult.Continue;
        }


        //修改射弹的ai
        private void OnProjAIUpdate(ProjectileAiUpdateEventArgs args)
        {
            if (!config.enableChallenge_是否启用挑战模式)
            {
                return;
            }

            //如果是血包射弹的话 (ai[0]是造成接触伤害的敌怪索引，ai[1]是造成的伤害，c_ai[2]是补给给玩家的血)
            BloodBagAI(args);

            //魔法剑射弹
            MagicSwordAI(args);

        }


        //npc击中时触发
        private void OnNpcStrike(NpcStrikeEventArgs args)
        {
            if (!config.enableChallenge_是否启用挑战模式)
            {
                return;
            }

            CrimsonArmorEffect(args);

            ShadowArmorEffect(args);
        }


        //游戏更新
        private void OnGameUpdate(EventArgs args)
        {
        }


        //修改npc的ai
        private void OnNpcPostAI(NPC npc)
        {
            if (!config.enableChallenge_是否启用挑战模式)
            {
                return;
            }

            int index = npc.netID;
            switch (index)
            {
                case 4:
                    EyeofCthulhu(npc);
                    break;
                default:
                    break;
            }
        }


        //测试
        private void OnTest(ServerChatEventArgs args)
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
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                args.Player.SendMessage($"挑战模式已取消，您觉得不够愉快？", Color.DeepPink);
            }
            else
            {
                config.enableChallenge_是否启用挑战模式 = true;
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                args.Player.SendMessage($"挑战模式启用，祝您愉快。", Color.GreenYellow);
            }
        }


    }
}
