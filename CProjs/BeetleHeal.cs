using Terraria;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;
using TShockAPI;
using Terraria.Audio;
using Terraria.ID;
using System;

namespace Challenger.CProjs
{
    public class BeetleHeal : CProjectile
    {
        private BeetleHeal() : base() { }
        private BeetleHeal(Projectile projectile) : base(projectile) { }
        private BeetleHeal(Projectile projectile, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int l1) : base(projectile, ai0, ai1, ai2, ai3, ai4, ai5, l1) { }

        public override void ProjectileAI(Projectile projectile)
        {
            CProjectile cprojectile = CMain.cProjectiles[projectile.whoAmI];
            Player player = Challenger.NearWeakPlayer(projectile.Center, 800*800);
            if (player != null && (int)c_ai[2] != player.whoAmI)
            {
                projectile.velocity = (player.Center - projectile.Center).SafeNormalize(Vector2.Zero) * 10f;
            }
            else
            {
                projectile.velocity *= 0.8f;
            }

            //被玩家捡走，回血
            if (projectile.active && projectile.timeLeft <= cprojectile.c_ai[1] - 60 )
            {
                //白光之女皇伤害溢出，判断下
                if (cprojectile. c_ai[0] > 200 || cprojectile.c_ai[0] < 0)
                {
                    cprojectile. c_ai[0] = 200;
                }
                try
                {
                    foreach (Player p in Main.player)
                    {
                        if ((projectile.Center - p.Center).LengthSquared() <= (p.width * p.height) / 2 && !p.dead && p.whoAmI != (int)cprojectile.c_ai[2])
                        {
                            Challenger.HealPlayer(p, (int)cprojectile.c_ai[0], false);
                            if (Challenger.config.EnableConsumptionMode_启用话痨模式)
                            {
                                Challenger.SendPlayerText($"甲虫治疗 ♥ + {(int)cprojectile.c_ai[0]} 治疗者:{Main.player[(int)cprojectile.c_ai[2]].name}", new Color(210, 0, 255), p.Center + new Vector2(Main.rand.Next(-60, 61), Main.rand.Next(61)));
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
                    TShock.Log.Error(ex.Message);
                    Console.WriteLine(ex.Message);
                }
            }
            projectile.netUpdate = true;

        }


        /// <summary>
        /// 甲虫套治疗射弹
        /// </summary>
        /// <param name="ai0">给玩家的治疗量</param>
        /// <param name="ai1">timeleft</param>
        /// <param name="ai2">生成该射弹的玩家索引</param>
        /// <returns></returns>
        public static BeetleHeal NewCProjectile(Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner = 255, float proj_ai0 = 0, float proj_ai1 = 0, float ai0 = 0, float ai1 = 0, float ai2 = 0, float ai3 = 0, float ai4 = 0, float ai5 = 0, int l1 = 0)
        {
            int index = Projectile.NewProjectile(null, position, velocity, Type, Damage, KnockBack, Owner, proj_ai0, proj_ai1);
            BeetleHeal b = new BeetleHeal(Main.projectile[index], ai0, ai1, ai2, ai3, ai4, ai5, l1);
            b.c_proj.tileCollide = false;
            b.c_proj.timeLeft = (int)ai1;
            b.c_ai[1] =  (int)ai1;
            b.c_proj.netUpdate = true;
            CMain.cProjectiles[index] = b;
            return b;
        }
    }
}
