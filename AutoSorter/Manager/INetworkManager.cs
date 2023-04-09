using AutoSorter.Messaging;
using AutoSorter.Wrappers;
using HarmonyLib;
using Newtonsoft.Json;
using pp.RaftMods.AutoSorter;
using pp.RaftMods.AutoSorter.Protocol;
using SocketIO;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

namespace AutoSorter.Manager
{
    public interface IASNetwork 
    {
        void Clear();

        void UnregisterNetworkBehaviour(CStorageBehaviour _behaviour);

        void SendTo(CDTO _object, CSteamID _id);

        void SendToHost(CDTO _object);

        void Broadcast(CDTO _object);

        void BroadcastInventoryState(ISorterBehaviour _storageBehaviour);

        void RegisterNetworkBehaviour(CStorageBehaviour _behaviour);
    }
}
