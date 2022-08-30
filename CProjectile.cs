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
    public class CProjectile
    {
        public Projectile c_proj;
        public float[] c_ai;
        public int c_index;

        public CProjectile()
        {
            c_proj = null;
            c_ai = new float[5] { 0f, 0f, 0f, 0f, 0f };
            c_index = -1;
        }

        public CProjectile(Projectile projectile)
        {
            c_proj = projectile;
            c_ai = new float[5] { projectile.ai[0], projectile.ai[1], 0f, 0f, 0f };
            c_index = projectile.whoAmI;

            CMain.cProjectiles[c_index] = new CProjectile();
            CMain.cProjectiles[c_index].c_proj = projectile;
            CMain.cProjectiles[c_index].c_ai = c_ai;
            CMain.cProjectiles[c_index].c_index = c_index;
        }

        public static CProjectile NewCProjectile(IEntitySource spawnSource, Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner = 255, float ai0 = 0, float ai1 = 0, float a2 = 0, float a3 = 0, float a4 = 0)
        {
            CProjectile cProjectile = new CProjectile(Main.projectile[Projectile.NewProjectile(spawnSource, position, velocity, Type, Damage, KnockBack, Owner, ai0, ai1)]);
            return cProjectile;
        }
    }
}
