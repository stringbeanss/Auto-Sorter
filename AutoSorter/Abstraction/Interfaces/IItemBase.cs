namespace AutoSorter.Wrappers
{
    public interface IItemBase
    {
        string Name { get; }
        int UniqueIndex { get; }
        IItemInventorySettings InventorySettings { get; }
    }
}
