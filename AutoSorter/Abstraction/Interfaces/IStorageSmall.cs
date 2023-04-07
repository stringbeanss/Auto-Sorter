using System.Collections.Generic;

namespace AutoSorter.Wrappers
{
    public interface IStorageSmall : IBlock
    {
        bool IsOpen { get; }
        IInventory GetInventoryReference();
        Storage_Small Unwrap();
    }
}
