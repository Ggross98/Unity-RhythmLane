using System.Collections.Generic;
using System.Linq;

using Codice.CM.Common;
using Codice.CM.WorkspaceServer;
using Unity.PlasticSCM.Editor.AssetUtils;

namespace Unity.PlasticSCM.Editor
{
    internal static class ProjectPackages
    {
        internal static bool ShouldBeResolved(
            List<string> updatedItems, WorkspaceInfo wkInfo, bool isGluonMode)
        {
            // We cannot obtain the updated items from a dynamic workspace, so for the moment,
            // we'll force the Packages reimport for these kind of workspaces.
            if (IsDynamicWorkspace.Check(wkInfo))
                return true;

            if (!isGluonMode)
                updatedItems = updatedItems.Select(GetPathFromDeveloperUpdateReport).ToList();

            return updatedItems.Any(ShouldPathBeResolved);
        }

        static bool ShouldPathBeResolved(string path)
        {
            return AssetsPath.IsPackagesRootElement(path) || AssetsPath.IsScript(path);
        }

        static string GetPathFromDeveloperUpdateReport(string item)
        {
            if (string.IsNullOrEmpty(item))
                return string.Empty;

            // For full workspaces we expect to receive the updated items with format <{UPDATE_TYPE}:{ITEM_PATH}>
            if (!item.StartsWith("<") || !item.EndsWith(">"))
                return string.Empty;

            int startIndex = item.IndexOf(":") + 1;

            if (startIndex == 0)
                return string.Empty;

            int endIndex = item.Length - 1;

            return item.Substring(startIndex, endIndex - startIndex);
        }
    }
}
