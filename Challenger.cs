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
        public override string Author => "z枳 & 星夜神花";
        public override string Description => "增强游戏难度，更好的游戏体验";
        public override string Name => "Challenger";
        public override Version Version => new Version(1, 0, 0, 0);

        public string ChallengerDir = TShock.SavePath + "/Challenger";

        public string configPath = Path.Combine(TShock.SavePath + "/Challenger", "ChallengerConfig.json");

        public Config config;

        public Challenger(Main game) : base(game)
        {
            CMain cMain = new CMain();
        }

        public override void Initialize()
        {
            SetChallengerFile(ChallengerDir);
            config = Config.LoadConfig();

            //运行时
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            GeneralHooks.ReloadEvent += OnReload;
            //怪物触碰玩家时吸血
            GetDataHandlers.PlayerDamage += PlayerSufferDamage;
            //怪物血量调整
            Hooks.Npc.Spawn += OnNpcSpawn;
            //更新所有射弹时的钩子
            ServerApi.Hooks.ProjectileAIUpdate.Register(this, OnProjAIUpdate);
            //更新所有npc时的钩子
            Hooks.Npc.PostAI += OnNpcPostAI;
            //测试钩子
            ServerApi.Hooks.ServerChat.Register(this, OnTest);
            //npc被击中时触发
            ServerApi.Hooks.NpcStrike.Register(this, OnNpcStrike);

            //指令
            Commands.ChatCommands.Add(new Command("challenger.enable", EnableModel, "cenable", "cenable")
            {
                HelpText = "输入 /enable 来启用挑战模式，再次使用取消"
            });

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
                GeneralHooks.ReloadEvent -= OnReload;
                GetDataHandlers.PlayerDamage -= PlayerSufferDamage; 
                Hooks.Npc.Spawn -= OnNpcSpawn;
                ServerApi.Hooks.ProjectileAIUpdate.Deregister(this, OnProjAIUpdate);
                Hooks.Npc.PostAI -= OnNpcPostAI; 
                ServerApi.Hooks.ServerChat.Deregister(this, OnTest);
                ServerApi.Hooks.NpcStrike.Deregister(this, OnNpcStrike);





            }
            base.Dispose(disposing);
        }

    }
}
