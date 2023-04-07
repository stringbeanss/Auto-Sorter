namespace AutoSorter.Wrappers
{
    public interface IItemInstance
    {
        string UniqueName { get; }
        int Amount { get; set; }
        int UniqueIndex { get; }
        IItemInstance Clone();
        void SetUsesToMax();
        ItemInstance Unwrap();
    }
}
