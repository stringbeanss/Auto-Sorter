using pp.RaftMods.AutoSorter;
using System.Collections;
using UnityEngine;

namespace AutoSorter.Wrappers
{
    public interface IAutoSorter
    {
        void ReimburseConstructionCosts(IItemManager _itemManager, INetworkPlayer _player, bool _applyDowngradeMultiplier);
        void Load(AssetBundle _assetBundle);
        void Destroy();
    }
}
