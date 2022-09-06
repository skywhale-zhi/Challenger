using System;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using TShockAPI;
using System.Collections.Generic;

namespace Challenger.CNPCs
{
    public class BrainofCthulhu : CNPC
    {
        public BrainofCthulhu() : base() { }
        public BrainofCthulhu(NPC npc) : base(npc) { }
        public BrainofCthulhu(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }

        int state = 0;
        public override void NPCAI(NPC npc)
        {
            //TSPlayer.All.SendInfoMessage($"state{State}, ai[0]:{npc.ai[0]}，ai[1]:{npc.ai[1]}，ai[2]:{npc.ai[2]}，ai[3]:{npc.ai[3]}");
            //ai[0]-3 ~ -1从远到进
            //ai[1]887
            //ai[2]176
            //ai[3]透明度
        }


        public override int SetState(NPC npc)
        {
            if (npc.life >= npc.lifeMax * 0.7f)
            {
                if (state == 0)
                {
                    state = 1;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("", Color.Red);
                }
                return 0;
            }
            else if (npc.life >= npc.lifeMax * 0.4f)
            {
                if (state == 1)
                {
                    state = 2;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("", Color.Red);
                }
                return 1;
            }
            else if (npc.life >= npc.lifeMax * 0.2f)
            {
                if (state == 2)
                {
                    state = 3;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("", Color.Red);
                }
                return 2;
            }
            else
            {
                if (state == 3)
                {
                    state = 4;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("", Color.Red);
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
                    Challenger.SendPlayerText("呃啊", Color.Red, npc.Center + new Vector2(0, -30));
                else if (i == 2)
                    Challenger.SendPlayerText("哇哇嗷", Color.Red, npc.Center + new Vector2(0, -30));
                else
                    Challenger.SendPlayerText("歪比", Color.Red, npc.Center + new Vector2(0, -30));
            }
        }
    }


    public class Creeper : CNPC
    {
        public Creeper() : base() { }
        public Creeper(NPC npc) : base(npc) { }
        public Creeper(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }

        int state = 0;
        public override void NPCAI(NPC npc)
        {

        }

        public override void OnKilled(NPC npc)
        {
            int num = Main.rand.Next(2, 6);
            for (int i = 0; i < num; i++)
            {
                float offx = (float)Main.rand.NextDouble() - 0.5f;
                float offy = -0.25f * (float)Math.Cos(MathHelper.PiOver2 / 0.5 * offx);
                NewProjectile(npc.position, new Vector2(offx, offy) * 17, ProjectileID.BloodShot, 16, 5);
            }
        }
    }
}
