using AutoSorter.Manager;
using System.Collections.Generic;
using System.Linq;

namespace AutoSorter.Wrappers
{
    internal static class CWrapperExtensions
    {
        public static INetworkPlayer Wrap(this Network_Player _player) => new CNetworkPlayerWrapper(_player);
        public static IItemBase Wrap(this Item_Base _item) => new CItemBaseWrapper(_item);
        public static ISlot Wrap(this Slot _slot) => new CSlotWrapper(_slot);
        public static IInventory Wrap(this Inventory _inventory) => new CInventoryWrapper(_inventory);
        public static IItemInstance Wrap(this ItemInstance _itemInstance) => new CItemInstanceWrapper(_itemInstance);
        public static IItemInventorySettings Wrap(this ItemInstance_Inventory _itemInventorySettings) => new CItemInventorySettingsWrapper(_itemInventorySettings);
        public static IRGDSlot Wrap(this RGD_Slot _slot) => new CRGDSlotWrapper(_slot);
        public static IEnumerable<IRGDSlot> Wrap(this IEnumerable<RGD_Slot> _slots) => _slots.Select(_o => _o.Wrap());
        public static IStorageSmall Wrap(this Storage_Small _storage) => new CStorageSmallWrapper(_storage);
        public static IRaftStorageManager Wrap(this StorageManager _storageManager) => new CRaftStorageManagerWrapper(_storageManager);
        public static IRemovePlaceable Wrap(this RemovePlaceables _removePlaceable) => new CRemovePlaceableWrapper(_removePlaceable);
        public static IBlock Wrap(this Block _block) => new CBlockWrapper(_block);
        public static IRaftNetwork Wrap(this Raft_Network _network) => new CRaftNetworkWrapper(_network);
        public static ISoundManager Wrap(this SoundManager _soundManager) => new CRaftSoundManagerWrapper(_soundManager);
    }
}
