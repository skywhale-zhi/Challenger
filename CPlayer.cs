using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;

namespace Challenger
{
    public class CPlayer
    {
        public double[] c_time;
        public int c_index;
        public bool tips = true;

        #region 额外数据，各种各样效果的变量需要和玩家绑定的都在这里
        public int ExtraLife = 0;
        public int ExtraMana = 0;

        public double MoltenArmorTimer = 0;
        public double CrystalAssassinArmorEffectTimer = 0;
        public double FossilArmorEffectTimer = 0;
        public bool FossilArmorEffectProj = false;
        public bool FossilArmorEffectTemp = false;
        public int FossilArmorEffectProjIndex = -1;
        public double CrimsonArmorEffectTimer;
        public double ShadowArmorEffectTimer;
        public double MeteorArmorEffectTimer = 0;
        public double BeeArmorEffectTimer = 0;
        public double SpiderArmorEffectTimer = 0;
        public bool ChlorophyteArmorEffectLife = false;
        public bool ChlorophyteArmorEffectTemp = false;
        public double TurtleArmorEffectTimer = 0;
        public bool TurtleArmorEffectLife = false;
        public bool TurtleArmorEffectTemp = false;
        public bool TikiArmorEffectLife = false;
        public bool TikiArmorEffectTemp = false;
        public bool BeetleArmorEffectLife = false;
        public bool BeetleArmorEffectTemp = false;
        public bool SpectreArmorEffectLife = false;
        public bool SpectreArmorEffectTemp = false;
        public bool SpectreArmorEffectMana = false;
        public bool SpectreArmorEffectTemp2 = false;
        public double RoyalGelTimer = 0;

        #endregion

        public CPlayer(int c_index, bool b1 = true, double d1 = 0, double d2 = 0, double d3 = 0, double d4 = 0, double d5 = 0, double d6 = 0)
        {
            this.c_index = c_index;
            tips = b1;
            c_time = new double[6] { d1, d2, d3, d4, d5, d6 };
        }
    }
}
