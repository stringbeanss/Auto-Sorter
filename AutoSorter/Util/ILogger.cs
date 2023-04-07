using System;

namespace pp.RaftMods.AutoSorter
{
    public interface IASLogger //util
    {
        string ModName { get; }
        bool IsDebugOutput { get; set; }
        void Log(object _message);
        void LogW(object _message);
        void LogE(object _message);
        void LogD(object _message);
    }

    public class LoggerFactory
    {
        public static LoggerFactory Default = new LoggerFactory();

        public bool Debug { get; set; }

        public virtual IASLogger GetLogger()
        {
            return new Logger()
            {
                IsDebugOutput = Debug
            };
        }
    }
}