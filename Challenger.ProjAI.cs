using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using TShockAPI.Hooks;
using Terraria;
using TerrariaApi.Server;
using System.IO;
using Terraria.Localization;
using System.Diagnostics;
using Terraria.ID;
using System.Data;
using TShockAPI.DB;
using System.Collections;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using OTAPI;

namespace Challenger
{
    public partial class Challenger : TerrariaPlugin
    {
        //被怪物伤害后，被抽走的血包射弹AI，其实就是红宝石射弹改AI
        public void BloodBagAI(ProjectileAiUpdateEventArgs args)
        {
            Projectile projectile = args.Projectile;
            NPC npc = null;
            if (projectile.type == 125 && projectile.ai[1] > 0)
            {
                //游戏刚开启的一段时间，这个cprojectile会变成null，我猜是没有加载完全导致的，为了避免异常，这里需要判断下
                CProjectile cprojectile = CMain.cProjectiles[projectile.whoAmI];
                if (cprojectile == null)
                {
                    projectile.Kill();
                    return;
                }
                npc = Main.npc[(int)projectile.ai[0]];
                //如果是接触伤害，且伤害玩家的敌对npc仍存在，让靠近他给他回血
                if ((int)projectile.ai[0] != 0 && npc != null && npc.active && (npc.position - projectile.Center).LengthSquared() <= 1500 * 1500)
                {
                    cprojectile.c_proj.velocity = (npc.Center - projectile.Center).SafeNormalize(Vector2.Zero) * 7f;
                }
                //如果造成接触伤害的敌怪死了 || 或者如果是射弹造成的伤害，即找不到对应发射该射弹的敌怪npc => 则找最近的一个敌对npc给他回血，若找不到，杀掉射弹
                else
                {

                    npc = NearestHostileNPC(cprojectile.c_proj.position, 1500 * 1500);
                    if (npc != null)
                    {
                        cprojectile.c_proj.velocity = (npc.Center - projectile.Center).SafeNormalize(Vector2.Zero) * 7f;
                    }
                    if (npc == null)
                    {
                        cprojectile.c_proj.velocity *= 0.95f;
                    }
                }
                //靠近了敌对npc，回血完毕，杀掉射弹
                if (npc != null && projectile.active && (projectile.position - npc.Center).LengthSquared() <= (npc.width * npc.height) / 2)
                {

                    npc.life += (int)cprojectile.c_proj.ai[1];
                    npc.life = npc.life > npc.lifeMax ? npc.lifeMax : npc.life;
                    npc.HealEffect((int)cprojectile.c_proj.ai[1]);
                    cprojectile.c_proj.Kill();
                    npc.netUpdate = true;
                }

                if (projectile.active && projectile.timeLeft <= 19 * 60)
                {
                    foreach (Player p in Main.player)
                    {
                        if ((projectile.Center - p.Center).LengthSquared() <= (p.width * p.height) / 2)
                        {
                            /*
                            p.statLife += (int)cprojectile.c_ai[2];

                            #region healeffect 的源码写法，这里用作发送信息至每个用户，让在他们本地绘制，因为player.healeffect无法在服务器端起作用
                            Rectangle r = new Rectangle((int)p.position.X, (int)p.position.Y, p.width, p.height);
                            CombatText.NewText(r, CombatText.HealLife, (int)cprojectile.c_ai[2]);
                            NetMessage.SendData(81, -1, -1, null, (int)CombatText.HealLife.PackedValue, r.Center.X, r.Center.Y, cprojectile.c_ai[2]);
                            #endregion

                            //同步生命值
                            NetMessage.SendData(16, -1, -1, NetworkText.Empty, p.whoAmI);
                            */
                            HealPlayer(p, (int)cprojectile.c_ai[2]);
                            projectile.Kill();

                            break;
                        }
                    }
                }
                projectile.netUpdate = true;
            }
        }


        //魔法剑射弹
        public void MagicSwordAI(ProjectileAiUpdateEventArgs args)
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
            if (projectile.type != 156 || projectile.ai[0] == 0)
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

                NPC targeNpc = NearestHostileNPC(projectile.Center, min_distance * min_distance);

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
