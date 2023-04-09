using AutoSorter.Wrappers;
using pp.RaftMods.AutoSorter;
using RaftModLoader;
using System.Linq;
using UnityEngine;

namespace AutoSorter.Commands
{
    public static class StorageCommands
    {
        [ConsoleCommand("asListStorages", "Lists all storages and their status.")]
        public static string ListStorages(string[] _args)
        {
            var storageManager = CASMod.DIContainer.Resolve<IStorageManager>();
            return $"### Tracked scene storages ({storageManager.SceneStorages?.Count ?? 0}) ###\n" +
                (
                    storageManager.SceneStorages.Count == 0 ?
                        "No registered storages in scene." :
                        "- " + string.Join("\n- ",
                            storageManager.SceneStorages.Values.Select(_o => $"\"{_o.StorageComponent.ObjectName}\" Dirty: {_o.IsInventoryDirty} AutoSorter: {_o.IsUpgraded} " +
                                $"{(_o.IsUpgraded ? $" Priority: {_o.Data.Priority} Filters: {_o.Data.Filters.Count}" : "")}" +
                                $"{(_o.AdditionalData != null ? " Ignore: " + _o.AdditionalData.Ignore : "")}")));
        }

        [ConsoleCommand("asTestReduceUses", "Sets the remaining uses of all items in the players inventory to half the maximum uses.")]
        public static string ReduceUses(string[] _args)
        {
            int c = 0;
            var network = ComponentManager<Raft_Network>.Value;
            foreach (var slot in network.GetLocalPlayer().Inventory.allSlots)
            {
                if (slot.HasValidItemInstance())
                {
                    slot.itemInstance.Uses = (int)Mathf.Ceil(slot.itemInstance.BaseItemMaxUses * 0.5f);
                    c++;
                }
            }
            return "Set item uses on " + c + " items.";
        }

        [ConsoleCommand("asTestMaxUseableAndStackable", "Prints all items that have maxUses > 1 and a stack size > 1.")]
        public static string PrintMaxUseableAndStackable(string[] _args)
        {
            var items = ItemManager.GetAllItems().Where(_o => _o.MaxUses > 1 && _o.settings_Inventory.Stackable);
            return "### Items ###\n" + (items.Any() ? "- " + string.Join("\n- ", items) : "None");
        }
    }
}
