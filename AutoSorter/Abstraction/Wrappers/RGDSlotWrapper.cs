namespace AutoSorter.Wrappers
{
    internal class CRGDSlotWrapper : IRGDSlot
    {
        private readonly RGD_Slot mi_slot;

        public CRGDSlotWrapper(RGD_Slot _slot) => mi_slot = _slot;

        public RGD_Slot Unwrap() => mi_slot;
    }
}
