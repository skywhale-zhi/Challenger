using Microsoft.Xna.Framework;
using System;
using Terraria;
using TShockAPI;

namespace Challenger.CProjs
{
    public class FossiArmorProj : CProjectile
    {
        private FossiArmorProj(Projectile projectile, int l1, float cai0, float cai1, float cai2, float cai3, float cai4, float cai5) : base(projectile, l1, cai0, cai1, cai2, cai3, cai4, cai5) { }

        public override void ProjectileAI(Projectile projectile)
        {
            if (Lable == 1)
            {
                //TSPlayer.All.SendInfoMessage($"timeleft:{projectile.timeLeft}");

                projectile.timeLeft = 90;
                projectile.Center = Main.player[(int)c_ai[1]].Center + new Vector2(0, -64);
                projectile.netUpdate = true;
                //TSPlayer.All.SendInfoMessage($"proj.name:{projectile.Name}, proj.whoamI:{projectile.whoAmI}, proj.ai[0]:{projectile.ai[0]}, proj.ai[1]:{projectile.ai[1]}");
                if (Main.time % 7 == 0)
                {
                    NPC target = Challenger.NearestHostileNPC(projectile.Center, 250 * 250);
                    if(target != null)
                    {
                        Projectile.NewProjectile(null, projectile.Center, projectile.Center.DirectionTo(target.Center) * 18, 732, 7, 5);
                    }
                }
            }
        }


        /// <summary>
        /// 化石盔甲套装的射弹
        /// </summary>
        /// <param name="l1">标记1</param>
        /// <param name="cai0">null</param>
        /// <param name="cai1">owner</param>
        /// <returns></returns>
        public static FossiArmorProj NewCProjectile(Vector2 position, Vector2 velocity, float proj_ai0 = 0, float proj_ai1 = 0, int l1 = 0, float cai0 = 0, float cai1 = 0, float cai2 = 0, float cai3 = 0, float cai4 = 0, float cai5 = 0)
        {
            int index = Projectile.NewProjectile(null, position, velocity, 597, 0, 0, 255, proj_ai0, proj_ai1);
            FossiArmorProj f = new FossiArmorProj(Main.projectile[index], l1, cai0, cai1, cai2, cai3, cai4, cai5);
            f.c_proj.tileCollide = false;
            f.c_proj.timeLeft = 90;
            f.c_proj.netUpdate = true;
            CMain.cProjectiles[index] = f;
            return f;
        }
    }
}
