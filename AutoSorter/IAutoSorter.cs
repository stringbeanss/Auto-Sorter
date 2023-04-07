using pp.RaftMods.AutoSorter;

namespace AutoSorter.Wrappers
{
    public interface IAutoSorter
    {
        void ReimburseConstructionCosts(IItemManager _itemManager, INetworkPlayer _player, bool _applyDowngradeMultiplier);
    }
}
