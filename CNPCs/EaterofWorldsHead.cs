using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TShockAPI;

namespace Challenger.CNPCs
{
    public class EaterofWorldsHead : CNPC
    {
        public EaterofWorldsHead() : base() { }
        public EaterofWorldsHead(NPC npc) : base(npc) { }
        public EaterofWorldsHead(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }


        public override void OnKilled(NPC npc)
        {
            if (EaterofWorldsBody.state == 0 || EaterofWorldsBody.state == 2)
            {
                Projectile proj = NewProjectile(npc.Center, Vector2.Zero, 501, 13, 0);
                proj.timeLeft = 1;
                for (int i = 0; i < 6; i++)
                {
                    Projectile.NewProjectile(null, npc.Center, Vector2.UnitY.RotatedBy(Math.PI * 2 / 6 * i) * 5, 909, 14, 0);
                }
            }
        }


        public override void OnHurtPlayers(NPC npc, GetDataHandlers.PlayerDamageEventArgs e)
        {
            if (Challenger.config.EnableConsumptionMode_启用话痨模式)
            {
                int i = Main.rand.Next(1, 4);
                if (i == 1)
                    Challenger.SendPlayerText("毒牙咬击", new Color(177, 94, 255), npc.Center + new Vector2(0, -30));
                else if (i == 2)
                    Challenger.SendPlayerText("创死你", new Color(177, 94, 255), npc.Center + new Vector2(0, -30));
                else
                    Challenger.SendPlayerText("呜哇哇", new Color(177, 94, 255), npc.Center + new Vector2(0, -30));
            }
        }
    }


    public class EaterofWorldsBody : CNPC
    {
        public EaterofWorldsBody() : base() { }
        public EaterofWorldsBody(NPC npc) : base(npc) { }
        public EaterofWorldsBody(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }

        public static int state = 0;


        public override void NPCAI(NPC npc)
        {
            State = SetState(npc);

            NPCAimedTarget target = npc.GetTargetData();
            switch (State)
            {
                case 0:
                    if (Vector2.DistanceSquared(target.Center, npc.Center) <= 500 * 500 && Main.rand.Next(1200) == 0)
                    {
                        NewProjectile(npc.Center, -Vector2.UnitY * 6, ProjectileID.DD2DrakinShot, 8, 0);
                    }
                    break;
                case 1:
                    if (Vector2.DistanceSquared(target.Center, npc.Center) <= 700 * 700 && Main.rand.Next(800) == 0)
                    {
                        NewProjectile(npc.Center, -Vector2.UnitY * 12, ProjectileID.DD2DrakinShot, 14, 0);
                    }
                    break;
                case 2:
                    if (Vector2.DistanceSquared(target.Center, npc.Center) <= 800 * 800 && Main.rand.Next(300) == 0)
                    {
                        if (Main.rand.Next(2) == 0)
                            NewProjectile(npc.Center, npc.Center.DirectionTo(target.Center).RotatedByRandom(0.1) * 12, ProjectileID.DD2DrakinShot, 17, 0);
                        else
                            NewProjectile(npc.Center, -Vector2.UnitY.RotatedByRandom(0.2) * 10, ProjectileID.DD2DrakinShot, 17, 0);
                    }
                    break;
                default:
                    break;
            }
        }


        public override int SetState(NPC npc)
        {
            int num = 0;
            foreach (NPC n in Main.npc)
            {
                if ((n.type == 13 || n.type == 14 || n.type == 15) && n.active)
                {
                    num++;
                }
            }
            //TSPlayer.All.SendInfoMessage($"num:{num}, state:{state}");
            if (num > 62)
            {
                if (state == 0)
                {
                    state = 1;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("邪恶的蠕虫寻找新的受害者", new Color(177, 94, 255));
                }
                return 0;
            }
            else if (num > 30)
            {
                if (state == 1)
                {
                    state = 2;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("有毒的鳞甲炸裂开来", new Color(177, 94, 255));
                }
                return 1;
            }
            else
            {
                if (state == 2)
                {
                    state = 0;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("怒不可遏", new Color(177, 94, 255));
                }
                return 2;
            }
        }


        public override void OnKilled(NPC npc)
        {
            if (state == 0 || state == 2)
            {
                Projectile proj = NewProjectile(npc.Center, Vector2.Zero, 501, 13, 0);
                proj.timeLeft = 1;
                for (int i = 0; i < 6; i++)
                {
                    Projectile.NewProjectile(null, npc.Center, Vector2.UnitY.RotatedBy(Math.PI * 2 / 6 * i) * 5, 909, 14, 0);
                }
            }
        }


        public override void OnHurtPlayers(NPC npc, GetDataHandlers.PlayerDamageEventArgs e)
        {
            if (Challenger.config.EnableConsumptionMode_启用话痨模式)
            {
                int i = Main.rand.Next(1, 3);
                if (i == 1)
                    Challenger.SendPlayerText("刺啦", new Color(177, 94, 255), npc.Center + new Vector2(0, -30));
                else
                    Challenger.SendPlayerText("小心我爆炸的鳞甲", new Color(177, 94, 255), npc.Center + new Vector2(0, -30));
            }
        }
    }


    public class EaterofWorldsTail : CNPC
    {
        public EaterofWorldsTail() : base() { }
        public EaterofWorldsTail(NPC npc) : base(npc) { }
        public EaterofWorldsTail(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }


        public override void OnKilled(NPC npc)
        {
            if (EaterofWorldsBody.state == 0 || EaterofWorldsBody.state == 2)
            {
                Projectile proj = NewProjectile(npc.Center, Vector2.Zero, 501, 13, 0);
                proj.timeLeft = 1;
                for (int i = 0; i < 6; i++)
                {
                    Projectile.NewProjectile(null, npc.Center, Vector2.UnitY.RotatedBy(Math.PI * 2 / 6 * i) * 5, 909, 14, 0);
                }
            }
        }
    }
}
