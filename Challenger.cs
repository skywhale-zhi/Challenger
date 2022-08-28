using Microsoft.Xna.Framework;
using OTAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Challenger
{
    [ApiVersion(2, 1)]
    public partial class Challenger : TerrariaPlugin
    {
        public override string Author => "z枳";
        public override string Description => "增强游戏难度，更好的游戏体验";
        public override string Name => "Challenger";
        public override Version Version => new Version(1, 0, 0, 0);

        public string ChallengerDir = TShock.SavePath + "/Challenger";

        public Config config;
        public Challenger(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            SetChallengerFile(ChallengerDir);
            config = Config.LoadConfig();
            GeneralHooks.ReloadEvent += OnReload;
            //怪物触碰玩家时吸血
            GetDataHandlers.PlayerDamage.Register(PlayerSufferDamage);
            //怪物属性调整
            //Hooks.Npc.Spawn += OnNPCSpawn;
            Hooks.Npc.PostNetDefaults += OnPostNetDefaults;
            Hooks.Npc.PreAI += OnPreAI;
            Hooks.Npc.PostAI += OnPostAI;



        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= OnReload;
                Hooks.Npc.PostNetDefaults -= OnPostNetDefaults;
                //Hooks.Npc.Spawn -= OnNPCSpawn;
                Hooks.Npc.PreAI -= OnPreAI;
                Hooks.Npc.PostAI -= OnPostAI; 

            }
            base.Dispose(disposing);
        }

    }
}
