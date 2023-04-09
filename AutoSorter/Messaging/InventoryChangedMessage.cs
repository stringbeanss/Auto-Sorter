using AutoSorter.Wrappers;

namespace AutoSorter.Messaging
{
    public class InventoryChangedMessage : IMessage
    {
        public IInventory Inventory { get; private set; }

        public InventoryChangedMessage(IInventory _inventory) => Inventory = _inventory;
    }
}
