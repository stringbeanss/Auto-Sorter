using AutoSorter.DI;
using AutoSorter.Manager;
using AutoSorter.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;

namespace pp.RaftMods.AutoSorter.Tests
{
    [TestClass]
    public class StorageManagerTests
    {
        private Dependencies mi_dependencies;

        [TestInitialize]
        public void Setup()
        {
            LoadBinds();
        }

        [TestMethod]
        public void RegisterStorage_RegisterInvalid_NotAdded()
        {
            var manager = CreateStorageManager();

            Assert.ThrowsException<ArgumentNullException>(() => manager.RegisterStorage(null));
        }

        [TestMethod]
        public void RegisterStorage_AlreadyRegistered_ShouldPrintWarning()
        {
            var mockLogger = Substitute.For<IASLogger>();
            var fakeStorage = Substitute.For<IStorageSmall>();
            fakeStorage.ObjectIndex.Returns(1u);

            mi_dependencies.Bind<IASLogger>().ToConstant(mockLogger);
            var manager = CreateStorageManager();

            manager.SceneStorages.Add(1, new CSceneStorage());

            manager.RegisterStorage(fakeStorage);

            mockLogger.Received().LogW(Arg.Any<string>());
        }

        [TestMethod]
        public void RegisterStorage_Register_ShouldRegisterInSceneStorages()
        {
            var mockLogger  = Substitute.For<IASLogger>();
            mi_dependencies.Bind<IASLogger>().ToConstant(mockLogger);

            var fakeStorage = Substitute.For<IStorageSmall>();
            var fakeNetwork = Substitute.For<IRaftNetwork>();

            fakeNetwork.IsHost.Returns(true);
            mi_dependencies.Bind<IRaftNetwork>().ToConstant(fakeNetwork);
            
            fakeStorage.ObjectIndex.Returns(1u);
            fakeStorage.AddComponent<CStorageBehaviour>().Returns(new CStorageBehaviour());

            var manager = CreateStorageManager();

            manager.RegisterStorage(fakeStorage);

            Assert.AreEqual(1, manager.SceneStorages.Count);
            mockLogger.Received().LogD(Arg.Is<string>(_o => _o.StartsWith("Registered")));
        }

        [TestMethod]
        public void Cleanup_WithStorages_ShouldDestroyStorage()
        {
            var mockStorage = Substitute.For<ISorterBehaviour>();

            var manager = CreateStorageManager();

            manager.SceneStorages.Add(0u, new CSceneStorage() { AutoSorter = mockStorage });
            manager.Cleanup();

            Assert.AreEqual(0, manager.SceneStorages.Count);
            mockStorage.Received().DestroyImmediate();
        }

        [TestMethod]
        public void SetStorageInventoryDirty_NotHost_ShouldNotSetDirty()
        {
            var stubStorage = Substitute.For<ISorterBehaviour>();
            var stubInventory = Substitute.For<IInventory>();
            stubStorage.Inventory.Returns(stubInventory);

            var stubNetwork = Substitute.For<IRaftNetwork>();
            stubNetwork.IsHost.Returns(false);
            mi_dependencies.Bind<IRaftNetwork>().ToConstant(stubNetwork);

            var stubSceneStorage = Substitute.For<ISceneStorage>();
            stubSceneStorage.AutoSorter.Returns(stubStorage);

            var manager = CreateStorageManager();

            manager.SceneStorages.Add(0u, stubSceneStorage);
            manager.SetStorageInventoryDirty(stubInventory);

            Assert.IsFalse(stubSceneStorage.IsInventoryDirty);
        }

        [TestMethod]
        public void SetStorageInventoryDirty_WrongInventoryType_ShouldNotSetDirty()
        {
            var stubStorage = Substitute.For<ISorterBehaviour>();
            var stubInventory = Substitute.For<IInventory>();
            stubInventory.Unwrap().Returns(new PlayerInventory());
            stubStorage.Inventory.Returns(stubInventory);

            var stubNetwork = Substitute.For<IRaftNetwork>();
            stubNetwork.IsHost.Returns(true);
            mi_dependencies.Bind<IRaftNetwork>().ToConstant(stubNetwork);

            var stubSceneStorage = Substitute.For<ISceneStorage>();
            stubSceneStorage.AutoSorter.Returns(stubStorage);

            var manager = CreateStorageManager();

            manager.SceneStorages.Add(0u, stubSceneStorage);
            manager.SetStorageInventoryDirty(stubInventory);

            Assert.IsFalse(stubSceneStorage.IsInventoryDirty);
        }

