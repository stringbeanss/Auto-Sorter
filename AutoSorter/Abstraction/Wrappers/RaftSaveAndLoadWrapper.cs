namespace AutoSorter.Wrappers
{
    internal class CRaftSaveAndLoadWrapper : ISaveAndLoad
    {
        public string CurrentGameFileName => SaveAndLoad.CurrentGameFileName;
    }
}
