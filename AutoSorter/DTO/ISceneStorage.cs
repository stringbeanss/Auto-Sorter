using AutoSorter.Wrappers;

namespace pp.RaftMods.AutoSorter
{ 
    public interface ISceneStorage
    {
        uint ObjectIndex { get; }
        string ObjectName { get; }
        bool IsUpgraded{ get; }
        bool IsInventoryDirty { get; set; }
        CGeneralStorageData AdditionalData { get; set; }
        CSorterStorageData Data { get; set; }
        ISorterBehaviour AutoSorter { get; set; }
        IStorageSmall StorageComponent { get; set; }
    }
}
