using Terraria;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;
using TShockAPI;
using Terraria.Audio;
using Terraria.ID;
using System;

namespace Challenger.CProjs
{
    public class CrystalLeafShot : CProjectile
    {
        private CrystalLeafShot() : base() { }
        public CrystalLeafShot(Projectile projectile) : base(projectile) { }
        private CrystalLeafShot(Projectile projectile, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int l1) : base(projectile, ai0, ai1, ai2, ai3, ai4, ai5, l1) { }

        public override void ProjectileAI(Projectile projectile)
        {
            try
            {
                if (c_ai[0] == 1)
                {
                    Projectile.NewProjectile(null, Main.player[projectile.owner].Center + new Vector2(0, -60), projectile.velocity.RotatedBy(-0.2) * 1.3f, 227, 75, 5);
                    Projectile.NewProjectile(null, Main.player[projectile.owner].Center + new Vector2(0, -60), projectile.velocity.RotatedBy(0.2) * 1.3f,  227, 75, 5);
                    Projectile.NewProjectile(null, Main.player[projectile.owner].Center + new Vector2(0, -60), projectile.velocity.RotatedBy(-0.1) * 1.6f, 227, 75, 5);
                    Projectile.NewProjectile(null, Main.player[projectile.owner].Center + new Vector2(0, -60), projectile.velocity.RotatedBy(0.1) * 1.6f,  227, 75, 5);
                    Projectile.NewProjectile(null, Main.player[projectile.owner].Center + new Vector2(0, -60), projectile.velocity *                    2, 227, 75, 5);
                    try
                    {
                        CMain.cProjectiles[projectile.whoAmI].c_ai[0] = 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("异常1：" + ex.Message);
                        TShock.Log.Warn("异常1：" + ex.Message);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("异常2：" + ex.Message);
                TShock.Log.Warn("异常2：" + ex.Message);
            }
        }

        public static CrystalLeafShot NewCProjectile(Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner = 255, float proj_ai0 = 0, float proj_ai1 = 0, float ai0 = 0, float ai1 = 0, float ai2 = 0, float ai3 = 0, float ai4 = 0, float ai5 = 0, int l1 = 0)
        {
            int index = Projectile.NewProjectile(null, position, velocity, Type, Damage, KnockBack, Owner, proj_ai0, proj_ai1);
            CrystalLeafShot c = new CrystalLeafShot(Main.projectile[index], ai0, ai1, ai2, ai3, ai4, ai5, l1);
            c.c_proj.timeLeft = 5 * 60;
            c.c_proj.netUpdate = true;
            CMain.cProjectiles[index] = c;
            return c;
        }
    }
}
