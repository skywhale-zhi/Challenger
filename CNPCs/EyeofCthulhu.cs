using System;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using TShockAPI;
using System.Collections.Generic;

namespace Challenger.CNPCs
{
    public class EyeofCthulhu : CNPC
    {
        public EyeofCthulhu() : base() { }
        public EyeofCthulhu(NPC npc) : base(npc) { }
        public EyeofCthulhu(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }


        public const float CooldownOfSkill0 = 150;
        public const float CooldownOfSkill1 = 150;
        public const float CooldownOfSkill2 = 500;

        public const int ProjectileSpeedOfState3 = 5;

        public float skill0 = CooldownOfSkill0;
        public float skill1 = CooldownOfSkill1;
        public float skill2 = CooldownOfSkill2;

        public int state = 0;

        private void Spawn(int number)
        {
            const int MaxServantofCthulhu = 20;
            int count = 0;
            foreach (NPC n in Main.npc)
            {
                if (n.active && n.type == NPCID.ServantofCthulhu)
                {
                    count++;
                }
            }
            if (count >= MaxServantofCthulhu)
                return;
            if (MaxServantofCthulhu - count > number)
            {
                for (int i = 0; i < number; i++)
                {
                    NPC.NewNPC(null, (int)c_npc.Bottom.X + Main.rand.Next(-32, 33), (int)c_npc.Bottom.Y, NPCID.ServantofCthulhu);

                }
            }
            else
            {
                for (int i = 0; i < MaxServantofCthulhu - count; i++)
                {
                    NPC.NewNPC(null, (int)c_npc.Bottom.X + Main.rand.Next(-32, 33), (int)c_npc.Bottom.Y, NPCID.ServantofCthulhu);
                }
            }
        }


