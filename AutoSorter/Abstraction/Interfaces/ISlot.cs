namespace AutoSorter.Wrappers
{
    public interface ISlot
    {
        bool Active { get; }
        bool Locked { get; }
        bool StackIsFull { get; }
        bool IsEmpty { get; }
        bool HasValidItemInstance { get; }
        IItemInstance ItemInstance { get; }
        void RefreshComponents();
        void SetItem(IItemInstance _item);
    }
}
