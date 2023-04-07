namespace AutoSorter.Wrappers
{
    internal class CItemBaseWrapper : IItemBase
    {
        public string Name => mi_item.name;

        public IItemInventorySettings InventorySettings => mi_item.settings_Inventory?.Wrap();
        
        public int UniqueIndex => mi_item.UniqueIndex;

        private Item_Base mi_item;

        public CItemBaseWrapper(Item_Base _item) => mi_item = _item;

        public Item_Base Unwrap() =>  mi_item;
    }
}
