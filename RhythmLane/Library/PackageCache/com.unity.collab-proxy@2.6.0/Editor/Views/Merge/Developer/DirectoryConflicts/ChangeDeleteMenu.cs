using UnityEditor;
using UnityEngine;

using Codice.CM.Common.Merge;
using PlasticGui;
using PlasticGui.WorkspaceWindow.Merge;

namespace Unity.PlasticSCM.Editor.Views.Merge.Developer.DirectoryConflicts
{
    internal class ChangeDeleteMenu : MergeViewDirectoryConflictMenu.IDirectoryConflictMenu
    {
        internal ChangeDeleteMenu(
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

        void DiffSourcePrevious_Click()
        {
            mMergeViewMenuOperations.DiffSourceWithAncestor();
        }

        void UpdateMenuItems(GenericMenu menu)
        {
            SelectedMergeChangesGroupInfo info =
                mMergeViewMenuOperations.GetSelectedMergeChangesGroupInfo();

            bool isAddedDeleted = IsAddedDeleted(info.SelectedConflict.DirectoryConflict);

            if (!isAddedDeleted)
                menu.AddItem(mDiffSourcePreviousMenuItemContent, false, DiffSourcePrevious_Click);

            mViewSourceAddedMenuItemContent.text = isAddedDeleted ?
                PlasticLocalization.Name.ChangeDeleteConflictViewSourceAdded.GetString() :
                PlasticLocalization.Name.ChangeDeleteConflictViewSourceChanged.GetString();
            menu.AddItem(mViewSourceAddedMenuItemContent, false, ViewSource_Click);

            menu.AddItem(mViewDestinationMenuItemContent, false, ViewDestination_Click);
        }

        static bool IsAddedDeleted(DirectoryConflict conflict)
        {
            return ((ChangeDeleteConflict)conflict).Src.Status == Difference.DiffNodeStatus.Added;

            // otherwise is modified - deleted
        }

        void BuildComponents()
        {
            mDiffSourcePreviousMenuItemContent = new GUIContent(
                PlasticLocalization.Name.DiffSourceWithDestinationChangeDelete.GetString());

            mViewSourceAddedMenuItemContent = new GUIContent();

            mViewDestinationMenuItemContent = new GUIContent(
                PlasticLocalization.Name.ChangeDeleteConflictViewDestination.GetString());
        }

        GenericMenu mMenu;

        GUIContent mDiffSourcePreviousMenuItemContent;
        GUIContent mViewSourceAddedMenuItemContent;
        GUIContent mViewDestinationMenuItemContent;

        readonly IMergeViewMenuOperations mMergeViewMenuOperations;
    }
}
