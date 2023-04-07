using AutoSorter.Wrappers;
using NSubstitute;

namespace pp.RaftMods.AutoSorter.Tests
{
    public static class FakeItemInstance
    {
        public static IItemInstance Create(int _itemIndex, int _amount)
        {
            IItemInstance fakeItemInstance = Substitute.For<IItemInstance>();

            fakeItemInstance.Amount.Returns(_amount);
            fakeItemInstance.UniqueIndex.Returns(_itemIndex);

            return fakeItemInstance;
        }

        public static IItemInstance Create(string _itemName)
        {
            IItemInstance fakeItemInstance = Substitute.For<IItemInstance>();

            fakeItemInstance.UniqueName.Returns("someItem");

            return fakeItemInstance;
        }
    }
}
