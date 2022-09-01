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
        //返回离pos位置distancesquared开方范围内最近的那个敌怪npc, distanceSquared是距离平方
        //为什么用平方判断，因为平方速度快
        public static NPC NearestHostileNPC(Vector2 pos, float distanceSquared)
        {
            NPC npc = null;
            foreach(NPC n in Main.npc)
            {
                if(n.active && !n.friendly && n.CanBeChasedBy() && distanceSquared > (n.Center - pos).LengthSquared() && n.life > 10)
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
                if (n.active && !n.friendly && n.CanBeChasedBy() && distanceSquared > (n.Center - pos).LengthSquared() && n.life > 10)
                {
                    npcs.Add(n);
                }
            }
            return npcs.ToArray();
        }


        //玩家回血和治疗视觉效果
        public static void HealPlayer(Player player, int num)
        {

            player.statLife += num;

            //healeffect 的源码写法，这里用作发送信息至每个用户，让在他们本地绘制，因为player.healeffect无法在服务器端起作用
            Rectangle r = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
            CombatText.NewText(r, CombatText.HealLife, num);
            NetMessage.SendData(81, -1, -1, null, (int)CombatText.HealLife.PackedValue, r.Center.X, r.Center.Y, num);
            
            //同步生命值
            NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.whoAmI);
        }
    }
}
