using pp.RaftMods.AutoSorter;
using System;

namespace pp.RaftMods.AutoSorter.Tests
{
    internal class Logger : IASLogger
    {
        public string ModName => "TestMod";

        public bool IsDebugOutput { get; set; }

        public void Log(object _message)
        {
            Console.WriteLine($"[{ModName}][INFO] {_message}");
        }

        public void LogD(object _message)
        {
            if(IsDebugOutput)
            {
                Console.WriteLine($"[{ModName}][DEBUG] {_message}");
            }
        }

        public void LogE(object _message)
        {
            Console.WriteLine($"[{ModName}][ERROR] {_message}");
        }

        public void LogW(object _message)
        {
            Console.WriteLine($"[{ModName}][WARNING] {_message}");
        }
    }
}
