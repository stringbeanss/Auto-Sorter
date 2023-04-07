using AutoSorter.Messaging;
using AutoSorter.Wrappers;
using HarmonyLib;
using pp.RaftMods.AutoSorter;

namespace AutoSorter.Patches
{
    [HarmonyPatch(typeof(RemovePlaceables), "PickupBlock")]
    internal class CHarmonyPatch_RemovePlaceables_PickupBlock
    {
        [HarmonyPrefix]
        private static void PickupBlock(RemovePlaceables __instance, Block block)
            => Messenger.Default.Send(new PickupBlockMessage(__instance.Wrap(), block.Wrap()));
    }
}
