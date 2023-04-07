using Newtonsoft.Json;
using pp.RaftMods.AutoSorter;
using System.IO;

namespace AutoSorter.Manager
{
    public class CConfigManager
    {
        private static CModConfig ExtraSettingsAPI_Settings = new CModConfig();

        /// <summary>
        /// Mod configuration object. Loaded from disk on mod load. 
        /// </summary>
        public static CModConfig Config { get => ExtraSettingsAPI_Settings; private set { ExtraSettingsAPI_Settings = value; } }

        private string ModConfigFilePath => Path.Combine(mi_modDataDirectory, "config.json");

        private readonly string mi_modDataDirectory;
        private readonly IASLogger mi_logger;

        public CConfigManager(string _modDataDirectory)
        {
            mi_modDataDirectory = _modDataDirectory;
            mi_logger = LoggerFactory.Default.GetLogger();
        }

        public void SaveConfig()
        {
            try
            {
                if (!Directory.Exists(mi_modDataDirectory))
                {
                    Directory.CreateDirectory(mi_modDataDirectory);
                }

                if (Config == null)
                {
                    Config = new CModConfig();
                }

                mi_logger.LogD("Save configuration.");
                File.WriteAllText(
                    ModConfigFilePath,
                    JsonConvert.SerializeObject(
                        Config,
                        Formatting.Indented,
                        new JsonSerializerSettings()
                        {
                            DefaultValueHandling = DefaultValueHandling.Include
                        }) ?? throw new System.Exception("Failed to serialize"));
            }
            catch (System.Exception _e)
            {
                mi_logger.LogW("Failed to save mod configuration: " + _e.Message);
                mi_logger.LogD(_e.StackTrace);
            }
        }

        public void LoadConfig()
        {
            try
            {
                if (!File.Exists(ModConfigFilePath))
                {
                    SaveConfig();
                    return;
                }
                mi_logger.LogD("Load configuration.");
                Config = JsonConvert.DeserializeObject<CModConfig>(File.ReadAllText(ModConfigFilePath)) ?? throw new System.Exception("De-serialisation failed.");
                //if (Config.UpgradeCosts != null)
                //{
                //    foreach (var cost in Config.UpgradeCosts) cost.Load(mi_itemManager);
                //}
            }
            catch (System.Exception _e)
            {
                mi_logger.LogW("Failed to load mod configuration: " + _e.Message + ". Check your configuration file.");
                mi_logger.LogD(_e.StackTrace);
            }
        }
    }
}
