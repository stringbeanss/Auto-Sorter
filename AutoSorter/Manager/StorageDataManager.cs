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
        private const string MOD_DATA_NAME = "storagedata.json";
        private const string MOD_ADDITIONAL_DATA_NAME = "additional_storagedata.json";

        /// <summary>
        /// Storage data which is loaded and saved to disk to preserve auto-sorter configurations. 
        /// Combines the world name and the storage data for the world.
        /// </summary>
        private Dictionary<string, CSorterStorageData[]> SavedSorterStorageData { get; set; } = new Dictionary<string, CSorterStorageData[]>();

        /// <summary>
        /// Additional storage data that is loaded and saved to disk to preserve certain settings for all storages (auto-sorter or not).
        /// </summary>
        private Dictionary<string, CGeneralStorageData[]> SavedAdditionaStorageData { get; set; } = new Dictionary<string, CGeneralStorageData[]>();

        private readonly IASLogger mi_logger;

        public CStorageDataManager(IASLogger _logger)
        {
            mi_logger = _logger;
        }

        public void LoadStorageData(string _modDataDirectory)
        {
            LoadSorterStorageData(_modDataDirectory);
            LoadAdditionalStorageData(_modDataDirectory);
        }

        public void SaveStorageData(string _modDataDirectory, IEnumerable<ISceneStorage> _sceneStorages)
        {
            SaveSorterStorageData(_modDataDirectory, _sceneStorages);
            SaveAdditionalStorageData(_modDataDirectory, _sceneStorages);
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

        private void LoadSorterStorageData(string _modDataDirectory)
        {
            try
            {
                var modDataFilePath = Path.Combine(_modDataDirectory, MOD_DATA_NAME);

                if (!File.Exists(modDataFilePath)) return;

                CSorterStorageData[] data = JsonConvert.DeserializeObject<CSorterStorageData[]>(File.ReadAllText(modDataFilePath)) ?? throw new Exception("De-serialisation failed.");
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

        private void SaveSorterStorageData(string _modDataDirectory, IEnumerable<ISceneStorage> _sceneStorages)
        {
            try
            {
                if (_sceneStorages == null || !_sceneStorages.Any()) return;

                var modDataFilePath = Path.Combine(_modDataDirectory, MOD_DATA_NAME);
                if (File.Exists(modDataFilePath))
                {
                    File.Delete(modDataFilePath);
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
                    modDataFilePath,
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

        private void LoadAdditionalStorageData(string _modDataDirectory)
        {
            try
            {
                var modAdditionalDataFilePath = Path.Combine(_modDataDirectory, MOD_ADDITIONAL_DATA_NAME);
                if (!File.Exists(modAdditionalDataFilePath)) return;

                CGeneralStorageData[] data = JsonConvert.DeserializeObject<CGeneralStorageData[]>(File.ReadAllText(modAdditionalDataFilePath)) ?? throw new System.Exception("De-serialisation failed.");
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

        private void SaveAdditionalStorageData(string _modDataDirectory, IEnumerable<ISceneStorage> _storages)
        {
            try
            {
                if (_storages == null || !_storages.Any()) return;

                var modAdditionalDataFilePath = Path.Combine(_modDataDirectory, MOD_ADDITIONAL_DATA_NAME);

                if (File.Exists(modAdditionalDataFilePath))
                {
                    File.Delete(modAdditionalDataFilePath);
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
                    modAdditionalDataFilePath,
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
