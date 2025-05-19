﻿using UnityEditor.VersionControl;

using Codice.CM.Common;
using Codice.Client.Common.EventTracking;
using Codice.LogWrapper;
using PlasticGui;
using PlasticGui.WorkspaceWindow.Items;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.AssetUtils.Processor;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.Tool;

namespace Unity.PlasticSCM.Editor.AssetMenu
{
    internal class AssetMenuItems
    {
        internal static void Enable(
            WorkspaceInfo wkInfo,
            IAssetStatusCache assetStatusCache)
        {
            if (mIsEnabled)
                return;

            mLog.Debug("Enable");

            mWkInfo = wkInfo;
            mAssetStatusCache = assetStatusCache;

            mIsEnabled = true;

            mAssetSelection = new ProjectViewAssetSelection(UpdateFilterMenuItems);

            mAssetMenuCopyPathOperation = new AssetCopyPathOperation(
                wkInfo.ClientPath,
                assetStatusCache,
                mAssetSelection);

            mFilterMenuBuilder = new AssetFilesFilterPatternsMenuBuilder(
                IGNORE_MENU_ITEMS_PRIORITY,
                HIDDEN_MENU_ITEMS_PRIORITY);

            AddMenuItems();
        }

        internal static void Disable()
        {
            mLog.Debug("Disable");

            mIsEnabled = false;

            RemoveMenuItems();

            if (mAssetSelection != null)
                mAssetSelection.Dispose();

            mWkInfo = null;
            mAssetStatusCache = null;
            mAssetSelection = null;
            mFilterMenuBuilder = null;
            mAssetMenuVcsOperations = null;
            mAssetMenuCopyPathOperation = null;
        }

        internal static void BuildOperations(
            WorkspaceInfo wkInfo,
            WorkspaceWindow workspaceWindow,
            IViewSwitcher viewSwitcher,
            IHistoryViewLauncher historyViewLauncher,
            GluonGui.ViewHost viewHost,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            PlasticGui.WorkspaceWindow.NewIncomingChangesUpdater incomingChangesUpdater,
            IAssetStatusCache assetStatusCache,
            IMergeViewLauncher mergeViewLauncher,
            PlasticGui.Gluon.IGluonViewSwitcher gluonViewSwitcher,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            bool isGluonMode)
        {
            if (!mIsEnabled)
                Enable(wkInfo, assetStatusCache);

            AssetVcsOperations assetVcsOperations = new AssetVcsOperations(
                wkInfo,
                workspaceWindow,
                viewSwitcher,
                historyViewLauncher,
                viewHost,
                workspaceOperationsMonitor,
                incomingChangesUpdater,
                mAssetStatusCache,
                mergeViewLauncher,
                gluonViewSwitcher,
                mAssetSelection,
                showDownloadPlasticExeWindow,
                isGluonMode);

            mAssetMenuVcsOperations = assetVcsOperations;
            mFilterMenuBuilder.SetOperations(assetVcsOperations);
        }

        static void RemoveMenuItems()
        {
            mFilterMenuBuilder.RemoveMenuItems();

            HandleMenuItem.RemoveMenuItem(
                PlasticLocalization.GetString(PlasticLocalization.Name.PrefixUnityVersionControlMenu));

            HandleMenuItem.UpdateAllMenus();
        }

        static void UpdateFilterMenuItems()
        {
            AssetList assetList = ((AssetVcsOperations.IAssetSelection)
                mAssetSelection).GetSelectedAssets();

            SelectedPathsGroupInfo info = AssetsSelection.GetSelectedPathsGroupInfo(
                mWkInfo.ClientPath, assetList, mAssetStatusCache);

            FilterMenuActions actions =
                assetList.Count != info.SelectedCount ?
                new FilterMenuActions() :
                FilterMenuUpdater.GetMenuActions(info);

            mFilterMenuBuilder.UpdateMenuItems(actions);
        }

