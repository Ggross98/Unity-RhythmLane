using System.Collections.Generic;
using System.IO;

using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Codice.Client.BaseCommands.Merge;
using Codice.Client.Common;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.Merge;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Tree;
using UnityEditor;

namespace Unity.PlasticSCM.Editor.Views.Merge.Developer
{
    internal class MergeTreeView : TreeView
    {
        internal GenericMenu Menu { get { return mMenu.Menu; } }

        internal MergeTreeView(
            WorkspaceInfo wkInfo,
            MergeTreeHeaderState headerState,
            List<string> columnNames,
            MergeViewMenu menu)
            : base(new TreeViewState())
        {
            mWkInfo = wkInfo;
            mColumnNames = columnNames;
            mMenu = menu;

            multiColumnHeader = new MultiColumnHeader(headerState);
            multiColumnHeader.canSort = true;
            multiColumnHeader.sortingChanged += SortingChanged;

            customFoldoutYOffset = UnityConstants.TREEVIEW_FOLDOUT_Y_OFFSET;
            rowHeight = UnityConstants.TREEVIEW_ROW_HEIGHT;
            showAlternatingRowBackgrounds = false;

            mCooldownFilterAction = new CooldownWindowDelayer(
                DelayedSearchChanged, UnityConstants.SEARCH_DELAYED_INPUT_ACTION_INTERVAL);
        }

        public override IList<TreeViewItem> GetRows()
        {
            return mRows;
        }

        protected override bool CanChangeExpandedState(TreeViewItem item)
        {
            return item is ChangeCategoryTreeViewItem;
        }

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem(0, -1, string.Empty);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem rootItem)
        {
            try
            {
                RegenerateRows(
                    mMergeTree,
                    mTreeViewItemIds,
                    this,
                    rootItem,
                    mRows,
                    mExpandCategories);
            }
            finally
            {
                mExpandCategories = false;
            }

            return mRows;
        }

        protected override void CommandEventHandling()
        {
            // NOTE - empty override to prevent crash when pressing ctrl-a in the treeview
        }

        protected override void SearchChanged(string newSearch)
        {
            mCooldownFilterAction.Ping();
        }

        protected override void ContextClickedItem(int id)
        {
            mMenu.Popup();
            Repaint();
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            Event e = Event.current;

            if (e.type != EventType.KeyDown)
                return;

            bool isProcessed = mMenu.ProcessKeyActionIfNeeded(e);

            if (isProcessed)
                e.Use();
        }

        protected override void BeforeRowsGUI()
        {
            int firstRowVisible;
            int lastRowVisible;
            GetFirstAndLastVisibleRows(out firstRowVisible, out lastRowVisible);

            GUI.DrawTexture(new Rect(0,
                 firstRowVisible * rowHeight,
                 GetRowRect(0).width,
                 (lastRowVisible * rowHeight) + 1000),
                 Images.GetTreeviewBackgroundTexture());

            DrawTreeViewItem.InitializeStyles();
            base.BeforeRowsGUI();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.item is ChangeCategoryTreeViewItem)
            {
                ChangeCategoryTreeViewItem categoryItem =
                    (ChangeCategoryTreeViewItem)args.item;

                CategoryTreeViewItemGUI(
                    args.rowRect,
                    categoryItem,
                    GetSolvedChildrenCount(categoryItem.Category, mSolvedFileConflicts),
                    args.selected,
                    args.focused);
                return;
            }

            if (args.item is ChangeTreeViewItem)
            {
                ChangeTreeViewItem changeTreeViewItem =
                    (ChangeTreeViewItem)args.item;

                MergeChangeInfo changeInfo =
                    changeTreeViewItem.ChangeInfo;

                bool isCurrentConflict = IsCurrent.Conflict(
                    changeInfo,
                    mMergeTree.GetMetaChange(changeInfo),
                    mSolvedFileConflicts);

                bool isSolvedConflict = IsSolved.Conflict(
                    changeInfo,
                    mMergeTree.GetMetaChange(changeInfo),
                    mSolvedFileConflicts);

                MergeTreeViewItemGUI(
                    mWkInfo.ClientPath,
                    mMergeTree,
                    this,
                    changeTreeViewItem,
                    args,
                    isCurrentConflict,
                    isSolvedConflict);
                return;
            }

