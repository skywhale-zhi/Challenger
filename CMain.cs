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
    //自己构造了个 Terraria.Main 用来同步proj或npc的额外数据
    public class CMain
    {
        public static CProjectile[] cProjectiles = new CProjectile[Main.maxProjectiles];
    }
}
