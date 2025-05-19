using System.Collections.Generic;

using PlasticGui.WorkspaceWindow.Merge;

namespace Unity.PlasticSCM.Editor.Views.Merge.Developer
{
    internal static class MergeSelection
    {
        internal static List<string> GetPathsFromSelectedFileConflictsIncludingMeta(
            MergeTreeView treeView)
        {
            List<string> result = new List<string>();

            List<MergeChangeInfo> selection =
                treeView.GetSelectedFileConflicts();

            treeView.FillWithMeta(selection);

            foreach (MergeChangeInfo mergeChange in selection)
            {
                result.Add(mergeChange.GetPath());
            }

            return result;
        }

        internal static SelectedMergeChangesGroupInfo GetSelectedGroupInfo(
            MergeTreeView treeView, bool isIncomingMerge)
        {
            List<MergeChangeInfo> selectedMergeChanges =
                treeView.GetSelectedMergeChanges();

            return GetSelectedMergeChangesGroupInfo.For(
                selectedMergeChanges, isIncomingMerge);
        }

        internal static MergeChangeInfo GetSingleSelectedMergeChange(
            MergeTreeView treeView)
        {
            return treeView.GetSelectedMergeChange();
        }
    }
}
