using AutoSorter.Wrappers;
using NSubstitute;

namespace pp.RaftMods.AutoSorter.Tests
{
    public static class FakeItemBase
    {
        public static IItemBase Create(int _itemIndex, int _maxStackSize)
        {
            IItemBase fakeItemBase = Substitute.For<IItemBase>();
            IItemInventorySettings fakeInventorySettings = Substitute.For<IItemInventorySettings>();
            
            fakeInventorySettings.StackSize.Returns(_maxStackSize);

            fakeItemBase.InventorySettings.Returns(fakeInventorySettings);
            fakeItemBase.UniqueIndex.Returns(_itemIndex);

            return fakeItemBase;
        }
    }
}
