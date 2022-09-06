using System;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using TShockAPI;
using System.Collections.Generic;

namespace Challenger.CNPCs
{
    public class Deerclops : CNPC
    {
        public Deerclops() : base() { }
        public Deerclops(NPC npc) : base(npc) { }
        public Deerclops(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }

        int state = 0;

        const float CooldownOfSkill0 = 350;

        float skill0 = 0;

        public override void NPCAI(NPC npc)
        {
            skill0++;
            NPCAimedTarget target = npc.GetTargetData();

            if (npc.ai[0] == 6)
            {
                npc.life += 2;
                npc.StrikeNPC(0, 0, 0);
            }
            State = SetState(npc);

            switch (State)
            {
                case 0:
                    {
                        if (npc.ai[0] == 5 && npc.ai[1] == 59)
                        {
                            if (Main.netMode != 1)
                            {
                                for (int index = 0; index < 3; ++index)
                                {
                                    Projectile.RandomizeInsanityShadowFor(Main.player[npc.target], true, out Vector2 spawnposition, out Vector2 spawnvelocity, out float ai0, out float ai1);
                                    NewProjectile(spawnposition, spawnvelocity, 965, 12, 0, Main.myPlayer, ai0, ai1);
                                }
                            }

                        }
                    }
                    break;
                case 1:
                    {
                        if (npc.ai[0] == 1 && npc.ai[1] == 30)
                        {
                            Point tileCoordinates = npc.Top.ToTileCoordinates();
                            for (int whichOne = 5; whichOne < 20; ++whichOne)
                                npc.AI_123_Deerclops_ShootRubbleUp(ref target, ref tileCoordinates, 20, 1, 200, whichOne);
                            if (Main.rand.Next(1) == 0 && npc.ai[1] == 79)
                            {
                                npc.ai[0] = 5;
                                npc.ai[1] = 0;
                            }

                        }
                        else if (npc.ai[0] == 5 && npc.ai[1] == 59)
                        {
                            if (Main.netMode != 1)
                            {
                                for (int index = 0; index < 8; ++index)
                                {
                                    Vector2 spawnposition;
                                    Vector2 spawnvelocity;
                                    float ai0;
                                    float ai1;
                                    Projectile.RandomizeInsanityShadowFor(Main.player[npc.target], true, out spawnposition, out spawnvelocity, out ai0, out ai1);
                                    NewProjectile(spawnposition, spawnvelocity, 965, 12, 0, Main.myPlayer, ai0, ai1);
                                }
                            }

                        }
                        if (skill0 >= 7)
                        {
                            NewProjectile(target.Position + new Vector2(Main.rand.Next(-16 * 96, 16 * 96), -16 * 48), Vector2.UnitY, ProjectileID.IceSpike, 5, 0);
                            skill0 = 0;
                        }
                    }
                    break;
                case 2:
                    {
                        if (npc.ai[0] == 1 && npc.ai[1] == 30)
                        {
                            Point tileCoordinates = npc.Top.ToTileCoordinates();
                            for (int whichOne = 5; whichOne < 20; ++whichOne)
                                npc.AI_123_Deerclops_ShootRubbleUp(ref target, ref tileCoordinates, 20, 1, 200, whichOne);
                            if (Main.rand.Next(1) == 0 && npc.ai[1] == 79)
                            {
                                npc.ai[0] = 5;
                                npc.ai[1] = 0;
                            }

                        }
                        else if (npc.ai[0] == 5 && npc.ai[1] == 59)
                        {
                            if (Main.netMode != 1)
                            {
                                for (int index = 0; index < 8; ++index)
                                {
                                    Vector2 spawnposition;
                                    Vector2 spawnvelocity;
                                    float ai0;
                                    float ai1;
                                    Projectile.RandomizeInsanityShadowFor(Main.player[npc.target], true, out spawnposition, out spawnvelocity, out ai0, out ai1);
                                    NewProjectile(spawnposition, spawnvelocity, 965, 13, 0, Main.myPlayer, ai0, ai1);
                                }
                            }

                        }
                        if (skill0 >= 3)
                        {
                            NewProjectile(target.Position + new Vector2(Main.rand.Next(-16 * 64, 16 * 64), -16 * 64), Vector2.UnitY * 3f, ProjectileID.IceSpike, 9, 5);
                            skill0 = 0;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public override int SetState(NPC npc)
        {
            if (npc.life >= npc.lifeMax * 0.6f)
            {
                if (state == 0)
                {
                    state = 1;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("远方的巨兽将会摧毁你所拥有的一切", new Color(111, 160, 213));
                }
                return 0;
            }
            else if (npc.life >= npc.lifeMax * 0.3f)
            {
                if (state == 1)
                {
                    state = 2;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("冰雪从天而降", new Color(111, 160, 213));
                }
                return 1;
            }
            else
            {
                if (state == 2)
                {
                    state = 3;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("你将受到灭顶之灾", new Color(111, 160, 213));
                }
                return 2;
            }
        }

        public override void OnHurtPlayers(NPC npc, GetDataHandlers.PlayerDamageEventArgs e)
        {
            if (Challenger.config.EnableConsumptionMode_启用话痨模式)
            {
                int i = Main.rand.Next(1, 3);
                if (i == 1)
                    Challenger.SendPlayerText("拆掉拆掉！", new Color(111, 160, 213), npc.Center + new Vector2(0, -30));
                else
                    Challenger.SendPlayerText("嗷嗷", new Color(111, 160, 213), npc.Center + new Vector2(0, -30));
            }
        }
    }
}
