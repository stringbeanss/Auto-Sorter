using AutoSorter.Wrappers;

namespace AutoSorter.Messaging
{
    public class PickupBlockMessage : IMessage
    {
        public IRemovePlaceable Placeable { get; private set; }
        public IBlock Block { get; private set; }

        public PickupBlockMessage(IRemovePlaceable _placeable, IBlock _block) => (Placeable, Block) = (_placeable, _block);
    }
}
