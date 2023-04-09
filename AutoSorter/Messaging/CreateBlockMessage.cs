using AutoSorter.Wrappers;

namespace AutoSorter.Messaging
{
    public class CreateBlockMessage : IMessage
    {
        public IStorageSmall Storage { get; private set; }

        public CreateBlockMessage(IStorageSmall _storage) => Storage = _storage;
    }
}
