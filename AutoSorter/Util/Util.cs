using AutoSorter.Manager;
using AutoSorter.Wrappers;
using System.Linq;
using UnityEngine;

namespace pp.RaftMods.AutoSorter
{
    public static class CUtil //util
    {
        //public static void Log(object _message)
        //{
        //    Debug.Log($"[{CAutoSorter.MOD_NAME}] {_message}");
        //}

        //public static void LogW(object _message)
        //{
        //    Debug.LogWarning($"[{CAutoSorter.MOD_NAME}] {_message}");
        //}

        //public static void LogE(object _message)
        //{
        //    Debug.LogError($"[{CAutoSorter.MOD_NAME}] {_message}");
        //}

        //public static void LogD(object _message)
        //{
        //    if (!(CConfigManager.Config?.Debug ?? false)) return;
        //    Debug.Log($"[{CAutoSorter.MOD_NAME}][DEBUG] {_message}");
        //}

        /// <summary>
        /// Adds items to the specified inventory and takes care of properly creating item stacks while doing so.
        /// </summary>
        /// <param name="_inventory">The inventory to add items to.</param>
        /// <param name="_item">The item to add to the inventory.</param>
        /// <param name="_amount">The amount of items to add.</param>
        public static int StackedAddInventory(IInventory _inventory, IItemBase _item, int _amount)
        {
            if (_amount <= 0) return 0;

            if (_inventory == null) throw new System.ArgumentNullException("_inventory");
            if (_item == null) throw new System.ArgumentNullException("_item");

            int spaceLeft = SpaceLeftForItem(_inventory, _item);
            if (spaceLeft == 0) return 0;

            int actuallyAdded = _amount;
            if(_amount > spaceLeft)
            {
                actuallyAdded = spaceLeft;
            }

            int stackSize = _item.InventorySettings.StackSize;
            for (int i = 0; i < actuallyAdded / stackSize; ++i) //need to do the stacking ourself
            {
                _inventory.AddItem(_item.Name, stackSize);
            }
            int rest = actuallyAdded % stackSize;
            if (rest > 0)
            {
                _inventory.AddItem(_item.Name, rest);
            }

            return actuallyAdded;
        }

        public static int SpaceLeftForItem(IInventory _inventory, IItemBase _item)
        {
            if (_inventory == null) throw new System.ArgumentNullException("_inventory");
            if (_item == null) throw new System.ArgumentNullException("_item");

            int spaceLeft = 0;
            foreach(var slot in _inventory.AllSlots)
            {
                if (!slot.Active || slot.Locked || slot.StackIsFull || (!slot.IsEmpty && slot.ItemInstance.UniqueIndex != _item.UniqueIndex)) continue;
                spaceLeft += Mathf.Max(_item.InventorySettings.StackSize, 0) - (slot.ItemInstance?.Amount ?? 0);
            }
            return spaceLeft;
        }

        public static bool HasSpaceLeftForItem(IInventory _inventory, string _itemName)
        {
            return _inventory?.AllSlots.Any(_o => _o.Active && !_o.Locked && !_o.StackIsFull && (_o.IsEmpty || _o.ItemInstance.UniqueName == _itemName)) ?? false;
        }

        /// <summary>
        /// Reformats the given texture, copying its context to a new texture with the provided format.
        /// </summary>
        /// <param name="_texture">The texture to reformat.</param>
        /// <param name="_format">The new format of the texture.</param>
        /// <param name="_useMipMaps">Are mip-maps enabled for the newly created texture.</param>
        /// <returns>A new texture object reformatted with the provided settings.</returns>
        public static Texture2D MakeReadable(Texture2D _texture, TextureFormat _format = TextureFormat.RGB24, bool _useMipMaps = false)
        {
            RenderTexture r = RenderTexture.GetTemporary(_texture.width, _texture.height);
            Graphics.Blit(_texture, r);
            RenderTexture.active = r;
            var tex = new Texture2D(_texture.width, _texture.height, _format, _useMipMaps);
            tex.ReadPixels(new Rect(0, 0, r.width, r.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            r.Release();
            return tex;
        }
    }
}
