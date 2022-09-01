using Terraria;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;

namespace Challenger.CProjs
{
    public class MagicSwordProj : CProjectile
    {
        public MagicSwordProj() : base() { }
        public MagicSwordProj(Projectile projectile) : base(projectile) { }
        public MagicSwordProj(Projectile projectile, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int l1) : base(projectile, ai0, ai1, ai2, ai3, ai4, ai5, l1) { }

        public static void ProjectileAI(ProjectileAiUpdateEventArgs args)
        {

            /*
            Vector2 position = Main.player[e.Player.Index].Center;
            NPC npc = NearestHostileNPC(position, 1000 * 1000);
            CProjectile proj = CProjectile.NewCProjectile(null, position, (npc.Center - position).SafeNormalize(Vector2.Zero) * 10f, 156, 100, 0);
            proj.c_proj.ai[0] = 1;
            proj.c_proj.timeLeft = 10 * 60;
            proj.c_proj.tileCollide = true;
            */

            Projectile projectile = args.Projectile;
            NPC npc = null;
            if (projectile.ai[0] == 0)
            {
                return;
            }
            projectile.penetrate = 10;
            const int Ready = 1;
            const int Dash = 2;
            const int Search = 3;

            if (projectile.timeLeft > 10)
            {
                float min_distance = 600f;

                NPC targeNpc = Challenger.NearestHostileNPC(projectile.Center, min_distance * min_distance);

                //刚发射弹幕规定攻击状态为搜寻
                if (projectile.ai[1] == 0)
                    projectile.ai[0] = Search;

                switch (projectile.ai[0])
                {
                    case 3://搜寻目标
                        if (targeNpc != null)
                        {
                            //如果有目标且距离为370~110，则对目标进行跟踪
                            if ((projectile.Center - targeNpc.Center).Length() > 210f)
                            {
                                projectile.velocity = (targeNpc.Center - projectile.Center).SafeNormalize(Vector2.Zero) * 15f;
                            }
                            //如果接近目标（<200），则修改攻击状态为准备
                            if ((projectile.Center - targeNpc.Center).Length() < 200f)
                            {
                                projectile.velocity *= (targeNpc.Center - projectile.Center).SafeNormalize(Vector2.Zero) * 0.001f;
                                projectile.ai[0] = Ready;
                                projectile.ai[1] = 1;
                                projectile.tileCollide = false;
                            }
                        }
                        //如果没有目标则默认飞行
                        else
                        {
                            //若追踪失败导致速度降低至非0，则缓慢提升速度至14f
                            //这个情况来自dash不了快速移动的怪
                            if (projectile.velocity.LengthSquared() <= 15f * 15f && projectile.velocity.LengthSquared() > 0f)
                            {
                                projectile.velocity *= 1.1f;
                            }
                            //当速度为0，快速杀死该射弹
                            //这个情况来自ready旋转方向角时，目标怪被杀死，速度为0且无目标追踪
                            else if (projectile.velocity.LengthSquared() == 0)
                            {
                                projectile.timeLeft -= 30;
                            }
                            projectile.tileCollide = true;
                        }
                        projectile.netUpdate = true;
                        break;

                    case 1://ready
                        if (targeNpc == null)
                        {
                            projectile.ai[0] = Search;
                            projectile.tileCollide = true;
                            projectile.netUpdate = true;
                            break;
                        }
                        projectile.ai[1]++;
                        //如果倒计时6到了，则进行冲刺准备
                        if (projectile.ai[1] == 7)
                        {
                            projectile.velocity = (targeNpc.Center - projectile.Center).SafeNormalize(Vector2.Zero) * 30f;
                            projectile.ai[0] = Dash;
                            projectile.ai[1] = 1;
                        }
                        projectile.tileCollide = false;
                        projectile.netUpdate = true;
                        break;

                    case 2://dash
                        //接近目标冲刺点时停止冲刺，改变攻击状态
                        projectile.velocity *= 0.95f;
                        if (projectile.velocity.LengthSquared() < 4f)
                        {
                            projectile.ai[0] = Search;
                        }
                        projectile.tileCollide = false;
                        projectile.netUpdate = true;
                        break;

                    default:
                        projectile.ai[0] = Ready;
                        projectile.netUpdate = true;
                        break;
                }
            }
        }
    }
}
