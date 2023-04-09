using pp.RaftMods.AutoSorter;
using System.Collections;
using System.Collections.Generic;

namespace AutoSorter.Wrappers
{
    public interface IStorageManager
    {
        bool HasStorages { get; }
        Dictionary<uint, ISceneStorage> SceneStorages { get; }
        void UnregisterStorage(ISceneStorage _storage);
        void Cleanup();
        IEnumerator UpdateStorages();
        void RegisterStorage(IStorageSmall _storage);
        ISceneStorage GetStorageByIndex(uint _index);
        void SetStorageInventoryDirty(IInventory _inventory);
        ISceneStorage CreateSceneStorage(IStorageSmall _storage);
    }
}
