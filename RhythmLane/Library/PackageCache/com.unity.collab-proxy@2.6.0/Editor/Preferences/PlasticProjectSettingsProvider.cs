using System;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Codice.Client.Common.EventTracking;
using Codice.CM.Common;
using PlasticGui;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.Preferences
{
    class PlasticProjectSettingsProvider : SettingsProvider
    {
        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public PlasticProjectSettingsProvider(
            string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope)
        {
            label = UnityConstants.PROJECT_SETTINGS_TAB_TITLE;
        }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            if (!FindWorkspace.HasWorkspace(ApplicationDataPath.Get()))
                return null;

            PlasticApp.InitializeIfNeeded();

            return new PlasticProjectSettingsProvider(
                UnityConstants.PROJECT_SETTINGS_TAB_PATH, SettingsScope.Project);
        }

        public override void OnActivate(
            string searchContext,
            VisualElement rootElement)
        {
            mIsPluginEnabled = PlasticPluginIsEnabledPreference.IsEnabled();

            mWkInfo = FindWorkspace.InfoForApplicationPath(
                ApplicationDataPath.Get(), PlasticGui.Plastic.API);

            mIsProjectSettingsActivated = true;

            mPendingChangesOptionsFoldout.OnActivate(mWkInfo);
            mDiffAndMergePreferencesFoldout.OnActivate();
        }

        public override void OnDeactivate()
        {
            if (!mIsProjectSettingsActivated)
                return;

            mIsProjectSettingsActivated = false;

            mPendingChangesOptionsFoldout.OnDeactivate();
            mDiffAndMergePreferencesFoldout.OnDeactivate();
        }

        public override void OnGUI(string searchContext)
        {
            DrawSettingsSection(
                DoIsEnabledSetting);

            if (!mIsPluginEnabled)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            mIsPendingChangesFoldoutOpen =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    mIsPendingChangesFoldoutOpen,
                    PlasticLocalization.Name.PendingChangesOptionsSectionTitle.GetString(),
                    UnityStyles.ProjectSettings.FoldoutHeader);

            if (mIsPendingChangesFoldoutOpen)
                mPendingChangesOptionsFoldout.OnGUI();

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            mIsDiffAndMergeFoldoutOpen =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    mIsDiffAndMergeFoldoutOpen,
                    PlasticLocalization.Name.DiffAndMergeOptionsSectionTitle.GetString(),
                    UnityStyles.ProjectSettings.FoldoutHeader);

            if (mIsDiffAndMergeFoldoutOpen)
                mDiffAndMergePreferencesFoldout.OnGUI();

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }

        void DoIsEnabledSetting()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string message = PlasticLocalization.GetString(
                    mIsPluginEnabled ?
                        PlasticLocalization.Name.UnityVCSIsEnabled :
                        PlasticLocalization.Name.UnityVCSIsDisabled);

                GUILayout.Label(
                    message,
                    EditorStyles.boldLabel,
                    GUILayout.Height(20));

                EditorGUILayout.Space(8);

                DoIsEnabledButton();

                GUILayout.FlexibleSpace();
            }
        }

        void DoIsEnabledButton()
        {
            if (!GUILayout.Button(PlasticLocalization.GetString(
                    mIsPluginEnabled ?
                        PlasticLocalization.Name.DisableButton :
                        PlasticLocalization.Name.EnableButton),
                    UnityStyles.ProjectSettings.ToggleOn))
            {
                return;
            }

            if (!mIsPluginEnabled)
            {
                mIsPluginEnabled = true;

                TrackFeatureUseEvent.For(
                    PlasticGui.Plastic.API.GetRepositorySpec(mWkInfo),
                    TrackFeatureUseEvent.Features.UnityPackage.EnableManually);

                PlasticPlugin.Enable();
                PlasticPluginIsEnabledPreference.Enable();

                return;
            }

            if (mIsPluginEnabled)
            {
                mIsPluginEnabled = false;

                TrackFeatureUseEvent.For(
                    PlasticGui.Plastic.API.GetRepositorySpec(mWkInfo),
                    TrackFeatureUseEvent.Features.UnityPackage.DisableManually);

                PlasticPluginIsEnabledPreference.Disable();
                CloseWindowIfOpened.Plastic();
                PlasticShutdown.Shutdown();
                return;
            }
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

        bool mIsPendingChangesFoldoutOpen = true;
        bool mIsDiffAndMergeFoldoutOpen = true;

        bool mIsProjectSettingsActivated;

        bool mIsPluginEnabled;

        WorkspaceInfo mWkInfo;

        PendingChangesOptionsFoldout mPendingChangesOptionsFoldout = new PendingChangesOptionsFoldout();
        DiffAndMergePreferencesFoldout mDiffAndMergePreferencesFoldout = new DiffAndMergePreferencesFoldout();
    }
}
