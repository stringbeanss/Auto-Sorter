using AutoSorter.Messaging;
using AutoSorter.Wrappers;
using pp.RaftMods.AutoSorter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoSorter.Manager
{
    public class CStorageManager :  IStorageManager, 
                                    IRecipient<InventoryChangedMessage>, 
                                    IRecipient<InventoryDirtyMessage>
    {
        public bool HasStorages => SceneStorages.Count > 0;

        /// <summary>
        /// All currently loaded scene storages. Is loaded on world load.
        /// </summary>
        public Dictionary<uint, ISceneStorage> SceneStorages { get; private set; } = new Dictionary<uint, ISceneStorage>();

        private readonly Queue<ISceneStorage> mi_registerQueuedStorages = new Queue<ISceneStorage>();
        private readonly Queue<ISceneStorage> mi_unregisterQueuedStorages = new Queue<ISceneStorage>();
        private bool mi_deferStorageRegister;

        private readonly IAutoSorter mi_mod;
        private readonly IStorageDataManager mi_storageData;
        private readonly IItemManager mi_itemManager;
        private readonly IASLogger mi_logger;
        private readonly IRaftNetwork mi_network;
        private readonly IASNetwork mi_asNetwork;
        private readonly ISaveAndLoad mi_saveManager;

        public CStorageManager(
            IAutoSorter _mod, 
            IStorageDataManager _storageData, 
            IItemManager _itemManager,
            ISaveAndLoad _saveManager,
            IRaftNetwork _network,
            IASLogger _logger,
            IASNetwork _asNetwork)
        {
            mi_mod = _mod;
            mi_storageData = _storageData;
            mi_itemManager = _itemManager;
            mi_network = _network;
            mi_logger = _logger;
            mi_saveManager = _saveManager;
            mi_asNetwork = _asNetwork;

            Messenger.Default.Register<InventoryChangedMessage>(this);
            Messenger.Default.Register<InventoryDirtyMessage>(this);
        }

        public IEnumerator UpdateStorages()
        {
            mi_deferStorageRegister = true;
            var storages = SceneStorages.Where(_o => _o.Value.IsUpgraded).OrderByDescending(_o => _o.Value.Data.Priority);
            foreach (var storage in storages)
            {
                yield return storage.Value.AutoSorter.CheckItems();
            }
            foreach (var storage in SceneStorages)
            {
                storage.Value.IsInventoryDirty = false;
            }
            mi_deferStorageRegister = false;
            FlushStorageQueues();
        }

        public void Cleanup()
        {
            //undo all changes to the scene
            foreach(var storage in SceneStorages.Values)
            {
                storage.AutoSorter.DestroyImmediate();
            }
            SceneStorages.Clear();

            mi_registerQueuedStorages.Clear();
            mi_unregisterQueuedStorages.Clear();

            Messenger.Default.UnregisterAll(this);
        }

        public void RegisterStorage(IStorageSmall _storage)
        {
            if (_storage == null) throw new ArgumentNullException("_storage");

            if (SceneStorages.ContainsKey(_storage.ObjectIndex))
            {
                mi_logger.LogW($"Storage for index {_storage.ObjectIndex} has already been registered.");
                return;
            }

            var sceneStorage = CreateSceneStorage(_storage);
            if (mi_deferStorageRegister)
            {
                mi_registerQueuedStorages.Enqueue(sceneStorage);
                mi_logger.LogD("Enqueued storage \"" + _storage.ObjectName + "\" for register as we are currently running checks. Queued storages for register: " + mi_registerQueuedStorages.Count);
                return;
            }
            SceneStorages.Add(_storage.ObjectIndex, sceneStorage);
            mi_logger.LogD("Registered storage \"" + _storage.ObjectName + "\" Total storages: " + SceneStorages.Count);
        }

        public void SetStorageInventoryDirty(IInventory _inventory)
        {
            if (!mi_network.IsHost) return;
            if (_inventory.Unwrap() is PlayerInventory) return;
            var storageForInventory = SceneStorages.Values.FirstOrDefault(_o => _o.AutoSorter.Inventory == _inventory);
            if (storageForInventory == null) return;
            storageForInventory.IsInventoryDirty = true;
            mi_logger.LogD($"Inventory for storage \"{storageForInventory.StorageComponent.ObjectName}\"({storageForInventory.ObjectIndex}) is marked as dirty.");
        }
        
        public ISceneStorage CreateSceneStorage(IStorageSmall _storage)
        {
            var sceneStorage = new CSceneStorage();

            if (mi_network.IsHost)
            {
                if (mi_storageData.HasSorterDataForSave(mi_saveManager.CurrentGameFileName))
                {
                    var data = mi_storageData.GetSorterData(mi_saveManager.CurrentGameFileName, _storage.ObjectIndex);
                    if (data != null)
                    {
                        sceneStorage.Data = data;
                    }
                }
                if (mi_storageData.HasStorageDataForSave(mi_saveManager.CurrentGameFileName))
                {
                    var additionalData = mi_storageData.GetStorageData(mi_saveManager.CurrentGameFileName, _storage.ObjectIndex);
                    if (additionalData != null)
                    {
                        sceneStorage.AdditionalData = additionalData;
                    }
                }
            }

            sceneStorage.StorageComponent = _storage;
            sceneStorage.AutoSorter = _storage.AddComponent<CStorageBehaviour>();
            sceneStorage.StorageComponent.SetNetworkIdBehaviour(sceneStorage.AutoSorter);

            CASMod.DIContainer.Call(sceneStorage.AutoSorter, nameof(ISorterBehaviour.LoadDependencies));

            sceneStorage.AutoSorter.LoadStorage(sceneStorage);

            return sceneStorage;
        }

        public void OnInventoryChanged(IInventory _inventory)
        {
            if (_inventory is PlayerInventory) return;
            var storageForInventory = SceneStorages.Values.FirstOrDefault(_o => _o.AutoSorter.Inventory == _inventory);
            if (storageForInventory == null) return;
            if (mi_network.IsHost)
            {
                storageForInventory.IsInventoryDirty = true;
                mi_logger.LogD($"Inventory for storage \"{storageForInventory.ObjectName}\"({storageForInventory.ObjectIndex}) is marked as dirty.");
            }
            else
            {
                mi_asNetwork.BroadcastInventoryState(storageForInventory.AutoSorter);
                mi_logger.LogD($"Inventory for storage \"{storageForInventory.ObjectName}\"({storageForInventory.ObjectIndex}) changed.");
            }
        }

        private void FlushStorageQueues()
        {
            if (mi_registerQueuedStorages.Count > 0)
            {
                mi_logger.LogD("Registering " + mi_registerQueuedStorages.Count + " queued storages now.");
                while (mi_registerQueuedStorages.Count > 0)
                {
                    var storage = mi_registerQueuedStorages.Dequeue();
                    SceneStorages.Add(storage.ObjectIndex, storage);
                }
            }
            if (mi_unregisterQueuedStorages.Count > 0)
            {
                mi_logger.LogD("Unregistering " + mi_unregisterQueuedStorages.Count + " queued storages now.");
                while (mi_unregisterQueuedStorages.Count > 0)
                {
                    UnregisterStorage(mi_unregisterQueuedStorages.Dequeue());
                }
            }
        }

        public ISceneStorage GetStorageByIndex(uint _objectIndex)
        {
            if (!SceneStorages.ContainsKey(_objectIndex))
            {
                mi_logger.LogW($"Tried to get storage for index {_objectIndex} which could not be found.");
                return null;
            }
            return SceneStorages[_objectIndex];
        }

        public void UnregisterStorage(ISceneStorage _storage)
        {
            if (mi_deferStorageRegister)
            {
                mi_unregisterQueuedStorages.Enqueue(_storage);
                mi_logger.LogD("Enqueued storage \"" + _storage.StorageComponent.ObjectName + "\" for unregister as we are currently running checks. Queued storages for unregister: " + mi_unregisterQueuedStorages.Count);
                return;
            }

            SceneStorages.Remove(_storage.ObjectIndex);

            if (SceneStorages != null)
            {
                mi_logger.LogD("Unregistered storage \"" + (_storage.StorageComponent == null ? "UNKNOWN (storage was destroyed)" : _storage.StorageComponent.ObjectName) + "\" Total storages: " + (SceneStorages?.Count.ToString() ?? "No storages"));
            }
        }

        public void Receive(InventoryChangedMessage _message)
            => OnInventoryChanged(_message.Inventory);

        public void Receive(InventoryDirtyMessage _message)
            => SetStorageInventoryDirty(_message.Inventory);
    }
}
