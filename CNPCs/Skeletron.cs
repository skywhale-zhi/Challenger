using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TShockAPI;

namespace Challenger.CNPCs
{
    public class Skeletron : CNPC
    {
        public Skeletron() : base() { }
        public Skeletron(NPC npc) : base(npc) { }
        public Skeletron(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }

        int state = 0;

        float rotate = 0.0f;

        List<int> surrandIndex = new List<int>();

        const int MaxSurrandNum = 12;

        int skill0 = 240;

        public override void NPCAI(NPC npc)
        {
            //TSPlayer.All.SendInfoMessage($"state{State}, ai[0]:{npc.ai[0]}，ai[1]:{npc.ai[1]}，ai[2]:{npc.ai[2]}，ai[3]:{npc.ai[3]}");
            skill0--;
            State = SetState(npc);
            NPCAimedTarget target = npc.GetTargetData();
            switch (State)
            {
                case 0:
                    {
                        if (skill0 < 0)
                        {
                            Vector2 vector = npc.Center.DirectionTo(target.Center);
                            NewProjectile(npc.Center, vector.RotatedBy(MathHelper.Pi / 6), 270, 11, 5);
                            skill0 = 220 + Main.rand.Next(-60, 61);
                        }
                    }
                    break;
                case 1:
                    {
                        if (skill0 < 0)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                NewProjectile(npc.Center, Vector2.UnitY.RotatedBy(Math.PI * 2.0 / 4 * i + rotate) * 4, 270, 12, 5);
                            }
                            skill0 = 160 + Main.rand.Next(-60, 61);
                        }
                        break;
                    }
                case 2:
                    {
                        if (skill0 < 0)
                        {
                            int num = Main.rand.Next(4, 7);
                            for (int i = 0; i < num; i++)
                            {
                                NewProjectile(npc.Center, Vector2.UnitY.RotatedBy(Math.PI * 2.0 / 5 * i + rotate) * 3, 270, 15, 5);
                            }
                            skill0 = 120 + Main.rand.Next(-60, 61);
                        }

                        if (npc.ai[1] == 1 && npc.ai[2] % 10 == 0)
                        {
                            Projectile proj = NewProjectile(npc.Center, Vector2.Zero, ProjectileID.Shadowflames, 15, 5);
                            proj.timeLeft = 40;
                        }
                    }
                    break;
                case 3:
                    {
                        if (skill0 < 0)
                        {
                            int num = Main.rand.Next(5, 10);
                            if (Main.rand.Next(2) == 0)
                            {
                                for (int i = 0; i < num; i++)
                                {
                                    NewProjectile(npc.Center, Vector2.UnitY.RotatedBy(Math.PI * 2.0 / 6 * i + rotate) * 3, 270, 18, 30);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < num; i++)
                                {
                                    NewProjectile(npc.Center, Vector2.UnitY.RotatedBy(Math.PI * 2.0 / 8 * i + rotate) * 5, 299, 18, 30);
                                }
                            }
                            skill0 = 100 + Main.rand.Next(-60, 31);
                        }

                        if (npc.ai[1] == 1 && npc.ai[2] % 5 == 0)
                        {
                            Projectile proj = NewProjectile(npc.Center, Vector2.Zero, ProjectileID.Shadowflames, 20, 5);
                            proj.timeLeft = 180;
                        }
                    }
                    break;
            }

            if (c_ai[0] < MaxSurrandNum && Main.rand.Next(180) == 0)
            {
                int index = NPC.NewNPC(npc.GetSpawnSourceForNPCFromNPCAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.CursedSkull);

                CMain.cNPCs[index].c_ai[0] = npc.whoAmI;
                c_ai[0]++;
                surrandIndex.Add(index);

            }
            for (int v = 0; v < surrandIndex.Count; v++)
            {
                int i = surrandIndex[v];
                if (CMain.cNPCs[i] == null || CMain.cNPCs[i].c_npc.type != NPCID.CursedSkull || CMain.cNPCs[i].c_npc.type == 34 && CMain.cNPCs[i].c_ai[0] != npc.whoAmI)
                {
                    surrandIndex.Remove(i);
                    c_ai[0]--;
                }
            }

