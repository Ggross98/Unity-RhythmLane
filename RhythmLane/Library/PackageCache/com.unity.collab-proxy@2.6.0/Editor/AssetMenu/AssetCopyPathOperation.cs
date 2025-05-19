using System.Collections.Generic;

using UnityEditor;

using PlasticGui.WorkspaceWindow;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.AssetUtils;

namespace Unity.PlasticSCM.Editor.AssetMenu
{
    internal class AssetCopyPathOperation : IAssetMenuCopyPathOperation
    {
        internal AssetCopyPathOperation(
            string workspacePath,
            IAssetStatusCache assetStatusCache,
            AssetVcsOperations.IAssetSelection assetSelection)
        {
            mWorkspacePath = workspacePath;
            mAssetStatusCache = assetStatusCache;
            mAssetSelection = assetSelection;
        }

        void IAssetMenuCopyPathOperation.CopyFilePath(bool relativePath)
        {
            List<string> selectedPaths = GetSelectedPaths.ForOperation(
                mWorkspacePath,
                mAssetSelection.GetSelectedAssets(),
                mAssetStatusCache,
                AssetMenuOperations.CopyFilePath,
                includeMetaFiles: false);

            EditorGUIUtility.systemCopyBuffer = GetFilePathList.FromSelectedPaths(
                selectedPaths,
                relativePath,
                mWorkspacePath);
        }

        readonly string mWorkspacePath;
        readonly IAssetStatusCache mAssetStatusCache;
        readonly AssetVcsOperations.IAssetSelection mAssetSelection;
    }
}
