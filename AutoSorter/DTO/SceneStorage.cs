using AutoSorter.Wrappers;

namespace pp.RaftMods.AutoSorter
{ 
    /// <summary>
    /// Wrapper class is used to manage scene storages and their connected components as well as storing meta info about each auto-sorter.
    /// </summary>
    public class CSceneStorage : ISceneStorage
    {
        public bool IsUpgraded => Data != null;
        public uint ObjectIndex => StorageComponent.ObjectIndex;
        public string ObjectName => StorageComponent.ObjectName;
        public CSorterStorageData Data { get; set; }
        public CGeneralStorageData AdditionalData { get; set; }
        public ISorterBehaviour AutoSorter { get; set; }
        public IStorageSmall StorageComponent { get; set; }
        public bool IsInventoryDirty { get; set; }
    }
}
