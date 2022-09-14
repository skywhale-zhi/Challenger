using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace Challenger.CProjs
{
    public class Honey : CProjectile
    {
        private Honey(Projectile projectile, int l1, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5) : base(projectile, l1, ai0, ai1, ai2, ai3, ai4, ai5) { }

        public override void ProjectileAI(Projectile projectile)
        {
            //标记1，只有为1的射弹才启用这个ai，糖罐proj
            try
            {
                if (Lable == 1)
                {
                    if (projectile.active && projectile.timeLeft < c_ai[1] - 60)
                    {
                        foreach (Player p in Main.player)
                        {
                            if (p != null && !p.dead && (projectile.Center - p.Center).LengthSquared() <= (p.width * p.height) / 2 )
                            {
                                int heal = Main.rand.Next(3, 7);
                                if (p.whoAmI != c_ai[2])//对其他玩家治疗更高
                                {
                                    heal = Main.rand.Next(5, 11);
                                }
                                if (Challenger.config.EnableConsumptionMode_启用话痨模式)
                                {
                                    Challenger.HealPlayer(Main.player[p.whoAmI], heal, false);
                                    Challenger.SendPlayerText($"蜂糖罐治疗 + {heal}", new Color(232, 229, 74), p.Center);
                                }
                                else
                                {
                                    Challenger.HealPlayer(Main.player[p.whoAmI], heal);
                                }
                                TShock.Players[p.whoAmI].SetBuff(BuffID.Honey, 15 * 60);
                                projectile.Kill();
                                break;
                            }
                        }
                    }
                    //在60的时候及时杀掉，免得球爆炸调用原版ai释放出伤害弹片
                    if (projectile.timeLeft < 60)
                    {
                        projectile.Kill();
                    }
                }
            }
            catch(Exception ex)
            {
                TShock.Log.Error("异常Honey_lable==1:" + ex.Message);
            }
            //标记2，只有为2的射弹才启用这个ai，蜜蜂炸弹proj
            try
            {
                if (Lable == 2)
                {
                    if (projectile.active && projectile.timeLeft < c_ai[1] - 60)
                    {
                        int index = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, 566, 17, 0);
                        Main.projectile[index].usesLocalNPCImmunity = true;
                        Main.projectile[index].netUpdate = true;
                        projectile.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error("异常Honey_lable==2:" + ex.Message);
            }
        }


        /// <summary>
        /// 蜂蜜糖罐proj,由绿色尖叫怪的圣诞礼球修改而来ID:346
        /// </summary>
        /// <param name="l1">标记：1 蜜蜂糖罐，2 蜜蜂炸弹包</param>
        /// <param name="cai0">null</param>
        /// <param name="cai1">timeleft计时器，修改无效</param>
        /// <param name="cai2">射弹所有者</param>
        /// <returns></returns>
        public static Honey NewCProjectile(Vector2 position, Vector2 velocity, float proj_ai0 = 0, float proj_ai1 = 0, int l1 = 0, float cai0 = 0, float cai1 = 0, float cai2 = 0, float cai3 = 0, float cai4 = 0, float cai5 = 0)
        {
            int index = Projectile.NewProjectile(null, position, velocity, 346, 0, 0, 255, proj_ai0, proj_ai1);
            Honey h = new Honey(Main.projectile[index], l1, cai0, cai1, cai2, cai3, cai4, cai5);

            h.c_proj.timeLeft = 15 * 60;
            h.c_ai[1] = 15 * 60;

            h.c_proj.netUpdate = true;
            CMain.cProjectiles[index] = h;
            return h;
        }
    }
}
