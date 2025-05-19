using UnityEditor;
using UnityEditor.PackageManager;
using Unity.PlasticSCM.Editor.UI;

using AssetPostprocessor = Unity.PlasticSCM.Editor.AssetUtils.Processor.AssetPostprocessor;

namespace Unity.PlasticSCM.Editor.AssetUtils
{
    internal static class RefreshAsset
    {
        internal static void BeforeLongAssetOperation()
        {
            AssetDatabase.DisallowAutoRefresh();
        }

        internal static void AfterLongAssetOperation()
        {
            AfterLongAssetOperation(true);
        }

        internal static void AfterLongAssetOperation(bool isPackagesReimportNeeded)
        {
            AssetDatabase.AllowAutoRefresh();

            if (isPackagesReimportNeeded)
            {
                UnityAssetDatabaseAndPackageManagerAsync();
            }
            else
            {
                RefreshUnityAssetDatabase();
            }
        }

        internal static void UnityAssetDatabase()
        {
            RefreshUnityAssetDatabase();
        }

        internal static void UnityAssetDatabaseAndPackageManagerAsync()
        {
            // Client.Resolve() will resolve any pending packages added or removed from the project
            // VCS-1004718 - This is important so the domain gets reloaded first if needed
            Client.Resolve();

            mCooldownRefreshAssetsAction.Ping();
        }

        internal static void VersionControlCache()
        {
            ClearVersionControlCaches();

            ProjectWindow.Repaint();
            RepaintInspector.All();
        }

        static void ClearVersionControlCaches()
        {
            UnityEditor.VersionControl.Provider.ClearCache();

            if (PlasticPlugin.AssetStatusCache != null)
                PlasticPlugin.AssetStatusCache.Clear();
        }

        static void RefreshUnityAssetDatabase()
        {
            AssetDatabase.Refresh(ImportAssetOptions.Default);

            ClearVersionControlCaches();

            AssetPostprocessor.SetIsRepaintNeededAfterAssetDatabaseRefresh();
        }

        static CooldownWindowDelayer mCooldownRefreshAssetsAction = new CooldownWindowDelayer(
            RefreshUnityAssetDatabase,
            UnityConstants.REFRESH_ASSET_DATABASE_DELAYED_INTERVAL);
    }
}