        [TestMethod]
        public void SetStorageInventoryDirty_CantFindStorageForInventory_ShouldNotSetDirty()
        {
            var stubStorage = Substitute.For<ISorterBehaviour>();
            var stubInventory = Substitute.For<IInventory>();
            stubInventory.Unwrap().Returns(new Inventory());
            stubStorage.Inventory.Returns(stubInventory);

            var stubNetwork = Substitute.For<IRaftNetwork>();
            stubNetwork.IsHost.Returns(true);
            mi_dependencies.Bind<IRaftNetwork>().ToConstant(stubNetwork);

            var stubSceneStorage = Substitute.For<ISceneStorage>();
            stubSceneStorage.AutoSorter.Returns(stubStorage);

            var manager = CreateStorageManager();

            manager.SetStorageInventoryDirty(stubInventory);

            Assert.IsFalse(stubSceneStorage.IsInventoryDirty);
        }

        [TestMethod]
        public void SetStorageInventoryDirty_ValidInventory_ShouldSetDirty()
        {
            var mockLogger = Substitute.For<IASLogger>();

            var stubStorage = Substitute.For<ISorterBehaviour>();
            var stubInventory = Substitute.For<IInventory>();
            stubInventory.Unwrap().Returns(new Inventory());
            stubStorage.Inventory.Returns(stubInventory);

            var stubNetwork = Substitute.For<IRaftNetwork>();
            stubNetwork.IsHost.Returns(true);

            var stubSceneStorage = Substitute.For<ISceneStorage>();
            stubSceneStorage.AutoSorter.Returns(stubStorage);

            mi_dependencies.Bind<IASLogger>().ToConstant(mockLogger);
            mi_dependencies.Bind<IRaftNetwork>().ToConstant(stubNetwork);
            var manager = CreateStorageManager();

            manager.SceneStorages.Add(0u, stubSceneStorage);
            manager.SetStorageInventoryDirty(stubInventory);

            Assert.IsTrue(stubSceneStorage.IsInventoryDirty);
            mockLogger.Received().LogD(Arg.Is<string>(_o => _o.StartsWith("Inventory for storage")));
        }

        [TestMethod]
        public void CreateSceneStorage_AsClient_ShouldSetDirty()
        {
            var mockLogger = Substitute.For<IASLogger>();

            var stubStorage = Substitute.For<ISorterBehaviour>();
            var stubInventory = Substitute.For<IInventory>();
            stubInventory.Unwrap().Returns(new Inventory());
            stubStorage.Inventory.Returns(stubInventory);

            var stubNetwork = Substitute.For<IRaftNetwork>();
            stubNetwork.IsHost.Returns(true);

            var stubSceneStorage = Substitute.For<ISceneStorage>();
            stubSceneStorage.AutoSorter.Returns(stubStorage);

            mi_dependencies.Bind<IASLogger>().ToConstant(mockLogger);
            mi_dependencies.Bind<IRaftNetwork>().ToConstant(stubNetwork);
            var manager = CreateStorageManager();

            manager.SceneStorages.Add(0u, stubSceneStorage);
          //  manager.CreateSce(stubInventory);

            Assert.IsTrue(stubSceneStorage.IsInventoryDirty);
            mockLogger.Received().LogD(Arg.Is<string>(_o => _o.StartsWith("Inventory for storage")));
        }

        private IStorageManager CreateStorageManager()
        {
            return mi_dependencies.Resolve<IStorageManager>();
        }

        private void LoadBinds()
        {
            mi_dependencies = new Dependencies();

            mi_dependencies.Bind<IASLogger, Logger>();
            mi_dependencies.Bind<IASNetwork, CASNetwork>();

            mi_dependencies.Bind<ICoroutineHandler>().ToConstant(Substitute.For<ICoroutineHandler>());
            mi_dependencies.Bind<IAutoSorter, CAutoSorter>().AsSingleton();
            mi_dependencies.Bind<IStorageDataManager, CStorageDataManager>(string.Empty);
            mi_dependencies.Bind<CConfigManager>().AsSingleton();
            mi_dependencies.Bind<IStorageManager, CStorageManager>().AsSingleton();

            mi_dependencies.Bind<IItemManager>().ToConstant(Substitute.For<IItemManager>());
            mi_dependencies.Bind<IRaftNetwork>().ToConstant(Substitute.For<IRaftNetwork>());
        }
    }
}
