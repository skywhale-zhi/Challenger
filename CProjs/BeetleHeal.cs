using Microsoft.Xna.Framework;
using System;
using Terraria;
using TShockAPI;

namespace Challenger.CProjs
{
    public class BeetleHeal : CProjectile
    {
        private BeetleHeal(Projectile projectile, int l1, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5) : base(projectile, l1, ai0, ai1, ai2, ai3, ai4, ai5) { }

        public override void ProjectileAI(Projectile projectile)
        {
            CProjectile cprojectile = CMain.cProjectiles[projectile.whoAmI];
            Player player = Challenger.NearWeakestPlayer(projectile.Center, 800 * 800, Main.player[(int)c_ai[2]]);
            if (player != null && (int)c_ai[2] != player.whoAmI)
            {
                projectile.velocity = (player.Center - projectile.Center).SafeNormalize(Vector2.Zero) * 10f;
            }
            else
            {
                projectile.velocity *= 0.8f;
            }

            //被玩家捡走，回血
            if (projectile.active && projectile.timeLeft <= cprojectile.c_ai[1] - 60)
            {
                //白光之女皇伤害溢出，判断下
                if (cprojectile.c_ai[0] > 200 || cprojectile.c_ai[0] < 0)
                {
                    cprojectile.c_ai[0] = 200;
                }
                try
                {
                    foreach (Player p in Main.player)
                    {
                        if ((projectile.Center - p.Center).LengthSquared() <= p.width * p.height && !p.dead && p.whoAmI != (int)cprojectile.c_ai[2])
                        {
                            Challenger.HealPlayer(p, (int)cprojectile.c_ai[0], false);
                            if (Challenger.config.EnableConsumptionMode_启用话痨模式)
                            {
                                Challenger.SendPlayerText($"甲虫治疗 + {(int)cprojectile.c_ai[0]} 治疗者:{Main.player[(int)cprojectile.c_ai[2]].name}", new Color(210, 0, 255), p.Center + new Vector2(Main.rand.Next(-60, 61), Main.rand.Next(61)));
                            }
                            else
                            {
                                Challenger.SendPlayerText($"{(int)cprojectile.c_ai[0]}", new Color(0, 255, 0), p.Center + new Vector2(Main.rand.Next(-60, 61), Main.rand.Next(61)));
                            }
                            projectile.Kill();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("代码异常4：" + ex.Message);
                    Console.WriteLine("代码异常4：" + ex.Message);
                }
            }
            projectile.netUpdate = true;
        }


        /// <summary>
        /// 甲虫套治疗射弹
        /// </summary>
        /// <param name="l1">标签</param>
        /// <param name="cai0">给玩家的治疗量</param>
        /// <param name="cai1">timeleft</param>
        /// <param name="cai2">生成该射弹的玩家索引</param>
        /// <returns></returns>
        public static BeetleHeal NewCProjectile(Vector2 position, Vector2 velocity, float proj_ai0 = 0, float proj_ai1 = 0, int l1 = 0, float cai0 = 0, float cai1 = 0, float cai2 = 0, float cai3 = 0, float cai4 = 0, float cai5 = 0)
        {
            int index = Projectile.NewProjectile(null, position, velocity, 121, 0, 0, 255, proj_ai0, proj_ai1);
            BeetleHeal b = new BeetleHeal(Main.projectile[index], l1, cai0, cai1, cai2, cai3, cai4, cai5);
            b.c_proj.tileCollide = false;
            b.c_proj.timeLeft = (int)cai1;
            b.c_proj.netUpdate = true;
            CMain.cProjectiles[index] = b;
            return b;
        }
    }
}
