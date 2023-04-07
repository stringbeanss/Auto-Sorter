namespace AutoSorter.Wrappers
{
    internal class CRemovePlaceableWrapper : IRemovePlaceable
    {
        private readonly RemovePlaceables mi_placeable;

        public CRemovePlaceableWrapper(RemovePlaceables _placeable) => mi_placeable = _placeable;

        public RemovePlaceables Unwrap() => mi_placeable;
    }
}
