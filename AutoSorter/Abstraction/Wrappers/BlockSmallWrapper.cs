using pp.RaftMods.AutoSorter;
using System.Collections.Generic;

namespace AutoSorter.Wrappers
{
    internal class CBlockWrapper : IBlock
    {
        private readonly Block mi_block;

        public CBlockWrapper(Block _storage)
        {
            mi_block = _storage;
        }

        public uint ObjectIndex => mi_block.ObjectIndex;

        public string ObjectName => mi_block.name;

        public T AddComponent<T>() where T : class => mi_block.gameObject.AddComponent(typeof(T)) as T;

        public IEnumerable<T> GetComponentsInChildren<T>(bool _includeChildren) where T : class
            => mi_block.gameObject.GetComponentsInChildren(typeof(T), _includeChildren) as IEnumerable<T>;

        public void SetNetworkIdBehaviour(ISorterBehaviour _behaviour)
            => mi_block.networkedIDBehaviour = (CStorageBehaviour) _behaviour;
    }
}
