using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;

namespace Challenger
{
    public partial class Challenger : TerrariaPlugin
    {
        //返回离pos位置distancesquared开方范围内最近的那个敌怪npc, distanceSquared是距离平方
        //为什么用平方判断，因为平方速度快
        public static NPC NearestHostileNPC(Vector2 pos, float distanceSquared)
        {
            NPC npc = null;
            foreach(NPC n in Main.npc)
            {
                if(n.active && !n.friendly && n.CanBeChasedBy() && distanceSquared > (n.Center - pos).LengthSquared())
                {
                    distanceSquared = (n.Center - pos).LengthSquared();
                    npc = n;
                }
            }
            return npc;
        }


        //返回离pos位置distancesquared开方范围内血量比率最低的那个敌怪npc，若都满血则返回最近的npc。
        //若有boss则boss优先, boss价值最大者最先。distanceSquared是距离平方
        public static NPC NearestWeakestNPC(Vector2 pos, float distanceSquared)
        {
            NPC npc = null;
            bool hasBoss = false;
            float nvalue = 0f;              //boss价值
            float lifev = 2f;               //小怪生命比率 = life/lifemax
            float minDistanceSquared = distanceSquared; //小怪距离平方
            int minDistanceNpcIndex = -1;   //小怪索引
            foreach (NPC n in Main.npc)
            {
                float distanceS = (n.Center - pos).LengthSquared();
                if (n.boss && n.active && distanceSquared > distanceS && n.value >= nvalue)
                {
                    nvalue = n.value;
                    npc = n;
                    hasBoss = true;
                }
                if (n.active && !n.friendly && !hasBoss && n.CanBeChasedBy() && distanceSquared > distanceS && (n.life * 1f / n.lifeMax) <= lifev)
                {
                    if (n.lifeMax - n.life > 1)
                    {
                        lifev = n.life * 1f / n.lifeMax;
                        npc = n;
                    }
                    else if(minDistanceSquared > distanceS)
                    {
                        minDistanceSquared = distanceS;
                        minDistanceNpcIndex = n.whoAmI;
                    }
                }
            }
            //如果未找到npc(没有任何boss，且npc血量都是满的，且范围内存在npc)，否则返回null
            if(npc == null && minDistanceNpcIndex != -1)
            {
                npc = Main.npc[minDistanceNpcIndex];
            }
            return npc;
        }


        //返回pos位置distancesquared开方范围内的所有敌怪，distanceSquared是距离平方
        public static NPC[] NearAllHostileNPCs(Vector2 pos, float distanceSquared)
        {
            List<NPC> npcs = new List<NPC>();
            foreach (NPC n in Main.npc)
            {
                if (n.active && !n.friendly && n.CanBeChasedBy() && distanceSquared > (n.Center - pos).LengthSquared())
                {
                    npcs.Add(n);
                }
            }
            return npcs.ToArray();
        }


        //寻找范围内血量最低的那个玩家，第三个参数是你想排除的人，默认null
        public static Player NearWeakestPlayer(Vector2 pos, float distanceSquared, Player dontHealPlayer = null)
        {
            Player player = null;
            int Life = 0;
            foreach(Player p in Main.player)
            {
                if(!p.dead && (p.Center - pos).LengthSquared() < distanceSquared && p.statLifeMax - p.statLife > Life)
                {
                    if(dontHealPlayer != null && dontHealPlayer.whoAmI == p.whoAmI)
                    {
                        continue;
                    }
                    player = p;
                    Life = p.statLifeMax - p.statLife;
                }
            }
            return player;
        }


        //玩家回血和治疗视觉效果，可以启用治疗数字效果可见性，或关掉他，自己写独特的视觉效果
        public static void HealPlayer(Player player, int num, bool visible = true)
        {

            player.statLife += num;
            if (visible)
            {
                //healeffect 的源码写法，这里用作发送信息至每个用户，让在他们本地绘制，因为player.healeffect无法在服务器端起作用
                Rectangle r = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
                CombatText.NewText(r, CombatText.HealLife, num);
                NetMessage.SendData(81, -1, -1, null, (int)CombatText.HealLife.PackedValue, r.Center.X, r.Center.Y, num);
            }
            //同步生命值
            NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.whoAmI);
        }


        //玩家回魔力和治疗魔力视觉效果，可以。。。。。。同上
        public static void HealPlayerMana(Player player, int num, bool visible = true)
        {

            player.statMana += num;
            if (visible)
            {
                //healeffect 的源码写法，这里用作发送信息至每个用户，让在他们本地绘制
                Rectangle r = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
                CombatText.NewText(r, CombatText.HealLife, num);
                NetMessage.SendData(81, -1, -1, null, (int)CombatText.HealMana.PackedValue, r.Center.X, r.Center.Y, num);
            }
            //同步魔力值
            NetMessage.SendData(42, -1, -1, NetworkText.Empty, player.whoAmI);
        }


        //给单个玩家发送悬浮文本
        public static void SendPlayerText(TSPlayer player, string text, Color color, Vector2 position)
        {
            player.SendData(PacketTypes.CreateCombatTextExtended, text, (int)color.packedValue, position.X, position.Y);
        }


        //给全体玩家发送悬浮文本
        public static void SendPlayerText(string text, Color color, Vector2 position)
        {
            TSPlayer.All.SendData(PacketTypes.CreateCombatTextExtended, text, (int)color.packedValue, position.X, position.Y);
        }


        //给单个玩家发送悬浮数字（只能是int,因为float也只会显示int），为什么数字要单独分出来？因为原版分出来了，我觉得分开速度更快（也许）
        public static void SendPlayerText(TSPlayer player, int text, Color color, Vector2 position)
        {
            player.SendData(PacketTypes.CreateCombatText, null, (int)color.packedValue, position.X, position.Y, text);
        }


        //给全体玩家发送悬浮数字
        public static void SendPlayerText(int text, Color color, Vector2 position)
        {
            TSPlayer.All.SendData(PacketTypes.CreateCombatText, null, (int)color.packedValue, position.X, position.Y, text);
        }
    }
}
