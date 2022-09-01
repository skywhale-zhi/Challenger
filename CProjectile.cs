using Terraria;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using TerrariaApi.Server;
using TShockAPI;

namespace Challenger
{
    public abstract class CProjectile
    {
        public Projectile c_proj;
        public float[] c_ai;
        public int c_index;
        public int Lable;

        protected CProjectile()
        {
            c_proj = null;
            c_ai = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };
            c_index = -1;
            Lable = 0;
        }

        protected CProjectile(Projectile projectile)
        {
            c_proj = projectile;
            c_ai = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };
            c_index = projectile.whoAmI;
            Lable = 0;
        }
        
        protected CProjectile(Projectile projectile, float ai0, float ai1, float ai2, float ai3, float ai4, float ai5, int l1)
        {
            c_proj = projectile;
            c_ai = new float[6] { ai0, ai1, ai2, ai3, ai4, ai5 };
            c_index = projectile.whoAmI;
            Lable = l1;
        }

        public virtual void ProjectileAI(Projectile projectile) { }

    }
}
