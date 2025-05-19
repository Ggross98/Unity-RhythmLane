using UnityEditor;
using UnityEngine;

using PlasticGui;
using PlasticGui.WorkspaceWindow.Merge;

namespace Unity.PlasticSCM.Editor.Views.Merge.Developer.DirectoryConflicts
{
    internal class LoadedTwiceMenu : MergeViewDirectoryConflictMenu.IDirectoryConflictMenu
    {
        internal LoadedTwiceMenu(
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

        void ViewSource_Click()
        {
            mMergeViewMenuOperations.OpenSrcRevision();
        }

        void UpdateMenuItems(GenericMenu menu)
        {
            menu.AddItem(mViewSourceMenuItemContent, false, ViewSource_Click);
        }

        void BuildComponents()
        {
            mViewSourceMenuItemContent = new GUIContent(
                PlasticLocalization.Name.LoadedTwiceViewSource.GetString());
        }

        GenericMenu mMenu;

        GUIContent mViewSourceMenuItemContent;

        readonly IMergeViewMenuOperations mMergeViewMenuOperations;
    }
}
