using Steamworks;

namespace AutoSorter.Messaging
{
    public class NetworkPackageReceivedMessage : ResultMessage<bool>
    {
        public Packet_Multiple Packet { get; private set; }
        public CSteamID RemoteID { get; private set; }

        public NetworkPackageReceivedMessage(Packet_Multiple _packet, CSteamID _steamID) => (Packet, RemoteID) = (_packet, _steamID);
    }
}
