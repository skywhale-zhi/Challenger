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
        protected CProjectile()//lable是标签，用于区分用同一个射弹类在AI方法里实现不同功能进行区分，默认0
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
        
        protected CProjectile(Projectile projectile, int l1, float cai0, float cai1, float cai2, float cai3, float cai4, float cai5)
        {
            c_proj = projectile;
            c_ai = new float[6] { cai0, cai1, cai2, cai3, cai4, cai5 };
            c_index = projectile.whoAmI;
            Lable = l1;
        }

        
        /// <summary>
        /// 修改projAI
        /// </summary>
        /// <param name="projectile"></param>
        public virtual void ProjectileAI(Projectile projectile) { }

        /// <summary>
        /// 杀死射弹前的操作
        /// </summary>
        /// <param name="projectile"></param>
        public virtual void PreProjectileKilled(Projectile projectile) { }

    }
}
