using Steamworks;

namespace AutoSorter.Wrappers
{
    public interface IRaftNetwork
    {
        INetworkPlayer GetLocalPlayer();
        CSteamID HostID { get; }
        bool IsHost { get; }

        void SendP2P(CSteamID _steamID, Message _message, EP2PSend _sendMode, NetworkChannel _channel);
        void RPC(Message _message, Target _target, EP2PSend _sendMode, NetworkChannel _channel);
    }
}
