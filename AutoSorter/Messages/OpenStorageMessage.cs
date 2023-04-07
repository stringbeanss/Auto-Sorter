using AutoSorter.Wrappers;

namespace AutoSorter.Messaging
{
    public class OpenStorageMessage : IMessage
    {
        public IStorageSmall Storage { get; private set; }
        public INetworkPlayer Player { get; private set; }

        public OpenStorageMessage(IStorageSmall _storage, INetworkPlayer _player) => (Storage, Player) = (_storage, _player);
    }
}
