using System.Collections.Generic;
using System.Linq;

namespace AutoSorter.Wrappers
{
    internal class CInventoryWrapper : IInventory
    {
        protected readonly Inventory mi_inventory;

        public CInventoryWrapper(Inventory _inventory) => mi_inventory = _inventory;

        public string Name => mi_inventory.name;

        public IEnumerable<ISlot> AllSlots => mi_inventory.allSlots.Select(_o => _o.Wrap()).Cast<ISlot>();

        public void AddItem(string _uniqueName, int _stackSize) 
            => mi_inventory.AddItem(_uniqueName, _stackSize);

        public void AddItem(IItemInstance _item, bool _dropIfFull) 
            => mi_inventory.AddItem(((CItemInstanceWrapper)_item).Unwrap(), _dropIfFull);

        public virtual void DropItem(IItemBase _item, int _amount) => throw new System.NotImplementedException();

        public int GetItemCount(string _itemName) => mi_inventory.GetItemCount(_itemName);

        public int GetItemCount(IItemBase _item) => mi_inventory.GetItemCount(((CItemBaseWrapper)_item).Unwrap());

        public void RemoveItem(string _uniqueName, int _amount) => mi_inventory.RemoveItem(_uniqueName, _amount);

        public void SetSlotsFromRGD(IEnumerable<IRGDSlot> _slots) => mi_inventory.SetSlotsFromRGD(_slots.Select(_o => ((CRGDSlotWrapper)_o).Unwrap()).ToArray());

        public Inventory Unwrap() => mi_inventory;
    }
}
