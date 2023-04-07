using System.Collections.Generic;
using System.Security.Policy;

namespace AutoSorter.Wrappers
{
    public interface IInventory
    {
        string Name { get; }

        IEnumerable<ISlot> AllSlots { get; }

        void AddItem(IItemInstance _item, bool _dropIfFull);

        void AddItem(string _uniqueName, int _stackSize);

        void RemoveItem(string _uniqueName, int _amount);

        void DropItem(IItemBase _item, int _amount);

        void SetSlotsFromRGD(IEnumerable<IRGDSlot> _slots);

        int GetItemCount(string _itemName);

        int GetItemCount(IItemBase _item);

        Inventory Unwrap();
    }
}
