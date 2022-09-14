using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;

namespace Challenger.CNPCs
{
    public class TheDestroyer : CNPC
    {
        public TheDestroyer() : base() { }
        public TheDestroyer(NPC npc) : base(npc) { }
        public TheDestroyer(NPC npc, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int i1) : base(npc, ai0, ai1, ai2, ai3, ai4, ai5, i1) { }

        public override void WhenHurtByPlayer(NpcStrikeEventArgs args)
        {
            
        }
    }
}
