using AutoSorter.Wrappers;
using NSubstitute;

namespace pp.RaftMods.AutoSorter.Tests
{
    public static class FakeSlot
    {
        public static ISlot CreateFull()
        {
            ISlot fakeSlot = Substitute.For<ISlot>();

            fakeSlot.StackIsFull.Returns(true);

            return fakeSlot;
        }

        public static ISlot CreateEmpty()
        {
            var fakeSlot = Substitute.For<ISlot>();

            fakeSlot.Active.Returns(true);
            fakeSlot.IsEmpty.Returns(true);
            fakeSlot.ItemInstance.Returns(Substitute.For<IItemInstance>());

            return fakeSlot;
        }

        public static ISlot CreateLocked()
        {
            var fakeSlot = Substitute.For<ISlot>();

            fakeSlot.Active.Returns(true);
            fakeSlot.Locked.Returns(true);
            fakeSlot.IsEmpty.Returns(true);
            fakeSlot.ItemInstance.Returns(Substitute.For<IItemInstance>());

            return fakeSlot;
        }

        public static ISlot CreateInactive()
        {
            var fakeSlot = Substitute.For<ISlot>();

            fakeSlot.Locked.Returns(true);
            fakeSlot.IsEmpty.Returns(true);
            fakeSlot.ItemInstance.Returns(Substitute.For<IItemInstance> ());

            return fakeSlot;
        }

        public static ISlot CreateItem(IItemInstance _item)
        {
            var fakeSlot = Substitute.For<ISlot>();

            fakeSlot.Active.Returns(true);
            fakeSlot.ItemInstance.Returns(_item);

            return fakeSlot;
        }
    }
}
