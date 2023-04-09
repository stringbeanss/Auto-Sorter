namespace AutoSorter.DI
{
    public interface IDependencyBinder
    {
        void AsTransient();
        void AsSingleton();
        void ToConstant(object _instance);
    }
}
