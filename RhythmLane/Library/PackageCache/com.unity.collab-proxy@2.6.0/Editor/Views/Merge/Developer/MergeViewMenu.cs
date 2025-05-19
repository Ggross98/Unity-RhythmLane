using UnityEditor;

using PlasticGui.WorkspaceWindow.Merge;
using Unity.PlasticSCM.Editor.Views.Merge.Developer.DirectoryConflicts;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Views.Merge.Developer
{
    internal class MergeViewMenu
    {
        internal GenericMenu Menu
        {
            get
            {
                return mMergeViewFileConflictMenu != null ?
                       mMergeViewFileConflictMenu.Menu : mMergeViewDirectoryConflictMenu != null ?
                           mMergeViewDirectoryConflictMenu.Menu : null;
            }
        }

        internal MergeViewMenu(
            IMergeViewMenuOperations mergeViewMenuOperations,
            MergeViewFileConflictMenu.IMetaMenuOperations mergeMetaMenuOperations,
            bool isIncomingMerge,
            bool isMergeTo)
        {
            mMergeViewMenuOperations = mergeViewMenuOperations;
            mMergeMetaMenuOperations = mergeMetaMenuOperations;
            mIsIncomingMerge = isIncomingMerge;
            mIsMergeTo = isMergeTo;
        }

        internal void Popup()
        {
            SelectedMergeChangesGroupInfo selectedGroupInfo =
                mMergeViewMenuOperations.GetSelectedMergeChangesGroupInfo();

            if (selectedGroupInfo.SelectedConflict == null)
                return;

            if (selectedGroupInfo.IsDirectoryConflictsSelection)
            {
                GetMergeViewDirectoryConflictMenu().Popup();
                return;
            }

            GetMergeViewFileConflictMenu().Popup();
        }

        internal bool ProcessKeyActionIfNeeded(Event e)
        {
            SelectedMergeChangesGroupInfo selectedGroupInfo =
                mMergeViewMenuOperations.GetSelectedMergeChangesGroupInfo();

            if (selectedGroupInfo.SelectedConflict == null)
                return false;

            if (selectedGroupInfo.IsDirectoryConflictsSelection)
                return false;

            if (mMergeViewFileConflictMenu == null)
                return false;

            return mMergeViewFileConflictMenu.ProcessKeyActionIfNeeded(e);
        }

        MergeViewFileConflictMenu GetMergeViewFileConflictMenu()
        {
            if (mMergeViewFileConflictMenu == null)
            {
                mMergeViewFileConflictMenu =
                    new MergeViewFileConflictMenu(
                        mMergeViewMenuOperations,
                        mMergeMetaMenuOperations,
                        mIsIncomingMerge,
                        mIsMergeTo);
            }

            return mMergeViewFileConflictMenu;
        }

        MergeViewDirectoryConflictMenu GetMergeViewDirectoryConflictMenu()
        {
            if (mMergeViewDirectoryConflictMenu == null)
            {
                mMergeViewDirectoryConflictMenu =
                    new MergeViewDirectoryConflictMenu(mMergeViewMenuOperations);
            }

            return mMergeViewDirectoryConflictMenu;
        }

        MergeViewFileConflictMenu mMergeViewFileConflictMenu;
        MergeViewDirectoryConflictMenu mMergeViewDirectoryConflictMenu;

        readonly IMergeViewMenuOperations mMergeViewMenuOperations;
        readonly MergeViewFileConflictMenu.IMetaMenuOperations mMergeMetaMenuOperations;
        readonly bool mIsIncomingMerge;
        readonly bool mIsMergeTo;
    }
}
