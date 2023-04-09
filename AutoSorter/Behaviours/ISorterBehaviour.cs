using AutoSorter.Manager;
using pp.RaftMods.AutoSorter;
using System.Collections;

namespace AutoSorter.Wrappers
{
    public interface ISorterBehaviour
    {
        void LoadDependencies(IASLogger _logger, IAutoSorter _mod, IStorageManager _storageManager, IItemManager _itemManager, IRaftNetwork _network, IASNetwork _asNetwork, CConfigManager _configManager);
        void LoadStorage(ISceneStorage _storage);
        ISceneStorage SceneStorage { get; }
        IInventory Inventory { get; }
        IEnumerator CheckItems();
        void DestroyImmediate();
        INetworkPlayer LocalPlayer { get; }
        bool Upgrade();
        void Downgrade();
    }
}
