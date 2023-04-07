using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoSorter.IOC;
using AutoSorter.Manager;
using AutoSorter.Messaging;
using AutoSorter.Wrappers;
using HarmonyLib;
using HMLLibrary;
using pp.RaftMods.AutoSorter.Protocol;
using RaftModLoader;
using UnityEngine;

namespace pp.RaftMods.AutoSorter
{
    /// <summary>
    /// Main mod class handles harmony injections, saving/loading of storage and config data, loading of the mod asset bundle and registering/loading storages into the mods data structure.
    /// Also holds and runs the coroutine used to check auto-sorters and storages for item transfer.
    /// </summary>
    public class CAutoSorter : Mod, 
                                IAutoSorter,
                                IRecipient<OpenStorageMessage>,
                                IRecipient<PickupBlockMessage>,
                                IRecipient<CreateBlockMessage>,
                                IRecipient<CloseStorageMessage>
    {
        /// <summary>
        /// Explicitly typed mod instance for static access.
        /// </summary>
        private static CAutoSorter Get = null;

        public const string VERSION                                 = "1.5.0";
        public const string MOD_NAME                                = "AutoSorter";
        private const string MOD_NAMESPACE                          = "pp.RaftMods." + MOD_NAME;

        private const string ASSET_PAYLOAD_PATH                     = "assets/autosorter";

        private const string UI_CANVAS_ROOT                         = "Canvases/_CanvasGame_New";
        private const string UI_CONFIG_DIALOG_PREFAB_PATH           = "Assets/Prefabs/UI/AutoSorter_UIRoot.prefab";
        private const string UI_CONFIG_DIALOG_ITEM_PREFAB_PATH      = "Assets/Prefabs/UI/Windows/Config/Item_Entry.prefab";
        private const string UI_HELP_TEXT_PATH                      = "Assets/Config/help.txt";

        private string ModDataDirectory     => Path.Combine(Application.persistentDataPath, "Mods", MOD_NAME);

        /// <summary>
        /// The help text displayed when first opening the auto-sorter UI or whenever the help button is clicked.
        /// Is loaded from the mods asset bundle.
        /// </summary>
        public static string HelpText               { get; private set; }
        /// <summary>
        /// Dialog reference which can be used to prompt the user before specific actions.
        /// </summary>
        public CUIDialog Dialog                     { get; private set; }
        /// <summary>
        /// Handle to the Raft <see cref="SoundManager"/> to play UI sounds.
        /// </summary>
        public SoundManager Sounds                  { get; private set; }
        
        private Harmony mi_harmony;

        private Coroutine mi_chestRoutineHandle;
        private Coroutine mi_deferConfigUIRoutineHandle;
        private bool mi_checkChests;
        private double mi_lastCheckDurationSeconds;

        private AssetBundle mi_bundle;

        private GameObject mi_uiRoot;
        private CUISorterConfigDialog mi_configDialog;

        private IASLogger mi_logger;
        private IStorageManager mi_storageManager;

        private Dependencies mi_dependencies = new Dependencies();

        #region ENGINE_CALLBACKS
        /// <summary>
        /// Called when the mod is loaded.
        /// </summary>
        private IEnumerator Start()
        {
            if (Get)
            {
                DestroyImmediate(Get);
                Debug.LogWarning("Autosorter mod has been loaded twice. Destroying old mod instance.");
            }
            Get = this;

            mi_harmony = new Harmony(MOD_NAMESPACE);
            mi_harmony.PatchAll(Assembly.GetExecutingAssembly());

            LoadBinds();
            LoadDependencies();

            if (mi_bundle == null)
            {
                mi_logger.LogD("Loading mod bundle @ " + ASSET_PAYLOAD_PATH + "...");
                AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(GetEmbeddedFileBytes(ASSET_PAYLOAD_PATH));
                yield return request;
                mi_bundle = request.assetBundle;
                if (!mi_bundle)
                {
                    mi_logger.LogE("Failed to load embedded bundle. This usually happens if you load two instances of the auto sorter mod.");
                    DestroyImmediate(Get);
                    yield break;
                }
            }

            HelpText = mi_bundle.LoadAsset<TextAsset>(UI_HELP_TEXT_PATH)?.text?.Trim();
            if (string.IsNullOrEmpty(HelpText))
            {
                mi_logger.LogW("Help text file could not be read.");
                HelpText = "Failed to load!";
            }

            Sounds = ComponentManager<SoundManager>.Value;
            if (!Sounds)
            {
                mi_logger.LogW("Failed to get sound manager on mod load.");
            }

            mi_logger.Log($"{MOD_NAME} v. {VERSION} loaded.");
        }

        private void OnDestroy()
        {
            mi_logger.LogD($"Destroying {MOD_NAME}...");
            mi_checkChests = false;
            StopAllCoroutines();
            mi_storageManager.Cleanup();

            CNetwork.Clear();

            if (mi_harmony != null)
            {
                mi_harmony.UnpatchAll(MOD_NAMESPACE);
            }

            if (mi_bundle)
            {
                mi_bundle.Unload(true);
                mi_bundle = null;
            }

            if (mi_uiRoot)
            {
                GameObject.DestroyImmediate(mi_uiRoot);
            }
            mi_uiRoot = null;

            Get = null;
            mi_logger.LogD($"Destroyed {MOD_NAME}!");
        }
        #endregion

        #region MOD_CALLBACKS
        /// <summary>
        /// Called whenever the world is loaded while transitioning from the main menu to the world.
        /// </summary>
        public override void WorldEvent_WorldLoaded()
        {
            base.WorldEvent_WorldLoaded();
            if (mi_uiRoot == null)
            {
                LoadUI();
            }
            StartCoroutine(mi_configDialog.LoadItems());

            if (Raft_Network.IsHost)
            {
                mi_checkChests = true;
                mi_chestRoutineHandle = StartCoroutine(Run());
            }
            mi_logger.LogD($"World \"{SaveAndLoad.CurrentGameFileName}\" loaded.");
        }

        /// <summary>
        /// Called whenever the world is unloaded while transitioning from the world to the main menu.
        /// </summary>
        public override void WorldEvent_WorldUnloaded()
        {
            base.WorldEvent_WorldUnloaded();

            mi_checkChests = false;
            StopAllCoroutines();

            mi_storageManager.Cleanup();
            CNetwork.Clear();
        }

        /// <summary>
        /// Called whenever the user saves the world pressing "Save" in the in-game menu.
        /// </summary>
        public override void WorldEvent_WorldSaved()
        {
            base.WorldEvent_WorldSaved();
            var storageDataManager = mi_dependencies.Resolve<IStorageDataManager>();
            storageDataManager.SaveStorageData(mi_storageManager.SceneStorages.Values);
        }
        #endregion

        public virtual void LoadBinds()
        {
            mi_dependencies.Bind<IAutoSorter>().ToConstant(this);
            mi_dependencies.Bind<IASLogger>().ToConstant(LoggerFactory.Default.GetLogger());
            mi_dependencies.Bind<IStorageDataManager>().ToConstant(new CStorageDataManager(ModDataDirectory));
            mi_dependencies.Bind<CConfigManager>().ToConstant(new CConfigManager(ModDataDirectory));
            mi_dependencies.Bind<IStorageManager, CStorageManager>().AsSingleton();
            mi_dependencies.Bind<IItemManager, CItemManagerWrapper>().AsSingleton();
            mi_dependencies.Bind<IRaftNetwork>().ToConstant(ComponentManager<Raft_Network>.Value.Wrap());
        }

        public virtual void LoadDependencies()
        {
            mi_logger = mi_dependencies.Resolve<IASLogger>();
            mi_storageManager = mi_dependencies.Resolve<IStorageManager>();

            var storageDataManager = mi_dependencies.Resolve<IStorageDataManager>();
            storageDataManager.LoadStorageData();

            var configManager = mi_dependencies.Resolve<CConfigManager>();
            configManager.LoadConfig();

            LoggerFactory.Default.Debug = CConfigManager.Config.Debug;
        }

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
                if (mi_lastCheckDurationSeconds < CConfigManager.Config.CheckIntervalSeconds)
                {
                    yield return new WaitForSeconds((float)(CConfigManager.Config.CheckIntervalSeconds - mi_lastCheckDurationSeconds));
                }

                yield return new WaitForEndOfFrame();
            }
        }
        
        private void LoadUI()
        {
            mi_logger.LogD("Loading auto-sorter UI...");
            GameObject rootAsset = mi_bundle.LoadAsset<GameObject>(UI_CONFIG_DIALOG_PREFAB_PATH);
            if (!rootAsset)
            {
                mi_logger.LogE("Failed to load UI root asset from bundle.");
                return;
            }
            GameObject itemAsset = mi_bundle.LoadAsset<GameObject>(UI_CONFIG_DIALOG_ITEM_PREFAB_PATH);
            if (!rootAsset)
            {
                mi_logger.LogE("Failed to load UI item asset from bundle.");
                return;
            }

            var canvasRoot = GameObject.Find(UI_CANVAS_ROOT);
            if (!canvasRoot)
            {
                mi_logger.LogE("Failed to load rafts UI canvases anchor. The mod might be out of date.");
                return;
            }

            mi_uiRoot = GameObject.Instantiate(rootAsset, canvasRoot.transform, false);
            mi_uiRoot.transform.SetAsLastSibling();
            foreach(var t in mi_uiRoot.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = LayerMask.NameToLayer("UI");
            }

            Transform configDialogRoot = mi_uiRoot.transform.Find("ConfigDialog");
            mi_configDialog = configDialogRoot.gameObject.AddComponent<CUISorterConfigDialog>();
            mi_configDialog.Load(this, mi_dependencies.Resolve<CConfigManager>(), itemAsset);

            Transform dialogRoot = mi_uiRoot.transform.Find("Dialog");
            Dialog = dialogRoot.gameObject.AddComponent<CUIDialog>();
            mi_logger.LogD("Auto-sorter UI loaded!");
        }

        private IEnumerator WaitAndShowUI(ISceneStorage _storage)
        {
            while (mi_configDialog == null) yield return new WaitForEndOfFrame();
            if (!_storage.StorageComponent.IsOpen) yield break;
            mi_configDialog.Show(_storage);
        }

        public void ReimburseConstructionCosts(IItemManager _itemManager, INetworkPlayer _player, bool _applyDowngradeMultiplier)
        {
            int toAdd;
            foreach (var cost in CConfigManager.Config.UpgradeCosts)
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
                toAdd = (int)(cost.Amount * (_applyDowngradeMultiplier ? CConfigManager.Config.ReturnItemsOnDowngradeMultiplier : 1f));
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
        
        private void OnBlockCreated(IStorageSmall _storage)
        {
            if (Get == null) return; //mod is being unloaded

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

            if (mi_configDialog == null) //If the UI is not fully loaded directly after entering the scene, make sure we wait until its available before showing it to the user.
            {
                if (mi_deferConfigUIRoutineHandle != null)
                {
                    StopCoroutine(mi_deferConfigUIRoutineHandle);
                }
                mi_deferConfigUIRoutineHandle = StartCoroutine(WaitAndShowUI(storage));
                return;
            }

            mi_configDialog.Show(storage);
        }

        private void OnCloseStorage(IStorageSmall _storage, INetworkPlayer _player)
        {
            if (_player == null || !_player.IsLocalPlayer) return;

            mi_configDialog?.Hide();

            var storage = mi_storageManager.GetStorageByIndex(_storage.ObjectIndex);
            if (storage == null)
            {
                mi_logger.LogW("Failed to find matching storage on storage close. This is a bug and should be reported.");
                return;
            }

            CNetwork.Broadcast(new CDTO(EStorageRequestType.STORAGE_DATA_UPDATE, storage.StorageComponent.ObjectIndex) { Info = storage.Data });
        }

        private void OnPickupBlock(IRemovePlaceable _placeable, IBlock _block)
        {
            if (!(_block is Storage_Small)) return;
            var storage = Get.mi_storageManager.GetStorageByIndex(_block.ObjectIndex);
            if (storage == null)
            {
                mi_logger.LogW("Failed to find matching storage on storage pickup. This is a bug and should be reported.");
                return;
            }
            if (!storage.IsUpgraded) return;
            var pickupPlayer = Traverse.Create(((CRemovePlaceableWrapper)_placeable).Unwrap()).Field("playerNetwork").GetValue<Network_Player>();
            if (pickupPlayer == null || !pickupPlayer.IsLocalPlayer)
                return;
            ReimburseConstructionCosts(mi_dependencies.Resolve<IItemManager>(), pickupPlayer.Wrap(), false);
        }
        
        public void Receive(OpenStorageMessage _message)
            => OnOpenStorage(_message.Storage, _message.Player);

        public void Receive(PickupBlockMessage _message)
            => OnPickupBlock(_message.Placeable, _message.Block);

        public void Receive(CreateBlockMessage _message)
            => OnBlockCreated(_message.Storage);

        public void Receive(CloseStorageMessage _message)
            => OnCloseStorage(_message.Storage, _message.Player);
        #region PATCHES

        #endregion

        #region COMMANDS
        [ConsoleCommand("asListStorages", "Lists all storages and their status.")]
        public static string ListStorages(string[] _args)
        {
            return $"### Tracked scene storages ({Get.mi_storageManager.SceneStorages?.Count ?? 0}) ###\n" + 
                (
                    Get.mi_storageManager.SceneStorages.Count == 0 ? 
                        "No registered storages in scene." :
                        "- " + string.Join("\n- ",
                            Get.mi_storageManager.SceneStorages.Values.Select(_o => $"\"{_o.StorageComponent.ObjectName}\" Dirty: {_o.IsInventoryDirty} AutoSorter: {_o.IsUpgraded} " +
                                $"{(_o.IsUpgraded ? $" Priority: {_o.Data.Priority} Filters: {_o.Data.Filters.Count}" : "")}" +
                                $"{(_o.AdditionalData != null ? " Ignore: " + _o.AdditionalData.Ignore : "")}")));
        }

        [ConsoleCommand("asTestReduceUses", "Sets the remaining uses of all items in the players inventory to half the maximum uses.")]
        public static string ReduceUses(string[] _args)
        {
            int c = 0;
            var network = ComponentManager<Raft_Network>.Value;
            foreach (var slot in network.GetLocalPlayer().Inventory.allSlots)
            {
                if (slot.HasValidItemInstance())
                {
                    slot.itemInstance.Uses = (int)Mathf.Ceil(slot.itemInstance.BaseItemMaxUses * 0.5f);
                    c++;
                }
            }
            return "Set item uses on " + c + " items.";
        }

        [ConsoleCommand("asTestMaxUseableAndStackable", "Prints all items that have maxUses > 1 and a stack size > 1.")]
        public static string PrintMaxUseableAndStackable(string[] _args)
        {
            var items = ItemManager.GetAllItems().Where(_o => _o.MaxUses > 1 && _o.settings_Inventory.Stackable);
            return "### Items ###\n" + (items.Any() ? "- " + string.Join("\n- ", items) : "None");
        }
        #endregion
    }
}