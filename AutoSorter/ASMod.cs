using System.Collections;
using System.Reflection;
using AutoSorter.DI;
using AutoSorter.Manager;
using AutoSorter.Wrappers;
using HarmonyLib;
using HMLLibrary;
using UnityEngine;

namespace pp.RaftMods.AutoSorter
{
    public class CASMod : Mod, ICoroutineHandler
    {
        /// <summary>
        /// Explicitly typed mod instance for static access.
        /// </summary>
        private static CASMod Get = null;

        public readonly static Dependencies DIContainer = new Dependencies();

        private const string ASSET_PAYLOAD_PATH = "assets/autosorter";

        private Harmony mi_harmony;
        private IAutoSorter mi_autoSorter;

        private AssetBundle mi_bundle;

        private IEnumerator Start()
        {
            if (Get)
            {
                DestroyImmediate(Get);
                Debug.LogWarning("Autosorter mod has been loaded twice. Destroying old mod instance.");
            }
            Get = this;

            LoadBinds();

            mi_harmony = new Harmony(CAutoSorter.MOD_NAMESPACE);
            mi_harmony.PatchAll(Assembly.GetExecutingAssembly());

            yield return LoadModBundle();

            if (!mi_bundle)
            {
                Debug.LogError($"Failed to load {CAutoSorter.MOD_NAME} embedded bundle. This usually happens if you load two instances of the mod.");
                DestroyImmediate(Get);
                yield break;
            }

            mi_autoSorter = DIContainer.Resolve<IAutoSorter>();
            mi_autoSorter.Load(mi_bundle);
        }

        private void OnDestroy()
        {
            if (mi_harmony != null)
            {
                mi_harmony.UnpatchAll(CAutoSorter.MOD_NAMESPACE);
            }

            if (mi_bundle)
            {
                mi_bundle.Unload(true);
            }
            mi_bundle = null;

            mi_autoSorter.Destroy();
        }

        private void LoadBinds()
        {
            //ENGINE
            DIContainer.Bind<ICoroutineHandler>().ToConstant(this);
            DIContainer.Bind<IASLogger>().ToConstant(LoggerFactory.Default.GetLogger());

            //MOD
            DIContainer.Bind<IAutoSorter, CAutoSorter>().AsSingleton();
            DIContainer.Bind<CConfigManager>().AsSingleton();
            DIContainer.Bind<IStorageDataManager, CStorageDataManager>().AsSingleton();
            DIContainer.Bind<IStorageManager, CStorageManager>().AsSingleton();

            //RAFT
            DIContainer.Bind<IItemManager, CItemManagerWrapper>().AsSingleton();
            DIContainer.Bind<IRaftNetwork>().ToConstant(ComponentManager<Raft_Network>.Value.Wrap());
            DIContainer.Bind<ISoundManager>().ToConstant(ComponentManager<SoundManager>.Value.Wrap());
        }

        private IEnumerator LoadModBundle()
        {
            if (mi_bundle == null)
            {
                AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(GetEmbeddedFileBytes(ASSET_PAYLOAD_PATH));
                yield return request;
                mi_bundle = request.assetBundle;
            }
        }
    }
}