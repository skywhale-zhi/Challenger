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
                NPC npc = Main.npc[e.PlayerDeathReason._sourceNPCIndex];
                int number;
                if (!npc.boss)
                {
                    if (npc.lifeMax * 0.1f <= 200)
                        number = (int)((e.Damage * 0.7f + npc.lifeMax * 0.1f) * config.BloodAbsorptionRatio_吸血比率);
                    else
                        number = (int)((e.Damage * 0.7f + 250) * config.BloodAbsorptionRatio_吸血比率);
                    npc.life += number;
                    
                }
                else
                {
                    number = (int)((e.Damage * 0.3f + npc.lifeMax * 0.01f) * config.BloodAbsorptionRatio_吸血比率 * 2);
                    npc.life += number;
                }
                if (npc.life >= npc.lifeMax)
                {
                    npc.life = npc.lifeMax;
                }
                npc.HealEffect(number);
                npc.netUpdate = true;
            }
        }

        //怪物属性调整
        private HookResult OnNPCSpawn(ref int index)
        {
            if (!config.enableChallenge_是否启用挑战模式)
            {
                return HookResult.Cancel;
            }
            NPC npc = Main.npc[index];
            npc.life = npc.lifeMax *= 100;
            //TSPlayer.All.SendInfoMessage($"怪物{npc.FullName},的生命值{npc.life}/{npc.lifeMax}生成");
            return HookResult.Continue;
        }

        private void OnPostNetDefaults(NPC npc, ref int type, ref NPCSpawnParams spawnparams)
        {
            npc.life = npc.lifeMax *= 100;
        }

        private void OnPostAI(NPC npc)
        {
            //npc.life = npc.lifeMax *= 100;
            //TSPlayer.All.SendInfoMessage($"怪物{npc.FullName},的生命值{npc.life}/{npc.lifeMax}生成");
        }

        private HookResult OnPreAI(NPC npc)
        {
            return HookResult.Continue;
        }


    }

}
