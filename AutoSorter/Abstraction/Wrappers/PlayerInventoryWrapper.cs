namespace AutoSorter.Wrappers
{
    internal class CPlayerInventoryWrapper : CInventoryWrapper
    {
        public CPlayerInventoryWrapper(PlayerInventory _inventory) : base(_inventory) { }

        public override void DropItem(IItemBase _item, int _amount) => ((PlayerInventory)mi_inventory).DropItem(((CItemBaseWrapper)_item).Unwrap(), _amount);
    }
}
