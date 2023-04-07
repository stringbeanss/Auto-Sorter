using Newtonsoft.Json;
using pp.RaftMods.AutoSorter;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using AutoSorter.Wrappers;

namespace AutoSorter.Manager
{
    public class CStorageDataManager : IStorageDataManager
    {
        private string ModDataFilePath => Path.Combine(m_modDataDirectory, "storagedata.json");
        private string ModAdditionalDataFilePath => Path.Combine(m_modDataDirectory, "additional_storagedata.json");

        /// <summary>
        /// Storage data which is loaded and saved to disk to preserve auto-sorter configurations. 
        /// Combines the world name and the storage data for the world.
        /// </summary>
        private Dictionary<string, CSorterStorageData[]> SavedSorterStorageData { get; set; } = new Dictionary<string, CSorterStorageData[]>();

        /// <summary>
        /// Additional storage data that is loaded and saved to disk to preserve certain settings for all storages (auto-sorter or not).
        /// </summary>
        private Dictionary<string, CGeneralStorageData[]> SavedAdditionaStorageData { get; set; } = new Dictionary<string, CGeneralStorageData[]>();

        private readonly string m_modDataDirectory;
        private readonly IASLogger mi_logger;

        public CStorageDataManager(string _modDataDirectory)
        {
            m_modDataDirectory = _modDataDirectory;
            mi_logger = LoggerFactory.Default.GetLogger();
        }

        public void LoadStorageData()
        {
            LoadSorterStorageData();
            LoadAdditionalStorageData();
        }

        public void SaveStorageData(IEnumerable<ISceneStorage> _sceneStorages)
        {
            SaveSorterStorageData(_sceneStorages);
            SaveAdditionalStorageData(_sceneStorages);
        }

        public bool HasSorterDataForSave(string _saveName)
        {
            return SavedSorterStorageData.ContainsKey(_saveName);
        }

        public bool HasStorageDataForSave(string _saveName)
        {
            return SavedAdditionaStorageData.ContainsKey(_saveName);
        }

        public CSorterStorageData GetSorterData(string _saveName, ulong _objectIndex)
        {
            if (!SavedSorterStorageData.ContainsKey(_saveName)) return null;
            return SavedSorterStorageData[_saveName].FirstOrDefault(_o => _o.ObjectID == _objectIndex);
        }

        public CGeneralStorageData GetStorageData(string _saveName, ulong _objectIndex)
        {
            if (!SavedAdditionaStorageData.ContainsKey(_saveName)) return null;
            return SavedAdditionaStorageData[_saveName].FirstOrDefault(_o => _o.ObjectID == _objectIndex);
        }

        private void LoadSorterStorageData()
        {
            try
            {
                if (!File.Exists(ModDataFilePath)) return;

                CSorterStorageData[] data = JsonConvert.DeserializeObject<CSorterStorageData[]>(File.ReadAllText(ModDataFilePath)) ?? throw new Exception("De-serialisation failed.");
                SavedSorterStorageData = data
                    .Where(_o => !string.IsNullOrEmpty(_o.SaveName))
                    .GroupBy(_o => _o.SaveName)
                    .ToDictionary(_o => _o.Key, _o => _o.ToArray());

                foreach (var allStorageData in SavedSorterStorageData)
                {
                    foreach (var storageData in allStorageData.Value)
                    {
                        storageData.OnAfterDeserialize();
                    }
                }
            }
            catch (System.Exception _e)
            {
                mi_logger.LogW("Failed to load saved mod data: " + _e.Message + ". Storage data wont be loaded.");
                mi_logger.LogD(_e.StackTrace);
                SavedSorterStorageData = new Dictionary<string, CSorterStorageData[]>();
            }
        }

        private void SaveSorterStorageData(IEnumerable<ISceneStorage> _sceneStorages)
        {
            try
            {
                if (_sceneStorages == null || !_sceneStorages.Any()) return;

                if (File.Exists(ModDataFilePath))
                {
                    File.Delete(ModDataFilePath);
                }

                if (SavedSorterStorageData.ContainsKey(SaveAndLoad.CurrentGameFileName))
                {
                    SavedSorterStorageData.Remove(SaveAndLoad.CurrentGameFileName);
                }

                foreach (var storage in _sceneStorages)
                {
                    storage.Data?.OnBeforeSerialize();
                }

                SavedSorterStorageData.Add(
                    SaveAndLoad.CurrentGameFileName,
                    _sceneStorages
                        .Where(_o => _o.AutoSorter != null && _o.IsUpgraded)
                        .Select(_o => _o.Data)
                        .ToArray());

                File.WriteAllText(
                    ModDataFilePath,
                    JsonConvert.SerializeObject(
                        SavedSorterStorageData.SelectMany(_o => _o.Value).ToArray(),
                        Formatting.None,
                        new JsonSerializerSettings()
                        {
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        }) ?? throw new System.Exception("Failed to serialize"));
            }
            catch (System.Exception _e)
            {
                mi_logger.LogW("Failed to save mod data: " + _e.Message + ". Storage data wont be saved.");
                mi_logger.LogD(_e.StackTrace);
            }
        }

        private void LoadAdditionalStorageData()
        {
            try
            {
                if (!File.Exists(ModAdditionalDataFilePath)) return;

                CGeneralStorageData[] data = JsonConvert.DeserializeObject<CGeneralStorageData[]>(File.ReadAllText(ModAdditionalDataFilePath)) ?? throw new System.Exception("De-serialisation failed.");
                SavedAdditionaStorageData = data
                    .GroupBy(_o => _o.SaveName)
                    .Select(_o => new KeyValuePair<string, CGeneralStorageData[]>(_o.Key, _o.ToArray()))
                    .ToDictionary(_o => _o.Key, _o => _o.Value);
            }
            catch (System.Exception _e)
            {
                mi_logger.LogW("Failed to load additional saved mod data: " + _e.Message + ". Storage data wont be loaded.");
                SavedAdditionaStorageData = new Dictionary<string, CGeneralStorageData[]>();
                mi_logger.LogD(_e.StackTrace);
            }
        }

        private void SaveAdditionalStorageData(IEnumerable<ISceneStorage> _storages)
        {
            try
            {
                if (_storages == null || !_storages.Any()) return;

                if (File.Exists(ModAdditionalDataFilePath))
                {
                    File.Delete(ModAdditionalDataFilePath);
                }

                if (SavedAdditionaStorageData.ContainsKey(SaveAndLoad.CurrentGameFileName))
                {
                    SavedAdditionaStorageData.Remove(SaveAndLoad.CurrentGameFileName);
                }

                SavedAdditionaStorageData.Add(
                    SaveAndLoad.CurrentGameFileName,
                    _storages
                        .Where(_o => _o.AdditionalData != null)
                        .Select(_o => _o.AdditionalData)
                        .ToArray());

                File.WriteAllText(
                    ModAdditionalDataFilePath,
                    JsonConvert.SerializeObject(
                        SavedAdditionaStorageData.SelectMany(_o => _o.Value).ToArray(),
                        Formatting.None,
                        new JsonSerializerSettings()
                        {
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        }) ?? throw new System.Exception("Failed to serialize"));
            }
            catch (System.Exception _e)
            {
                mi_logger.LogW("Failed to save additional mod data: " + _e.Message + ". Storage data wont be saved.");
                mi_logger.LogD(_e.StackTrace);
            }
        }
    }
}
