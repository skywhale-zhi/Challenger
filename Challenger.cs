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

        public static Config config;

        public Challenger(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            SetChallengerFile(ChallengerDir);
            config = Config.LoadConfig();

            //运行时
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            //重载config文件
            GeneralHooks.ReloadEvent += OnReload;
            //怪物触碰玩家时吸血
            GetDataHandlers.PlayerDamage += PlayerSufferDamage;

            //更新所有射弹时的钩子
            ServerApi.Hooks.ProjectileAIUpdate.Register(this, OnProjAIUpdate);
            //在proj杀死时，清除CProj
            Hooks.Projectile.PostKilled += OnProjPostKilled;

            //怪物生成时设置CNPC
            Hooks.Npc.Spawn += OnNpcSpawn;
            //更新所有npc时的钩子
            Hooks.Npc.PostAI += OnNpcPostAI;
            //在npc被杀死时，清除CNPC
            Hooks.Npc.Killed += OnNpcKilled;

            //npc被击中时触发，用于触发某些套装效果
            ServerApi.Hooks.NpcStrike.Register(this, OnNpcStrike);


            //指令
            Commands.ChatCommands.Add(new Command("challenger.enable", EnableModel, "cenable", "cenable")
            {
                HelpText = "输入 /cenable 来启用挑战模式，再次使用取消"
            });

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
                GeneralHooks.ReloadEvent -= OnReload;
                GetDataHandlers.PlayerDamage -= PlayerSufferDamage;

                ServerApi.Hooks.ProjectileAIUpdate.Deregister(this, OnProjAIUpdate);
                Hooks.Projectile.PostKilled -= OnProjPostKilled;

                Hooks.Npc.Spawn -= OnNpcSpawn; 
                Hooks.Npc.PostAI -= OnNpcPostAI; 
                Hooks.Npc.Killed -= OnNpcKilled;

                ServerApi.Hooks.NpcStrike.Deregister(this, OnNpcStrike);
            }
            base.Dispose(disposing);
        }

    }
}
