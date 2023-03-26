using HarmonyLib;
using Newtonsoft.Json;
using pp.RaftMods.AutoSorter;
using pp.RaftMods.AutoSorter.Protocol;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AutoSorter.Manager
{
    public class CNetwork
    {
        private static Dictionary<uint, CStorageBehaviour> mi_registeredNetworkBehaviours = new Dictionary<uint, CStorageBehaviour>();
        private static Raft_Network mi_network = ComponentManager<Raft_Network>.Value;

        private static short mi_modMessagesFloor = short.MaxValue;
        private static short mi_modMessagesCeil = short.MinValue;

        static CNetwork()
        {
            foreach (EStorageRequestType m in System.Enum.GetValues(typeof(EStorageRequestType)))
            {
                mi_modMessagesFloor = (short)Mathf.Min(mi_modMessagesFloor, (short)m);
                mi_modMessagesCeil = (short)Mathf.Max(mi_modMessagesCeil, (short)m);
            }
        }

        public static void Clear()
        {
            mi_registeredNetworkBehaviours.Clear();
        }

        public static void UnregisterNetworkBehaviour(CStorageBehaviour _behaviour)
        {
            mi_registeredNetworkBehaviours.Remove(_behaviour.ObjectIndex);
        }

        public static void SendTo(CDTO _object, CSteamID _id)
        {
            CUtil.LogD("Sending " + _object.Type + " to " + _id.m_SteamID + ".");
            mi_network.SendP2P(_id, CreateCarrierDTO(_object), EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
        }

        public static void SendToHost(CDTO _object) => SendTo(_object, mi_network.HostID);

        public static void Broadcast(CDTO _object)
        {
            CUtil.LogD("Broadcasting " + _object.Type + " to others.");
            mi_network.RPC(CreateCarrierDTO(_object), Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
        }

        public static void BroadcastInventoryState(CStorageBehaviour _storageBehaviour)
        {
            CUtil.LogD("Broadcasting storage inventory change to others.");
            mi_network.RPC(new Message_Storage_Close((Messages)EStorageRequestType.STORAGE_INVENTORY_UPDATE, _storageBehaviour.LocalPlayer.StorageManager, _storageBehaviour.SceneStorage.StorageComponent), Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
        }

        public static void RegisterNetworkBehaviour(CStorageBehaviour _behaviour)
        {
            if (mi_registeredNetworkBehaviours.ContainsKey(_behaviour.ObjectIndex))
            {
                CUtil.LogW("Behaviour with ID" + _behaviour.ObjectIndex + " \"" + _behaviour.name + "\" was already registered.");
                return;
            }

            mi_registeredNetworkBehaviours.Add(_behaviour.ObjectIndex, _behaviour);
        }

        private static Message CreateCarrierDTO(CDTO _object)
        {
            if (_object.Info != null)
            {
                _object.Info.OnBeforeSerialize();
            }
            return new Message_InitiateConnection(
                                    (Messages)_object.Type,
                                    0,
                                    JsonConvert.SerializeObject(_object));
        }

        [HarmonyPatch(typeof(NetworkUpdateManager), "Deserialize")]
        private class CHarmonyPatch_NetworkUpdateManager_Deserialize
        {
            [HarmonyPrefix]
            private static bool Deserialize(Packet_Multiple packet, CSteamID remoteID)
            {
                List<Message> resultMessages = packet.messages.ToList();
                List<Message> messages = packet.messages.ToList();

                foreach (Message package in messages)
                {
                    if (package.t > mi_modMessagesCeil || package.t < mi_modMessagesFloor)
                    {
                        continue; //this is a message type not from this mod, ignore this package.
                    }

                    var inventoryUpdate = package as Message_Storage_Close;
                    var msg = package as Message_InitiateConnection;
                    if (msg == null && inventoryUpdate == null)
                    {
                        CUtil.LogW("Invalid auto-sorter mod message received. Make sure all connected players use the same mod version.");
                        continue;
                    }

                    resultMessages.Remove(package);

                    try
                    {
                        if (inventoryUpdate != null)
                        {
                            if (!mi_registeredNetworkBehaviours.ContainsKey(inventoryUpdate.storageObjectIndex))
                            {
                                CUtil.LogW("No receiver with ID " + inventoryUpdate.storageObjectIndex + " found.");
                                continue;
                            }
                            mi_registeredNetworkBehaviours[inventoryUpdate.storageObjectIndex].OnInventoryUpdateReceived(inventoryUpdate);
                            continue;
                        }

                        CDTO modMessage = JsonConvert.DeserializeObject<CDTO>(msg.password);
                        if (modMessage == null)
                        {
                            CUtil.LogW("Invalid network message received. Update the AutoSorter mod or make sure all connected players use the same version.");
                            continue;
                        }

                        if (!mi_registeredNetworkBehaviours.ContainsKey(modMessage.ObjectIndex))
                        {
                            CUtil.LogW("No receiver with ID " + modMessage.ObjectIndex + " found.");
                            continue;
                        }

                        if (modMessage.Info != null)
                        {
                            modMessage.Info.OnAfterDeserialize();
                        }

                        CUtil.LogD($"Received {modMessage.Type}({package.t}) message from \"{remoteID}\".");
                        mi_registeredNetworkBehaviours[modMessage.ObjectIndex].OnNetworkMessageReceived(modMessage, remoteID);
                    }
                    catch (System.Exception _e)
                    {
                        CUtil.LogW($"Failed to read mod network message ({package.Type}) as {(Raft_Network.IsHost ? "host" : "client")}. You or one of your fellow players might have to update the mod.");
                        CUtil.LogD(_e.Message);
                        CUtil.LogD(_e.StackTrace);
                    }
                }

                if (resultMessages.Count == 0) return false; //no packages left, nothing todo. Dont even call the vanilla method

                //we remove all custom messages from the provided package and reassign the modified list so it is passed to the vanilla method.
                //this is to make sure we dont lose any vanilla packages
                packet.messages = resultMessages.ToArray();
                return true; //nothing for the mod left to do here, let the vanilla behaviour take over
            }
        }
    }
}
