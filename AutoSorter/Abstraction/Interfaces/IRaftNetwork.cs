using Steamworks;

namespace AutoSorter.Wrappers
{
    public interface IRaftNetwork
    {
        INetworkPlayer GetLocalPlayer();
        CSteamID HostID { get; }
        bool IsHost { get; }
    }
}
