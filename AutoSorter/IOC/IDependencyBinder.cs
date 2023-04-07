namespace AutoSorter.IOC
{
    public interface IDependencyBinder
    {
        void AsTransient();
        void AsSingleton();
        void ToConstant(object _instance);
    }
}
