using System;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using TShockAPI;
using System.Collections.Generic;

namespace Challenger.CNPCs
{
    public class QueenBee : CNPC
    {
        public QueenBee() : base() { }
        public QueenBee(NPC npc) : base(npc) { }
        public QueenBee(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }

        int state = 0;

        int timer = 0;
        public override void NPCAI(NPC npc)
        {
            NPCAimedTarget target = npc.GetTargetData();
            State = SetState(npc);
            //TSPlayer.All.SendInfoMessage($"state {State}, ai0:{npc.ai[0]}，ai1:{npc.ai[1]}，ai2:{npc.ai[2]}，ai3:{npc.ai[3]}");
            //ai[0]=0,ai[1]=1,3,5,ai[2] = 0（开始冲）/1可能用来判断是否冲刺
            //ai[0]=3,ai[1]0~700,ai[2] = 0,ai[3] = 0 发射毒刺
            //ai[0]=1,ai[1]0~00,ai[2]发射了几个蜜蜂
            

            switch (State)
            {
                case 0://强化冲刺力度
                    if (npc.ai[0] == 0 && (npc.ai[1] == 1 || npc.ai[1] == 3 || npc.ai[1] == 5) && npc.ai[2] == 0 && timer < 1)
                    {
                        timer++;
                        if (npc.direction == 1 && target.Position.X > npc.position.X || npc.direction == -1 && target.Position.X < npc.position.X)
                        {
                            npc.velocity += npc.velocity * 0.03f;
                        }
                        else
                        {
                            npc.velocity -= npc.velocity * 0.03f;
                        }
                        npc.netUpdate = true;
                    }
                    else
                    {
                        timer = 0;
                    }
                    break;
                case 1:
                    if (npc.ai[0] == 0 && (npc.ai[1] == 1 || npc.ai[1] == 3 || npc.ai[1] == 5) && npc.ai[2] == 0 && timer < 1)
                    {
                        timer++;
                        if (npc.direction == 1 && target.Position.X > npc.position.X || npc.direction == -1 && target.Position.X < npc.position.X)
                        {
                            npc.velocity += npc.velocity * 0.06f;
                        }
                        else
                        {
                            npc.velocity -= npc.velocity * 0.06f;
                        }
                        npc.netUpdate = true;
                    }
                    else
                    {
                        timer = 0;
                    }
                    break;
                case 2:
                    if (npc.ai[0] == 0 && (npc.ai[1] == 1 || npc.ai[1] == 3 || npc.ai[1] == 5) && npc.ai[2] == 0 && timer < 1)
                    {
                        timer++;
                        if (npc.direction == 1 && target.Position.X > npc.position.X || npc.direction == -1 && target.Position.X < npc.position.X)
                        {
                            npc.velocity += npc.velocity * 0.1f;
                        }
                        else
                        {
                            npc.velocity -= npc.velocity * 0.1f;
                        }
                        npc.netUpdate = true;
                    }
                    else
                    {
                        timer = 0;
                    }



                    if (Main.rand.Next(6) == 0)
                    {
                        NewProjectile(npc.Bottom, Vector2.UnitY.RotateRandom(Math.PI / 2) * -8, ProjectileID.QueenBeeStinger, 12, 1);
                    }
                    break;
                case 3:
                    if (npc.ai[0] == 0 && (npc.ai[1] == 1 || npc.ai[1] == 3 || npc.ai[1] == 5) && npc.ai[2] == 0 && timer < 1)
                    {
                        timer++;
                        if (npc.direction == 1 && target.Position.X > npc.position.X || npc.direction == -1 && target.Position.X < npc.position.X)
                        {
                            npc.velocity += npc.velocity * 0.12f;
                        }
                        else
                        {
                            npc.velocity -= npc.velocity * 0.12f;
                        }
                        npc.netUpdate = true;
                    }
                    else
                    {
                        timer = 0;
                    }



                    if (npc.ai[1] % 12 == 0)
                    {
                        NewProjectile(npc.position - new Vector2(Main.rand.Next(16 * -64, 16 * 64), 16 * 24), Vector2.UnitY * -3, ProjectileID.QueenBeeStinger, 20, 1);
                    }
                    break;
                default:
                    break;
            }
        }

        public override int SetState(NPC npc)
        {
            if (npc.life >= npc.lifeMax * 0.7f)
            {
                if (state == 0)
                {
                    state = 1;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("谁人惊扰了我的蜂巢！", Color.Yellow);
                }
                return 0;
            }
            else if (npc.life >= npc.lifeMax * 0.4f)
            {
                if (state == 1)
                {
                    state = 2;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("不许抢我的蜂蜜", Color.Yellow);
                }
                return 1;
            }
            else if (npc.life >= npc.lifeMax * 0.2f)
            {
                if (state == 2)
                {
                    state = 3;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("毒刺射你一脸", Color.Yellow);
                }
                return 2;
            }
            else
            {
                if (state == 3)
                {
                    state = 4;
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
                    Challenger.SendPlayerText("嗡嗡", Color.Yellow, npc.Center + new Vector2(0, -30));
                else if (i == 2)
                    Challenger.SendPlayerText("嗡嗡嗡嗡", Color.Yellow, npc.Center + new Vector2(0, -30));
                else
                    Challenger.SendPlayerText("吱嗡", Color.Yellow, npc.Center + new Vector2(0, -30));
            }
        }
    }
}
