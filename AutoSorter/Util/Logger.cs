using UnityEngine;

namespace pp.RaftMods.AutoSorter
{
    internal class Logger : IASLogger
    {
        public bool IsDebugOutput { get; set; }
        public string ModName { get => CAutoSorter.MOD_NAME; }
        public string OwnerName { get; set; }

        public void Log(object _message)
        {
            Debug.Log($"[{ModName}][{OwnerName}] {_message}");
        }

        public void LogD(object _message)
        {
            if (IsDebugOutput)
            {
                Debug.Log($"[{ModName}][{OwnerName}][DEBUG] {_message}");
            }
        }

        public void LogE(object _message)
        {
            Debug.LogError($"[{ModName}][{OwnerName}] {_message}");
        }

        public void LogW(object _message)
        {
            Debug.LogWarning($"[{ModName}][{OwnerName}] {_message}");
        }
    }
}
