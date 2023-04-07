using System;

namespace AutoSorter.IOC
{
    public class InjectionException : Exception
    {
        public InjectionException(string _message) : base(_message) { }
    }
}
