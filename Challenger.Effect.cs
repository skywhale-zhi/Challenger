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
        //接触敌怪吸血效果
        public void TouchedAndBeSucked(GetDataHandlers.PlayerDamageEventArgs e)
        {
            NPC npc = Main.npc[e.PlayerDeathReason._sourceNPCIndex];
            int number;
            if (!npc.boss)
            {
                if (npc.lifeMax * 0.1f <= 200)
                    number = (int)((e.Damage * 0.7f + npc.lifeMax * 0.1f) * config.BloodAbsorptionRatio_吸血比率);
                else
                    number = (int)((e.Damage * 0.7f + 250) * config.BloodAbsorptionRatio_吸血比率);

            }
            else
            {
                number = (int)((e.Damage * 0.3f + npc.lifeMax * 0.01f) * config.BloodAbsorptionRatio_吸血比率 * 2);
            }
            if (npc.life >= npc.lifeMax)
            {
                npc.life = npc.lifeMax;
            }

            //制作吸血视觉效果
            Vector2 position = Main.player[e.Player.TPlayer.whoAmI].Center;
            Vector2 v = (npc.Center - position).SafeNormalize(Vector2.Zero);
            CProjectile proj = CProjectile.NewCProjectile(null, position, v, 125, 0, 0);
            proj.c_proj.ai[0] = e.PlayerDeathReason._sourceNPCIndex;
            proj.c_proj.ai[1] = number;

            //若是血包被玩家接住，则回血，回血量 = （敌怪伤害 - 玩家防御）* （1 - 玩家伤害减免）, 血包放在c_ai[2]里保存
            int da = e.Damage - Main.player[e.Player.Index].statDefense >= 0 ? e.Damage - Main.player[e.Player.Index].statDefense : 0;
            da = (int)((1f - Main.player[e.Player.Index].endurance) * da);
            proj.c_ai[2] = da / 2;

            proj.c_proj.tileCollide = false;
            proj.c_proj.timeLeft = 20 * 60;
            proj.c_proj.netUpdate = true;

        }


        //被射弹射中敌怪吸血效果
        public void ProjAndBeSucked(GetDataHandlers.PlayerDamageEventArgs e)
        {
            //制作吸血效果
            Vector2 position = Main.player[e.Player.TPlayer.whoAmI].Center;
            CProjectile proj = CProjectile.NewCProjectile(null, position, Vector2.Zero, 125, 0, 0);
            proj.c_proj.ai[0] = 0;
            proj.c_proj.ai[1] = e.Damage * config.BloodAbsorptionRatio_吸血比率;

            //若是血包被玩家接住，则回血，回血量 = （敌怪伤害 - 玩家防御）* （1 - 玩家伤害减免）；
            int da = e.Damage - Main.player[e.Player.Index].statDefense >= 0 ? e.Damage - Main.player[e.Player.Index].statDefense : 0;
            da = (int)((1f - Main.player[e.Player.Index].endurance) * da);
            proj.c_ai[2] = da / 2;

            proj.c_proj.tileCollide = false;
            proj.c_proj.timeLeft = 20 * 60;
            proj.c_proj.netUpdate = true;


        }


        //血腥套装吸血效果
        public void CrimsonArmorEffect(NpcStrikeEventArgs args)
        {
            Item[] items = args.Player.armor;
            if (items[0].netID == 792 && items[1].netID == 793 && items[2].netID == 794 && args.Critical)
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
                    int index = Projectile.NewProjectile(null, n.Center, Vector2.Zero, 305, 0, 0);
                    HealPlayer(args.Player, healnum);
                }
            }
        }


        //魔矿套效果
        public void ShadowArmorEffect(NpcStrikeEventArgs args)
        {
            Item[] items = args.Player.armor;
            NPC npc = args.Npc;
            //102 101 100
            //956 957 958
            if ((items[0].netID == 102 || items[0].netID == 956) && (items[1].netID == 101 || items[1].netID == 957) && (items[2].netID == 100 || items[2].netID == 958) && args.Critical)
            {
                int count = Main.rand.Next(1,4);
                for (int i = 0; i < count; i++)
                {
                    int index = Projectile.NewProjectile(null, args.Player.Center, new Vector2((float)Math.Cos(Main.rand.NextDouble() * MathHelper.TwoPi), (float)Math.Sin(Main.rand.NextDouble() * MathHelper.TwoPi)), 307, 25, 2);
                    Main.projectile[index].scale *= 0.5f;
                }
            }

        }


    }
}
