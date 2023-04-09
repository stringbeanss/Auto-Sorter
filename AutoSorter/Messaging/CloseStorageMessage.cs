using AutoSorter.Wrappers;

namespace AutoSorter.Messaging
{
    public class CloseStorageMessage : IMessage
    {
        public IStorageSmall Storage { get; private set; }
        public INetworkPlayer Player { get; private set; }

        public CloseStorageMessage(IStorageSmall _storage, INetworkPlayer _player) => (Storage, Player) = (_storage, _player);
    }
}
