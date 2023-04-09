using pp.RaftMods.AutoSorter;
using System.Collections.Generic;

namespace AutoSorter.Wrappers
{
    public interface IStorageDataManager
    {
        bool HasSorterDataForSave(string _saveName);
        bool HasStorageDataForSave(string _saveName);
        CSorterStorageData GetSorterData(string _saveName, ulong _objectIndex);
        CGeneralStorageData GetStorageData(string _saveName, ulong _objectIndex);
        void LoadStorageData(string _modDataDirectory);
        void SaveStorageData(string _modDataDirectory, IEnumerable<ISceneStorage> _storages);
    }
}
