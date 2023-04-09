using AutoSorter.Messaging;
using HarmonyLib;
using Newtonsoft.Json;
using pp.RaftMods.AutoSorter.Protocol;
using Steamworks;
using System.Collections.Generic;
using System.Linq;

namespace AutoSorter.Patches
{
    [HarmonyPatch(typeof(NetworkUpdateManager), "Deserialize")]
    internal class CHarmonyPatch_NetworkUpdateManager_Deserialize
    {
        [HarmonyPrefix]
        private static bool Deserialize(Packet_Multiple packet, CSteamID remoteID)
         => Messenger.Default.Send<NetworkPackageReceivedMessage, bool>(new NetworkPackageReceivedMessage(packet, remoteID));
    }
}
