using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace Challenger.CProjs
{
    public class SpiderArmorProj : CProjectile
    {
        private SpiderArmorProj(Projectile projectile, int l1, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5) : base(projectile, l1, ai0, ai1, ai2, ai3, ai4, ai5) { }
        public override void ProjectileAI(Projectile projectile)
        { 
        }

        public override void PreProjectileKilled(Projectile projectile)
        {
            if(Lable == 1)
            {
                for (int j = 0; j < 15; j++)
                {
                    Vector2 v = Vector2.UnitY.RotatedBy(MathHelper.TwoPi / 15 * j + MathHelper.TwoPi / 30);
                    int index = Projectile.NewProjectile(null, projectile.Center, v * 4, 265, 40, 5);
                }
            }
        }


        public static SpiderArmorProj NewCProjectile(Vector2 position, Vector2 velocity, float proj_ai0 = 0, float proj_ai1 = 0, int l1 = 0, float cai0 = 0, float cai1 = 0, float cai2 = 0, float cai3 = 0, float cai4 = 0, float cai5 = 0)
        {
            int index = Projectile.NewProjectile(null, position, velocity, 371, 20, 0, 255, proj_ai0, proj_ai1);
            SpiderArmorProj s = new SpiderArmorProj(Main.projectile[index], l1, cai0, cai1, cai2, cai3, cai4, cai5);

            s.c_proj.timeLeft = 10 * 60;
            s.c_ai[0] = cai0;//标记
            s.c_ai[1] = 15 * 60;

            s.c_proj.netUpdate = true;
            CMain.cProjectiles[index] = s;
            return s;
        }
    }
}
