using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;

namespace Challenger
{
    public class Config
    {
        static string configPath = Path.Combine(TShock.SavePath + "/Challenger", "ChallengerConfig.json");

        public static Config LoadConfig()
        {
            if (!File.Exists(configPath))
            {
                Config config = new Config(
                    true,true,0.5f
                );
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                return config;
            }
            else
            {
                Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
                return config;
            }
        }

        public Config(bool b1, bool b2, float f1)
        {
            enableChallenge_是否启用挑战模式 = b1;
            enableMonsterSucksBlood_是否启用怪物吸血 = b2;
            BloodAbsorptionRatio_吸血比率 = f1;
        }

        public bool enableChallenge_是否启用挑战模式;
        public bool enableMonsterSucksBlood_是否启用怪物吸血;
        public float BloodAbsorptionRatio_吸血比率;
    }
}
