namespace AutoSorter.Wrappers
{
    internal class CRaftStorageManagerWrapper : IRaftStorageManager
    {
        private readonly StorageManager mi_storageManager;

        public CRaftStorageManagerWrapper(StorageManager _storageManager) => mi_storageManager = _storageManager;

        public StorageManager Unwrap() => mi_storageManager;
    }
}