            rotate += 0.1f;
        }


        public override int SetState(NPC npc)
        {
            if (npc.life >= npc.lifeMax * 0.7f)
            {
                if (state == 0)
                {
                    state = 1;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("被封印的骷髅帝王苏醒", new Color(150, 143, 102));
                }
                return 0;
            }
            else if (npc.life >= npc.lifeMax * 0.4f)
            {
                if (state == 1)
                {
                    state = 2;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("嘎吱作响", new Color(150, 143, 102));
                }
                return 1;
            }
            else if (npc.life >= npc.lifeMax * 0.2f)
            {
                if (state == 2)
                {
                    state = 3;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("诅咒开始应验", new Color(150, 143, 102));
                }
                return 2;
            }
            else
            {
                if (state == 3)
                {
                    state = 4;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("惨朽不堪", new Color(150, 143, 102));
                }
                return 3;
            }
        }


        public override void OnHurtPlayers(NPC npc, GetDataHandlers.PlayerDamageEventArgs e)
        {
            if (Challenger.config.EnableConsumptionMode_启用话痨模式)
            {
                int i = Main.rand.Next(1, 4);
                if (i == 1)
                    Challenger.SendPlayerText("再让我逮到一下你就玩玩", new Color(150, 143, 102), npc.Center + new Vector2(0, -30));
                else if (i == 2)
                    Challenger.SendPlayerText("创死你", new Color(150, 143, 102), npc.Center + new Vector2(0, -30));
                else
                    Challenger.SendPlayerText("想再贴贴吗？", new Color(150, 143, 102), npc.Center + new Vector2(0, -30));
            }
        }


        public override void OnKilled(NPC npc)
        {
            for (int i = 0; i < 35; i++)
            {
                Projectile.NewProjectile(null, npc.Center, Vector2.UnitY.RotatedBy(Math.PI * 2 / 35 * i) * 5, 299, 21, 10);
            }
        }
    }


    public class SkeletronHand : CNPC
    {
        public SkeletronHand() : base() { }
        public SkeletronHand(NPC npc) : base(npc) { }
        public SkeletronHand(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }

        int state = 0;

        int timer = 0;

        public override void NPCAI(NPC npc)
        {
            State = SetState(npc);
            switch (State)
            {
                case 0:
                    if (npc.ai[2] == 2 || npc.ai[2] == 5)
                    {
                        Projectile proj = NewProjectile(npc.Center, Vector2.Zero, ProjectileID.Shadowflames, 10, 5);
                        proj.timeLeft = 30;
                    }
                    break;
                case 1:
                    if (npc.ai[2] == 2 || npc.ai[2] == 5)
                    {
                        timer++;
                        if (timer % 5 == 0)
                        {
                            Projectile proj = NewProjectile(npc.Center, Vector2.Zero, ProjectileID.Shadowflames, 10, 5);
                            proj.timeLeft = 60 * 40;
                        }
                    }
                    else
                    {
                        timer = 0;
                    }
                    break;
                default:
                    break;
            }
        }


        public override int SetState(NPC npc)
        {
            if (npc.life >= npc.lifeMax * 0.5f)
            {
                if (state == 0)
                {
                    state = 1;
                }
                return 0;
            }
            else
            {
                if (state == 1)
                {
                    state = 2;
                    if (npc.ai[0] == -1 && Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("你打痛我左手了！！！", new Color(150, 143, 102));
                    if (npc.ai[0] == 1 && Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("你打痛我右手了！！！", new Color(150, 143, 102));
                }
                return 1;
            }
        }

        public override void OnKilled(NPC npc)
        {
            if (npc.ai[0] == -1)
            {
                double f = Main.rand.NextDouble() * 3;
                for (int i = 0; i < 10; i++)
                {
                    Projectile.NewProjectile(null, npc.Center, Vector2.UnitY.RotatedBy(Math.PI * 2 / 10 * i + f) * 5, 270, 20, 30);
                }
            }
            else
            {
                double f = Main.rand.NextDouble() * 3;
                for (int i = 0; i < 10; i++)
                {
                    Projectile.NewProjectile(null, npc.Center, Vector2.UnitY.RotatedBy(Math.PI * 2 / 10 * i + f) * 5, 299, 20, 30);
                }
            }
        }

        public override void OnHurtPlayers(NPC npc, GetDataHandlers.PlayerDamageEventArgs e)
        {
            if (Challenger.config.EnableConsumptionMode_启用话痨模式)
            {
                int i = Main.rand.Next(1, 4);
                if (i == 1)
                    Challenger.SendPlayerText("就这还想打倒我骷髅王爷爷", new Color(150, 143, 102), npc.Center + new Vector2(0, -30));
                else if (i == 2)
                    Challenger.SendPlayerText("看我一记耳光", new Color(150, 143, 102), npc.Center + new Vector2(0, -30));
                else
                    Challenger.SendPlayerText("离地牢远点！！！", new Color(150, 143, 102), npc.Center + new Vector2(0, -30));
            }
        }

    }


    public class Skeletron_Surrand : CNPC
    {
        public Skeletron_Surrand() : base() { }
        public Skeletron_Surrand(NPC npc) : base(npc) { }
        public Skeletron_Surrand(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }

        int skill0 = 240;

        public override void NPCAI(NPC npc)
        {
            //TSPlayer.All.SendInfoMessage($"state{State}, ai[0]:{npc.ai[0]}，ai[1]:{npc.ai[1]}，ai[2]:{npc.ai[2]}，ai[3]:{npc.ai[3]}");
            if (Main.npc[(int)c_ai[0]].active && Main.npc[(int)c_ai[0]].type == NPCID.SkeletronHead)
            {
                skill0--;

                if (skill0 < 0)
                {
                    NPC.NewNPC(npc.GetSpawnSourceForNPCFromNPCAI(), (int)npc.Center.X, (int)npc.Center.Y, 33);
                    skill0 = 180 + Main.rand.Next(-60, 60);
                }
            }

        }

        public override void OnKilled(NPC npc)
        {
            NPCAimedTarget target = npc.GetTargetData();
            NewProjectile(npc.Center, npc.Center.DirectionTo(target.Center) * 5f, 270, 6, 30);
        }
    }
}
