using AutoSorter.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;

namespace pp.RaftMods.AutoSorter.Tests
{
    [TestClass]
    public class UtilTests
    {
        #region StackedAddInventory
        [TestMethod, TestCategory("StackedAddInventory")]
        public void TestStackedAddInventory_ItemAmountZero_ReturnsZero()
        {
            var added = CUtil.StackedAddInventory(null, null, _amount: 0);
            Assert.AreEqual(0, added);
        }

        [TestMethod, TestCategory("StackedAddInventory")]
        public void TestStackedAddInventory_ItemAmountNegative_ReturnsZero()
        {
            var added = CUtil.StackedAddInventory(null, null, _amount: -1);
            Assert.AreEqual(0, added);
        }

        [TestMethod, TestCategory("StackedAddInventory")]
        public void TestStackedAddInventory_ProvideNullInventory_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => CUtil.StackedAddInventory(null, Substitute.For<IItemBase>(), _amount: 1));
        }

        [TestMethod, TestCategory("StackedAddInventory")]
        public void TestStackedAddInventory_ProvideNullItem_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => CUtil.StackedAddInventory(Substitute.For<IInventory>(), null, _amount: 1));
        }

        [TestMethod, TestCategory("StackedAddInventory")]
        public void TestStackedAddInventory_InventoryFull_ReturnsZero()
        {
            IInventory fakeInventory = FakeInventory.CreateFromSlots(
                    new List<ISlot>()
                    {
                        FakeSlot.CreateFull(),
                        FakeSlot.CreateFull(),
                        FakeSlot.CreateFull()
                    });

            const int fullStackSize = 20;
            var itemsAdded = CUtil.StackedAddInventory(fakeInventory, FakeItemBase.Create(_itemIndex: 5, fullStackSize), fullStackSize);

            Assert.AreEqual(0, itemsAdded);
        }

        [TestMethod, TestCategory("StackedAddInventory")]
        public void TestStackedAddInventory_InventoryPartiallFull_ReturnsSpaceAvailable()
        {
            IInventory fakeInventory = FakeInventory.CreateFromSlots(
                    new List<ISlot>()
                    {
                        FakeSlot.CreateFull(),
                        FakeSlot.CreateFull(),
                        FakeSlot.CreateItem(FakeItemInstance.Create(_itemIndex: 0, _amount: 10)),
                        FakeSlot.CreateItem(FakeItemInstance.Create(_itemIndex: 5, _amount: 5)), //with a max stack size of 20, we have 15 items of space left
                    });

            const int fullStackSize = 20;
            var itemsAdded = CUtil.StackedAddInventory(fakeInventory, FakeItemBase.Create(_itemIndex: 5, fullStackSize), fullStackSize);

            Assert.AreEqual(15, itemsAdded);
        }

        [TestMethod, TestCategory("StackedAddInventory")]
        public void TestStackedAddInventory_AddMoreThanMaxStackSize_SplitAddItemCallsAppropriately()
        {
            IInventory fakeInventory = FakeInventory.CreateMockFromSlots(
                    new List<ISlot>()
                    {
                        FakeSlot.CreateFull(),
                        FakeSlot.CreateFull(),
                        FakeSlot.CreateItem(FakeItemInstance.Create(_itemIndex: 0, _amount: 10)),
                        FakeSlot.CreateItem(FakeItemInstance.Create(_itemIndex: 5, _amount: 5)), //with a max stack size of 20, we have 15 items of space left
                        FakeSlot.CreateItem(FakeItemInstance.Create(_itemIndex: 5, _amount: 5)), //with a max stack size of 20, we have 15 items of space left
                    });

            const int fullStackSize = 20;
            var itemsAdded = CUtil.StackedAddInventory(fakeInventory, FakeItemBase.Create(_itemIndex: 5, fullStackSize), fullStackSize + 2);

            fakeInventory.Received().AddItem(Arg.Any<string>(), fullStackSize);
            fakeInventory.Received().AddItem(Arg.Any<string>(), 2);

            Assert.AreEqual(22, itemsAdded);
        }
        #endregion

        #region HasSpaceLeftForItem
        [TestMethod, TestCategory("HasSpaceLeftForItem")]
        public void HasSpaceLeftForItem_InventoryNull_ReturnsFalse()
        {
            var hasSpaceLeft = CUtil.HasSpaceLeftForItem(null, "someItem");

            Assert.IsFalse(hasSpaceLeft);
        }

        [TestMethod, TestCategory("HasSpaceLeftForItem")]
        public void HasSpaceLeftForItem_InventoryInvalid_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => CUtil.HasSpaceLeftForItem(FakeInventory.CreateInvalid(), "someItem"));
        }

        [TestMethod, TestCategory("HasSpaceLeftForItem")]
        public void HasSpaceLeftForItem_AllSlotsFull_ReturnsFalse()
        {
            IInventory fakeInventory = FakeInventory.CreateFromSlots(
                    new List<ISlot>()
                    {
                        FakeSlot.CreateFull(),
                        FakeSlot.CreateFull()
                    });

            var hasSpaceLeft = CUtil.HasSpaceLeftForItem(fakeInventory, "someItem");

            Assert.IsFalse(hasSpaceLeft);
        }

        [TestMethod, TestCategory("HasSpaceLeftForItem")]
        public void HasSpaceLeftForItem_SlotEmpty_ReturnsTrue()
        {
            IInventory fakeInventory = FakeInventory.CreateFromSlots(
                    new List<ISlot>()
                    {
                        FakeSlot.CreateFull(),
                        FakeSlot.CreateEmpty()
                    });

            var hasSpaceLeft = CUtil.HasSpaceLeftForItem(fakeInventory, "someItem");

            Assert.IsTrue(hasSpaceLeft);
        }

        [TestMethod, TestCategory("HasSpaceLeftForItem")]
        public void HasSpaceLeftForItem_SlotLocked_ReturnsTrue()
        {
            IInventory fakeInventory = FakeInventory.CreateFromSlot(FakeSlot.CreateLocked());

            var hasSpaceLeft = CUtil.HasSpaceLeftForItem(fakeInventory, "someItem");

            Assert.IsFalse(hasSpaceLeft);
        }

        [TestMethod, TestCategory("HasSpaceLeftForItem")]
        public void HasSpaceLeftForItem_SlotInactive_ReturnsFalse()
        {
            IInventory fakeInventory = FakeInventory.CreateFromSlot(FakeSlot.CreateInactive());

            var hasSpaceLeft = CUtil.HasSpaceLeftForItem(fakeInventory, "someItem");

            Assert.IsFalse(hasSpaceLeft);
        }

        [TestMethod, TestCategory("HasSpaceLeftForItem")]
        [DataRow("someItem", DisplayName = "ItemUniqueNameMatches")]
        [DataRow("someOtherItem", DisplayName = "ItemUniqueNameDoesNotMatch")]
        public void HasSpaceLeftForItem_ItemNameMatches(string _itemName)
        {
            IInventory fakeInventory = FakeInventory.CreateFromSlots(
                   new List<ISlot>()
                   {
                        FakeSlot.CreateFull(),
                        FakeSlot.CreateItem(FakeItemInstance.Create("someItem"))
                   });

            var hasSpaceLeft = CUtil.HasSpaceLeftForItem(fakeInventory, _itemName);

            Assert.AreEqual(_itemName == "someItem", hasSpaceLeft);
        }
        #endregion

        #region SpaceLeftForItem
        [TestMethod, TestCategory("SpaceLeftForItem")]
        public void SpaceLeftForItem_InventoryNull_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => CUtil.SpaceLeftForItem(null, Substitute.For<IItemBase>()));
        }

        [TestMethod, TestCategory("SpaceLeftForItem")]
        public void SpaceLeftForItem_ItemBaseNull_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => CUtil.SpaceLeftForItem(Substitute.For<IInventory>(), null));
        }

        [TestMethod, TestCategory("SpaceLeftForItem")]
        public void SpaceLeftForItem_SlotsInactive_ReturnsZero()
        {
            IInventory fakeInventory = FakeInventory.CreateFromSlot(FakeSlot.CreateInactive());

            var spaceAvailable = CUtil.SpaceLeftForItem(fakeInventory, Substitute.For<IItemBase>());

            Assert.AreEqual(0, spaceAvailable);
        }

        [TestMethod, TestCategory("SpaceLeftForItem")]
        public void SpaceLeftForItem_SlotsLocked_ReturnsZero()
        {
            IInventory fakeInventory = FakeInventory.CreateFromSlot(FakeSlot.CreateLocked());

            var spaceAvailable = CUtil.SpaceLeftForItem(fakeInventory, Substitute.For<IItemBase>());

            Assert.AreEqual(0, spaceAvailable);
        }

        [TestMethod, TestCategory("SpaceLeftForItem")]
        [DataRow(20, DisplayName = "NormalItemStackSize")]
        [DataRow(0, DisplayName = "ZeroItemStackSize")]
        [DataRow(-10, DisplayName = "NegativeItemStackSize")] //pretty unlikely but as there are mods to set the stack size better test it.
        public void SpaceLeftForItem_TwoSlotsAvailable_ReturnsFullStackSizeTimesTwo(int _maxStackSize)
        {
            IInventory fakeInventory = FakeInventory.CreateFromSlots(
                    new List<ISlot>()
                    {
                        FakeSlot.CreateFull(),
                        FakeSlot.CreateEmpty(), //two empty slots in inventory
                        FakeSlot.CreateEmpty()
                    });

            var spaceAvailable = CUtil.SpaceLeftForItem(fakeInventory, FakeItemBase.Create(0, _maxStackSize));

            Assert.AreEqual(Math.Max(_maxStackSize * 2, 0), spaceAvailable);
        }

        [TestMethod, TestCategory("SpaceLeftForItem")]
        [DataRow(5, true, DisplayName = "SlotHasMatchingUniqueIndex")]
        [DataRow(1, false, DisplayName = "SlotDoesNotHaveMatchingUniqueIndex")]
        public void SpaceLeftForItem_TwoSlotsAvailable_ReturnsRemainingStackSpace(int  _uniqueIndex, bool _uniqueIndexMatches)
        {
            IInventory fakeInventory = FakeInventory.CreateFromSlots(
                    new List<ISlot>()
                    {
                        FakeSlot.CreateFull(),
                        FakeSlot.CreateItem(FakeItemInstance.Create(_uniqueIndex, 5)), //two slots in inventory, one empty, one containing 5 items
                        FakeSlot.CreateEmpty()
                    });

            const int fullStackSize = 20;
            var spaceAvailable = CUtil.SpaceLeftForItem(fakeInventory, FakeItemBase.Create(5, fullStackSize));

            Assert.AreEqual((_uniqueIndexMatches ? 15 : 0) + 20, spaceAvailable);
        }
        #endregion
    }
}
