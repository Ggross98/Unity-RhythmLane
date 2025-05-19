using System;

using UnityEditor;
using UnityEngine;

using Codice.Client.Commands;
using Codice.Client.Common.FsNodeReaders;
using Codice.Client.Common.Threading;
using Codice.CM.Common;
using Codice.Utils;
using PlasticGui;
using PlasticGui.WorkspaceWindow.PendingChanges;
using Unity.PlasticSCM.Editor.UI;

using AssetPostprocessor = Unity.PlasticSCM.Editor.AssetUtils.Processor.AssetPostprocessor;

namespace Unity.PlasticSCM.Editor.Preferences
{
    class PendingChangesOptionsFoldout
    {
        internal void OnActivate(WorkspaceInfo wkInfo)
        {
            mWkInfo = wkInfo;

            IAutoRefreshView autoRefreshView = GetPendingChangesView();

            if (autoRefreshView != null)
                autoRefreshView.DisableAutoRefresh();

            CheckFsWatcher(mWkInfo);

            mAutomaticAdd = AssetPostprocessor.AutomaticAdd;

            mPendingChangesSavedOptions = new PendingChangesOptions();
            mPendingChangesSavedOptions.LoadPendingChangesOptions();

            SetPendingChangesOptions(mPendingChangesSavedOptions);
        }

        internal void OnDeactivate()
        {
            bool arePendingChangesOptionsChanged = false;

            try
            {
                AssetPostprocessor.SetAutomaticAddOption(mAutomaticAdd);

                PendingChangesOptions newPendingChangesOptions = GetPendingChangesOptions();

                arePendingChangesOptionsChanged = !mPendingChangesSavedOptions.AreSameOptions(newPendingChangesOptions);

                if (arePendingChangesOptionsChanged)
                {
                    newPendingChangesOptions.SavePreferences();
                }
            }
            finally
            {
                IAutoRefreshView autoRefreshView = GetPendingChangesView();

                if (autoRefreshView != null)
                {
                    autoRefreshView.EnableAutoRefresh();

                    if (arePendingChangesOptionsChanged)
                    {
                        autoRefreshView.ForceRefresh();
                    }
                }
            }
        }

        internal void OnGUI()
        {
            DrawSplitter.ForWidth(UnityConstants.SETTINGS_GUI_WIDTH);

            DrawSettingsSection(
                DoPendingChangesSettings);
        }

        void DoPendingChangesSettings()
        {
            DoGeneralSettings();

            DoWhatToFindSettings();

            DoWhatToShowSettings();

            DoMoveDetectionSettings();
        }

        void DoGeneralSettings()
        {
            mAutomaticAdd = EditorGUILayout.Toggle(Styles.AutomaticAdd, mAutomaticAdd);
            mAutoRefresh = EditorGUILayout.Toggle(Styles.AutoRefresh, mAutoRefresh);
        }

        void DoWhatToFindSettings()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label(
                PlasticLocalization.Name.PendingChangesWhatToFindTab.GetString(),
                UnityStyles.ProjectSettings.SectionTitle);

            mShowCheckouts = EditorGUILayout.Toggle(Styles.ShowCheckouts, mShowCheckouts);
            mShowChangedFiles = EditorGUILayout.Toggle(Styles.ShowChangedFiles, mShowChangedFiles);

            DrawTabbedContent(
                DoCheckFileContentCheckbox);

            DoFsWatcherMessage(mFSWatcherEnabled);
        }

        void DoCheckFileContentCheckbox()
        {
            mCheckFileContent = EditorGUILayout.Toggle(Styles.CheckFileContent, mCheckFileContent);
        }

        void DoFsWatcherMessage(bool isEnabled)
        {
            GUIContent message = new GUIContent(
                isEnabled ?
                    GetFsWatcherEnabledMessage() :
                    GetFsWatcherDisabledMessage(),
                isEnabled ?
                    Images.GetInfoIcon() :
                    Images.GetWarnIcon());

            GUILayout.Label(message, UnityStyles.ProjectSettings.Title, GUILayout.Height(26));
            GUILayout.Space(-4);

            string formattedExplanation = isEnabled ?
                GetFsWatcherEnabledExplanation() :
                GetFsWatcherDisabledExplanation();

            ExternalLink externalLink = new ExternalLink
            {
                Label = PlasticLocalization.Name.UnityVersionControlSupport.GetString(),
                Url = SUPPORT_URL
            };

            DrawTextBlockWithEndLink.For(externalLink, formattedExplanation, UnityStyles.ProjectSettings.Paragraph);
        }

