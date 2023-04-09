using AutoSorter.Wrappers;
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
        public CModConfig Config { get => ExtraSettingsAPI_Settings; private set { ExtraSettingsAPI_Settings = value; } }

        private const string CONFIG_NAME = "config.json";

        private readonly IASLogger mi_logger;
        private readonly IItemManager mi_itemManager;

        public CConfigManager(IASLogger _logger, IItemManager _itemManager)
        {
            mi_logger = _logger;
            mi_itemManager = _itemManager;
        }

        public void SaveConfig(string _modDataDirectory)
        {
            try
            {
                if (!Directory.Exists(_modDataDirectory))
                {
                    Directory.CreateDirectory(_modDataDirectory);
                }

                if (Config == null)
                {
                    Config = new CModConfig();
                }

                mi_logger.LogD("Save configuration.");
                File.WriteAllText(
                    Path.Combine(_modDataDirectory, CONFIG_NAME),
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

        public void LoadConfig(string _modDataDirectory)
        {
            try
            {
                var configPath = Path.Combine(_modDataDirectory, CONFIG_NAME);
                if (!File.Exists(configPath))
                {
                    SaveConfig(_modDataDirectory);
                    return;
                }
                mi_logger.LogD("Load configuration.");
                Config = JsonConvert.DeserializeObject<CModConfig>(File.ReadAllText(Path.Combine(_modDataDirectory, CONFIG_NAME))) ?? throw new System.Exception("De-serialisation failed.");
                if (Config.UpgradeCosts != null)
                {
                    foreach (var cost in Config.UpgradeCosts) cost.Load(mi_itemManager);
                }
            }
            catch (System.Exception _e)
            {
                mi_logger.LogW("Failed to load mod configuration: " + _e.Message + ". Check your configuration file.");
                mi_logger.LogD(_e.StackTrace);
            }
        }
    }
}
