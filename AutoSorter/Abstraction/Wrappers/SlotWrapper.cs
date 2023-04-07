namespace AutoSorter.Wrappers
{
    internal class CSlotWrapper : ISlot
    {
        public bool Active => mi_slot.active;

        public bool Locked => mi_slot.locked;

        public bool StackIsFull => mi_slot.StackIsFull();

        public bool IsEmpty => mi_slot.IsEmpty;

        public IItemInstance ItemInstance => mi_slot.itemInstance?.Wrap();

        public bool HasValidItemInstance => mi_slot.HasValidItemInstance();

        private readonly Slot mi_slot;

        public CSlotWrapper(Slot _slot) =>  mi_slot = _slot;

        public void RefreshComponents() => mi_slot.RefreshComponents();

        public void SetItem(IItemInstance _item) => mi_slot.SetItem(_item.Unwrap());
    }
}
