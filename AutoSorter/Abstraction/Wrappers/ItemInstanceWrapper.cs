namespace AutoSorter.Wrappers
{
    internal class CItemInstanceWrapper : IItemInstance
    {
        public int Amount => mi_itemInstance.Amount;

        public int UniqueIndex => mi_itemInstance.UniqueIndex;

        public string UniqueName => mi_itemInstance.UniqueName;

        int IItemInstance.Amount { get => mi_itemInstance.Amount; set => mi_itemInstance.Amount = value; }

        private ItemInstance mi_itemInstance;

        public CItemInstanceWrapper(ItemInstance _itemInstance) => mi_itemInstance = _itemInstance;

        public ItemInstance Unwrap() => mi_itemInstance;

        public IItemInstance Clone() => mi_itemInstance.Clone().Wrap();

        public void SetUsesToMax() => mi_itemInstance.SetUsesToMax();
    }
}
