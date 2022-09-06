using System;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using TShockAPI;

namespace Challenger.CNPCs
{
    public class SlimeKing : CNPC
    {
        public SlimeKing() : base() { }
        public SlimeKing(NPC npc) : base(npc) { }
        public SlimeKing(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }


        public readonly float CooldownOfSkill0 = 250;
        public readonly float CooldownOfSkill1 = 250;
        public readonly float CooldownOfSkill2 = 250;

        int state = 0;

        //ai设置 cai[0],cai[1],cai[2]用来设置冷却时间，cai[3]用来进行状态宣告
        public override void NPCAI(NPC npc)
        {
            CNPC slimeKing = CMain.cNPCs[npc.whoAmI];

            //获取目标player的信息
            NPCAimedTarget target = npc.GetTargetData();
            //产生砸击冰雪尖刺
            if (npc.ai[0] > -200 && npc.ai[0] < -120)
            {
                Point tileCoordinates = npc.Bottom.ToTileCoordinates();
                int howMany = 20;
                int num4 = 1;
                tileCoordinates.X += npc.direction * 3;
                int num5 = (int)((npc.ai[0] / 2) + 101);
                int num6 = 4;
                int num7 = num5 / num6 * num6;
                int num8 = num7 + num6;
                if (num5 % num6 != 0)
                    num8 = num7;
                for (int whichOne = num7; whichOne < num8 && whichOne < howMany; ++whichOne)
                {
                    int xOffset = whichOne * num4;
                    npc.AI_123_Deerclops_TryMakingSpike(ref tileCoordinates, npc.direction, howMany, whichOne, xOffset);
                    npc.AI_123_Deerclops_TryMakingSpike(ref tileCoordinates, -npc.direction, howMany, whichOne, xOffset);
                }
            }

            //设置状态
            slimeKing.State = SetState(npc);

            switch (slimeKing.State)
            {
                case 0:
                    {
                        slimeKing.c_ai[0]--;
                        if (slimeKing.c_ai[0] < 0 && npc.ai[0] != -120 && npc.ai[0] != -200)
                        {
                            var vector = new Vector2((target.Position.X - npc.Center.X) > 0 ? 1 : -1, -2);
                            npc.velocity.X += vector.X * Main.rand.Next(5, 12);
                            npc.velocity.Y += vector.Y * Main.rand.Next(1, 4);
                            slimeKing.c_ai[0] = CooldownOfSkill0 + Main.rand.Next(121);
                            npc.netUpdate = true;
                        }
                    }
                    break;
                case 1:
                    {
                        slimeKing.c_ai[0]--;
                        slimeKing.c_ai[1]--;
                        if (slimeKing.c_ai[0] < 0 && npc.ai[0] != -120 && npc.ai[0] != -200)
                        {
                            var vector = new Vector2((target.Position.X - npc.Center.X) > 0 ? 1 : -1, -2);
                            npc.velocity += (vector * Main.rand.Next(3, 7));
                            slimeKing.c_ai[0] = CooldownOfSkill0 + Main.rand.Next(81);
                            npc.netUpdate = true;
                        }
                        if (slimeKing.c_ai[1] < 0)
                        {
                            slimeKing.NewProjectile(npc.Center, Vector2.Zero, ProjectileID.CultistBossIceMist, 16, 32);
                            slimeKing.c_ai[1] = CooldownOfSkill1 + Main.rand.Next(151);
                        }
                    }
                    break;
                case 2:
                    {
                        slimeKing.c_ai[1]--;
                        slimeKing.c_ai[2]--;
                        var vector = npc.DirectionTo(target.Position);
                        vector.Normalize();
                        vector *= 2.5f;
                        if (slimeKing.c_ai[1] < 0)
                        {
                            Projectile.NewProjectile(null, npc.Center, Vector2.Zero, ProjectileID.CultistBossIceMist, 16, 64);
                            slimeKing.c_ai[1] = CooldownOfSkill1 + Main.rand.Next(121);
                        }
                        if (slimeKing.c_ai[2] < 120 && slimeKing.c_ai[2] % 40 == 0)
                        {
                            Projectile.NewProjectile(null, npc.Center, vector, ProjectileID.FrostWave, 18, 64);
                        }
                        if (slimeKing.c_ai[2] < 0)
                        {
                            slimeKing.c_ai[2] = CooldownOfSkill2 + 300;
                        }
                    }
                    break;
                case 3:
                    {
                        slimeKing.c_ai[1]--;
                        slimeKing.c_ai[2]--;
                        if (slimeKing.c_ai[1] < 0 && slimeKing.c_ai[2] == 500)
                        {
                            Projectile.NewProjectile(null, npc.Center, Vector2.One.RotatedBy(Main.rand.NextDouble() * Math.PI), ProjectileID.CultistBossIceMist, 8, 64);
                            Projectile.NewProjectile(null, npc.Center, Vector2.One.RotatedBy(Main.rand.NextDouble() * Math.PI), ProjectileID.CultistBossIceMist, 8, 64);
                            slimeKing.c_ai[1] = CooldownOfSkill1 + Main.rand.Next(151);
                        }
                        var vector = npc.DirectionTo(target.Position);
                        vector.Normalize();
                        vector *= 2.25f;
                        if (slimeKing.c_ai[2] < 180 && slimeKing.c_ai[2] % 60 == 0)
                        {
                            if (Main.rand.Next(1, 3) == 2)
                            {
                                Projectile.NewProjectile(null, npc.Center, vector, ProjectileID.FrostWave, 16, 64);
                                Projectile.NewProjectile(null, npc.Center, vector.RotatedBy(0.45), ProjectileID.FrostWave, 14, 64);
                                Projectile.NewProjectile(null, npc.Center, vector.RotatedBy(-0.45), ProjectileID.FrostWave, 14, 64);
                            }
                            else
                            {
                                Projectile.NewProjectile(null, npc.Center, vector.RotatedBy(0.4), ProjectileID.FrostWave, 14, 64);
                                Projectile.NewProjectile(null, npc.Center, vector.RotatedBy(-0.4), ProjectileID.FrostWave, 14, 64);
                            }
                        }
                        if (slimeKing.c_ai[2] < 0)
                            slimeKing.c_ai[2] = CooldownOfSkill2 + 200;
                    }
                    break;
            }
        }


        //状态设置
        public override int SetState(NPC npc)
        {
            if (npc.life >= npc.lifeMax * 0.7f)
            {
                if (state == 0)
                {
                    state = 1;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("史莱姆王习得冰魔法归来", new Color(0, 146, 255));
                }
                return 0;
            }
            else if (npc.life >= npc.lifeMax * 0.4f)
            {
                if (state == 1)
                {
                    state = 2;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("寒风呼啸", new Color(0, 146, 255));
                }
                return 1;
            }
            else if (npc.life >= npc.lifeMax * 0.2f)
            {
                if (state == 2)
                {
                    state = 3;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("你感觉寒冷刺骨", new Color(0, 146, 255));
                }
                return 2;
            }
            else
            {
                if (state == 3)
                {
                    state = 4;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("史莱姆王发怒了", new Color(0, 146, 255));
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
                    Challenger.SendPlayerText("走位真菜", new Color(0, 146, 255), npc.Center + new Vector2(0, -30));
                else if (i == 2)
                    Challenger.SendPlayerText("连我都打不过，回家喝奶吧你", new Color(0, 146, 255), npc.Center + new Vector2(0, -30));
                else
                    Challenger.SendPlayerText("小辣鸡", new Color(0, 146, 255), npc.Center + new Vector2(0, -30));
            }
        }
    }
}
