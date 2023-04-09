using Steamworks;

namespace AutoSorter.Wrappers
{
    internal class CRaftNetworkWrapper : IRaftNetwork
    {
        private readonly Raft_Network mi_network;

        public CRaftNetworkWrapper(Raft_Network _network) => mi_network = _network;

        public INetworkPlayer GetLocalPlayer() => mi_network.GetLocalPlayer().Wrap();

        public void SendP2P(CSteamID _steamID, Message _message, EP2PSend _sendMode, NetworkChannel _channel)
            => mi_network.SendP2P(_steamID, _message, _sendMode, _channel);

        public void RPC(Message _message, Target _target, EP2PSend _sendMode, NetworkChannel _channel)
            => mi_network.RPC(_message, _target, _sendMode, _channel);

        public CSteamID HostID => mi_network.HostID;

        public bool IsHost => Raft_Network.IsHost;
    }
}