        static void AddMenuItems()
        {
            // TODO: Try removing this
            // Somehow first item always disappears. So this is a filler item
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.PendingChangesPlasticMenu),
                PENDING_CHANGES_MENU_ITEM_PRIORITY,
                PendingChanges, ValidatePendingChanges);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.PendingChangesPlasticMenu),
                PENDING_CHANGES_MENU_ITEM_PRIORITY,
                PendingChanges, ValidatePendingChanges);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.AddPlasticMenu),
                ADD_MENU_ITEM_PRIORITY,
                Add, ValidateAdd);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.CheckoutPlasticMenu),
                CHECKOUT_MENU_ITEM_PRIORITY,
                Checkout, ValidateCheckout);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.CheckinPlasticMenu),
                CHECKIN_MENU_ITEM_PRIORITY,
                Checkin, ValidateCheckin);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.UndoPlasticMenu),
                UNDO_MENU_ITEM_PRIORITY,
                Undo, ValidateUndo);

            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.CopyFilePathMenuItem),
                COPY_FILE_PATH_MENU_ITEM_PRIORITY,
                CopyFilePath,
                ValidateCopyFilePath);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.CopyRelativeFilePathMenuItem),
                COPY_RELATIVE_FILE_PATH_MENU_ITEM_PRIORITY,
                CopyRelativeFilePath,
                ValidateCopyFilePath);

            UpdateFilterMenuItems();

            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.DiffPlasticMenu),
                GetPlasticShortcut.ForAssetDiff(),
                DIFF_MENU_ITEM_PRIORITY,
                Diff, ValidateDiff);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.HistoryPlasticMenu),
                GetPlasticShortcut.ForHistory(),
                HISTORY_MENU_ITEM_PRIORITY,
                History, ValidateHistory);

            HandleMenuItem.UpdateAllMenus();
        }

        static void PendingChanges()
        {
            ShowWindow.Plastic();

            mAssetMenuVcsOperations.ShowPendingChanges();
        }

        static bool ValidatePendingChanges()
        {
            return true;
        }

        static void Add()
        {
            if (mAssetMenuVcsOperations == null)
                ShowWindow.Plastic();

            mAssetMenuVcsOperations.Add();
        }

        static bool ValidateAdd()
        {
            return ShouldMenuItemBeEnabled(
                mWkInfo.ClientPath, mAssetSelection, mAssetStatusCache,
                AssetMenuOperations.Add);
        }

        static void Checkout()
        {
            if (mAssetMenuVcsOperations == null)
                ShowWindow.Plastic();

            mAssetMenuVcsOperations.Checkout();
        }

        static bool ValidateCheckout()
        {
            return ShouldMenuItemBeEnabled(
                mWkInfo.ClientPath, mAssetSelection, mAssetStatusCache,
                AssetMenuOperations.Checkout);
        }

        static void Checkin()
        {
            TrackFeatureUseEvent.For(
                PlasticGui.Plastic.API.GetRepositorySpec(mWkInfo),
                TrackFeatureUseEvent.Features.ContextMenuCheckinOption);

            if (mAssetMenuVcsOperations == null)
                ShowWindow.Plastic();

            mAssetMenuVcsOperations.Checkin();
        }

        static bool ValidateCheckin()
        {
            return ShouldMenuItemBeEnabled(
                mWkInfo.ClientPath, mAssetSelection, mAssetStatusCache,
                AssetMenuOperations.Checkin);
        }

        static void Undo()
        {
            if (mAssetMenuVcsOperations == null)
                ShowWindow.Plastic();

            mAssetMenuVcsOperations.Undo();
        }

        static bool ValidateUndo()
        {
            return ShouldMenuItemBeEnabled(
                mWkInfo.ClientPath, mAssetSelection, mAssetStatusCache,
                AssetMenuOperations.Undo);
        }

        static void CopyFilePath()
        {
            mAssetMenuCopyPathOperation.CopyFilePath(relativePath: false);
        }

        static void CopyRelativeFilePath()
        {
            mAssetMenuCopyPathOperation.CopyFilePath(relativePath: true);
        }

        static bool ValidateCopyFilePath()
        {
            return ShouldMenuItemBeEnabled(
                mWkInfo.ClientPath, mAssetSelection, mAssetStatusCache,
                AssetMenuOperations.CopyFilePath);
        }

        static void Diff()
        {
            if (mAssetMenuVcsOperations == null)
                ShowWindow.Plastic();

            mAssetMenuVcsOperations.ShowDiff();
        }

        static bool ValidateDiff()
        {
            return ShouldMenuItemBeEnabled(
                mWkInfo.ClientPath, mAssetSelection, mAssetStatusCache,
                AssetMenuOperations.Diff);
        }

        static void History()
        {
            ShowWindow.Plastic();

            mAssetMenuVcsOperations.ShowHistory();
        }

        static bool ValidateHistory()
        {
            return ShouldMenuItemBeEnabled(
                mWkInfo.ClientPath, mAssetSelection, mAssetStatusCache,
                AssetMenuOperations.History);
        }

        static bool ShouldMenuItemBeEnabled(
            string wkPath,
            AssetVcsOperations.IAssetSelection assetSelection,
            IAssetStatusCache statusCache,
            AssetMenuOperations operation)
        {
            AssetList assetList = assetSelection.GetSelectedAssets();

            if (assetList.Count == 0)
                return false;

            SelectedAssetGroupInfo selectedGroupInfo = SelectedAssetGroupInfo.
                BuildFromAssetList(wkPath, assetList, statusCache);

            if (assetList.Count != selectedGroupInfo.SelectedCount)
                return false;

            AssetMenuOperations operations = AssetMenuUpdater.
                GetAvailableMenuOperations(selectedGroupInfo);

            return operations.HasFlag(operation);
        }

        static string GetPlasticMenuItemName(PlasticLocalization.Name name)
        {
            return string.Format("{0}/{1}",
                PlasticLocalization.GetString(PlasticLocalization.Name.PrefixUnityVersionControlMenu),
                PlasticLocalization.GetString(name));
        }

        static IAssetMenuVCSOperations mAssetMenuVcsOperations;
        static IAssetMenuCopyPathOperation mAssetMenuCopyPathOperation;

        static ProjectViewAssetSelection mAssetSelection;
        static AssetFilesFilterPatternsMenuBuilder mFilterMenuBuilder;

        static bool mIsEnabled;
        static IAssetStatusCache mAssetStatusCache;
        static WorkspaceInfo mWkInfo;

#if UNITY_6000_0_OR_NEWER
        // Puts Unity Version Control in a new section, as it precedes the Create menu with the old value
        const int BASE_MENU_ITEM_PRIORITY = 71;
#else
        // Puts Unity Version Control right below the Create menu
        const int BASE_MENU_ITEM_PRIORITY = 19;
#endif

        // incrementing the "order" param by 11 causes the menu system to add a separator
        const int PENDING_CHANGES_MENU_ITEM_PRIORITY = BASE_MENU_ITEM_PRIORITY;
        const int ADD_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 11;
        const int CHECKOUT_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 12;
        const int CHECKIN_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 13;
        const int UNDO_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 14;
        const int COPY_FILE_PATH_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 25;
        const int COPY_RELATIVE_FILE_PATH_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 26;
        const int IGNORE_MENU_ITEMS_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 37;
        const int HIDDEN_MENU_ITEMS_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 38;
        const int DIFF_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 49;
        const int HISTORY_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 50;

        static readonly ILog mLog = PlasticApp.GetLogger("AssetMenuItems");
    }
}
