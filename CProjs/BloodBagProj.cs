using Microsoft.Xna.Framework;
using System;
using Terraria;
using TShockAPI;

namespace Challenger.CProjs
{
    public class BloodBagProj : CProjectile
    {
        private BloodBagProj() : base() { }
        private BloodBagProj(Projectile projectile) : base(projectile) { }
        private BloodBagProj(Projectile projectile, int l1, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5) : base(projectile,  l1, ai0, ai1, ai2, ai3, ai4, ai5) { }

        public float v;
        public override void ProjectileAI(Projectile projectile)
        {
            //(ai[0]是造成接触伤害的敌怪索引，ai[1]治愈npc的量，c_ai[0]是补给给玩家的血，c_ai[1]表示timeleft，c_ai[2]记录自残惩罚类型)
            NPC npc = Main.npc[(int)projectile.ai[0]];
            CProjectile cprojectile = CMain.cProjectiles[projectile.whoAmI];

            //根据时期设置血包飞行速度，为了尽可能的让大家注意血包，增加玩家的拾取意愿，我已经吧速度降的很低了
            if (NPC.downedAncientCultist)
            {
                v = 20f;
            }
            else if (NPC.downedEmpressOfLight)
            {
                v = 18f;
            }
            else if (NPC.downedFishron)
            {
                v = 17f;
            }
            else if (NPC.downedMartians)
            {
                v = 16f;
            }
            else if (NPC.downedGolemBoss)
            {
                v = 15f;
            }
            else if (NPC.downedPlantBoss)
            {
                v = 14f;
            }
            else if (NPC.downedMechBossAny)
            {
                v = 12f;
            }
            else if (Main.hardMode)
            {
                v = 10f;
            }
            else if (NPC.downedQueenBee)
            {
                v = 8.7f;
            }
            else if (NPC.downedBoss3)
            {
                v = 6.8f;
            }
            else if (NPC.downedDeerclops)
            {
                v = 5.2f;
            }
            else if (NPC.downedBoss2)
            {
                v = 3.4f;
            }
            else if (NPC.downedBoss1)
            {
                v = 2.8f;
            }
            else if (NPC.downedSlimeKing)
            {
                v = 2.4f;
            }
            else
            {
                v = 2f;
            }
            //对毁灭者追击加速
            if (npc.type == 134 || npc.type == 135 || npc.type == 136)
            {
                v = 25f;
            }

            //如果是接触伤害，且伤害玩家的敌对npc仍存在，让靠近他给他回血
            if ((int)projectile.ai[0] != 0 && npc != null && npc.active)
            {
                projectile.velocity = (npc.Center - projectile.Center).SafeNormalize(Vector2.Zero) * v;

            }
            //如果造成接触伤害的敌怪死了 || 或者如果是射弹造成的伤害，即找不到对应发射该射弹的敌怪npc => 则找最近的一个敌对npc给他回血，若找不到，杀掉射弹
            else
            {
                npc = Challenger.NearestWeakestNPC(cprojectile.c_proj.position, 2000 * 2000);
                if (npc != null)
                {
                    cprojectile.c_proj.velocity = (npc.Center - projectile.Center).SafeNormalize(Vector2.Zero) * v;
                }
                if (npc == null)
                {
                    cprojectile.c_proj.velocity *= 0.9f;
                }
            }
            //靠近了敌对npc，回血完毕，杀掉射弹
            if (npc != null && npc.active && projectile.active && (projectile.position - npc.Center).LengthSquared() <= (npc.width * npc.height) / 2)
            {
                //白光之女皇伤害溢出，判断下免得有人用光女的伤害回血包去秒杀其他boss
                if (projectile.ai[1] >= npc.lifeMax - npc.life)
                {
                    npc.life = npc.lifeMax;
                }
                else
                {
                    npc.life += (int)projectile.ai[1];
                }
                if (Challenger.config.EnableConsumptionMode_启用话痨模式)
                {
                    if (c_ai[2] == 0)
                        Challenger.SendPlayerText($"敌怪治疗 + {(int)projectile.ai[1]}", new Color(190, 255, 0), npc.Center);
                    else
                        Challenger.SendPlayerText($"自残惩罚 + {(int)projectile.ai[1]}", new Color(0, 255, 190), npc.Center);
                }
                else
                {
                    npc.HealEffect((int)projectile.ai[1]);
                }
                projectile.Kill();
                npc.netUpdate = true;
            }
            //被玩家捡走，回血，timeleft< x - 60 之后才能被拾取
            if (projectile.active && projectile.timeLeft <= cprojectile.c_ai[1] - 60)
            {
                try
                {
                    foreach (Player p in Main.player)
                    {
                        if (p != null && (projectile.Center - p.Center).LengthSquared() <= (p.width * p.height) / 2 && !p.dead)
                        {
                            if (Challenger.config.EnableConsumptionMode_启用话痨模式)
                            {
                                Challenger.HealPlayer(Main.player[p.whoAmI], (int)cprojectile.c_ai[0], false);
                                Challenger.SendPlayerText($"血包治疗 + {(int)cprojectile.c_ai[0]}", new Color(0, 255, 0), p.Center);
                            }
                            else
                            {
                                Challenger.HealPlayer(Main.player[p.whoAmI], (int)cprojectile.c_ai[0]);
                            }
                            projectile.Kill();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("代码异常1：" + ex.Message);
                    Console.WriteLine("代码异常1：" + ex.Message);
                }
            }
            projectile.netUpdate = true;
        }

        /// <summary>
        /// 血包创造方法，自动配置好射弹属性，（红宝石射弹ID:125）
        /// </summary>
        /// <param name="proj_ai0">原projcetile.ai[0]，记录被伤害的敌怪索引，若无记为0</param>
        /// <param name="proj_ai1">原projcetile.ai[1]，记录heal敌怪npc值</param>
        /// <param name="l1">标签</param>
        /// <param name="cai0">这个是给玩家的回血量healplayer</param>
        /// <param name="cai1">这个是射弹timeleft，修改无效</param>
        /// <param name="cai2">这个是自残惩罚类型惩罚</param>
        /// <returns></returns>
        public static BloodBagProj NewCProjectile(Vector2 position, Vector2 velocity, float proj_ai0 = 0, float proj_ai1 = 0, int l1 = 0, float cai0 = 0, float cai1 = 0, float cai2 = 0, float cai3 = 0, float cai4 = 0, float cai5 = 0)
        {
            int index = Projectile.NewProjectile(null, position, velocity, 125, 0, 0, 255, proj_ai0, proj_ai1);
            BloodBagProj b = new BloodBagProj(Main.projectile[index], l1, cai0, cai1, cai2, cai3, cai4, cai5);
            if (!Main.hardMode)
            {
                b.c_proj.tileCollide = true;
                b.c_proj.timeLeft = 20 * 60;
                b.c_ai[1] = 20 * 60;
            }
            else
            {
                b.c_proj.tileCollide = false;//肉后可穿墙
                b.c_proj.timeLeft = 40 * 60;
                b.c_ai[1] = 40 * 60;
            }
            b.c_proj.netUpdate = true;
            CMain.cProjectiles[index] = b;
            return b;
        }
    }
}
