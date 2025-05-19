using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEngine;

using Codice.Client.BaseCommands;
using Codice.Client.Commands;
using Codice.Client.Common;
using Codice.Client.Common.FsNodeReaders;
using Codice.CM.Common;
using GluonGui;
using PlasticGui;
using PlasticGui.Gluon.WorkspaceWindow;
using PlasticGui.Gluon.WorkspaceWindow.Views.IncomingChanges;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.Diff;
using PlasticGui.WorkspaceWindow.Merge;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.Tool;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;
using Unity.PlasticSCM.Editor.UI.StatusBar;
using Unity.PlasticSCM.Editor.UI.Tree;
using Unity.PlasticSCM.Editor.Views.IncomingChanges.Gluon.Errors;
using Unity.PlasticSCM.Editor.Views.Merge;
using UnityEditor.IMGUI.Controls;
using CheckIncomingChanges = PlasticGui.Gluon.WorkspaceWindow.CheckIncomingChanges;
using NewIncomingChangesUpdater = PlasticGui.Gluon.WorkspaceWindow.NewIncomingChangesUpdater;

namespace Unity.PlasticSCM.Editor.Views.IncomingChanges.Gluon
{
    internal class IncomingChangesTab :
        IIncomingChangesTab,
        IRefreshableView,
        IncomingChangesViewLogic.IIncomingChangesView,
        IIncomingChangesViewMenuOperations,
        IncomingChangesViewMenu.IMetaMenuOperations
    {
        internal IncomingChangesTab(
            WorkspaceInfo wkInfo,
            ViewHost viewHost,
            WorkspaceWindow workspaceWindow,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            NewIncomingChangesUpdater newIncomingChangesUpdater,
            CheckIncomingChanges.IUpdateIncomingChanges updateIncomingChanges,
            StatusBar statusBar,
            EditorWindow parentWindow)
        {
            mWkInfo = wkInfo;
            mShowDownloadPlasticExeWindow = showDownloadPlasticExeWindow;
            mNewIncomingChangesUpdater = newIncomingChangesUpdater;
            mParentWindow = parentWindow;
            mStatusBar = statusBar;

            BuildComponents();

            mProgressControls = new ProgressControlsForViews();

            mCooldownClearUpdateSuccessAction = new CooldownWindowDelayer(
                DelayedClearUpdateSuccess,
                UnityConstants.NOTIFICATION_CLEAR_INTERVAL);

            mErrorsSplitterState = PlasticSplitterGUILayout.InitSplitterState(
                new float[] { 0.75f, 0.25f },
                new int[] { 100, 100 },
                new int[] { 100000, 100000 }
            );

            mErrorDetailsSplitterState = PlasticSplitterGUILayout.InitSplitterState(
                new float[] { 0.60f, 0.40f },
                new int[] { 100, 100 },
                new int[] { 100000, 100000 }
            );

            mIncomingChangesViewLogic = new IncomingChangesViewLogic(
                wkInfo, viewHost, this, new UnityPlasticGuiMessage(),
                mProgressControls, updateIncomingChanges,
                workspaceWindow.GluonProgressOperationHandler, workspaceWindow,
                new IncomingChangesViewLogic.ApplyGluonWorkspaceLocalChanges(),
                new IncomingChangesViewLogic.OutOfDateItemsOperations(),
                new IncomingChangesViewLogic.GetWorkingBranch(),
                new IncomingChangesViewLogic.ResolveUserName(),
                new ResolveChangeset(),
                NewChangesInWk.Build(wkInfo, new BuildWorkspacekIsRelevantNewChange()),
                null);

            mIncomingChangesViewLogic.Refresh();
        }

        bool IIncomingChangesTab.IsVisible
        {
            get { return mIsVisible; }
            set { mIsVisible = value; }
        }

        void IIncomingChangesTab.OnEnable()
        {
            mSearchField.downOrUpArrowKeyPressed +=
                SearchField_OnDownOrUpArrowKeyPressed;
        }

        void IIncomingChangesTab.OnDisable()
        {
            mSearchField.downOrUpArrowKeyPressed -=
                SearchField_OnDownOrUpArrowKeyPressed;

            TreeHeaderSettings.Save(
                mIncomingChangesTreeView.multiColumnHeader.state,
                UnityConstants.GLUON_INCOMING_CHANGES_TABLE_SETTINGS_NAME);

            TreeHeaderSettings.Save(
                mErrorsListView.multiColumnHeader.state,
                UnityConstants.GLUON_INCOMING_ERRORS_TABLE_SETTINGS_NAME);
        }

        void IIncomingChangesTab.Update()
        {
            mProgressControls.UpdateProgress(mParentWindow);
        }

        void IIncomingChangesTab.OnGUI()
        {
            bool splitterNeeded = mIsErrorsListVisible;

            if (splitterNeeded)
                PlasticSplitterGUILayout.BeginVerticalSplit(mErrorsSplitterState);

            DoIncomingChangesArea(
                mIncomingChangesTreeView,
                mProgressControls.IsOperationRunning(),
                mHasNothingToDownload,
                mIsUpdateSuccessful);

            DoErrorsArea(
                mErrorsListView,
                mErrorDetailsSplitterState,
                mIsErrorsListVisible);

            if (splitterNeeded)
                PlasticSplitterGUILayout.EndVerticalSplit();

            DrawActionToolbar.Begin(mParentWindow);

            if (!mProgressControls.IsOperationRunning())
            {
                DoActionToolbarMessage(
                    mIsMessageLabelVisible,
                    mMessageLabelText,
                    mHasNothingToDownload,
                    mIsErrorMessageLabelVisible,
                    mErrorMessageLabelText,
                    mFileConflictCount,
                    mChangesSummary);

                if (mIsProcessMergesButtonVisible)
                {
                    DoProcessMergesButton(
                        mIsProcessMergesButtonEnabled,
                        mProcessMergesButtonText,
                        mShowDownloadPlasticExeWindow,
                        mIncomingChangesViewLogic,
                        mIncomingChangesTreeView,
                        mWkInfo,
                        RefreshAsset.BeforeLongAssetOperation,
                        (c) => AfterProcessMerges(c));
                }

                if (mIsCancelMergesButtonVisible)
                {
                    mIsCancelMergesButtonEnabled = DoCancelMergesButton(
                        mIsCancelMergesButtonEnabled,
                        mIncomingChangesViewLogic);
                }
            }
            else
            {
                DrawProgressForViews.ForIndeterminateProgress(
                    mProgressControls.ProgressData);
            }

            DrawActionToolbar.End();

            if (mProgressControls.HasNotification())
            {
                DrawProgressForViews.ForNotificationArea(
                    mProgressControls.ProgressData);
            }
        }

        void IIncomingChangesTab.DrawSearchFieldForTab()
        {
            // We don't have the filtering implemented at the plasticgui level: IncomingChangesTree.Filter
            // When it's implemented, just uncomment this method to show the filter textbox for partial workspaces
            // VCS-1006158 [Desktop] gluon: add filter in incoming changes view

            /*
            DrawSearchField.For(
                mSearchField,
                mIncomingChangesTreeView,
                UnityConstants.SEARCH_FIELD_WIDTH);
            */
        }

        void IIncomingChangesTab.AutoRefresh()
        {
            mIncomingChangesViewLogic.AutoRefresh(DateTime.Now);
        }

        void IRefreshableView.Refresh()
        {
            if (mNewIncomingChangesUpdater != null)
                mNewIncomingChangesUpdater.Update(DateTime.Now);

            mIncomingChangesViewLogic.Refresh();
        }

        void IncomingChangesViewLogic.IIncomingChangesView.UpdateData(
            IncomingChangesTree tree,
            List<ErrorMessage> errorMessages,
            string processMergesButtonText,
            PendingConflictsLabelData conflictsLabelData,
            string changesToApplySummaryText)
        {
            ShowProcessMergesButton(processMergesButtonText);

            ((IncomingChangesViewLogic.IIncomingChangesView)this).
                UpdatePendingConflictsLabel(conflictsLabelData);

            UpdateIncomingChangesTree(mIncomingChangesTreeView, tree);

            UpdateErrorsList(mErrorsListView, errorMessages);

            mIsErrorsListVisible = errorMessages.Count > 0;

            UpdateOverview(tree, conflictsLabelData);
        }

        void IncomingChangesViewLogic.IIncomingChangesView.UpdatePendingConflictsLabel(
            PendingConflictsLabelData data)
        {
        }

        void IncomingChangesViewLogic.IIncomingChangesView.UpdateSolvedFileConflicts(
            List<IncomingChangeInfo> solvedConflicts,
            IncomingChangeInfo currentConflict)
        {
            mIncomingChangesTreeView.UpdateSolvedFileConflicts(
                solvedConflicts, currentConflict);
        }

        void IncomingChangesViewLogic.IIncomingChangesView.ShowMessage(
            string message, bool isErrorMessage)
        {
            if (isErrorMessage)
            {
                mErrorMessageLabelText = message;
                mIsErrorMessageLabelVisible = true;
                return;
            }

            mMessageLabelText = message;
            mIsMessageLabelVisible = true;
            mHasNothingToDownload = message == PlasticLocalization.GetString(
                PlasticLocalization.Name.MergeNothingToDownloadForIncomingView);
        }

        void IncomingChangesViewLogic.IIncomingChangesView.HideMessage()
        {
            mMessageLabelText = string.Empty;
            mIsMessageLabelVisible = false;
            mHasNothingToDownload = false;

            mErrorMessageLabelText = string.Empty;
            mIsErrorMessageLabelVisible = false;
        }

        void IncomingChangesViewLogic.IIncomingChangesView.DisableProcessMergesButton()
        {
            mIsProcessMergesButtonEnabled = false;
        }

        void IncomingChangesViewLogic.IIncomingChangesView.ShowCancelButton()
        {
            mIsCancelMergesButtonEnabled = true;
            mIsCancelMergesButtonVisible = true;
        }

        void IncomingChangesViewLogic.IIncomingChangesView.HideCancelButton()
        {
            mIsCancelMergesButtonEnabled = false;
            mIsCancelMergesButtonVisible = false;
        }

        SelectedIncomingChangesGroupInfo IIncomingChangesViewMenuOperations.GetSelectedIncomingChangesGroupInfo()
        {
            return IncomingChangesSelection.GetSelectedGroupInfo(mIncomingChangesTreeView);
        }

        void IIncomingChangesViewMenuOperations.MergeContributors()
        {
            List<IncomingChangeInfo> fileConflicts = IncomingChangesSelection.
                GetSelectedFileConflictsIncludingMeta(mIncomingChangesTreeView);

            mIncomingChangesViewLogic.ProcessMergesForConflicts(
                MergeContributorType.MergeContributors,
                PlasticExeLauncher.BuildForMergeSelectedFiles(mWkInfo, true, mShowDownloadPlasticExeWindow),
                fileConflicts,
                RefreshAsset.BeforeLongAssetOperation,
                AfterProcessMerges);
        }

        void IIncomingChangesViewMenuOperations.MergeKeepingSourceChanges()
        {
            List<IncomingChangeInfo> fileConflicts = IncomingChangesSelection.
                GetSelectedFileConflictsIncludingMeta(mIncomingChangesTreeView);

            mIncomingChangesViewLogic.ProcessMergesForConflicts(
                MergeContributorType.KeepSource,
                null,
                fileConflicts,
                RefreshAsset.BeforeLongAssetOperation,
                AfterProcessMerges);
        }

        void IIncomingChangesViewMenuOperations.MergeKeepingWorkspaceChanges()
        {
            List<IncomingChangeInfo> fileConflicts = IncomingChangesSelection.
                GetSelectedFileConflictsIncludingMeta(mIncomingChangesTreeView);

            mIncomingChangesViewLogic.ProcessMergesForConflicts(
                MergeContributorType.KeepDestination,
                null,
                fileConflicts,
                RefreshAsset.BeforeLongAssetOperation,
                AfterProcessMerges);
        }

        void IIncomingChangesViewMenuOperations.DiffIncomingChanges()
        {
            IncomingChangeInfo incomingChange = IncomingChangesSelection.
                GetSingleSelectedIncomingChange(mIncomingChangesTreeView);

            if (incomingChange == null)
                return;

            DiffIncomingChanges(
                mShowDownloadPlasticExeWindow,
                incomingChange,
                mWkInfo);
        }

        void IIncomingChangesViewMenuOperations.DiffYoursWithIncoming()
        {
            IncomingChangeInfo incomingChange = IncomingChangesSelection.
                GetSingleSelectedIncomingChange(mIncomingChangesTreeView);

            if (incomingChange == null)
                return;

            DiffYoursWithIncoming(
                mShowDownloadPlasticExeWindow,
                incomingChange,
                mWkInfo);
        }

        void IIncomingChangesViewMenuOperations.CopyFilePath(bool relativePath)
        {
            EditorGUIUtility.systemCopyBuffer = GetFilePathList.FromIncomingChangeInfos(
                IncomingChangesSelection.GetSelectedFileConflictsIncludingMeta(
                    mIncomingChangesTreeView),
                relativePath,
                mWkInfo.ClientPath);
        }

        void IncomingChangesViewMenu.IMetaMenuOperations.DiffIncomingChanges()
        {
            IncomingChangeInfo incomingChange = IncomingChangesSelection.
                GetSingleSelectedIncomingChange(mIncomingChangesTreeView);

            if (incomingChange == null)
                return;

            DiffIncomingChanges(
                mShowDownloadPlasticExeWindow,
                mIncomingChangesTreeView.GetMetaChange(incomingChange),
                mWkInfo);
        }

        void IncomingChangesViewMenu.IMetaMenuOperations.DiffYoursWithIncoming()
        {
            IncomingChangeInfo incomingChange = IncomingChangesSelection.
                GetSingleSelectedIncomingChange(mIncomingChangesTreeView);

            if (incomingChange == null)
                return;

            DiffYoursWithIncoming(
                mShowDownloadPlasticExeWindow,
                mIncomingChangesTreeView.GetMetaChange(incomingChange),
                mWkInfo);
        }

        bool IncomingChangesViewMenu.IMetaMenuOperations.SelectionHasMeta()
        {
            return mIncomingChangesTreeView.SelectionHasMeta();
        }

        void SearchField_OnDownOrUpArrowKeyPressed()
        {
            mIncomingChangesTreeView.SetFocusAndEnsureSelectedItem();
        }

        static void DiffIncomingChanges(
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            IncomingChangeInfo incomingChange,
            WorkspaceInfo wkInfo)
        {
            DiffOperation.DiffRevisions(
                wkInfo,
                incomingChange.GetMount().RepSpec,
                incomingChange.GetBaseRevision(),
                incomingChange.GetRevision(),
                incomingChange.GetPath(),
                incomingChange.GetPath(),
                true,
                PlasticExeLauncher.BuildForDiffContributors(wkInfo, true, showDownloadPlasticExeWindow),
                imageDiffLauncher: null);
        }

        static void DiffYoursWithIncoming(
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            IncomingChangeInfo incomingChange,
            WorkspaceInfo wkInfo)
        {
            DiffOperation.DiffYoursWithIncoming(
                wkInfo,
                incomingChange.GetMount(),
                incomingChange.GetRevision(),
                incomingChange.GetPath(),
                PlasticExeLauncher.BuildForDiffContributors(wkInfo, true, showDownloadPlasticExeWindow),
                imageDiffLauncher: null);
        }

        static void UpdateErrorsList(
            ErrorsListView errorsListView,
            List<ErrorMessage> errorMessages)
        {
            errorsListView.BuildModel(errorMessages);

            errorsListView.Reload();
        }

        void UpdateProcessMergesButtonText()
        {
            mProcessMergesButtonText =
                mIncomingChangesViewLogic.GetProcessMergesButtonText();
        }

        void ShowProcessMergesButton(string processMergesButtonText)
        {
            mProcessMergesButtonText = processMergesButtonText;
            mIsProcessMergesButtonEnabled = true;
            mIsProcessMergesButtonVisible = true;
        }

        static void DoActionToolbarMessage(
            bool isMessageLabelVisible,
            string messageLabelText,
            bool hasNothingToDownload,
            bool isErrorMessageLabelVisible,
            string errorMessageLabelText,
            int fileConflictCount,
            MergeViewTexts.ChangesToApplySummary changesSummary)
        {
            if (isMessageLabelVisible)
            {
                string message = messageLabelText;

                if (hasNothingToDownload)
                {
                    message = PlasticLocalization.GetString(
                        PlasticLocalization.Name.WorkspaceIsUpToDate);
                }

                DoInfoMessage(message);
            }

            if (isErrorMessageLabelVisible)
            {
                DoErrorMessage(errorMessageLabelText);
            }

            if (!isMessageLabelVisible && !isErrorMessageLabelVisible)
            {
                DrawMergeOverview.For(
                    0,
                    fileConflictCount,
                    changesSummary);
            }
        }

        static void DoInfoMessage(string message)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(message, UnityStyles.MergeTab.ChangesToApplySummaryLabel);

            EditorGUILayout.EndHorizontal();
        }

