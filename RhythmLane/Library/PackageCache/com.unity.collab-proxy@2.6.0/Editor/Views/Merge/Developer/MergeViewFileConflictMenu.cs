using UnityEditor;
using UnityEngine;

using PlasticGui;
using PlasticGui.WorkspaceWindow.Merge;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.Views.Merge.Developer
{
    internal class MergeViewFileConflictMenu
    {
        internal interface IMetaMenuOperations
        {
            void DiffSourceWithAncestor();
            void DiffDestinationWithAncestor();
            void DiffSourceWithDestination();
            void ShowHistory();
            bool SelectionHasMeta();
        }

        internal GenericMenu Menu { get { return mMenu; } }

        internal MergeViewFileConflictMenu(
            IMergeViewMenuOperations mergeViewMenuOperations,
            IMetaMenuOperations mergeMetaMenuOperations,
            bool isIncomingMerge,
            bool isMergeTo)
        {
            mMergeViewMenuOperations = mergeViewMenuOperations;
            mMergeMetaMenuOperations = mergeMetaMenuOperations;

            BuildComponents(isIncomingMerge, isMergeTo);
        }

        internal void Popup()
        {
            mMenu = new GenericMenu();

            UpdateMenuItems(mMenu);

            mMenu.ShowAsContext();
        }

        internal bool ProcessKeyActionIfNeeded(Event e)
        {
            MergeMenuOperations operationToExecute = GetMenuOperations(e);

            if (operationToExecute == MergeMenuOperations.None)
                return false;

            SelectedMergeChangesGroupInfo info =
                mMergeViewMenuOperations.GetSelectedMergeChangesGroupInfo();
            MergeMenuOperations operations =
                UpdateMergeMenu.GetAvailableMenuOperations(info);

            if (!operations.HasFlag(operationToExecute))
                return false;

            ProcessMenuOperation(operationToExecute, mMergeViewMenuOperations);
            return true;
        }

        void MergeSelectedFilesMenuItem_Click()
        {
            mMergeViewMenuOperations.MergeContributors();
        }

        void MergeKeepingSourceChangesMenuItem_Click()
        {
            mMergeViewMenuOperations.MergeKeepingSourceChanges();
        }

        void MergeKeepingWorkspaceChangesMenuItem_Click()
        {
            mMergeViewMenuOperations.MergeKeepingWorkspaceChanges();
        }

        void DiffSourceWithDestinationMenuItem_Click()
        {
            mMergeViewMenuOperations.DiffSourceWithDestination();
        }

        void DiffDestinationWithAncestorMenuItem_Click()
        {
            mMergeViewMenuOperations.DiffDestinationWithAncestor();
        }

        void DiffSourceWithAncestorMenuItem_Click()
        {
            mMergeViewMenuOperations.DiffSourceWithAncestor();
        }

        void DiffMetaSourceWithDestinationMenuItem_Click()
        {
            mMergeMetaMenuOperations.DiffSourceWithDestination();
        }

        void DiffMetaDestinationWithAncestorMenuItem_Click()
        {
            mMergeMetaMenuOperations.DiffDestinationWithAncestor();
        }

        void DiffMetaSourceWithAncestorMenuItem_Click()
        {
            mMergeMetaMenuOperations.DiffSourceWithAncestor();
        }

        void HistoryMetaMenuItem_Click()
        {
            mMergeMetaMenuOperations.ShowHistory();
        }

        void CopyFilePathMenuItem_Click()
        {
            mMergeViewMenuOperations.CopyFilePath(relativePath: false);
        }

        void CopyRelativeFilePathMenuItem_Click()
        {
            mMergeViewMenuOperations.CopyFilePath(relativePath: true);
        }

        void HistoryMenuItem_Click()
        {
            mMergeViewMenuOperations.ShowHistory();
        }

        void UpdateMenuItems(GenericMenu menu)
        {
            SelectedMergeChangesGroupInfo info =
                mMergeViewMenuOperations.GetSelectedMergeChangesGroupInfo();
            MergeMenuOperations operations =
                UpdateMergeMenu.GetAvailableMenuOperations(info);

            if (operations == MergeMenuOperations.None)
            {
                menu.AddDisabledItem(GetNoActionMenuItemContent());
                return;
            }

            AddMergeActions(menu, operations);

            menu.AddSeparator(string.Empty);

            AddDiffActions(menu, info, operations);

            if (mMergeMetaMenuOperations.SelectionHasMeta())
            {
                menu.AddSeparator(string.Empty);

                AddMetaActions(menu, info, operations);
            }

            menu.AddSeparator(string.Empty);

            AddCopyFilePathActions(menu, operations);

            menu.AddSeparator(string.Empty);

            AddHistoryMenuItem(mHistoryMenuItemContent, menu, operations, HistoryMenuItem_Click);
        }

        void AddMergeActions(
            GenericMenu menu,
            MergeMenuOperations operations)
        {
            AddMergeSelectedFilesMenuItem(
                mMergeSelectedFilesMenuItemContent, menu, operations, MergeSelectedFilesMenuItem_Click);

            AddMergeKeepingSourceChangesMenuItem(
                mMergeKeepingSourceChangesMenuItemContent, menu, operations, MergeKeepingSourceChangesMenuItem_Click);

            AddMergeKeepingWorkspaceChangesMenuItem(
                mMergeKeepingWorkspaceChangesMenuItemContent, menu, operations, MergeKeepingWorkspaceChangesMenuItem_Click);
        }

        void AddDiffActions(
            GenericMenu menu,
            SelectedMergeChangesGroupInfo info,
            MergeMenuOperations operations)
        {
            mDiffSourceWithDestinationMenuItemContent.text =
                UnityMenuItem.EscapedText(info.DiffSourceWithDestinationMenuItemText);
            AddDiffSourceWithDestinationMenuItem(
                mDiffSourceWithDestinationMenuItemContent, menu, operations, DiffSourceWithDestinationMenuItem_Click);

            AddDiffDestinationWithAncestorMenuItem(
                mDiffDestinationWithAncestorMenuItemContent, menu, operations, DiffDestinationWithAncestorMenuItem_Click);

            AddDiffSourceWithAncestorMenuItem(
                mDiffSourceWithAncestorMenuItem, menu, operations, DiffSourceWithAncestorMenuItem_Click);
        }

        void AddMetaActions(
            GenericMenu menu,
            SelectedMergeChangesGroupInfo info,
            MergeMenuOperations operations)
        {
            mDiffMetaSourceWithDestinationMenuItemContent.text = string.Format(
                "{0}/{1}",
                MetaPath.META_EXTENSION,
                UnityMenuItem.EscapedText(info.DiffSourceWithDestinationMenuItemText));
            AddDiffSourceWithDestinationMenuItem(
                mDiffMetaSourceWithDestinationMenuItemContent, menu, operations, DiffMetaSourceWithDestinationMenuItem_Click);

            AddDiffDestinationWithAncestorMenuItem(
                mDiffMetaDestinationWithAncestorMenuItemContent, menu, operations, DiffMetaDestinationWithAncestorMenuItem_Click);

            AddDiffSourceWithAncestorMenuItem(
                mDiffMetaSourceWithAncestorMenuItemContent, menu, operations, DiffMetaSourceWithAncestorMenuItem_Click);

            AddHistoryMenuItem(
                mHistoryMetaMenuItemContent, menu, operations, HistoryMetaMenuItem_Click);
        }

        void AddCopyFilePathActions(
            GenericMenu menu,
            MergeMenuOperations operations)
        {
            AddCopyFilePathMenuItem(
                mCopyFilePathMenuItemContent, menu, operations, CopyFilePathMenuItem_Click);

            AddCopyFilePathMenuItem(
                mCopyRelativeFilePathMenuItemContent, menu, operations, CopyRelativeFilePathMenuItem_Click);
        }

        GUIContent GetNoActionMenuItemContent()
        {
            if (mNoActionMenuItemContent == null)
            {
                mNoActionMenuItemContent = new GUIContent(
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.NoActionMenuItem));
            }

            return mNoActionMenuItemContent;
        }

        static void AddMergeSelectedFilesMenuItem(
            GUIContent menuItemContent,
            GenericMenu menu,
            MergeMenuOperations operations,
            GenericMenu.MenuFunction menuFunction)
        {
            if (operations.HasFlag(MergeMenuOperations.MergeContributors))
            {
                menu.AddItem(
                    menuItemContent,
                    false,
                    menuFunction);

                return;
            }

            menu.AddDisabledItem(menuItemContent);
        }

        static void AddMergeKeepingSourceChangesMenuItem(
            GUIContent menuItemContent,
            GenericMenu menu,
            MergeMenuOperations operations,
            GenericMenu.MenuFunction menuFunction)
        {
            if (operations.HasFlag(MergeMenuOperations.MergeKeepingSourceChanges))
            {
                menu.AddItem(
                    menuItemContent,
                    false,
                    menuFunction);

                return;
            }

            menu.AddDisabledItem(menuItemContent);
        }

        static void AddMergeKeepingWorkspaceChangesMenuItem(
            GUIContent menuItemContent,
            GenericMenu menu,
            MergeMenuOperations operations,
            GenericMenu.MenuFunction menuFunction)
        {
            if (operations.HasFlag(MergeMenuOperations.MergeKeepingWorkspaceChanges))
            {
                menu.AddItem(
                    menuItemContent,
                    false,
                    menuFunction);

                return;
            }

            menu.AddDisabledItem(menuItemContent);
        }

        static void AddDiffSourceWithDestinationMenuItem(
            GUIContent menuItemContent,
            GenericMenu menu,
            MergeMenuOperations operations,
            GenericMenu.MenuFunction menuFunction)
        {
            if (operations.HasFlag(MergeMenuOperations.DiffSourceWithDestination))
            {
                menu.AddItem(
                    menuItemContent,
                    false,
                    menuFunction);

                return;
            }

            menu.AddDisabledItem(menuItemContent);
        }

        static void AddDiffDestinationWithAncestorMenuItem(
            GUIContent menuItemContent,
            GenericMenu menu,
            MergeMenuOperations operations,
            GenericMenu.MenuFunction menuFunction)
        {
            if (!operations.HasFlag(MergeMenuOperations.DiffDestinationWithAncestor))
                return;

            menu.AddItem(
                menuItemContent,
                false,
                menuFunction);
        }

        static void AddDiffSourceWithAncestorMenuItem(
            GUIContent menuItemContent,
            GenericMenu menu,
            MergeMenuOperations operations,
            GenericMenu.MenuFunction menuFunction)
        {
            if (operations.HasFlag(MergeMenuOperations.DiffSourceWithAncestor))
            {
                menu.AddItem(
                    menuItemContent,
                    false,
                    menuFunction);

                return;
            }

            menu.AddDisabledItem(menuItemContent);
        }

        static void AddCopyFilePathMenuItem(
            GUIContent menuItemContent,
            GenericMenu menu,
            MergeMenuOperations operations,
            GenericMenu.MenuFunction menuFunction)
        {
            if (operations.HasFlag(MergeMenuOperations.CopyFilePath))
            {
                menu.AddItem(
                    menuItemContent,
                    false,
                    menuFunction);

                return;
            }

            menu.AddDisabledItem(menuItemContent);
        }

        static void AddHistoryMenuItem(
            GUIContent menuItemContent,
            GenericMenu menu,
            MergeMenuOperations operations,
            GenericMenu.MenuFunction menuFunction)
        {
            if (operations.HasFlag(MergeMenuOperations.ShowHistory))
            {
                menu.AddItem(
                    menuItemContent,
                    false,
                    menuFunction);

                return;
            }

            menu.AddDisabledItem(menuItemContent);
        }

        static MergeMenuOperations GetMenuOperations(Event e)
        {
            if (Keyboard.IsControlOrCommandKeyPressed(e) &&
                Keyboard.IsKeyPressed(e, KeyCode.H))
                return MergeMenuOperations.ShowHistory;

            return MergeMenuOperations.None;
        }

        static void ProcessMenuOperation(
            MergeMenuOperations operationToExecute,
            IMergeViewMenuOperations mergeViewMenuOperations)
        {
            if (operationToExecute == MergeMenuOperations.ShowHistory)
            {
                mergeViewMenuOperations.ShowHistory();
                return;
            }
        }

        void BuildComponents(
            bool isIncomingMerge,
            bool isMergeTo)
        {
            mMergeSelectedFilesMenuItemContent = new GUIContent(
                PlasticLocalization.GetString(PlasticLocalization.
                    Name.MergeSelectedFiles));

            mMergeKeepingSourceChangesMenuItemContent = new GUIContent(
                UnityMenuItem.EscapedText(
                    MergeViewTexts.GetMergeKeepingSourceChangesMenuItemText(
                        isIncomingMerge, isMergeTo)));

            mMergeKeepingWorkspaceChangesMenuItemContent = new GUIContent(
                UnityMenuItem.EscapedText(
                    MergeViewTexts.GetMergeKeepingWorkspaceChangesMenuItemText(
                        isIncomingMerge, isMergeTo)));

            string diffSourceWithAncestorText = UnityMenuItem.EscapedText(
                MergeViewTexts.GetDiffSourceWithAncestorMenuItemText(isIncomingMerge));

            mDiffSourceWithDestinationMenuItemContent = new GUIContent(string.Empty);

            mDiffDestinationWithAncestorMenuItemContent = new GUIContent(
                UnityMenuItem.EscapedText(
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.DiffDestinationWithAncestor)));

            mDiffSourceWithAncestorMenuItem = new GUIContent(
                diffSourceWithAncestorText);

            mDiffMetaSourceWithDestinationMenuItemContent = new GUIContent(string.Empty);
            mDiffMetaDestinationWithAncestorMenuItemContent = new GUIContent(
                string.Format(
                    "{0}/{1}",
                    MetaPath.META_EXTENSION,
                    UnityMenuItem.EscapedText(
                        PlasticLocalization.GetString(
                            PlasticLocalization.Name.DiffDestinationWithAncestor))));

            mDiffMetaSourceWithAncestorMenuItemContent = new GUIContent(
                string.Format(
                    "{0}/{1}",
                    MetaPath.META_EXTENSION,
                    diffSourceWithAncestorText));

            mHistoryMetaMenuItemContent = new GUIContent(
                string.Format(
                    "{0}/{1}",
                    MetaPath.META_EXTENSION,
                    PlasticLocalization.Name.ItemsMenuItemHistory.GetString()));

            mCopyFilePathMenuItemContent = new GUIContent(PlasticLocalization.Name.CopyFilePathMenuItem.GetString());

            mCopyRelativeFilePathMenuItemContent =
                new GUIContent(PlasticLocalization.Name.CopyRelativeFilePathMenuItem.GetString());

            mHistoryMenuItemContent = new GUIContent(string.Format("{0} {1}",
                PlasticLocalization.Name.ItemsMenuItemHistory.GetString(),
                    GetPlasticShortcut.ForHistory()));
        }

        GenericMenu mMenu;

        GUIContent mNoActionMenuItemContent;

        GUIContent mMergeSelectedFilesMenuItemContent;
        GUIContent mMergeKeepingSourceChangesMenuItemContent;
        GUIContent mMergeKeepingWorkspaceChangesMenuItemContent;

        GUIContent mDiffSourceWithDestinationMenuItemContent;
        GUIContent mDiffDestinationWithAncestorMenuItemContent;
        GUIContent mDiffSourceWithAncestorMenuItem;

        GUIContent mDiffMetaSourceWithDestinationMenuItemContent;
        GUIContent mDiffMetaDestinationWithAncestorMenuItemContent;
        GUIContent mDiffMetaSourceWithAncestorMenuItemContent;
        GUIContent mHistoryMetaMenuItemContent;

        GUIContent mCopyFilePathMenuItemContent;
        GUIContent mCopyRelativeFilePathMenuItemContent;
        GUIContent mHistoryMenuItemContent;

        readonly IMergeViewMenuOperations mMergeViewMenuOperations;
        readonly IMetaMenuOperations mMergeMetaMenuOperations;
    }
}
