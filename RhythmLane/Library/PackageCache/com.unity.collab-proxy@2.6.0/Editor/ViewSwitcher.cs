using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Codice.Client.Common.EventTracking;
using Codice.Client.Common.Threading;
using Codice.CM.Common;
using GluonGui;
using PlasticGui;
using PlasticGui.Gluon;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.Merge;
using PlasticGui.WorkspaceWindow.QueryViews;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.AssetUtils.Processor;
using Unity.PlasticSCM.Editor.Tool;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.StatusBar;
using Unity.PlasticSCM.Editor.Views.Branches;
using Unity.PlasticSCM.Editor.Views.Changesets;
using Unity.PlasticSCM.Editor.Views.History;
using Unity.PlasticSCM.Editor.Views.IncomingChanges.Gluon;
using Unity.PlasticSCM.Editor.Views.Locks;
using Unity.PlasticSCM.Editor.Views.Merge.Developer;
using Unity.PlasticSCM.Editor.Views.Merge;
using Unity.PlasticSCM.Editor.Views.PendingChanges;

using GluonNewIncomingChangesUpdater = PlasticGui.Gluon.WorkspaceWindow.NewIncomingChangesUpdater;
using ObjectInfo = Codice.CM.Common.ObjectInfo;

namespace Unity.PlasticSCM.Editor
{
    [Serializable]
    internal class ViewSwitcher :
        IViewSwitcher,
        IMergeViewLauncher,
        IGluonViewSwitcher,
        IHistoryViewLauncher,
        MergeInProgress.IShowMergeView
    {
        internal enum SelectedTab
        {
            None = 0,
            PendingChanges = 1,
            IncomingChanges = 2,
            Changesets = 3,
            Branches = 4,
            Locks = 5,
            Merge = 6,
            History = 7
        }

        internal PendingChangesTab PendingChangesTab { get; private set; }
        internal IIncomingChangesTab IncomingChangesTab { get; private set; }
        internal ChangesetsTab ChangesetsTab { get; private set; }
        internal BranchesTab BranchesTab { get; private set; }
        internal LocksTab LocksTab { get; private set; }
        internal MergeTab MergeTab { get; private set; }
        internal HistoryTab HistoryTab { get; private set; }

        internal SelectedTab SelectedView { get { return mSelectedTab; } }

        internal ViewSwitcher(
            RepositorySpec repSpec,
            WorkspaceInfo wkInfo,
            ViewHost viewHost,
            bool isGluonMode,
            IAssetStatusCache assetStatusCache,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            StatusBar statusBar,
            EditorWindow parentWindow)
        {
            mRepSpec = repSpec;
            mWkInfo = wkInfo;
            mViewHost = viewHost;
            mIsGluonMode = isGluonMode;
            mAssetStatusCache = assetStatusCache;
            mShowDownloadPlasticExeWindow = showDownloadPlasticExeWindow;
            mProcessExecutor = processExecutor;
            mWorkspaceOperationsMonitor = workspaceOperationsMonitor;
            mStatusBar = statusBar;
            mParentWindow = parentWindow;

            mPendingChangesTabButton = new TabButton();
            mIncomingChangesTabButton = new TabButton();
            mChangesetsTabButton = new TabButton();
            mBranchesTabButton = new TabButton();
            mLocksTabButton = new TabButton();
            mMergeTabButton = new TabButton();
            mHistoryTabButton = new TabButton();
        }

        internal bool IsViewSelected(SelectedTab tab)
        {
            return mSelectedTab == tab;
        }

        internal void SetNewIncomingChanges(
            NewIncomingChangesUpdater developerNewIncomingChangesUpdater,
            GluonNewIncomingChangesUpdater gluonNewIncomingChangesUpdater,
            IIncomingChangesNotifier incomingChangesNotifier)
        {
            mDeveloperNewIncomingChangesUpdater = developerNewIncomingChangesUpdater;
            mGluonNewIncomingChangesUpdater = gluonNewIncomingChangesUpdater;
            mIncomingChangesNotifier = incomingChangesNotifier;
        }

        internal void SetWorkspaceWindow(WorkspaceWindow workspaceWindow)
        {
            mWorkspaceWindow = workspaceWindow;
        }

        internal void ShowInitialView(SelectedTab viewToShow)
        {
            ShowView(viewToShow);

            if (mSelectedTab != SelectedTab.None)
                return;

            ShowPendingChangesView();
        }

        internal void AutoRefreshPendingChangesView()
        {
            AutoRefresh.PendingChangesView(PendingChangesTab);
        }

        internal void AutoRefreshIncomingChangesView()
        {
            AutoRefresh.IncomingChangesView(IncomingChangesTab);
        }

        internal void RefreshView(ViewType viewType)
        {
            IRefreshableView view = GetRefreshableView(viewType);

            if (view == null)
                return;

            view.Refresh();
        }

        internal void RefreshSelectedView()
        {
            IRefreshableView view = GetRefreshableViewBasedOnSelectedTab(mSelectedTab);

            if (view == null)
                return;

            view.Refresh();
        }

        internal void RefreshWorkingObjectInfoForSelectedView(
            ViewType viewType,
            WorkingObjectInfo homeInfo)
        {
            switch (viewType)
            {
                case ViewType.BranchesView:
                    if (BranchesTab != null)
                        BranchesTab.SetWorkingObjectInfo(homeInfo);
                    break;
                case ViewType.ChangesetsView:
                    if (ChangesetsTab != null)
                        ChangesetsTab.SetWorkingObjectInfo(homeInfo);
                    break;
            }
        }

        internal void OnEnable()
        {
            mWorkspaceOperationsMonitor.
                RegisterPendingChangesView(PendingChangesTab);
            mWorkspaceOperationsMonitor.
                RegisterIncomingChangesView(IncomingChangesTab);

            if (PendingChangesTab != null)
                PendingChangesTab.OnEnable();

            if (IncomingChangesTab != null)
                IncomingChangesTab.OnEnable();

            if (ChangesetsTab != null)
                ChangesetsTab.OnEnable();

            if (BranchesTab != null)
                BranchesTab.OnEnable();

            if (LocksTab != null)
                LocksTab.OnEnable();

            if (MergeTab != null)
                MergeTab.OnEnable();

            if (HistoryTab != null)
                HistoryTab.OnEnable();
        }

        internal void OnDisable()
        {
            mWorkspaceOperationsMonitor.UnRegisterViews();

            if (PendingChangesTab != null)
                PendingChangesTab.OnDisable();

            if (IncomingChangesTab != null)
                IncomingChangesTab.OnDisable();

            if (ChangesetsTab != null)
                ChangesetsTab.OnDisable();

            if (BranchesTab != null)
                BranchesTab.OnDisable();

            if (LocksTab != null)
                LocksTab.OnDisable();

            if (MergeTab != null)
                MergeTab.OnDisable();

            if (HistoryTab != null)
                HistoryTab.OnDisable();

        }

        internal void Update()
        {
            if (IsViewSelected(SelectedTab.PendingChanges))
            {
                PendingChangesTab.Update();
                return;
            }

            if (IsViewSelected(SelectedTab.IncomingChanges))
            {
                IncomingChangesTab.Update();
                return;
            }

            if (IsViewSelected(SelectedTab.Changesets))
            {
                ChangesetsTab.Update();
                return;
            }

            if (IsViewSelected(SelectedTab.Branches))
            {
                BranchesTab.Update();
                return;
            }

            if (IsViewSelected(SelectedTab.Locks))
            {
                LocksTab.Update();
                return;
            }

            if (IsViewSelected(SelectedTab.Merge))
            {
                MergeTab.Update();
                return;
            }

            if (IsViewSelected(SelectedTab.History))
            {
                HistoryTab.Update();
                return;
            }
        }

        internal void TabButtonsGUI()
        {
            InitializeTabButtonWidth();

            PendingChangesTabButtonGUI();

            IncomingChangesTabButtonGUI();

            ChangesetsTabButtonGUI();

            BranchesTabButtonGUI();

            LocksTabButtonGUI();

            MergeTabButtonGUI();

            HistoryTabButtonGUI();
        }

        internal void TabViewGUI()
        {
            if (IsViewSelected(SelectedTab.PendingChanges))
            {
                PendingChangesTab.OnGUI();
                return;
            }

            if (IsViewSelected(SelectedTab.IncomingChanges))
            {
                IncomingChangesTab.OnGUI();
                return;
            }

            if (IsViewSelected(SelectedTab.Changesets))
            {
                ChangesetsTab.OnGUI();
                return;
            }

            if (IsViewSelected(SelectedTab.Branches))
            {
                BranchesTab.OnGUI();
                return;
            }

            if (IsViewSelected(SelectedTab.Locks))
            {
                LocksTab.OnGUI();
                return;
            }

            if (IsViewSelected(SelectedTab.Merge))
            {
                MergeTab.OnGUI();
                return;
            }

            if (IsViewSelected(SelectedTab.History))
            {
                HistoryTab.OnGUI();
                return;
            }
        }

        internal void ShowPendingChangesView()
        {
            OpenPendingChangesTab();

            bool wasPendingChangesSelected =
                IsViewSelected(SelectedTab.PendingChanges);

            if (!wasPendingChangesSelected)
            {
                PendingChangesTab.AutoRefresh();
            }

            SetSelectedView(SelectedTab.PendingChanges);
        }

        internal void ShowChangesetsView()
        {
            if (ChangesetsTab == null)
            {
                OpenPendingChangesTab();

                ChangesetsTab = new ChangesetsTab(
                    mWkInfo,
                    mWorkspaceWindow,
                    this,
                    this,
                    this,
                    mViewHost,
                    mWorkspaceWindow,
                    mWorkspaceWindow,
                    mDeveloperNewIncomingChangesUpdater,
                    PendingChangesTab,
                    mShowDownloadPlasticExeWindow,
                    mProcessExecutor,
                    mParentWindow,
                    mIsGluonMode);

                mViewHost.AddRefreshableView(
                    ViewType.ChangesetsView,
                    ChangesetsTab);
            }

            bool wasChangesetsSelected =
                IsViewSelected(SelectedTab.Changesets);

            if (!wasChangesetsSelected)
                ((IRefreshableView)ChangesetsTab).Refresh();

            SetSelectedView(SelectedTab.Changesets);
        }

        internal void ShowBranchesViewIfNeeded()
        {
            if (!BoolSetting.Load(UnityConstants.SHOW_BRANCHES_VIEW_KEY_NAME, true))
                return;

            string query = QueryConstants.BranchesBeginningQuery;

            ViewQueryResult queryResult = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter();
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    queryResult = new ViewQueryResult(
                        PlasticGui.Plastic.API.FindQuery(mWkInfo, query));
                },
                /*afterOperationDelegate*/ delegate
                {
                    if (waiter.Exception != null)
                    {
                        ExceptionsHandler.DisplayException(waiter.Exception);
                        return;
                    }

                    if (queryResult == null)
                        return;

                    if (queryResult.Count()>0)
                        OpenBranchesTab();
                });
        }

        internal void ShowBranchesView()
        {
            OpenBranchesTab();

            bool wasBranchesSelected =
                IsViewSelected(SelectedTab.Branches);

            if (!wasBranchesSelected)
                ((IRefreshableView)BranchesTab).Refresh();

            SetSelectedView(SelectedTab.Branches);
        }

        internal void ShowLocksViewIfNeeded()
        {
            if (!BoolSetting.Load(UnityConstants.SHOW_LOCKS_VIEW_KEY_NAME, false))
                return;

            OpenLocksTab();
        }

        internal void ShowLocksView()
        {
            OpenLocksTab();

            bool wasLocksViewSelected =
                IsViewSelected(SelectedTab.Locks);

            if (!wasLocksViewSelected)
                ((IRefreshableView)LocksTab).Refresh();

            SetSelectedView(SelectedTab.Locks);
        }

        internal void ShowHistoryView(
            RepositorySpec repSpec,
            long itemId,
            string path,
            bool isDirectory)
        {
            if (HistoryTab == null)
            {
                HistoryTab = new HistoryTab(
                    mWkInfo,
                    mWorkspaceWindow,
                    repSpec,
                    mShowDownloadPlasticExeWindow,
                    mProcessExecutor,
                    mDeveloperNewIncomingChangesUpdater,
                    mViewHost,
                    mParentWindow,
                    mIsGluonMode);

                mViewHost.AddRefreshableView(
                    ViewType.HistoryView, HistoryTab);
            }

            HistoryTab.RefreshForItem(
                itemId,
                path,
                isDirectory);

            SetSelectedView(SelectedTab.History);
        }

        internal void ShowBranchesViewForTesting(BranchesTab branchesTab)
        {
            BranchesTab = branchesTab;

            ShowBranchesView();
        }

        internal void ShowMergeViewForTesting(MergeTab mergeTab)
        {
            MergeTab = mergeTab;

            ShowMergeView();
        }

        void IViewSwitcher.ShowView(ViewType viewType)
        {
        }

        void IViewSwitcher.ShowPendingChanges()
        {
            ShowPendingChangesView();
            mParentWindow.Repaint();
        }

        void IViewSwitcher.ShowSyncView(string syncViewToSelect)
        {
            throw new NotImplementedException();
        }

        void IViewSwitcher.ShowBranchExplorerView()
        {
            //TODO: Codice
            //launch plastic with branch explorer view option
        }

        void IViewSwitcher.DisableMergeView()
        {
        }

        bool IViewSwitcher.IsIncomingChangesView()
        {
            return IsViewSelected(SelectedTab.IncomingChanges);
        }

        void IViewSwitcher.CloseIncomingChangesView()
        {
            ((IViewSwitcher)this).DisableMergeView();
        }

        IMergeView IMergeViewLauncher.MergeFrom(
            RepositorySpec repSpec,
            ObjectInfo objectInfo,
            EnumMergeType mergeType,
            List<MergeViewAction> actions)
        {
            return ((IMergeViewLauncher)this).MergeFromInterval(
                repSpec, objectInfo, null, mergeType, actions);
        }

        IMergeView IMergeViewLauncher.MergeFrom(
            RepositorySpec repSpec,
            ObjectInfo objectInfo,
            EnumMergeType mergeType,
            ShowIncomingChangesFrom from,
            List<MergeViewAction> actions)
        {
            return MergeFromInterval(repSpec, objectInfo, null, mergeType, from, actions);
        }

        IMergeView IMergeViewLauncher.MergeFromInterval(
            RepositorySpec repSpec,
            ObjectInfo objectInfo,
            ObjectInfo ancestorChangesetInfo,
            EnumMergeType mergeType,
            List<MergeViewAction> actions)
        {
            return MergeFromInterval(
                repSpec, objectInfo, null, mergeType, ShowIncomingChangesFrom.NotificationBar, actions);
        }

        IMergeView IMergeViewLauncher.FromCalculatedMerge(
            RepositorySpec repSpec,
            ObjectInfo objectInfo,
            EnumMergeType mergeType,
            CalculatedMergeResult calculatedMergeResult,
            List<MergeViewAction> actions)
        {
            return ((IMergeViewLauncher)this).MergeFromInterval(
                repSpec, objectInfo, null, mergeType, actions);
        }

        void IGluonViewSwitcher.ShowIncomingChangesView()
        {
            ShowIncomingChangesView();
            mParentWindow.Repaint();
        }

        void IHistoryViewLauncher.ShowHistoryView(
            RepositorySpec repSpec,
            long itemId,
            string path,
            bool isDirectory)
        {
            ShowHistoryView(
                repSpec,
                itemId,
                path,
                isDirectory);

            mParentWindow.Repaint();
        }

        void MergeInProgress.IShowMergeView.MergeLinkNotFound()
        {
            // Nothing to do on the plugin when there is no pending merge link
        }

        void MergeInProgress.IShowMergeView.ForPendingMergeLink(
            RepositorySpec repSpec,
            MergeType pendingLinkMergeType,
            ChangesetInfo srcChangeset,
            ChangesetInfo baseChangeset)
        {
            EnumMergeType mergeType = MergeTypeConverter.TranslateMergeType(pendingLinkMergeType);

            MergeTab = BuildMergeTab(
                repSpec,
                srcChangeset,
                baseChangeset,
                mergeType,
                ShowIncomingChangesFrom.None,
                MergeTypeClassifier.IsIncomingMerge(mergeType),
                null);

            mViewHost.AddRefreshableView(ViewType.MergeView, MergeTab);

            ShowMergeView();
        }

        IMergeView MergeFromInterval(
            RepositorySpec repSpec,
            ObjectInfo objectInfo,
            ObjectInfo ancestorChangesetInfo,
            EnumMergeType mergeType,
            ShowIncomingChangesFrom from,
            List<MergeViewAction> actions)
        {
            if (MergeTypeClassifier.IsIncomingMerge(mergeType))
            {
                ShowIncomingChangesView();
                mParentWindow.Repaint();
                return IncomingChangesTab as IMergeView;
            }

            ShowMergeViewFromInterval(
                repSpec, objectInfo, ancestorChangesetInfo, mergeType, from, actions);
            mParentWindow.Repaint();
            return MergeTab;
        }

        void OpenPendingChangesTab()
        {
            if (PendingChangesTab != null)
                return;

            PendingChangesTab = new PendingChangesTab(
                mWkInfo,
                mViewHost,
                mIsGluonMode,
                mWorkspaceWindow,
                this,
                this,
                this,
                mShowDownloadPlasticExeWindow,
                mWorkspaceOperationsMonitor,
                mDeveloperNewIncomingChangesUpdater,
                mGluonNewIncomingChangesUpdater,
                mAssetStatusCache,
                mStatusBar,
                mParentWindow);

            mViewHost.AddRefreshableView(
                ViewType.CheckinView,
                PendingChangesTab);

            mWorkspaceOperationsMonitor.RegisterPendingChangesView(
                PendingChangesTab);
        }

        void OpenBranchesTab()
        {
            if (BranchesTab == null)
            {
                BranchesTab = new BranchesTab(
                     mWkInfo,
                     mWorkspaceWindow,
                     this,
                     this,
                     mViewHost,
                     mWorkspaceWindow,
                     mWorkspaceWindow,
                     mDeveloperNewIncomingChangesUpdater,
                     mGluonNewIncomingChangesUpdater,
                     mParentWindow,
                     mIsGluonMode);

                mViewHost.AddRefreshableView(
                    ViewType.BranchesView, BranchesTab);
            }

            BoolSetting.Save(true, UnityConstants.SHOW_BRANCHES_VIEW_KEY_NAME);
        }

        void OpenLocksTab()
        {
            if (LocksTab == null)
            {
                LocksTab = new LocksTab(mRepSpec, mWorkspaceWindow, mParentWindow);

                mViewHost.AddRefreshableView(ViewType.LocksView, LocksTab);

                TrackFeatureUseEvent.For(mRepSpec,
                    TrackFeatureUseEvent.Features.OpenLocksView);
            }

            BoolSetting.Save(true, UnityConstants.SHOW_LOCKS_VIEW_KEY_NAME);
        }

        void ShowIncomingChangesView()
        {
            if (IncomingChangesTab == null)
            {
                IncomingChangesTab = BuildIncomingChangesTab(mIsGluonMode);

                mViewHost.AddRefreshableView(
                    ViewType.IncomingChangesView,
                    (IRefreshableView)IncomingChangesTab);

                mWorkspaceOperationsMonitor.RegisterIncomingChangesView(
                    IncomingChangesTab);
            }

            bool wasIncomingChangesSelected =
                IsViewSelected(SelectedTab.IncomingChanges);

            if (!wasIncomingChangesSelected)
                IncomingChangesTab.AutoRefresh();

            SetSelectedView(SelectedTab.IncomingChanges);
        }

        void ShowMergeViewFromInterval(
            RepositorySpec repSpec,
            ObjectInfo objectInfo,
            ObjectInfo ancestorChangesetInfo,
            EnumMergeType mergeType,
            ShowIncomingChangesFrom from,
            List<MergeViewAction> extraActions)
        {
            if (MergeTab != null && MergeTab.IsProcessingMerge)
            {
                ShowMergeView();
                return;
            }

            if (MergeTab != null)
            {
                mViewHost.RemoveRefreshableView(ViewType.MergeView, MergeTab);
                MergeTab.OnDisable();
            }

            MergeTab = BuildMergeTab(
                repSpec, objectInfo, ancestorChangesetInfo, mergeType, from, false, extraActions);

            mViewHost.AddRefreshableView(ViewType.MergeView, MergeTab);

            ShowMergeView();
        }

        void ShowMergeView()
        {
            if (MergeTab == null)
                return;

            bool wasMergeTabSelected =
                IsViewSelected(SelectedTab.Merge);

            if (!wasMergeTabSelected)
                MergeTab.AutoRefresh();

            SetSelectedView(SelectedTab.Merge);
        }

        void ShowHistoryView()
        {
            if (HistoryTab == null)
                return;

            ((IRefreshableView)HistoryTab).Refresh();

            SetSelectedView(SelectedTab.History);
        }

        void CloseBranchesTab()
        {
            BoolSetting.Save(false, UnityConstants.SHOW_BRANCHES_VIEW_KEY_NAME);

            mViewHost.RemoveRefreshableView(
                ViewType.BranchesView, BranchesTab);

            BranchesTab.OnDisable();
            BranchesTab = null;

            ShowPreviousViewFrom(SelectedTab.Branches);

            mParentWindow.Repaint();
        }

        void CloseLocksTab()
        {
            BoolSetting.Save(false, UnityConstants.SHOW_LOCKS_VIEW_KEY_NAME);

            TrackFeatureUseEvent.For(mRepSpec,
                TrackFeatureUseEvent.Features.CloseLocksView);

            mViewHost.RemoveRefreshableView(ViewType.LocksView, LocksTab);

            LocksTab.OnDisable();
            LocksTab = null;

            ShowPreviousViewFrom(SelectedTab.Locks);

            mParentWindow.Repaint();
        }

        void CloseMergeTab()
        {
            mViewHost.RemoveRefreshableView(
                ViewType.MergeView, MergeTab);

            MergeTab.OnDisable();
            MergeTab = null;

            ShowPreviousViewFrom(SelectedTab.Merge);

            mParentWindow.Repaint();
        }

        void CloseHistoryTab()
        {
            mViewHost.RemoveRefreshableView(
                ViewType.HistoryView, HistoryTab);

            HistoryTab.OnDisable();
            HistoryTab = null;

            ShowPreviousViewFrom(SelectedTab.History);

            mParentWindow.Repaint();
        }

        void InitializeTabButtonWidth()
        {
            if (mTabButtonWidth != -1)
                return;

            mTabButtonWidth = MeasureMaxWidth.ForTexts(
                UnityStyles.PlasticWindow.TabButton,
                PlasticLocalization.GetString(PlasticLocalization.Name.PendingChangesViewTitle),
                PlasticLocalization.GetString(PlasticLocalization.Name.IncomingChangesViewTitle),
                PlasticLocalization.GetString(PlasticLocalization.Name.BranchesViewTitle),
                PlasticLocalization.GetString(PlasticLocalization.Name.ChangesetsViewTitle),
                PlasticLocalization.GetString(PlasticLocalization.Name.FileHistory),
                PlasticLocalization.GetString(PlasticLocalization.Name.LocksViewTitle));
        }

        IIncomingChangesTab BuildIncomingChangesTab(bool isGluonMode)
        {
            if (isGluonMode)
            {
                return new IncomingChangesTab(
                    mWkInfo,
                    mViewHost,
                    mWorkspaceWindow,
                    mShowDownloadPlasticExeWindow,
                    mGluonNewIncomingChangesUpdater,
                    (Gluon.IncomingChangesNotifier)mIncomingChangesNotifier,
                    mStatusBar,
                    mParentWindow);
            }

            PlasticNotifier plasticNotifier = new PlasticNotifier();

            MergeViewLogic.IMergeController mergeController = new MergeController(
                mWkInfo,
                mRepSpec,
                null,
                null,
                EnumMergeType.IncomingMerge,
                true,
                plasticNotifier);

            return new MergeTab(
                mWkInfo,
                mRepSpec,
                mWorkspaceWindow,
                this,
                mShowDownloadPlasticExeWindow,
                this,
                mDeveloperNewIncomingChangesUpdater,
                mParentWindow,
                null,
                null,
                EnumMergeType.IncomingMerge,
                ShowIncomingChangesFrom.NotificationBar,
                plasticNotifier,
                mergeController,
                new MergeViewLogic.GetWorkingBranch(),
                true,
                new List<MergeViewAction>());
        }

        MergeTab BuildMergeTab(
            RepositorySpec repSpec,
            ObjectInfo objectInfo,
            ObjectInfo ancestorObjectInfo,
            EnumMergeType mergeType,
            ShowIncomingChangesFrom from,
            bool isIncomingMerge,
            List<MergeViewAction> extraActions)
        {
            PlasticNotifier plasticNotifier = new PlasticNotifier();

            MergeViewLogic.IMergeController mergeController = new MergeController(
                mWkInfo,
                repSpec,
                objectInfo,
                ancestorObjectInfo,
                mergeType,
                false,
                plasticNotifier);

            return new MergeTab(
                mWkInfo,
                repSpec,
                mWorkspaceWindow,
                this,
                mShowDownloadPlasticExeWindow,
                this,
                mDeveloperNewIncomingChangesUpdater,
                mParentWindow,
                objectInfo,
                ancestorObjectInfo,
                mergeType,
                from,
                plasticNotifier,
                mergeController,
                new MergeViewLogic.GetWorkingBranch(),
                isIncomingMerge,
                extraActions);
        }

        void ShowView(SelectedTab viewToShow)
        {
            switch (viewToShow)
            {
                case SelectedTab.PendingChanges:
                    ShowPendingChangesView();
                    break;

                case SelectedTab.IncomingChanges:
                    ShowIncomingChangesView();
                    break;

                case SelectedTab.Changesets:
                    ShowChangesetsView();
                    break;

                case SelectedTab.Branches:
                    ShowBranchesView();
                    break;

                case SelectedTab.Locks:
                    ShowLocksView();
                    break;

                case SelectedTab.Merge:
                    ShowMergeView();
                    break;

                case SelectedTab.History:
                    ShowHistoryView();
                    break;
            }
        }

        void ShowPreviousViewFrom(SelectedTab tabToClose)
        {
            if (!IsViewSelected(tabToClose))
                return;

            if (GetRefreshableViewBasedOnSelectedTab(mPreviousSelectedTab) == null)
                mPreviousSelectedTab = SelectedTab.PendingChanges;

            ShowView(mPreviousSelectedTab);
        }

        void SetSelectedView(SelectedTab tab)
        {
            if (mSelectedTab != tab)
                mPreviousSelectedTab = mSelectedTab;

            mSelectedTab = tab;

            if (IncomingChangesTab == null)
                return;

            IncomingChangesTab.IsVisible =
                tab == SelectedTab.IncomingChanges;
        }

        IRefreshableView GetRefreshableViewBasedOnSelectedTab(SelectedTab selectedTab)
        {
            switch (selectedTab)
            {
                case SelectedTab.PendingChanges:
                    return PendingChangesTab;

                case SelectedTab.IncomingChanges:
                    return (IRefreshableView)IncomingChangesTab;

                case SelectedTab.Changesets:
                    return ChangesetsTab;

                case SelectedTab.Branches:
                    return BranchesTab;

                case SelectedTab.Locks:
                    return LocksTab;

                case SelectedTab.Merge:
                    return MergeTab;

                case SelectedTab.History:
                    return HistoryTab;

                default:
                    return null;
            }
        }

        IRefreshableView GetRefreshableView(ViewType viewType)
        {
            switch (viewType)
            {
                case ViewType.PendingChangesView:
                    return PendingChangesTab;

                case ViewType.IncomingChangesView:
                    return (IRefreshableView)IncomingChangesTab;

                case ViewType.ChangesetsView:
                    return ChangesetsTab;

                case ViewType.BranchesView:
                    return BranchesTab;

                case ViewType.LocksView:
                    return LocksTab;

                case ViewType.MergeView:
                    return MergeTab;

                case ViewType.HistoryView:
                    return HistoryTab;

                default:
                    return null;
            }
        }

        void PendingChangesTabButtonGUI()
        {
            bool wasPendingChangesSelected =
                IsViewSelected(SelectedTab.PendingChanges);

            bool isPendingChangesSelected = mPendingChangesTabButton.
                DrawTabButton(
                    PlasticLocalization.GetString(PlasticLocalization.Name.PendingChangesViewTitle),
                    wasPendingChangesSelected,
                    mTabButtonWidth);

            if (isPendingChangesSelected)
                ShowPendingChangesView();
        }

        void IncomingChangesTabButtonGUI()
        {
            bool wasIncomingChangesSelected =
                IsViewSelected(SelectedTab.IncomingChanges);

            bool isIncomingChangesSelected = mIncomingChangesTabButton.
                DrawTabButton(
                    PlasticLocalization.GetString(PlasticLocalization.Name.IncomingChangesViewTitle),
                    wasIncomingChangesSelected,
                    mTabButtonWidth);

            if (isIncomingChangesSelected)
                ShowIncomingChangesView();
        }

        void ChangesetsTabButtonGUI()
        {
            bool wasChangesetsSelected =
                IsViewSelected(SelectedTab.Changesets);

            bool isChangesetsSelected = mChangesetsTabButton.
                DrawTabButton(
                    PlasticLocalization.GetString(PlasticLocalization.Name.ChangesetsViewTitle),
                    wasChangesetsSelected,
                    mTabButtonWidth);

            if (isChangesetsSelected)
                ShowChangesetsView();
        }

        void BranchesTabButtonGUI()
        {
            if (BranchesTab == null)
                return;

            bool wasBranchesSelected =
                 IsViewSelected(SelectedTab.Branches);

            bool isCloseButtonClicked;

            bool isBranchesSelected = mBranchesTabButton.
                DrawClosableTabButton(PlasticLocalization.GetString(
                    PlasticLocalization.Name.BranchesViewTitle),
                    wasBranchesSelected,
                    true,
                    mTabButtonWidth,
                    mParentWindow.Repaint,
                    out isCloseButtonClicked);

            if (isCloseButtonClicked)
            {
                CloseBranchesTab();
                return;
            }

            if (isBranchesSelected)
                SetSelectedView(SelectedTab.Branches);
        }

        void LocksTabButtonGUI()
        {
            if (LocksTab == null)
            {
                return;
            }

            var wasLocksTabSelected = IsViewSelected(SelectedTab.Locks);

            bool isCloseButtonClicked;

            var isLocksTabSelected = mLocksTabButton.DrawClosableTabButton(
                PlasticLocalization.Name.LocksViewTitle.GetString(),
                wasLocksTabSelected,
                true,
                mTabButtonWidth,
                mParentWindow.Repaint,
                out isCloseButtonClicked);

            if (isCloseButtonClicked)
            {
                CloseLocksTab();
                return;
            }

            if (isLocksTabSelected)
            {
                SetSelectedView(SelectedTab.Locks);
            }
        }

        void MergeTabButtonGUI()
        {
            if (MergeTab == null)
                return;

            bool wasMergeSelected =
                IsViewSelected(SelectedTab.Merge);

            bool isCloseButtonClicked;

            bool isMergeSelected = mMergeTabButton.
                DrawClosableTabButton(
                    PlasticLocalization.Name.MainSidebarMergeItem.GetString(),
                    wasMergeSelected,
                    true,
                    mTabButtonWidth,
                    mParentWindow.Repaint,
                    out isCloseButtonClicked);

            if (isCloseButtonClicked)
            {
                CloseMergeTab();
                return;
            }

            if (isMergeSelected)
                ShowMergeView();
        }

        void HistoryTabButtonGUI()
        {
            if (HistoryTab == null)
                return;

            bool wasHistorySelected =
                IsViewSelected(SelectedTab.History);

            bool isCloseButtonClicked;

            bool isHistorySelected = mHistoryTabButton.
                DrawClosableTabButton(
                    PlasticLocalization.GetString(PlasticLocalization.Name.FileHistory),
                    wasHistorySelected,
                    true,
                    mTabButtonWidth,
                    mParentWindow.Repaint,
                    out isCloseButtonClicked);

            if (isCloseButtonClicked)
            {
                CloseHistoryTab();
                return;
            }

            if (isHistorySelected)
                SetSelectedView(SelectedTab.History);
        }

        float mTabButtonWidth = -1;

        [SerializeField]
        SelectedTab mSelectedTab;
        SelectedTab mPreviousSelectedTab;

        TabButton mPendingChangesTabButton;
        TabButton mIncomingChangesTabButton;
        TabButton mBranchesTabButton;
        TabButton mChangesetsTabButton;
        TabButton mLocksTabButton;
        TabButton mMergeTabButton;
        TabButton mHistoryTabButton;

        IIncomingChangesNotifier mIncomingChangesNotifier;
        GluonNewIncomingChangesUpdater mGluonNewIncomingChangesUpdater;
        NewIncomingChangesUpdater mDeveloperNewIncomingChangesUpdater;
        WorkspaceWindow mWorkspaceWindow;

        readonly EditorWindow mParentWindow;
        readonly StatusBar mStatusBar;
        readonly WorkspaceOperationsMonitor mWorkspaceOperationsMonitor;
        readonly LaunchTool.IProcessExecutor mProcessExecutor;
        readonly LaunchTool.IShowDownloadPlasticExeWindow mShowDownloadPlasticExeWindow;
        readonly IAssetStatusCache mAssetStatusCache;
        readonly bool mIsGluonMode;
        readonly ViewHost mViewHost;
        readonly WorkspaceInfo mWkInfo;
        readonly RepositorySpec mRepSpec;
    }
}
