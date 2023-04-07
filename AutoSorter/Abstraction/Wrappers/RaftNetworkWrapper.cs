using Steamworks;

namespace AutoSorter.Wrappers
{
    internal class CRaftNetworkWrapper : IRaftNetwork
    {
        private readonly Raft_Network mi_network;

        public CRaftNetworkWrapper(Raft_Network _network) => mi_network = _network;

        public INetworkPlayer GetLocalPlayer() => mi_network.GetLocalPlayer().Wrap();

        public CSteamID HostID => mi_network.HostID;

        public bool IsHost => Raft_Network.IsHost;
    }
}
