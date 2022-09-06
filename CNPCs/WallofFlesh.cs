using System;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using TShockAPI;

namespace Challenger.CNPCs
{
    public class WallofFlesh : CNPC
    {
        public WallofFlesh() : base() { }
        public WallofFlesh(NPC npc) : base(npc) { }
        public WallofFlesh(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }

        public static readonly float CooldownOfSkill0 = 150;
        public static readonly float CooldownOfSkill1 = 120;

        public float skill0 = CooldownOfSkill0;
        public float skill1 = CooldownOfSkill1;

        int state = 0;
        public override void NPCAI(NPC npc)
        {
            skill0--;
            skill1--;
            NPCAimedTarget target = npc.GetTargetData();
            Vector2 vector = npc.DirectionTo(target.Position);

            State = SetState(npc);
            if (skill0 < 0)
            {
                switch (State)
                {
                    case 0:
                        NewProjectile(npc.Center, vector * 13, ProjectileID.CultistBossFireBall, 14, 5);
                        skill0 = CooldownOfSkill0 + Main.rand.Next(51);
                        break;
                    case 1:
                        NewProjectile(npc.Center, vector * 20, ProjectileID.CultistBossFireBall, 16, 5);
                        skill0 = CooldownOfSkill0;
                        break;
                    case 2:
                        NewProjectile(npc.Center, vector * 26, ProjectileID.CultistBossFireBall, 20, 5);
                        skill0 = CooldownOfSkill0 - 20;
                        break;
                    default:
                        break;
                }
            }
            if (skill1 < 0)
            {
                switch (State)
                {
                    case 0:
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);

                        skill1 = CooldownOfSkill0 + Main.rand.Next(51);
                        break;
                    case 1:
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);

                        skill1 = CooldownOfSkill0;
                        break;
                    case 2:
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);
                        NewProjectile(npc.Center + new Vector2(0, Main.rand.Next(-200, 200)), (vector + Vector2.One.RotateRandom(Math.PI) * 0.2f) * 10, 811, 10, 5);

                        skill1 = CooldownOfSkill0;
                        break;
                    default:
                        break;
                }
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
                        TSPlayer.All.SendMessage("罪恶血祭召唤远古守卫", new Color(255, 77, 0));
                }
                return 0;
            }
            else if (npc.life >= npc.lifeMax * 0.3f)
            {
                if (state == 1)
                {
                    state = 2;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("付出代价吧！", new Color(255, 77, 0));
                }
                return 1;
            }
            else
            {
                if (state == 2)
                {
                    state = 3;
                    if (Challenger.config.EnableBroadcastConsumptionMode_启用广播话痨模式)
                        TSPlayer.All.SendMessage("生死时速", new Color(255, 77, 0));
                }
                return 2;
            }
        }

        public override void OnHurtPlayers(NPC npc, GetDataHandlers.PlayerDamageEventArgs e)
        {
            if (Challenger.config.EnableConsumptionMode_启用话痨模式)
            {
                Challenger.SendPlayerText("咬碎你", new Color(0, 146, 255), npc.Center);
            }
        }
    }



    public class WallofFleshEye : CNPC
    {
        public WallofFleshEye() : base() { }
        public WallofFleshEye(NPC npc) : base(npc) { }
        public WallofFleshEye(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }

        public static readonly float CooldownOfSkill0 = 140;

        public float skill0 = CooldownOfSkill0;

        int state = 0;
        public override void NPCAI(NPC npc)
        {
            skill0--;
            if (skill0 < 0)
            {
                NPCAimedTarget target = npc.GetTargetData();
                Vector2 vector = npc.DirectionTo(target.Position);
                State = SetState(npc);
                switch (State)
                {
                    case 0:
                        NewProjectile(npc.Center, vector * 8f, ProjectileID.EyeLaser, 12, 5);
                        skill0 += CooldownOfSkill0 + Main.rand.Next(100);
                        break;
                    case 1:
                        NewProjectile(npc.Center, vector * 9f, ProjectileID.EyeLaser, 12, 5);
                        NewProjectile(npc.Center, vector.RotatedBy(-0.1) * 8f, ProjectileID.EyeLaser, 12, 5);
                        NewProjectile(npc.Center, vector.RotatedBy(0.1) * 8f, ProjectileID.EyeLaser, 12, 5);
                        skill0 += CooldownOfSkill0 + Main.rand.Next(80);
                        break;
                    case 2:
                        NewProjectile(npc.Center, vector * 9f, ProjectileID.EyeLaser, 14, 20);
                        NewProjectile(npc.Center, vector.RotatedBy(0.1) * 10f, ProjectileID.EyeLaser, 14, 5);
                        NewProjectile(npc.Center, vector.RotatedBy(-0.1) * 10f, ProjectileID.EyeLaser, 14, 5);
                        NewProjectile(npc.Center, vector.RotatedBy(0.2) * 8f, ProjectileID.EyeLaser, 14, 5);
                        NewProjectile(npc.Center, vector.RotatedBy(-0.2) * 8f, ProjectileID.EyeLaser, 14, 5);
                        skill0 += CooldownOfSkill0 + Main.rand.Next(30);
                        break;
                    case 3:
                        NewProjectile(npc.Center, vector * 15f, ProjectileID.EyeLaser, 15, 20);
                        NewProjectile(npc.Center, vector.RotatedBy(0.1) * 15f, ProjectileID.EyeLaser, 15, 5);
                        NewProjectile(npc.Center, vector.RotatedBy(-0.1) * 15f, ProjectileID.EyeLaser, 15, 5);
                        NewProjectile(npc.Center, vector.RotatedBy(0.15) * 14f, ProjectileID.EyeLaser, 15, 5);
                        NewProjectile(npc.Center, vector.RotatedBy(-0.15) * 14f, ProjectileID.EyeLaser, 15, 5);
                        NewProjectile(npc.Center, vector.RotatedBy(0.2) * 13f, ProjectileID.EyeLaser, 15, 5);
                        NewProjectile(npc.Center, vector.RotatedBy(-0.2) * 13f, ProjectileID.EyeLaser, 15, 5);
                        NewProjectile(npc.Center, vector.RotatedBy(0.25) * 12f, ProjectileID.EyeLaser, 15, 5);
                        NewProjectile(npc.Center, vector.RotatedBy(-0.25) * 12f, ProjectileID.EyeLaser, 15, 5);
                        skill0 += CooldownOfSkill0;
                        break;
                }
            }
        }


        public override int SetState(NPC npc)
        {
            if (npc.life >= npc.lifeMax * 0.7f)
            {
                if (state == 0)
                {
                    state = 1;
                }
                return 0;
            }
            else if (npc.life >= npc.lifeMax * 0.4f)
            {
                if (state == 1)
                {
                    state = 2;
                }
                return 1;
            }
            else if (npc.life >= npc.lifeMax * 0.2f)
            {
                if (state == 2)
                {
                    state = 3;
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
                Challenger.SendPlayerText("这么想看清我的卡姿兰大眼是吧", new Color(0, 146, 255), npc.Center + new Vector2(0, -30));
            }
        }
    }
}
