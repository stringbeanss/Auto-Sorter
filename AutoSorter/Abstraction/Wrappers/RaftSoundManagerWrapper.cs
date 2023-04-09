using AutoSorter.Manager;

namespace AutoSorter.Wrappers
{
    internal class CRaftSoundManagerWrapper : ISoundManager
    {
        private SoundManager mi_soundManager;

        public CRaftSoundManagerWrapper(SoundManager _soundManager)
        {
            mi_soundManager = _soundManager;
        }

        public void PlayUI_Click() => mi_soundManager.PlayUI_Click();

        public void PlayUI_Click_Fail() => mi_soundManager.PlayUI_Click_Fail();

        public void PlayUI_OpenMenu() => mi_soundManager.PlayUI_OpenMenu();
    }
}
