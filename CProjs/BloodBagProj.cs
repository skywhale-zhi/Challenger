using Terraria;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;
using TShockAPI;
using Terraria.Audio;
using Terraria.ID;
using System;

namespace Challenger.CProjs
{
    public class BloodBagProj : CProjectile
    {
        private BloodBagProj() : base() { }
        private BloodBagProj(Projectile projectile) : base(projectile) { }
        private BloodBagProj(Projectile projectile, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int l1) : base(projectile, ai0, ai1, ai2, ai3, ai4, ai5, l1) { }

        public float v;
        public override void ProjectileAI(Projectile projectile)
        {
            //(ai[0]是造成接触伤害的敌怪索引，ai[1]是造成的伤害，c_ai[0]是补给给玩家的血)
            NPC npc = Main.npc[(int)projectile.ai[0]];
            CProjectile cprojectile = CMain.cProjectiles[projectile.whoAmI];

            //根据时期设置血包飞行速度
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
            else if (NPC.downedGolemBoss)
            {
                v = 14f;
            }
            else if (NPC.downedPlantBoss)
            {
                v = 12f;
            }
            else if (NPC.downedMechBossAny)
            {
                v = 10f;
            }
            else if (Main.hardMode)
            {
                v = 7f;
            }
            else if (NPC.downedBoss2)
            {
                v = 4f;
            }
            else if (NPC.downedBoss1)
            {
                v = 3f;
            }
            else
            {
                v = 2f;
            }

            //如果是接触伤害，且伤害玩家的敌对npc仍存在，让靠近他给他回血
            if ((int)projectile.ai[0] != 0 && npc != null && npc.active && (npc.position - projectile.Center).LengthSquared() <= 1500 * 1500)
            {
                cprojectile.c_proj.velocity = (npc.Center - projectile.Center).SafeNormalize(Vector2.Zero) * v;
            }
            //如果造成接触伤害的敌怪死了 || 或者如果是射弹造成的伤害，即找不到对应发射该射弹的敌怪npc => 则找最近的一个敌对npc给他回血，若找不到，杀掉射弹
            else
            {
                npc = Challenger.NearestWeakestNPC(cprojectile.c_proj.position, 1500 * 1500);
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
            if (npc != null && projectile.active && (projectile.position - npc.Center).LengthSquared() <= (npc.width * npc.height) / 2)
            {
                //白光之女皇伤害溢出，判断下免得有人用光女的伤害回血包去秒杀其他boss
                cprojectile.c_proj.ai[1] = cprojectile.c_proj.ai[1] > 99999 ? 99999 : cprojectile.c_proj.ai[1];
                npc.life += (int)cprojectile.c_proj.ai[1];
                if(npc.life > npc.lifeMax || npc.life <= 0)
                {
                    npc.life = npc.lifeMax;
                }
                npc.HealEffect((int)cprojectile.c_proj.ai[1]);
                cprojectile.c_proj.Kill();
                npc.netUpdate = true;
            }
            //被玩家捡走，回血
            if (projectile.active && projectile.timeLeft <= cprojectile.c_ai[1] - 60)
            {
                //白光之女皇伤害溢出，判断下免得有人被光女的回血弹造成数值溢出秒杀
                cprojectile.c_proj.ai[0] = cprojectile.c_proj.ai[0] > 500 ? 500 : cprojectile.c_proj.ai[0];
                try
                {
                    foreach (Player p in Main.player)
                    {
                        if ((projectile.Center - p.Center).LengthSquared() <= (p.width * p.height) / 2 && !p.dead)
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
                catch(Exception ex)
                {
                    TShock.Log.Error(ex.Message);
                }
            }
            projectile.netUpdate = true;

        }

        /// <summary>
        /// 自己加的创造射弹方法，能自动配置好射弹属性
        /// </summary>
        /// <param name="proj_ai0">原projcetile.ai[0]，记录被伤害的敌怪索引，若无记为0</param>
        /// <param name="proj_ai1">原projcetile.ai[1]，记录被伤害的敌怪伤害</param>
        /// <param name="ai0">这个是给玩家的回血量</param>
        /// <param name="ai1">这个是射弹timeleft</param>
        /// <returns></returns>
        public static BloodBagProj NewCProjectile(Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner = 255, float proj_ai0 = 0, float proj_ai1 = 0, float ai0 = 0, float ai1 = 0, float ai2 = 0, float ai3 = 0, float ai4 = 0, float ai5 = 0, int l1 = 0)
        {
            int index = Projectile.NewProjectile(null, position, velocity, Type, Damage, KnockBack, Owner, proj_ai0, proj_ai1);
            BloodBagProj b = new BloodBagProj(Main.projectile[index], ai0, ai1, ai2, ai3, ai4, ai5, l1);
            if (!Main.hardMode)
            {
                b.c_proj.tileCollide = true;
                b.c_proj.timeLeft = 8 * 60;
                b.c_ai[1] = 8 * 60;
            }
            else
            {
                b.c_proj.tileCollide = false;
                b.c_proj.timeLeft = 15 * 60;
                b.c_ai[1] = 15 * 60;
            }
            b.c_proj.netUpdate = true;
            CMain.cProjectiles[index] = b;
            return b;
        }
    }
}
