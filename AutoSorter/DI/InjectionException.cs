using System;

namespace AutoSorter.DI
{
    public class InjectionException : Exception
    {
        public InjectionException(string _message) : base(_message) { }
    }
}
