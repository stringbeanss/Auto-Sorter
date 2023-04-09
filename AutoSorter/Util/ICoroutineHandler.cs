using System.Collections;
using UnityEngine;

namespace pp.RaftMods.AutoSorter
{
    public interface ICoroutineHandler
    {
        Coroutine StartCoroutine(IEnumerator _enumerator);
        void StopCoroutine(Coroutine _coroutine);
        void StopAllCoroutines();
    }
}
