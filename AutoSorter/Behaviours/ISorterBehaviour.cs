using AutoSorter.Manager;
using pp.RaftMods.AutoSorter;
using System.Collections;

namespace AutoSorter.Wrappers
{
    public interface ISorterBehaviour
    {
        void Load(IAutoSorter _sorter, IStorageManager _storageManager, IItemManager _itemManager, IRaftNetwork _network, CSceneStorage _storage);
        CSceneStorage SceneStorage { get; }
        IInventory Inventory { get; }
        IEnumerator CheckItems();
        void DestroyImmediate();
        INetworkPlayer LocalPlayer { get; }
        bool Upgrade();
        void Downgrade();
    }
}
