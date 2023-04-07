using AutoSorter.Messaging;
using AutoSorter.Wrappers;
using HarmonyLib;

namespace AutoSorter.Patches
{
    [HarmonyPatch(typeof(Inventory))]
    internal class CHarmonyPatch_Inventory
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddItem", typeof(string), typeof(int))]
        private static void AddItem(Inventory __instance, string uniqueItemName, int amount)
            => Messenger.Default.Send(new InventoryChangedMessage(__instance.Wrap()));

        [HarmonyPrefix]
        [HarmonyPatch("AddItem", typeof(string), typeof(Slot), typeof(int))]
        private static void AddItem(Inventory __instance, string uniqueItemName, Slot slot, int amount)
            => Messenger.Default.Send(new InventoryChangedMessage(__instance.Wrap()));

        [HarmonyPrefix]
        [HarmonyPatch("AddItem", typeof(ItemInstance), typeof(bool))]
        private static void AddItem(Inventory __instance, ItemInstance itemInstance, bool dropIfFull = true)
            => Messenger.Default.Send(new InventoryChangedMessage(__instance.Wrap()));

        [HarmonyPrefix]
        [HarmonyPatch("MoveItem")]
        private static void MoveItem(Inventory __instance, Slot slot, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (slot == null || slot.IsEmpty || __instance.secondInventory == null) return; //if items are moved within the player inventory, ignore.

            Slot movedToSlot = Traverse.Create<Inventory>().Field("toSlot").GetValue<Slot>();
            Inventory movedTo = movedToSlot != null ? Traverse.Create(movedToSlot).Field("inventory").GetValue<Inventory>() : null;
            if (movedTo == null || movedTo == __instance) return; //if items are moved within the same inventory, ignore.
            Messenger.Default.Send(new InventoryChangedMessage(__instance.Wrap()));
            Messenger.Default.Send(new InventoryChangedMessage(movedTo.Wrap()));
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetSlotsFromRGD")]
        private static void SetSlotsFromRGD(Inventory __instance, RGD_Slot[] slots) 
            => Messenger.Default.Send(new InventoryDirtyMessage(__instance.Wrap()));
    }
}
