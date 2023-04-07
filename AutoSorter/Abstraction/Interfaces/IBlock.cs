using System.Collections.Generic;

namespace AutoSorter.Wrappers
{
    public interface IBlock
    {
        uint ObjectIndex { get; }
        string ObjectName { get; }
        void SetNetworkIdBehaviour(ISorterBehaviour _behaviour);
        T AddComponent<T>() where T : class;
        IEnumerable<T> GetComponentsInChildren<T>(bool _includeChildren) where T : class;
    }
}
