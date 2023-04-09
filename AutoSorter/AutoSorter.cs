using System.Collections;
using System.IO;
using System.Linq;
using AutoSorter.DI;
using AutoSorter.Manager;
using AutoSorter.Messaging;
using AutoSorter.Wrappers;
using HarmonyLib;
using pp.RaftMods.AutoSorter.Protocol;
using RaftModLoader;
using Unity.RemoteConfig;
using UnityEngine;

namespace pp.RaftMods.AutoSorter
{
    /// <summary>
    /// Main mod class handles harmony injections, saving/loading of storage and config data, loading of the mod asset bundle and registering/loading storages into the mods data structure.
    /// Also holds and runs the coroutine used to check auto-sorters and storages for item transfer.
    /// </summary>
    public class CAutoSorter :  IAutoSorter,
                                IRecipient<WorldSavedMessage>,
                                IRecipient<WorldLoadedMessage>,
                                IRecipient<WorldUnloadedMessage>,
                                IRecipient<OpenStorageMessage>,
                                IRecipient<PickupBlockMessage>,
                                IRecipient<CreateBlockMessage>,
                                IRecipient<CloseStorageMessage>,
                                IRecipient<ConfigChangedMessage>
    {
        public const string VERSION = "1.5.0";
        public const string MOD_NAME = "AutoSorter";
        public const string MOD_NAMESPACE = "pp.RaftMods." + MOD_NAME;

        private string ModDataDirectory     => Path.Combine(Application.persistentDataPath, "Mods", MOD_NAME);
        
        private Coroutine mi_chestRoutineHandle;
        private bool mi_checkChests;
        private double mi_lastCheckDurationSeconds;

        private readonly IASLogger mi_logger;
        private readonly IStorageManager mi_storageManager;
        private readonly IStorageDataManager mi_storageDataManager;
        private readonly IItemManager mi_itemManager;
        private readonly IASNetwork mi_asNetwork;

        private readonly CConfigManager mi_configManager;
        private readonly ICoroutineHandler mi_coroutineHandler;
        private readonly CModUI mi_modUI;
        private readonly ISoundManager mi_soundManager;

        private AssetBundle mi_bundle;

        public CAutoSorter( IASLogger _logger, 
                            IStorageManager _storageManager, 
                            IStorageDataManager _storageDataManager, 
                            IItemManager _itemManager,
                            ICoroutineHandler _coroutineHandler,
                            CConfigManager _configManager, 
                            CModUI _modUI,
                            ISoundManager _soundManager,
                            IASNetwork _asNetwork)
        {
            mi_logger = _logger;
            mi_storageManager = _storageManager;
            mi_storageDataManager = _storageDataManager;
            mi_itemManager = _itemManager;
            mi_coroutineHandler = _coroutineHandler;
            mi_configManager = _configManager;
            mi_modUI = _modUI;
            mi_soundManager = _soundManager;
            mi_asNetwork = _asNetwork;
        }

        #region ENGINE_CALLBACKS
        /// <summary>
        /// Called when the mod is loaded.
        /// </summary>
        public void Load(AssetBundle _assetBundle)
        {
            mi_bundle = _assetBundle;

            mi_storageDataManager.LoadStorageData(ModDataDirectory);
            mi_configManager.LoadConfig(ModDataDirectory);

            Messenger.Default.RegisterAll(this);

            mi_logger.Log($"{MOD_NAME} v. {VERSION} loaded.");
        }

        public void Destroy()
        {
            mi_logger.LogD($"Destroying {MOD_NAME}...");
            mi_checkChests = false;
            mi_coroutineHandler.StopAllCoroutines();
            mi_storageManager.Cleanup();

            mi_asNetwork.Clear();

            mi_modUI.Destroy();
            mi_logger.LogD($"Destroyed {MOD_NAME}!");
        }
        #endregion

        private IEnumerator Run()
        {
            while (mi_checkChests)
            {
                if (!RAPI.IsCurrentSceneGame())
                {
                    mi_checkChests = false;
                    break;
                }

                if (!GameModeValueManager.GetCurrentGameModeValue().playerSpecificVariables.unlimitedResources && 
                    mi_storageManager.HasStorages)
                {
                    System.DateTime now = System.DateTime.UtcNow;

                    yield return mi_storageManager.UpdateStorages();

                    mi_lastCheckDurationSeconds = (System.DateTime.UtcNow - now).TotalSeconds;
                }
                if (mi_lastCheckDurationSeconds < mi_configManager.Config.CheckIntervalSeconds)
                {
                    yield return new WaitForSeconds((float)(mi_configManager.Config.CheckIntervalSeconds - mi_lastCheckDurationSeconds));
                }

                yield return new WaitForEndOfFrame();
            }
        }
        
        public void ReimburseConstructionCosts(IItemManager _itemManager, INetworkPlayer _player, bool _applyDowngradeMultiplier)
        {
            int toAdd;
            foreach (var cost in mi_configManager.Config.UpgradeCosts)
            {
                IItemBase item = _itemManager.GetItemByName(cost.Name);
                if (item == null)
                {
                    mi_logger.LogW($"Configured reimbursement item \"{cost.Name}\" was not found. Please check your \"{nameof(CModConfig.UpgradeCosts)}\" setting in the config file and make sure the items in there exist in your raft.");
                    continue;
                }
                if (cost.Amount < 0)
                {
                    mi_logger.LogW($"Invalid amount configured for item \"{cost.Name}\" in the \"{nameof(CModConfig.UpgradeCosts)}\" of your config file. Make sure you set the amount to a value >0.");
                    continue;
                }
                toAdd = (int)(cost.Amount * (_applyDowngradeMultiplier ? mi_configManager.Config.ReturnItemsOnDowngradeMultiplier : 1f));
                if (toAdd > 0)
                {
                    int added = CUtil.StackedAddInventory(_player.Inventory, item, toAdd);
                    if (added < toAdd)
                    {
                        _player.Inventory.DropItem(item, toAdd - added);
                    }
                }
            }
        }

        private void OnWorldSaved()
        {
            mi_storageDataManager.SaveStorageData(ModDataDirectory, mi_storageManager.SceneStorages.Values);
        }

        private void OnWorldLoaded()
        {
            mi_modUI.LoadUI(mi_bundle);

            if (Raft_Network.IsHost)
            {
                mi_checkChests = true;
                mi_chestRoutineHandle = mi_coroutineHandler.StartCoroutine(Run());
            }
            mi_logger.LogD($"World \"{SaveAndLoad.CurrentGameFileName}\" loaded.");
        } 

        private void OnWorldUnloaded()
        {
            mi_checkChests = false;
            mi_coroutineHandler.StopAllCoroutines();

            mi_storageManager.Cleanup();
            mi_asNetwork.Clear();
        }

        private void OnBlockCreated(IStorageSmall _storage)
        {
            if (_storage == null) return;

            mi_storageManager.RegisterStorage(_storage);
        }

        private void OnOpenStorage(IStorageSmall _storage, INetworkPlayer _player)
        {
            if (_player == null || !_player.IsLocalPlayer) return;

            var storage = mi_storageManager.GetStorageByIndex(_storage.ObjectIndex);
            if (storage == null)
            {
                mi_logger.LogW("Failed to find matching storage on storage open. This is a bug and should be reported.");
                return;
            }

            mi_modUI.ShowConfigDialog(storage);
        }

        private void OnCloseStorage(IStorageSmall _storage, INetworkPlayer _player)
        {
            if (_player == null || !_player.IsLocalPlayer) return;

            mi_modUI?.HideConfigDialog();

            var storage = mi_storageManager.GetStorageByIndex(_storage.ObjectIndex);
            if (storage == null)
            {
                mi_logger.LogW("Failed to find matching storage on storage close. This is a bug and should be reported.");
                return;
            }

            mi_asNetwork.Broadcast(new CDTO(EStorageRequestType.STORAGE_DATA_UPDATE, storage.StorageComponent.ObjectIndex) { Info = storage.Data });
        }

        private void OnPickupBlock(IRemovePlaceable _placeable, IBlock _block)
        {
            if (!(_block is Storage_Small)) return;
            var storage = mi_storageManager.GetStorageByIndex(_block.ObjectIndex);
            if (storage == null)
            {
                mi_logger.LogW("Failed to find matching storage on storage pickup. This is a bug and should be reported.");
                return;
            }
            if (!storage.IsUpgraded) return;
            var pickupPlayer = Traverse.Create(((CRemovePlaceableWrapper)_placeable).Unwrap()).Field("playerNetwork").GetValue<Network_Player>();
            if (pickupPlayer == null || !pickupPlayer.IsLocalPlayer)
                return;
            ReimburseConstructionCosts(mi_itemManager, pickupPlayer.Wrap(), false);
        }
        
        public void Receive(OpenStorageMessage _message)
            => OnOpenStorage(_message.Storage, _message.Player);

        public void Receive(PickupBlockMessage _message)
            => OnPickupBlock(_message.Placeable, _message.Block);

        public void Receive(CreateBlockMessage _message)
            => OnBlockCreated(_message.Storage);

        public void Receive(CloseStorageMessage _message)
            => OnCloseStorage(_message.Storage, _message.Player);

        public void Receive(WorldLoadedMessage _message)
            => OnWorldSaved();

        public void Receive(WorldSavedMessage _message)
            => OnWorldLoaded();

        public void Receive(WorldUnloadedMessage _message)
            => OnWorldUnloaded();

        public void Receive(ConfigChangedMessage _message)
            => mi_configManager.SaveConfig(ModDataDirectory);
    }
}