using AutoSorter.Manager;
using System.Collections;
using UnityEngine;

namespace pp.RaftMods.AutoSorter
{
    public class CModUI
    {
        private const string UI_CANVAS_ROOT = "Canvases/_CanvasGame_New";
        private const string UI_CONFIG_DIALOG_PREFAB_PATH = "Assets/Prefabs/UI/AutoSorter_UIRoot.prefab";
        private const string UI_CONFIG_DIALOG_ITEM_PREFAB_PATH = "Assets/Prefabs/UI/Windows/Config/Item_Entry.prefab";
        private const string UI_HELP_TEXT_PATH = "Assets/Config/help.txt";

        /// <summary>
        /// The help text displayed when first opening the auto-sorter UI or whenever the help button is clicked.
        /// Is loaded from the mods asset bundle.
        /// </summary>
        public string HelpText { get; private set; }

        /// <summary>
        /// Dialog reference which can be used to prompt the user before specific actions.
        /// </summary>
        public CUIDialog Dialog { get; private set; }

        private Coroutine mi_deferConfigUIRoutineHandle;

        private GameObject mi_uiRoot;
        private CUISorterConfigDialog mi_configDialog;

        private readonly IASLogger mi_logger;
        private readonly ICoroutineHandler mi_coroutineHandler;
        private readonly ISoundManager mi_soundManager;

        public CModUI(IASLogger _logger, ICoroutineHandler _coroutineHandler, ISoundManager _soundManager)
        {
            mi_logger = _logger;
            mi_coroutineHandler = _coroutineHandler;
            mi_soundManager = _soundManager;
        }

        public void LoadUI(AssetBundle _bundle)
        {
            mi_logger.LogD("Loading auto-sorter UI...");
            GameObject rootAsset = _bundle.LoadAsset<GameObject>(UI_CONFIG_DIALOG_PREFAB_PATH);
            if (!rootAsset)
            {
                mi_logger.LogE("Failed to load UI root asset from bundle.");
                return;
            }
            GameObject itemAsset = _bundle.LoadAsset<GameObject>(UI_CONFIG_DIALOG_ITEM_PREFAB_PATH);
            if (!rootAsset)
            {
                mi_logger.LogE("Failed to load UI item asset from bundle.");
                return;
            }

            var canvasRoot = GameObject.Find(UI_CANVAS_ROOT);
            if (!canvasRoot)
            {
                mi_logger.LogE("Failed to load rafts UI canvases anchor. The mod might be out of date.");
                return;
            }

            HelpText = _bundle.LoadAsset<TextAsset>(UI_HELP_TEXT_PATH)?.text?.Trim();
            if (string.IsNullOrEmpty(HelpText))
            {
                mi_logger.LogW("Help text file could not be read.");
                HelpText = "Failed to load!";
            }

            mi_uiRoot = GameObject.Instantiate(rootAsset, canvasRoot.transform, false);
            mi_uiRoot.transform.SetAsLastSibling();
            foreach (var t in mi_uiRoot.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = LayerMask.NameToLayer("UI");
            }

            Transform configDialogRoot = mi_uiRoot.transform.Find("ConfigDialog");
            mi_configDialog = configDialogRoot.gameObject.AddComponent<CUISorterConfigDialog>();

            CASMod.DIContainer.Call(mi_configDialog, nameof(CUISorterConfigDialog.Load));

            Transform dialogRoot = mi_uiRoot.transform.Find("Dialog");
            Dialog = dialogRoot.gameObject.AddComponent<CUIDialog>();
            mi_logger.LogD("Auto-sorter UI loaded!");
        }
        
        public void Destroy()
        {
            if (mi_uiRoot)
            {
                GameObject.DestroyImmediate(mi_uiRoot);
            }
            mi_uiRoot = null;
        }

        public void HideConfigDialog()
        {
            mi_configDialog?.Hide();
        }

        public void ShowConfigDialog(ISceneStorage _storage)
        {
            if (mi_configDialog == null) //If the UI is not fully loaded directly after entering the scene, make sure we wait until its available before showing it to the user.
            {
                if (mi_deferConfigUIRoutineHandle != null)
                {
                    mi_coroutineHandler.StopCoroutine(mi_deferConfigUIRoutineHandle);
                }
                mi_deferConfigUIRoutineHandle = mi_coroutineHandler.StartCoroutine(WaitAndShowUI(_storage));
                return;
            }

            mi_configDialog.Show(_storage);
        }

        public IEnumerator WaitAndShowUI(ISceneStorage _storage)
        {
            while (mi_configDialog == null) yield return new WaitForEndOfFrame();
            if (!_storage.StorageComponent.IsOpen) yield break;
            mi_configDialog.Show(_storage);
        }
    }
}