        static void DoErrorMessage(string message)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(message, UnityStyles.MergeTab.RedPendingConflictsOfTotalLabel);

            EditorGUILayout.EndHorizontal();
        }

        static void DoIncomingChangesArea(
            IncomingChangesTreeView incomingChangesTreeView,
            bool isOperationRunning,
            bool hasNothingToDownload,
            bool isUpdateSuccessful)
        {
            EditorGUILayout.BeginVertical();

            DoIncomingChangesTreeViewArea(
                incomingChangesTreeView,
                isOperationRunning,
                hasNothingToDownload,
                isUpdateSuccessful);

            EditorGUILayout.EndVertical();
        }

        static void DoProcessMergesButton(
            bool isEnabled,
            string processMergesButtonText,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            IncomingChangesViewLogic incomingChangesViewLogic,
            IncomingChangesTreeView incomingChangesTreeView,
            WorkspaceInfo wkInfo,
            Action beforeProcessMergesAction,
            EndOperationDelegateForUpdatedItems afterProcessMergesAction)
        {
            GUI.enabled = isEnabled;

            if (DrawActionButton.For(processMergesButtonText))
            {
                List<IncomingChangeInfo> incomingChanges =
                    incomingChangesViewLogic.GetCheckedChanges();

                incomingChangesTreeView.FillWithMeta(incomingChanges);

                if (incomingChanges.Count == 0)
                    return;

                incomingChangesViewLogic.ProcessMergesForItems(
                    incomingChanges,
                    PlasticExeLauncher.BuildForResolveConflicts(wkInfo, true, showDownloadPlasticExeWindow),
                    beforeProcessMergesAction,
                    afterProcessMergesAction);
            }

            GUI.enabled = true;
        }

        static bool DoCancelMergesButton(
            bool isEnabled,
            IncomingChangesViewLogic incomingChangesViewLogic)
        {
            bool shouldCancelMergesButtonEnabled = true;

            GUI.enabled = isEnabled;

            if (DrawActionButton.For(PlasticLocalization.GetString(
                    PlasticLocalization.Name.CancelButton)))
            {
                incomingChangesViewLogic.Cancel();

                shouldCancelMergesButtonEnabled = false;
            }

            GUI.enabled = true;

            return shouldCancelMergesButtonEnabled;
        }

        void DoErrorsArea(
            ErrorsListView errorsListView,
            object splitterState,
            bool isErrorsListVisible)
        {
            EditorGUILayout.BeginVertical();

            if (isErrorsListVisible)
            {
                DrawSplitter.ForHorizontalIndicator();
                DoErrorsListArea(errorsListView, splitterState);
            }

            EditorGUILayout.EndVertical();
        }

        static void UpdateIncomingChangesTree(
            IncomingChangesTreeView incomingChangesTreeView,
            IncomingChangesTree tree)
        {
            incomingChangesTreeView.BuildModel(
                UnityIncomingChangesTree.BuildIncomingChangeCategories(tree));
            incomingChangesTreeView.Refilter();
            incomingChangesTreeView.Sort();
            incomingChangesTreeView.Reload();
        }

        static void DoIncomingChangesTreeViewArea(
            IncomingChangesTreeView incomingChangesTreeView,
            bool isOperationRunning,
            bool hasNothingToDownload,
            bool isUpdateSuccessful)
        {
            GUI.enabled = !isOperationRunning;

            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            incomingChangesTreeView.OnGUI(rect);

            if (hasNothingToDownload)
                DrawEmptyState(rect, isUpdateSuccessful);

            GUI.enabled = true;
        }

        static void DrawEmptyState(
            Rect rect,
            bool isUpdateSuccessful)
        {
            if (isUpdateSuccessful)
            {
                DrawTreeViewEmptyState.For(
                    rect,
                    PlasticLocalization.GetString(PlasticLocalization.Name.WorkspaceUpdateCompleted),
                    Images.GetStepOkIcon());

                return;
            }

            DrawTreeViewEmptyState.For(
                rect,
                PlasticLocalization.GetString(PlasticLocalization.Name.NoIncomingChanges));
        }

        void DoErrorsListArea(
            ErrorsListView errorsListView,
            object splitterState)
        {
            EditorGUILayout.BeginVertical();

            GUILayout.Label(
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.IncomingChangesCannotBeApplied),
                EditorStyles.boldLabel);

            DoErrorsListSplitViewArea(
                errorsListView, splitterState);

            EditorGUILayout.EndVertical();
        }

        void DoErrorsListSplitViewArea(
            ErrorsListView errorsListView,
            object splitterState)
        {
            EditorGUILayout.BeginHorizontal();

            PlasticSplitterGUILayout.BeginHorizontalSplit(splitterState);

            DoErrorsListViewArea(errorsListView);

            DoErrorDetailsTextArea(errorsListView.GetSelectedError());

            PlasticSplitterGUILayout.EndHorizontalSplit();

            EditorGUILayout.EndHorizontal();
        }

        static void DoErrorsListViewArea(
            ErrorsListView errorsListView)
        {
            Rect treeRect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            errorsListView.OnGUI(treeRect);
        }

        void DoErrorDetailsTextArea(ErrorMessage selectedErrorMessage)
        {
            string errorDetailsText = selectedErrorMessage == null ?
                string.Empty : selectedErrorMessage.Error;

            mErrorDetailsScrollPosition = GUILayout.BeginScrollView(
                mErrorDetailsScrollPosition);

            GUILayout.TextArea(
                errorDetailsText, UnityStyles.TextFieldWithWrapping,
                GUILayout.ExpandHeight(true));

            GUILayout.EndScrollView();
        }

        void AfterProcessMerges(List<string> changes)
        {
            RefreshAsset.AfterLongAssetOperation(
                ProjectPackages.ShouldBeResolved(changes, mWkInfo, true));

            bool isTreeViewEmpty = mIncomingChangesTreeView.GetCheckedItemCount() ==
                mIncomingChangesTreeView.GetTotalItemCount();

            if (isTreeViewEmpty)
            {
                mIsUpdateSuccessful = true;
                mCooldownClearUpdateSuccessAction.Ping();
                return;
            }

            mStatusBar.Notify(
                PlasticLocalization.GetString(PlasticLocalization.Name.WorkspaceUpdateCompleted),
                MessageType.None,
                Images.GetStepOkIcon());
        }

        void UpdateOverview(
            IncomingChangesTree incomingChangesTree,
            PendingConflictsLabelData conflictsLabelData)
        {
            mChangesSummary = BuildFilesSummaryData.
                GetChangesToApplySummary(incomingChangesTree);

            mFileConflictCount = conflictsLabelData.PendingConflictsCount;
        }

        void DelayedClearUpdateSuccess()
        {
            mIsUpdateSuccessful = false;
        }

        void BuildComponents()
        {
            mSearchField = new SearchField();
            mSearchField.downOrUpArrowKeyPressed +=
                SearchField_OnDownOrUpArrowKeyPressed;

            IncomingChangesTreeHeaderState incomingChangesHeaderState =
                IncomingChangesTreeHeaderState.GetDefault();
            TreeHeaderSettings.Load(incomingChangesHeaderState,
                UnityConstants.GLUON_INCOMING_CHANGES_TABLE_SETTINGS_NAME,
                (int)IncomingChangesTreeColumn.Path, true);

            mIncomingChangesTreeView = new IncomingChangesTreeView(
                mWkInfo, incomingChangesHeaderState,
                IncomingChangesTreeHeaderState.GetColumnNames(),
                new IncomingChangesViewMenu(this, this),
                UpdateProcessMergesButtonText);
            mIncomingChangesTreeView.Reload();

            ErrorsListHeaderState errorsListHeaderState =
                ErrorsListHeaderState.GetDefault();
            TreeHeaderSettings.Load(errorsListHeaderState,
                UnityConstants.GLUON_INCOMING_ERRORS_TABLE_SETTINGS_NAME,
                UnityConstants.UNSORT_COLUMN_ID);

            mErrorsListView = new ErrorsListView(errorsListHeaderState);
            mErrorsListView.Reload();
        }

        bool mIsVisible;
        bool mIsProcessMergesButtonVisible;
        bool mIsCancelMergesButtonVisible;
        bool mIsMessageLabelVisible;
        bool mIsErrorMessageLabelVisible;
        bool mIsErrorsListVisible;

        bool mIsProcessMergesButtonEnabled;
        bool mIsCancelMergesButtonEnabled;

        string mProcessMergesButtonText;
        string mMessageLabelText;
        string mErrorMessageLabelText;
        bool mHasNothingToDownload;
        bool mIsUpdateSuccessful;

        SearchField mSearchField;

        IncomingChangesTreeView mIncomingChangesTreeView;
        ErrorsListView mErrorsListView;

        int mFileConflictCount;
        MergeViewTexts.ChangesToApplySummary mChangesSummary;

        object mErrorsSplitterState;
        object mErrorDetailsSplitterState;

        readonly ProgressControlsForViews mProgressControls;
        readonly CooldownWindowDelayer mCooldownClearUpdateSuccessAction;
        Vector2 mErrorDetailsScrollPosition;

        readonly IncomingChangesViewLogic mIncomingChangesViewLogic;
        readonly EditorWindow mParentWindow;
        readonly StatusBar mStatusBar;
        readonly NewIncomingChangesUpdater mNewIncomingChangesUpdater;
        readonly LaunchTool.IShowDownloadPlasticExeWindow mShowDownloadPlasticExeWindow;
        readonly WorkspaceInfo mWkInfo;
    }
}
