namespace AutoSorter.Wrappers
{
    internal class CNetworkPlayerWrapper : INetworkPlayer
    {
        public IInventory Inventory => mi_player.Inventory?.Wrap();

        public IRaftStorageManager StorageManager => mi_player.StorageManager?.Wrap();

        public bool IsLocalPlayer => mi_player.IsLocalPlayer;

        private readonly Network_Player mi_player;

        public CNetworkPlayerWrapper(Network_Player _player) => mi_player = _player;
    }
}
