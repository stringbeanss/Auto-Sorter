namespace AutoSorter.Wrappers
{
    internal class CStorageSmallWrapper : CBlockWrapper, IStorageSmall
    {
        private readonly Storage_Small mi_storage;

        public CStorageSmallWrapper(Storage_Small _storage)
            : base(_storage) => mi_storage = _storage;

        public bool IsOpen => mi_storage.IsOpen;

        public IInventory GetInventoryReference()
            => mi_storage.GetInventoryReference()?.Wrap();

        public Storage_Small Unwrap() => mi_storage;
    }
}
