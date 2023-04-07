using HarmonyLib;
using AutoSorter.Messaging;
using AutoSorter.Wrappers;

namespace AutoSorter.Patches
{
    [HarmonyPatch(typeof(Storage_Small))]
    internal class CHarmonyPatch_Storage_Small
    {
        [HarmonyPostfix]
        [HarmonyPatch("Open")]
        private static void Open(Storage_Small __instance, Network_Player player)
            => Messenger.Default.Send(new OpenStorageMessage(__instance.Wrap(), player.Wrap()));

        [HarmonyPostfix]
        [HarmonyPatch("Close")]
        private static void Close(Storage_Small __instance, Network_Player player)
        {
            Messenger.Default.Send(new CloseStorageMessage(__instance.Wrap(), player.Wrap()));
        }
    }
}
