using Microsoft.Xna.Framework;
using Terraria;

namespace Challenger
{
    public abstract class CNPC
    {
        public NPC c_npc;
        public float[] c_ai;
        public int c_index;
        public int State;


        protected CNPC()
        {
            c_npc = null;
            c_ai = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };
            c_index = -1;
            State = 0;
        }

        protected CNPC(NPC npc)
        {
            c_npc = npc;
            c_ai = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };
            c_index = npc.whoAmI;
            State = 0;
        }

        protected CNPC(NPC npc, float ai0 = 0, float ai1 = 0, float ai2 = 0, float ai3 = 0, float ai4 = 0, float ai5 = 0, int i1 = 0)
        {
            c_npc = npc;
            c_ai = new float[6] { ai0, ai1, ai2, ai3, ai4, ai5 };
            c_index = npc.whoAmI;
            State = i1;
        }
        
        public Projectile NewProjectile(Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner = 255, float ai0 = 0, float ai1 = 0)
        {
            int index = Projectile.NewProjectile(c_npc.GetSpawnSourceForNPCFromNPCAI(), position, velocity, Type, Damage, KnockBack, Owner, ai0, ai1);
            return Main.projectile[index];
        }

        public virtual void NPCAI(NPC npc) { }

        public virtual int SetState(NPC npc) { return 0; }

        public virtual void OnHurtPlayers(NPC npc) { }
    }
}
