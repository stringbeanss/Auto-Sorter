using AutoSorter.Wrappers;

namespace AutoSorter.Messaging
{
    public class InventoryDirtyMessage : IMessage
    {
        public IInventory Inventory { get; private set; }

        public InventoryDirtyMessage(IInventory _inventory) => Inventory = _inventory;
    }
}