        public override void NPCAI(NPC npc)
        {
            NPCAimedTarget target = npc.GetTargetData();

            //变身时发射弹幕
            if (npc.ai[0] == 1 && npc.ai[1] % 2 == 0)
            {
                Projectile proj = NewProjectile(npc.Center, npc.rotation.ToRotationVector2() * 3, ProjectileID.CursedFlameHostile, 8, 5);
                proj.timeLeft = 3 * 60;
            }

            State = SetState(npc);

            switch (State)
            {
                case 0:
                    {
                        //TSPlayer.All.SendInfoMessage($"state 0, ai0:{npc.ai[0]}，ai1:{npc.ai[1]}，ai2:{npc.ai[2]}，ai3:{npc.ai[3]}");
                        skill0--;
                        if (skill0 < 0 && npc.ai[1] == 0)
                        {
                            Vector2 vector = npc.DirectionTo(target.Position);
                            if (npc.ai[2] == 40)
                            {
                                NewProjectile(npc.Bottom, vector * 8, ProjectileID.CursedFlameHostile, 4, 5);
                            }
                            else if (npc.ai[2] == 80)
                            {
                                NewProjectile(npc.Bottom, vector * 10, ProjectileID.CursedFlameHostile, 5, 5);
                            }
                            else if (npc.ai[2] == 100)
                            {
                                NewProjectile(npc.Bottom, vector * 10, ProjectileID.CursedFlameHostile, 6, 6);
                                skill0 = CooldownOfSkill0 + Main.rand.Next(100);
                            }
                        }
                        //增大冲刺力度
                        if (npc.ai[1] == 2 && (npc.ai[3] == 0 || npc.ai[3] == 1 || npc.ai[3] == 2) && npc.ai[2] <= 2)
                        {
                            npc.velocity += npc.velocity * 0.1f;
                            npc.netUpdate = true;
                        }
                    }
                    break;
                case 1:
                    {
                        //TSPlayer.All.SendInfoMessage($"state 1, ai0:{npc.ai[0]}，ai1:{npc.ai[1]}，ai2:{npc.ai[2]}，ai3:{npc.ai[3]}");

                        skill0--;
                        if (skill0 < 0 && npc.ai[1] == 0)
                        {
                            var vector = npc.DirectionTo(target.Position);
                            if (npc.ai[2] == 120)
                            {
                                if (Main.rand.Next(1, 3) == 1)
                                {
                                    NewProjectile(npc.Center, vector * 12, ProjectileID.CursedFlameHostile, 8, 5);
                                    NewProjectile(npc.Center, vector.RotatedBy(0.1) * 11, ProjectileID.CursedFlameHostile, 7, 5);
                                    NewProjectile(npc.Center, vector.RotatedBy(-0.1) * 11, ProjectileID.CursedFlameHostile, 7, 5);
                                }
                                else
                                {
                                    NewProjectile(npc.Center, vector.RotatedBy(0.1) * 11, ProjectileID.CursedFlameHostile, 7, 5);
                                    NewProjectile(npc.Center, vector.RotatedBy(-0.1) * 11, ProjectileID.CursedFlameHostile, 7, 5);
                                }
                                skill0 = CooldownOfSkill0;
                                npc.ai[2] = 100;
                                Spawn(2);
                            }
                        }
                        //增大冲刺力度，并发射射弹
                        if (npc.ai[1] == 2 && (npc.ai[3] == 0 || npc.ai[3] == 1 || npc.ai[3] == 2) && npc.ai[2] == 0)
                        {
                            npc.velocity += npc.velocity;
                            Vector2 unit = npc.velocity.SafeNormalize(Vector2.Zero);
                            npc.netUpdate = true;
                            NewProjectile(npc.Center + unit * 2, unit * 18, ProjectileID.CursedFlameHostile, 5, 5);
                        }
                    }
                    break;
                case 2:
                    {
                        //TSPlayer.All.SendInfoMessage($"state 2, ai0:{npc.ai[0]}，ai1:{npc.ai[1]}，ai2:{npc.ai[2]}，ai3:{npc.ai[3]}");

                        skill1--;

                        if (skill1 < 0)
                        {
                            Spawn(3);
                            skill1 = CooldownOfSkill1 + Main.rand.Next(51);
                        }
                        if (npc.ai[1] == 4 && npc.ai[2] % 15 == 0)
                        {
                            Projectile proj = NewProjectile(npc.Center, Vector2.Zero, ProjectileID.CursedFlameHostile, 5, 5);
                            proj.timeLeft = 3 * 60;
                        }
                        //增大冲刺力度，并发射射弹
                        if (npc.ai[1] == 4 && (npc.ai[3] == 0 || npc.ai[3] == 1 || npc.ai[3] == 2) && npc.ai[2] == 0)
                        {
                            npc.velocity += npc.velocity;
                            Vector2 unit = npc.velocity.SafeNormalize(Vector2.Zero);
                            npc.netUpdate = true;
                            NewProjectile(npc.Center + unit * 2, unit * 20, ProjectileID.CursedFlameHostile, 5, 5);
                        }
                    }
                    break;
                case 3:
                    {
                        //TSPlayer.All.SendInfoMessage($"state 3, ai0:{npc.ai[0]}，ai1:{npc.ai[1]}，ai2:{npc.ai[2]}，ai3:{npc.ai[3]}");
                        skill1--;
                        skill2--;
                        if (skill1 < 0)
                        {
                            Spawn(3);
                            skill1 = CooldownOfSkill1 + 100 + Main.rand.Next(150);
                        }
                        if (skill2 < 0 && npc.ai[1] == 4 && npc.ai[3] == 4)
                        {
                            npc.ai[3] = 0;
                            skill2 = CooldownOfSkill2;
                        }
                        if (npc.ai[1] == 4 && npc.ai[2] % 10 == 0)
                        {

                            Projectile proj = NewProjectile(npc.Center, Vector2.One.RotateRandom(Math.PI) * 0.5f, ProjectileID.CursedFlameHostile, 9, 5);
                            proj.timeLeft = 5 * 60;
                        }
                        //增大冲刺力度，并发射射弹
                        if (npc.ai[1] == 4 && (npc.ai[3] == 0 || npc.ai[3] == 1 || npc.ai[3] == 2) && npc.ai[2] == 0)
                        {
                            npc.velocity += npc.velocity;
                            Vector2 unit = npc.velocity.SafeNormalize(Vector2.Zero);
                            npc.netUpdate = true;
                            NewProjectile(npc.Center + unit * 2, unit * 30, ProjectileID.CursedFlameHostile, 6, 5);
                        }
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
                    TSPlayer.All.SendMessage("燃烧！无法熄灭的火焰", new Color(225, 71, 71));
                }
                return 0;
            }
            else if (npc.life >= npc.lifeMax * 0.4f)
            {
                if (state == 1)
                {
                    state = 2;
                    TSPlayer.All.SendMessage("你找到那颗子弹了吗", new Color(225, 71, 71));
                }
                return 1;
            }
            else if (npc.life >= npc.lifeMax * 0.2f)
            {
                if (state == 2)
                {
                    state = 3;
                    TSPlayer.All.SendMessage("猪突猛进！", new Color(225, 71, 71));
                }
                return 2;
            }
            else
            {
                if (state == 3)
                {
                    state = 4;
                    TSPlayer.All.SendMessage("疯狗狂叫！！！", new Color(225, 71, 71));
                }
                return 3;
            }
        }
    }


    public class EyeofCthulhu_DemonEye : CNPC
    {
        public EyeofCthulhu_DemonEye() : base() { }
        public EyeofCthulhu_DemonEye(NPC npc) : base(npc) { }
        public EyeofCthulhu_DemonEye(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }


        public static readonly float CooldownOfSkill0 = 200;

        public float skill0 = CooldownOfSkill0;

        public override void NPCAI(NPC npc)
        {
            skill0--;
            NPCAimedTarget target = npc.GetTargetData();
            var vector = npc.DirectionTo(target.Position + new Vector2(Main.rand.Next(-32, 33), Main.rand.Next(-32, 33)));

            if (skill0 < 0 && (vector.X * npc.velocity.X > 0) && npc.HasPlayerTarget)
            {
                NewProjectile(npc.Center, vector * 6f, ProjectileID.PinkLaser, 4, 5);
                skill0 += CooldownOfSkill0 + Main.rand.Next(51);
            }
        }
    }
}
