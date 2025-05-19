using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEditor;
using UnityEngine;

using Codice.Client.Common;
using Codice.CM.Client.Differences;
using Codice.CM.Client.Differences.Merge;
using MergetoolGui;
using PlasticGui;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.Preferences
{
    class DiffAndMergePreferencesFoldout
    {
        internal void OnActivate()
        {
            PlasticGuiConfigData data = PlasticGuiConfig.Get().Configuration;

            mComparisonMethodSelectedIndex = ComparisonMethod.GetTypeIndexFromString(data.ComparisonMethod);
            mDefaultEncodingSelectedIndex = Array.IndexOf(mEncodingValues, data.Encoding);
            mResultEncodingSelectedIndex = Array.IndexOf(mEncodingValues, data.ResultEncoding);

            mManualConflictResolution = data.MergeResolutionType == MergeResolutionType.OnlyOne.ToString();
            mAutomaticConflictResolution = !mManualConflictResolution;

            mCloseMergeView = data.CloseMergeAndOpenPendingChanges;
            mMergeWithChanges = ClientConfig.Get().MergeWithPendingChanges();
        }

        internal void OnDeactivate()
        {
            PlasticGuiConfigData guiConfigData = PlasticGuiConfig.Get().Configuration;

            guiConfigData.ComparisonMethod = ComparisonMethod.GetStringFromTypeIndex(mComparisonMethodSelectedIndex);
            guiConfigData.Encoding = GetEncodingValue(mDefaultEncodingSelectedIndex);
            guiConfigData.ResultEncoding = GetEncodingValue(mResultEncodingSelectedIndex);

            guiConfigData.MergeResolutionType = mAutomaticConflictResolution ?
                MergeResolutionType.Forced.ToString() : MergeResolutionType.OnlyOne.ToString();

            guiConfigData.CloseMergeAndOpenPendingChanges = mCloseMergeView;

            ClientConfigData configData = ClientConfig.Get().GetClientConfigData();
            configData.SetMergeWithPendingChanges(mMergeWithChanges);

            ClientConfig.Get().Save(configData);
            PlasticGuiConfig.Get().Save();
        }

        internal void OnGUI()
        {
            DrawSplitter.ForWidth(UnityConstants.SETTINGS_GUI_WIDTH);

            DrawSettingsSection(DoDiffAndMergePreferences);
        }

        internal void SelectComparisonMethodForTesting(ComparisonMethodTypes comparisonMethod)
        {
            mComparisonMethodSelectedIndex = (int)comparisonMethod;
        }

        internal void SelectDefaultEncodingForTesting(string defaultEncoding)
        {
            mDefaultEncodingSelectedIndex = Array.IndexOf(mEncodingValues, defaultEncoding);
        }

        internal void SelectResultEncodingForTesting(string resultEncoding)
        {
            mResultEncodingSelectedIndex = Array.IndexOf(mEncodingValues, resultEncoding);
        }

        internal void AutomaticConflictResolutionRadioButtonRaiseCheckedForTesting()
        {
            mAutomaticConflictResolution = true;
            mManualConflictResolution = false;
        }

        internal void ManualConflictResolutionRadioButtonRaiseCheckedForTesting()
        {
            mManualConflictResolution = true;
            mAutomaticConflictResolution = false;
        }

        internal void CloseMergeViewCheckboxRaiseCheckedForTesting()
        {
            mCloseMergeView = true;
        }

        internal void CloseMergeViewCheckboxRaiseUncheckedForTesting()
        {
            mCloseMergeView = false;
        }

        internal void MergeWithChangesCheckBoxRaiseCheckedForTesting()
        {
            mMergeWithChanges = true;
        }

        internal void MergeWithChangesCheckBoxRaiseUncheckedForTesting()
        {
            mMergeWithChanges = false;
        }

        void DoDiffAndMergePreferences()
        {
            DoComparisonMethodAndEncodingSettings();

            DoMergeConflictResolutionSettings();

            DoMergeViewBehaviorSettings();
        }

        void DoComparisonMethodAndEncodingSettings()
        {
            GUILayout.Label(
                PlasticLocalization.Name.ComparisonMethodAndEncodingTitle.GetString(),
                UnityStyles.ProjectSettings.SectionTitle);

            mComparisonMethodSelectedIndex = EditorGUILayout.Popup(
                PlasticLocalization.Name.ComparisonMethod.GetString(),
                mComparisonMethodSelectedIndex,
                mComparisonMethods);

            mDefaultEncodingSelectedIndex = EditorGUILayout.Popup(
                MergetoolLocalization.Name.DefaultEncoding.GetString(),
                mDefaultEncodingSelectedIndex,
                mEncodingValuesToDisplay);

            mResultEncodingSelectedIndex = EditorGUILayout.Popup(
                MergetoolLocalization.Name.ResultEncoding.GetString(),
                mResultEncodingSelectedIndex,
                mEncodingValuesToDisplay);
        }

        void DoMergeConflictResolutionSettings()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label(
                PlasticLocalization.Name.ConflictResolutionTitle.GetString(),
                UnityStyles.ProjectSettings.SectionTitle);

            if (EditorGUILayout.Toggle(
                Styles.ManualConflictResolution,
                mManualConflictResolution,
                new GUIStyle(EditorStyles.radioButton)))
            {
                mManualConflictResolution = true;
                mAutomaticConflictResolution = false;
            }

            if (EditorGUILayout.Toggle(
                Styles.AutomaticConflictResolution,
                mAutomaticConflictResolution,
                new GUIStyle(EditorStyles.radioButton)))
            {
                mAutomaticConflictResolution = true;
                mManualConflictResolution = false;
            }
        }

        void DoMergeViewBehaviorSettings()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label(
                PlasticLocalization.Name.MergeViewBehaviorTitle.GetString(),
                UnityStyles.ProjectSettings.SectionTitle);

            mCloseMergeView = EditorGUILayout.Toggle(Styles.CloseMergeView, mCloseMergeView);
            mMergeWithChanges = EditorGUILayout.Toggle(Styles.MergeWithChanges, mMergeWithChanges);
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

        class Styles
        {
            internal static GUIContent ManualConflictResolution =
                new GUIContent(
                    PlasticLocalization.Name.ManualConflictResolution.GetString(),
                    PlasticLocalization.Name.ManualConflictResolutionTooltip.GetString());

            internal static GUIContent AutomaticConflictResolution =
                new GUIContent(
                    PlasticLocalization.Name.AutomaticConflictResolution.GetString(),
                    PlasticLocalization.Name.AutomaticConflictResolutionTooltip.GetString());

            internal static GUIContent CloseMergeView =
                new GUIContent(
                    PlasticLocalization.Name.CloseMergeAndOpenPendingChanges.GetString());

            internal static GUIContent MergeWithChanges =
                new GUIContent(
                    PlasticLocalization.Name.MergeWithPendingChanges.GetString(),
                    PlasticLocalization.Name.MergeWithPendingChangesExplanation.GetString());
        }

        string GetEncodingValue(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex > mEncodingValues.Length)
                return string.Empty;

            return mEncodingValues[selectedIndex];
        }

        static string[] GetEncodingValuesToDisplay()
        {
            List<string> encodingValues = new List<string>();

            encodingValues.AddRange(EncodingManager.GetPredefinedEncodings());

            IEnumerable<Encoding> systemEncodings = EncodingManager.GetSystemEncodings()
                .Select(ei => ei.GetEncoding())
                .OrderBy(e => e.EncodingName);

            foreach (Encoding encoding in systemEncodings)
            {
                encodingValues.Add(
                    UnityMenuItem.EscapedText(
                        string.Format("{0} {1} ({2})",
                            encoding.EncodingName, encoding.WebName, encoding.CodePage.ToString())));
            }

            return encodingValues.ToArray();
        }

        static string[] mComparisonMethods = {
                MergetoolLocalization.GetString(MergetoolLocalization.Name.IgnoreEol),
                MergetoolLocalization.GetString(MergetoolLocalization.Name.IgnoreWhitespace),
                MergetoolLocalization.GetString(MergetoolLocalization.Name.IgnoreEolWhitespace),
                MergetoolLocalization.GetString(MergetoolLocalization.Name.RecognizeAll) };

        static string[] mEncodingValues = EncodingManager.GetEncodingValues();
        static string[] mEncodingValuesToDisplay = GetEncodingValuesToDisplay();

        int mComparisonMethodSelectedIndex;
        int mDefaultEncodingSelectedIndex;
        int mResultEncodingSelectedIndex;

        bool mManualConflictResolution;
        bool mAutomaticConflictResolution;

        bool mCloseMergeView;
        bool mMergeWithChanges;
    }
}
