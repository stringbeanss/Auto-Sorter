namespace AutoSorter.Wrappers
{
    public interface INetworkPlayer
    {
        IInventory Inventory { get; }

        bool IsLocalPlayer { get; }

        IRaftStorageManager StorageManager { get; }
    }
}
