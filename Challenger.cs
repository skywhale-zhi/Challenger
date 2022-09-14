using OTAPI;
using System;
using System.IO;
using System.Linq;
using Terraria;
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

        //测试数据，一会删
        public static int testnum1 = 1;
        public static int testnum2 = 1;
        public static int testnum3 = 1;

        public Challenger(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            SetChallengerFile(ChallengerDir);
            config = Config.LoadConfig();

            //运行时
            //ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            //重载config文件
            GeneralHooks.ReloadEvent += OnReload;
            //怪物触碰玩家时吸血
            GetDataHandlers.PlayerDamage += PlayerSufferDamage;
            

            //更新所有射弹时的钩子
            ServerApi.Hooks.ProjectileAIUpdate.Register(this, OnProjAIUpdate);
            //在proj杀死前，添加亡语等
            Hooks.Projectile.PreKill += OnProjPreKilled;
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
            

            //拿持修改后的物品时添加提示词
            GetDataHandlers.PlayerSlot += OnHoldItem;

            //一直执行
            Hooks.Player.PreUpdate += OnPreUpdate;
            //Hooks.Player.PostUpdate += OnPostUpdate;
            //GetDataHandlers.PlayerBuffUpdate += OnPlayerBuffSpawnOrKilled;//生成或消除buff的时候执行

            //玩家进出服务器时处理Cplayer
            ServerApi.Hooks.ServerJoin.Register(this, OnServerjoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);


            //指令
            Commands.ChatCommands.Add(new Command("challenger.enable", EnableModel, "cenable", "cenable")
            {
                HelpText = "输入 /cenable 来启用挑战模式，再次使用取消"
            });

            //指令
            Commands.ChatCommands.Add(new Command("challenger.tips", EnableTips, "tips", "TIPS")
            {
                HelpText = "输入 /tips 来启用内容提示，如各种物品的强化文字提示，再次使用取消"
            });

            //测试指令，等会删
            Commands.ChatCommands.Add(new Command("challenger.test", test, "t", "t")
            {
                HelpText = "输入 /t num num"
            });

        }


        //测试指令，等会删
        private void test(CommandArgs args)
        {
            if (!args.Parameters.Any() || args.Parameters.Count < 2)
            {
                args.Player.SendInfoMessage("输入错误/t num num");
                return;
            }
            try
            {
                if (args.Parameters[0] == "1")
                {
                    testnum1 = int.Parse(args.Parameters[1]);
                }
                if (args.Parameters[0] == "2")
                {
                    testnum2 = int.Parse(args.Parameters[1]);
                }
                if (args.Parameters[0] == "3")
                {
                    testnum3 = int.Parse(args.Parameters[1]);
                }
                args.Player.SendInfoMessage("成功");
            }
            catch(Exception ex)
            {
                args.Player.SendInfoMessage(ex.Message);
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
                GeneralHooks.ReloadEvent -= OnReload;
                GetDataHandlers.PlayerDamage -= PlayerSufferDamage;

                ServerApi.Hooks.ProjectileAIUpdate.Deregister(this, OnProjAIUpdate);
                Hooks.Projectile.PreKill += OnProjPreKilled;
                Hooks.Projectile.PostKilled -= OnProjPostKilled;

                Hooks.Npc.Spawn -= OnNpcSpawn; 
                Hooks.Npc.PostAI -= OnNpcPostAI; 
                Hooks.Npc.Killed -= OnNpcKilled;

                GetDataHandlers.PlayerSlot -= OnHoldItem;
                ServerApi.Hooks.NpcStrike.Deregister(this, OnNpcStrike);
                Hooks.Player.PreUpdate -= OnPreUpdate;
                //Hooks.Player.PostUpdate -= OnPostUpdate;
                //GetDataHandlers.PlayerBuffUpdate -= OnPlayerBuffSpawnOrKilled;
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerjoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
            }
            base.Dispose(disposing);
        }

    }
}
