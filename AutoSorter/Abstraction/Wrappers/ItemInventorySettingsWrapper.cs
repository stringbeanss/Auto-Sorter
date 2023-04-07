namespace AutoSorter.Wrappers
{
    internal class CItemInventorySettingsWrapper : IItemInventorySettings
    {
        public int StackSize => mi_inventorySettings.StackSize;

        private readonly ItemInstance_Inventory mi_inventorySettings;

        public CItemInventorySettingsWrapper(ItemInstance_Inventory _inventory) => mi_inventorySettings = _inventory;
    }
}
