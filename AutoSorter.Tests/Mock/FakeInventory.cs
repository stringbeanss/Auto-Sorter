using AutoSorter.Wrappers;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Collections.Generic;

namespace pp.RaftMods.AutoSorter.Tests
{
    public static class FakeInventory
    {
        public static IInventory CreateInvalid()
        {
            IInventory stubInventory = Substitute.For<IInventory>();

            stubInventory.AllSlots.ReturnsNull();

            return stubInventory;
        }

        public static IInventory CreateEmpty()
        {
            IInventory fakeInventory = Substitute.For<IInventory>();

            fakeInventory.AllSlots.Returns(new List<ISlot>());

            return fakeInventory;
        }

        public static IInventory CreateFromSlot(ISlot _slot)
            => CreateFromSlots(new List<ISlot>() { _slot });

        public static IInventory CreateFromSlots(List<ISlot> _slots)
        {
            return CreateMockFromSlots(_slots);
        }

        public static IInventory CreateMockFromSlots(List<ISlot> _slots)
        {
            IInventory fakeInventory = Substitute.For<IInventory>();

            fakeInventory.AllSlots.Returns(_slots);

            return fakeInventory;
        }
    }
}
