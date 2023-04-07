using AutoSorter.Messaging;
using AutoSorter.Wrappers;
using HarmonyLib;

namespace AutoSorter.Patches
{
    [HarmonyPatch(typeof(BlockCreator), "CreateBlock")]
    internal class CHarmonyPatch_BlockCreator_CreateBlock
    {
        //Intercept create block method so we can check each created block and register it if it is a storage
        [HarmonyPostfix]
        private static void CreateBlock(BlockCreator __instance, Block __result)
            => Messenger.Default.Send(new CreateBlockMessage((__result as Storage_Small)?.Wrap()));
    }
}