            base.RowGUI(args);
        }

        internal void SelectFirstUnsolvedDirectoryConflict()
        {
            foreach (MergeChangesCategory category in mMergeTree.GetNodes())
            {
                if (category.CategoryType != MergeChangesCategory.Type.DirectoryConflicts)
                    continue;

                foreach (MergeChangeInfo changeInfo in category.GetChanges())
                {
                    if (changeInfo.DirectoryConflict.IsResolved())
                        continue;

                    int itemId = -1;
                    if (mTreeViewItemIds.TryGetInfoItemId(changeInfo, out itemId))
                    {
                        SetSelection(new List<int>() { itemId });
                        return;
                    }
                }
            }
        }

        internal void BuildModel(UnityMergeTree tree)
        {
            mTreeViewItemIds.Clear();

            mMergeTree = tree;
            mSolvedFileConflicts = null;

            mExpandCategories = true;
        }

        internal void Refilter()
        {
            Filter filter = new Filter(searchString);
            mMergeTree.Filter(filter, mColumnNames);

            mExpandCategories = true;
        }

        internal void Sort()
        {
            if (mMergeTree == null)
                return;

            int sortedColumnIdx = multiColumnHeader.state.sortedColumnIndex;
            bool sortAscending = multiColumnHeader.IsSortedAscending(sortedColumnIdx);

            mMergeTree.Sort(mColumnNames[sortedColumnIdx], sortAscending);
        }

        internal void UpdateSolvedFileConflicts(
            MergeSolvedFileConflicts solvedFileConflicts)
        {
            mSolvedFileConflicts = solvedFileConflicts;
        }

        internal MergeChangeInfo GetMetaChange(MergeChangeInfo change)
        {
            if (change == null)
                return null;

            return mMergeTree.GetMetaChange(change);
        }

        internal void FillWithMeta(List<MergeChangeInfo> changes)
        {
            mMergeTree.FillWithMeta(changes);
        }

        internal bool SelectionHasMeta()
        {
            MergeChangeInfo selectedChangeInfo = GetSelectedMergeChange();

            if (selectedChangeInfo == null)
                return false;

            return mMergeTree.HasMeta(selectedChangeInfo);
        }

        internal MergeChangeInfo GetSelectedMergeChange()
        {
            IList<int> selectedIds = GetSelection();

            if (selectedIds.Count != 1)
                return null;

            int selectedId = selectedIds[0];

            foreach (KeyValuePair<MergeChangeInfo, int> item
                in mTreeViewItemIds.GetInfoItems())
            {
                if (selectedId == item.Value)
                    return item.Key;
            }

            return null;
        }

        internal List<MergeChangeInfo> GetSelectedMergeChanges()
        {
            List<MergeChangeInfo> result = new List<MergeChangeInfo>();

            IList<int> selectedIds = GetSelection();

            if (selectedIds.Count == 0)
                return result;

            foreach (KeyValuePair<MergeChangeInfo, int> item
                in mTreeViewItemIds.GetInfoItems())
            {
                if (!selectedIds.Contains(item.Value))
                    continue;

                result.Add(item.Key);
            }

            return result;
        }

        internal List<MergeChangeInfo> GetSelectedFileConflicts()
        {
            List<MergeChangeInfo> result = new List<MergeChangeInfo>();

            IList<int> selectedIds = GetSelection();

            if (selectedIds.Count == 0)
                return result;

            foreach (KeyValuePair<MergeChangeInfo, int> item
                in mTreeViewItemIds.GetInfoItems())
            {
                if (!selectedIds.Contains(item.Value))
                    continue;

                if (item.Key.CategoryType !=
                        MergeChangesCategory.Type.FileConflicts)
                    continue;

                result.Add(item.Key);
            }

            return result;
        }

        void DelayedSearchChanged()
        {
            Refilter();

            Sort();

            Reload();

            TableViewOperations.ScrollToSelection(this);
        }

        void SortingChanged(MultiColumnHeader multiColumnHeader)
        {
            Sort();

            Reload();
        }

        static void RegenerateRows(
            UnityMergeTree mergeTree,
            TreeViewItemIds<MergeChangesCategory, MergeChangeInfo> treeViewItemIds,
            MergeTreeView treeView,
            TreeViewItem rootItem,
            List<TreeViewItem> rows,
            bool expandCategories)
        {
            if (mergeTree == null)
                return;

            ClearRows(rootItem, rows);

            List<MergeChangesCategory> categories = mergeTree.GetNodes();

            if (categories == null)
                return;

            List<int> categoriesToExpand = new List<int>();

            foreach (MergeChangesCategory category in categories)
            {
                int categoryId;
                if (!treeViewItemIds.TryGetCategoryItemId(category, out categoryId))
                    categoryId = treeViewItemIds.AddCategoryItem(category);

                ChangeCategoryTreeViewItem categoryTreeViewItem =
                    new ChangeCategoryTreeViewItem(categoryId, category);

                rootItem.AddChild(categoryTreeViewItem);
                rows.Add(categoryTreeViewItem);

                if (!ShouldExpandCategory(
                        treeView,
                        categoryTreeViewItem,
                        expandCategories,
                        categories.Count))
                    continue;

                categoriesToExpand.Add(categoryTreeViewItem.id);

                foreach (MergeChangeInfo changeInfo in category.GetChanges())
                {
                    int differenceId;
                    if (!treeViewItemIds.TryGetInfoItemId(changeInfo, out differenceId))
                        differenceId = treeViewItemIds.AddInfoItem(changeInfo);

                    TreeViewItem changeTreeViewItem =
                        new ChangeTreeViewItem(differenceId, changeInfo);

                    categoryTreeViewItem.AddChild(changeTreeViewItem);
                    rows.Add(changeTreeViewItem);
                }
            }

            treeView.state.expandedIDs = categoriesToExpand;
        }

        static void ClearRows(
            TreeViewItem rootItem,
            List<TreeViewItem> rows)
        {
            if (rootItem.hasChildren)
                rootItem.children.Clear();

            rows.Clear();
        }

        static void CategoryTreeViewItemGUI(
            Rect rowRect,
            ChangeCategoryTreeViewItem item,
            int solvedChildrenCount,
            bool isSelected,
            bool isFocused)
        {
            string label = item.Category.GetCategoryName();
            string infoLabel = item.Category.GetChildrenCountText();

            DefaultStyles.label = GetCategoryStyle(
                item.Category,
                solvedChildrenCount,
                isSelected);

            DrawTreeViewItem.ForCategoryItem(
                rowRect,
                item.depth,
                label,
                infoLabel,
                isSelected,
                isFocused);

            DefaultStyles.label = UnityStyles.Tree.Label;
        }

        static void MergeTreeViewItemGUI(
            string wkPath,
            UnityMergeTree mergeTree,
            MergeTreeView treeView,
            ChangeTreeViewItem item,
            RowGUIArgs args,
            bool isCurrentConflict,
            bool isSolvedConflict)
        {
            for (int visibleColumnIdx = 0; visibleColumnIdx < args.GetNumVisibleColumns(); visibleColumnIdx++)
            {
                Rect cellRect = args.GetCellRect(visibleColumnIdx);

                MergeTreeColumn column =
                    (MergeTreeColumn)args.GetColumn(visibleColumnIdx);

                MergeTreeViewItemCellGUI(
                    wkPath,
                    cellRect,
                    treeView.rowHeight,
                    mergeTree,
                    treeView,
                    item,
                    column,
                    args.selected,
                    args.focused,
                    isCurrentConflict,
                    isSolvedConflict);
            }
        }

        static void MergeTreeViewItemCellGUI(
            string wkPath,
            Rect rect,
            float rowHeight,
            UnityMergeTree mergeTree,
            MergeTreeView treeView,
            ChangeTreeViewItem item,
            MergeTreeColumn column,
            bool isSelected,
            bool isFocused,
            bool isCurrentConflict,
            bool isSolvedConflict)
        {
            MergeChangeInfo mergeChange = item.ChangeInfo;

            string label = mergeChange.GetColumnText(
                MergeTreeHeaderState.GetColumnName(column));

            if (column == MergeTreeColumn.Path)
            {
                if (mergeTree.HasMeta(item.ChangeInfo))
                    label = string.Concat(label, UnityConstants.TREEVIEW_META_LABEL);

                Texture icon = GetIcon(wkPath, mergeChange);

                Texture overlayIcon =
                    GetChangesOverlayIcon.ForPlasticMergeChange(
                        mergeChange, isSolvedConflict);

                DrawTreeViewItem.ForItemCell(
                    rect,
                    rowHeight,
                    item.depth,
                    icon,
                    overlayIcon,
                    label,
                    isSelected,
                    isFocused,
                    isCurrentConflict,
                    false);

                return;
            }

            if (column == MergeTreeColumn.Size)
            {
                // If there is a meta file, add the meta file to the file size so that it is consistent
                // with the Merge overview
                if (mergeTree.HasMeta(item.ChangeInfo))
                {
                    MergeChangeInfo metaFileInfo = mergeTree.GetMetaChange(mergeChange);
                    long metaFileSize = metaFileInfo.GetSize();
                    long fileSize = mergeChange.GetSize();

                    label = SizeConverter.ConvertToSizeString(fileSize + metaFileSize);
                }

                DrawTreeViewItem.ForSecondaryLabelRightAligned(
                    rect, label, isSelected, isFocused, isCurrentConflict);
                return;
            }

            DrawTreeViewItem.ForSecondaryLabel(
                rect, label, isSelected, isFocused, isCurrentConflict);
        }

        static Texture GetIcon(
            string wkPath,
            MergeChangeInfo mergeChange)
        {
            RevisionInfo revInfo = mergeChange.GetRevision();
            bool isDirectory = revInfo.
                Type == EnumRevisionType.enDirectory;

            if (isDirectory || mergeChange.IsXLink())
                return Images.GetDirectoryIcon();

            string fullPath = WorkspacePath.GetWorkspacePathFromCmPath(
                wkPath,
                mergeChange.GetPath(),
                Path.DirectorySeparatorChar);

            return Images.GetFileIcon(fullPath);
        }

        static GUIStyle GetCategoryStyle(
            MergeChangesCategory category,
            int solvedChildrenCount,
            bool isSelected)
        {
            if (isSelected)
                return UnityStyles.Tree.Label;

            if (category.CategoryType == MergeChangesCategory.Type.FileConflicts ||
                category.CategoryType == MergeChangesCategory.Type.DirectoryConflicts)
            {
                return category.GetChildrenCount() > solvedChildrenCount ?
                    UnityStyles.Tree.RedLabel : UnityStyles.Tree.GreenLabel;
            }

            return UnityStyles.Tree.Label;
        }

        static bool ShouldExpandCategory(
            MergeTreeView treeView,
            ChangeCategoryTreeViewItem categoryTreeViewItem,
            bool expandCategories,
            int categoriesCount)
        {
            if (expandCategories)
            {
                if (categoriesCount == 1)
                    return true;

                if (categoryTreeViewItem.Category.CategoryType ==
                    MergeChangesCategory.Type.FileConflicts)
                    return true;

                if (categoryTreeViewItem.Category.GetChildrenCount() >
                    NODES_TO_EXPAND_CATEGORY)
                    return false;

                return true;
            }

            return treeView.IsExpanded(categoryTreeViewItem.id);
        }

        static int GetSolvedChildrenCount(
            MergeChangesCategory category,
            MergeSolvedFileConflicts solvedFileConflicts)
        {
            int solvedDirConflicts = 0;
            if (category.CategoryType == MergeChangesCategory.Type.DirectoryConflicts)
            {
                foreach (MergeChangeInfo change in category.GetChanges())
                {
                    if (change.DirectoryConflict.IsResolved())
                        solvedDirConflicts++;
                }

                return solvedDirConflicts;
            }

            return (solvedFileConflicts == null) ? 0 :
                solvedFileConflicts.GetCount();
        }

        bool mExpandCategories;

        TreeViewItemIds<MergeChangesCategory, MergeChangeInfo> mTreeViewItemIds =
            new TreeViewItemIds<MergeChangesCategory, MergeChangeInfo>();
        List<TreeViewItem> mRows = new List<TreeViewItem>();

        MergeSolvedFileConflicts mSolvedFileConflicts;
        UnityMergeTree mMergeTree;
        CooldownWindowDelayer mCooldownFilterAction;

        readonly MergeViewMenu mMenu;
        readonly List<string> mColumnNames;
        readonly WorkspaceInfo mWkInfo;

        const int NODES_TO_EXPAND_CATEGORY = 10;
    }
}
