namespace AutoSorter.Wrappers
{
    internal class CItemManagerWrapper : IItemManager
    {
        public IItemBase GetItemByName(string _name)
        {
            var item = ItemManager.GetItemByName(_name);
            if (item != null)
            {
                return item.Wrap();
            }
            return null;
        }
    }
}