        void DoWhatToShowSettings()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label(
                PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesWhatToShowTab),
                UnityStyles.ProjectSettings.SectionTitle);

            mUseChangeLists = EditorGUILayout.Toggle(Styles.UseChangeLists, mUseChangeLists);
            mShowPrivateFields = EditorGUILayout.Toggle(Styles.ShowPrivateFields, mShowPrivateFields);
            mShowIgnoredFiles = EditorGUILayout.Toggle(Styles.ShowIgnoredFields, mShowIgnoredFiles);
            mShowHiddenFiles = EditorGUILayout.Toggle(Styles.ShowHiddenFields, mShowHiddenFiles);
            mShowDeletedFiles = EditorGUILayout.Toggle(Styles.ShowDeletedFilesDirs, mShowDeletedFiles);
        }

        void DoMoveDetectionSettings()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label(
                PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesMoveDetectionTab),
                UnityStyles.ProjectSettings.SectionTitle);

            mShowMovedFiles = EditorGUILayout.Toggle(Styles.ShowMovedFiles, mShowMovedFiles);

            DrawTabbedContent(
                DoMovedFilesCheckboxes);
        }

        void DoMovedFilesCheckboxes()
        {
            mMatchBinarySameExtension =
                EditorGUILayout.Toggle(Styles.MatchBinarySameExtension, mMatchBinarySameExtension);
            mMatchTextSameExtension = EditorGUILayout.Toggle(Styles.MatchTextSameExtension, mMatchTextSameExtension);
            mSimilarityPercent = EditorGUILayout.IntSlider(Styles.SimilarityPercent, mSimilarityPercent, 0, 100);
        }

        void CheckFsWatcher(WorkspaceInfo wkInfo)
        {
            bool isFileSystemWatcherEnabled = false;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
                waiter.Execute(
                    /*threadOperationDelegate*/
                    delegate
                    {
                        isFileSystemWatcherEnabled =
                            IsFileSystemWatcherEnabled(wkInfo);
                    },
                    /*afterOperationDelegate*/
                    delegate
                    {
                        if (waiter.Exception != null)
                            return;

                        mFSWatcherEnabled = isFileSystemWatcherEnabled;
                    });
        }

        void SetPendingChangesOptions(PendingChangesOptions options)
        {
            mShowCheckouts = IsEnabled(
                WorkspaceStatusOptions.FindCheckouts, options.WorkspaceStatusOptions);
            mAutoRefresh = options.AutoRefresh;

            mShowChangedFiles = IsEnabled(
                WorkspaceStatusOptions.FindChanged, options.WorkspaceStatusOptions);
            mCheckFileContent = options.CheckFileContentForChanged;

            mUseChangeLists = options.UseChangeLists;

            mShowPrivateFields = IsEnabled(
                WorkspaceStatusOptions.FindPrivates, options.WorkspaceStatusOptions);
            mShowIgnoredFiles = IsEnabled(
                WorkspaceStatusOptions.ShowIgnored, options.WorkspaceStatusOptions);
            mShowHiddenFiles = IsEnabled(
                WorkspaceStatusOptions.ShowHiddenChanges, options.WorkspaceStatusOptions);
            mShowDeletedFiles = IsEnabled(
                WorkspaceStatusOptions.FindLocallyDeleted, options.WorkspaceStatusOptions);

            mShowMovedFiles = IsEnabled(
                WorkspaceStatusOptions.CalculateLocalMoves, options.WorkspaceStatusOptions);
            mMatchBinarySameExtension =
                options.MovedMatchingOptions.bBinMatchingOnlySameExtension;
            mMatchTextSameExtension =
                options.MovedMatchingOptions.bTxtMatchingOnlySameExtension;
            mSimilarityPercent = (int)((1 - options.MovedMatchingOptions.AllowedChangesPerUnit) * 100f);
        }

        PendingChangesOptions GetPendingChangesOptions()
        {
            WorkspaceStatusOptions resultWkStatusOptions =
                WorkspaceStatusOptions.None;

            if (mShowCheckouts)
            {
                resultWkStatusOptions |= WorkspaceStatusOptions.FindCheckouts;
                resultWkStatusOptions |= WorkspaceStatusOptions.FindReplaced;
                resultWkStatusOptions |= WorkspaceStatusOptions.FindCopied;
            }

            if (mShowChangedFiles)
                resultWkStatusOptions |= WorkspaceStatusOptions.FindChanged;
            if (mShowPrivateFields)
                resultWkStatusOptions |= WorkspaceStatusOptions.FindPrivates;
            if (mShowIgnoredFiles)
                resultWkStatusOptions |= WorkspaceStatusOptions.ShowIgnored;
            if (mShowHiddenFiles)
                resultWkStatusOptions |= WorkspaceStatusOptions.ShowHiddenChanges;
            if (mShowDeletedFiles)
                resultWkStatusOptions |= WorkspaceStatusOptions.FindLocallyDeleted;
            if (mShowMovedFiles)
                resultWkStatusOptions |= WorkspaceStatusOptions.CalculateLocalMoves;

            MovedMatchingOptions matchingOptions = new MovedMatchingOptions();
            matchingOptions.AllowedChangesPerUnit =
                (100 - mSimilarityPercent) / 100f;
            matchingOptions.bBinMatchingOnlySameExtension =
                mMatchBinarySameExtension;
            matchingOptions.bTxtMatchingOnlySameExtension =
                mMatchTextSameExtension;

            return new PendingChangesOptions(
                resultWkStatusOptions,
                matchingOptions,
                mUseChangeLists,
                true,
                false,
                mAutoRefresh,
                false,
                mCheckFileContent,
                false);
        }

        static void DrawSettingsSection(Action drawSettings)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;

            try
            {
                EditorGUIUtility.labelWidth = UnityConstants.SETTINGS_GUI_WIDTH;

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);

                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Space(10);

                        drawSettings();

                        GUILayout.Space(10);
                    }

                    GUILayout.Space(10);
                }
            }
            finally
            {
                EditorGUIUtility.labelWidth = originalLabelWidth;
            }
        }

        static void DrawTabbedContent(Action drawContent)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;

            try
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.Space(15);
                    EditorGUIUtility.labelWidth -= 15;
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Space(0);
                        drawContent();
                    }

                    GUILayout.FlexibleSpace();
                }
            }
            finally
            {
                EditorGUIUtility.labelWidth = originalLabelWidth;
            }
        }

        static IAutoRefreshView GetPendingChangesView()
        {
            PlasticWindow window = GetWindowIfOpened.Plastic();

            if (window == null)
                return null;

            return window.GetPendingChangesView();
        }

        static string GetFsWatcherEnabledMessage()
        {
            if (PlatformIdentifier.IsWindows() || PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesFilesystemWatcherEnabled);

            return PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesINotifyEnabled);
        }

        static string GetFsWatcherDisabledMessage()
        {
            if (PlatformIdentifier.IsWindows() || PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesFilesystemWatcherDisabled);

            return PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesINotifyDisabled);
        }

        static string GetFsWatcherEnabledExplanation()
        {
            if (PlatformIdentifier.IsWindows() || PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesFilesystemWatcherEnabledExplanationUnityVCS);

            return PlasticLocalization.GetString(
            PlasticLocalization.Name.PendingChangesINotifyEnabledExplanation);
        }

        static string GetFsWatcherDisabledExplanation()
        {
            if (PlatformIdentifier.IsWindows() || PlatformIdentifier.IsMac())
            {
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesFilesystemWatcherDisabledExplanationUnityVCS)
                    .Replace("[[HELP_URL|{0}]]", "");
            }

            return PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesINotifyDisabledExplanation);
        }

        static bool IsFileSystemWatcherEnabled(
            WorkspaceInfo wkInfo)
        {
            return WorkspaceWatcherFsNodeReadersCache.Get().
                IsWatcherEnabled(wkInfo);
        }

        static bool IsEnabled(
            WorkspaceStatusOptions option,
            WorkspaceStatusOptions options)
        {
            return (options & option) == option;
        }

        internal interface IAutoRefreshView
        {
            void DisableAutoRefresh();
            void EnableAutoRefresh();
            void ForceRefresh();
        }

        class Styles
        {
            internal static GUIContent AutomaticAdd =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.ProjectSettingsAutomaticAdd),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.ProjectSettingsAutomaticAddExplanation));
            internal static GUIContent ShowCheckouts =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesShowCheckouts),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesShowCheckoutsExplanation));
            internal static GUIContent AutoRefresh =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesAutoRefresh),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesAutoRefreshExplanation));
            internal static GUIContent ShowChangedFiles =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesFindChanged),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesFindChangedExplanation));
            internal static GUIContent CheckFileContent =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesCheckFileContent),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesCheckFileContentExplanation));
            internal static GUIContent UseChangeLists =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesGroupInChangeLists),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesGroupInChangeListsExplanation));
            internal static GUIContent ShowPrivateFields =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesShowPrivateFiles),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesShowPrivateFilesExplanation));
            internal static GUIContent ShowIgnoredFields =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesShowIgnoredFiles),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesShowIgnoredFilesExplanation));
            internal static GUIContent ShowHiddenFields =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesShowHiddenFiles),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesShowHiddenFilesExplanation));
            internal static GUIContent ShowDeletedFilesDirs =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesShowDeletedFiles),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesShowDeletedFilesExplanation));
            internal static GUIContent ShowMovedFiles =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesFindMovedFiles),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesFindMovedFilesExplanation));
            internal static GUIContent MatchBinarySameExtension =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesMatchBinarySameExtension),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesMatchBinarySameExtensionExplanation));
            internal static GUIContent MatchTextSameExtension =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesMatchTextSameExtension),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesMatchTextSameExtensionExplanation));
            internal static GUIContent SimilarityPercent =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesSimilarityPercentage),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesSimilarityPercentageExplanation));
        }

        WorkspaceInfo mWkInfo;
        PendingChangesOptions mPendingChangesSavedOptions;

        bool mAutomaticAdd;
        bool mShowCheckouts;
        bool mAutoRefresh;
        bool mFSWatcherEnabled;

        bool mShowChangedFiles;
        bool mCheckFileContent;

        bool mUseChangeLists;
        bool mShowPrivateFields;
        bool mShowIgnoredFiles;
        bool mShowHiddenFiles;
        bool mShowDeletedFiles;

        bool mShowMovedFiles;
        bool mMatchBinarySameExtension;
        bool mMatchTextSameExtension;
        int mSimilarityPercent;

        const string SUPPORT_URL = "https://support.unity.com/hc/en-us/requests/new?ticket_form_id=360001051792";
    }
}
