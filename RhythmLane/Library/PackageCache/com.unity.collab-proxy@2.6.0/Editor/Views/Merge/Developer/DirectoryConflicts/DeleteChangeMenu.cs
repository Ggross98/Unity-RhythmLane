using UnityEditor;
using UnityEngine;

using PlasticGui;
using PlasticGui.WorkspaceWindow.Merge;
using Codice.CM.Common.Merge;

namespace Unity.PlasticSCM.Editor.Views.Merge.Developer.DirectoryConflicts
{
    internal class DeleteChangeMenu : MergeViewDirectoryConflictMenu.IDirectoryConflictMenu
    {
        internal DeleteChangeMenu(
            IMergeViewMenuOperations mergeViewMenuOperations)
        {
            mMergeViewMenuOperations = mergeViewMenuOperations;

            BuildComponents();
        }

        GenericMenu MergeViewDirectoryConflictMenu.IDirectoryConflictMenu.Menu
        {
            get { return mMenu; }
        }

        void MergeViewDirectoryConflictMenu.IDirectoryConflictMenu.Popup()
        {
            mMenu = new GenericMenu();

            UpdateMenuItems(mMenu);

            mMenu.ShowAsContext();
        }

        void ViewDestination_Click()
        {
            mMergeViewMenuOperations.OpenDstRevision();
        }

        void ViewSource_Click()
        {
            mMergeViewMenuOperations.OpenSrcRevision();
        }

        void DiffDestinationPrevious_Click()
        {
            mMergeViewMenuOperations.DiffDestinationWithAncestor();
        }

        void UpdateMenuItems(GenericMenu menu)
        {
            SelectedMergeChangesGroupInfo info =
                mMergeViewMenuOperations.GetSelectedMergeChangesGroupInfo();

            bool isDeleteAdded = IsDeletedAdded(info.SelectedConflict.DirectoryConflict);

            if (!isDeleteAdded)
                menu.AddItem(mDiffDestinationPreviousMenuItemContent, false, DiffDestinationPrevious_Click);

            menu.AddItem(mViewSourceAddedMenuItemContent, false, ViewSource_Click);

            mViewDestinationMenuItemContent.text = isDeleteAdded ?
                PlasticLocalization.Name.DeleteChangeConflictViewDestinationAdded.GetString() :
                PlasticLocalization.Name.DeleteChangeConflictViewDestinationChanged.GetString();
            menu.AddItem(mViewDestinationMenuItemContent, false, ViewDestination_Click);
        }

        static bool IsDeletedAdded(DirectoryConflict conflict)
        {
            return ((ChangeDeleteConflict)conflict).Dst.Status == Difference.DiffNodeStatus.Added;

            // otherwise is delete - modified
        }

        void BuildComponents()
        {
            mDiffDestinationPreviousMenuItemContent = new GUIContent(
                 PlasticLocalization.Name.DiffSourceWithDestinationDeleteChange.GetString());

            mViewSourceAddedMenuItemContent = new GUIContent(
                PlasticLocalization.Name.DeleteChangeConflictViewSource.GetString());

            mViewDestinationMenuItemContent = new GUIContent();
        }

        GenericMenu mMenu;

        GUIContent mDiffDestinationPreviousMenuItemContent;
        GUIContent mViewSourceAddedMenuItemContent;
        GUIContent mViewDestinationMenuItemContent;

        readonly IMergeViewMenuOperations mMergeViewMenuOperations;
    }
}
