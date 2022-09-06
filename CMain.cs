using Terraria;

namespace Challenger
{
    //自己构造了个类似 Terraria.Main 用来同步proj或npc的额外数据，分别存储到cproj和cnpc里
    public class CMain
    {
        public static CProjectile[] cProjectiles = new CProjectile[Main.maxProjectiles];

        public static CNPC[] cNPCs = new CNPC[Main.maxNPCs];

        public static CPlayer[] cPlayers = new CPlayer[Main.maxPlayers];
    }
}
