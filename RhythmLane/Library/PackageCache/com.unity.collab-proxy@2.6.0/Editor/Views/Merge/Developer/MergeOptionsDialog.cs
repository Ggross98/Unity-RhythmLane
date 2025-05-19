using System;

using Codice.Client.Commands;
using UnityEditor;
using UnityEngine;

using Codice.Client.Common;
using Codice.CM.Client.Differences.Merge;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.Merge;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;

namespace Unity.PlasticSCM.Editor.Views.Merge.Developer
{
    internal class MergeOptionsDialog : PlasticDialog
    {
        internal bool MergeBothRadioToggleIsChecked { get { return mMergeBothRadioToggle; } }
        internal bool MergeFromDestinationRadioToggleIsChecked { get { return mMergeFromDestinationRadioToggle; } }
        internal bool MergeFromSourceRadioToggleIsChecked { get { return mMergeFromSourceRadioToggle; } }
        internal bool OnlyOneRadioToggleIsChecked { get { return mOnlyOneRadioToggle; } }
        internal bool ForcedRadioToggleIsChecked { get { return mForcedRadioToggle; } }
        internal bool IgnoreMergeTrackingToggleIsChecked
        {
            get { return mIgnoreMergeTrackingToggle; }
            set { mIgnoreMergeTrackingToggle = value; }
        }
        internal bool AutomaticAncestorRadioToggleIsChecked { get { return mAutomaticAncestorRadioToggle; } }
        internal bool ManualAncestorRadioToggleIsChecked { get { return mManualAncestorRadioToggle; } }
        internal string AncestorTextField { set { mAncestorTextField = value; } }

        internal static bool MergeOptions(MergeDialogParameters mergeDialogParameters)
        {
            MergeOptionsDialog dialog = Create(
                mergeDialogParameters,
                new ProgressControlsForDialogs());

            dialog.SetMergeOptions();

            return dialog.RunModal(focusedWindow) == ResponseType.Ok;
        }

        internal static MergeOptionsDialog MergeOptionsForTesting(MergeDialogParameters mergeDialogParameters)
        {
            MergeOptionsDialog dialog = Create(
                mergeDialogParameters,
                new ProgressControlsForDialogs());

            dialog.SetMergeOptions();
            return dialog;
        }

        protected override Rect DefaultRect
        {
            get
            {
                var baseRect = base.DefaultRect;
                return new Rect(baseRect.x, baseRect.y, 600, 450);
            }
        }

        protected override string GetTitle()
        {
            return PlasticLocalization.GetString(
                PlasticLocalization.Name.MergeOptionsTitle);
        }

        protected override void OnModalGUI()
        {
            GUILayout.Label(
                PlasticLocalization.Name.MergeOptionsTitle.GetString(),
                UnityStyles.Dialog.MessageTitle);

            GUILayout.Label(
                PlasticLocalization.Name.MergeOptionsExplanation.GetString(),
                UnityStyles.Paragraph);

            DrawSection(
                PlasticLocalization.Name.MergeOptionsContributorsTitle.GetString(),
                DoMergeContributorsArea);

            DrawSection(
                PlasticLocalization.Name.ConflictResolutionTitle.GetString(),
                DoMergeConflictResolutionArea);

            DrawSection(
                PlasticLocalization.Name.MergeOptionSkipMergeTrackingTitle.GetString(),
                DoSkipMergeTrackingArea);

            DrawSection(
                PlasticLocalization.Name.MergeOptionAdvancedTitle.GetString(),
                DoAdvancedArea);

            DrawProgressForDialogs.For(
                mProgressControls.ProgressData);

            DoButtonsArea();

            mProgressControls.ForcedUpdateProgress(this);
        }

        void DoMergeContributorsArea()
        {
            if (GUILayout.Toggle(
                    mMergeBothRadioToggle,
                    PlasticLocalization.Name.MergeOptionBoth.GetString(),
                    UnityStyles.Dialog.RadioToggle))
            {
                CheckMergeBothRadioToggle();
            }

            GUI.enabled = mParameters.IsRegularMerge();

            if (GUILayout.Toggle(
                    mMergeFromDestinationRadioToggle,
                    PlasticLocalization.Name.MergeOptionFromDestination.GetString(),
                    UnityStyles.Dialog.RadioToggle))
            {
                CheckMergeFromDestinationRadioToggle();
            }

            if (GUILayout.Toggle(
                    mMergeFromSourceRadioToggle,
                    PlasticLocalization.Name.MergeOptionFromSource.GetString(),
                    UnityStyles.Dialog.RadioToggle))
            {
                CheckMergeFromSourceRadioToggle();
            }

            GUI.enabled = true;
        }

        internal void CheckMergeBothRadioToggle()
        {
            mMergeBothRadioToggle = true;
            mMergeFromDestinationRadioToggle = false;
            mMergeFromSourceRadioToggle = false;
        }

        internal void CheckMergeFromDestinationRadioToggle()
        {
            mMergeBothRadioToggle = false;
            mMergeFromDestinationRadioToggle = true;
            mMergeFromSourceRadioToggle = false;
        }

        internal void CheckMergeFromSourceRadioToggle()
        {
            mMergeBothRadioToggle = false;
            mMergeFromDestinationRadioToggle = false;
            mMergeFromSourceRadioToggle = true;
        }

        void DoMergeConflictResolutionArea()
        {
            if (GUILayout.Toggle(
                mOnlyOneRadioToggle,
                Styles.ManualConflictResolution,
                UnityStyles.Dialog.RadioToggle))
            {
                CheckOnlyOneRadioToggle();
            }

            if (GUILayout.Toggle(
                mForcedRadioToggle,
                Styles.AutomaticConflictResolution,
                UnityStyles.Dialog.RadioToggle))
            {
                CheckForcedRadioToggle();
            }
        }

        internal void CheckOnlyOneRadioToggle()
        {
            mOnlyOneRadioToggle = true;
            mForcedRadioToggle = false;
        }

        internal void CheckForcedRadioToggle()
        {
            mOnlyOneRadioToggle = false;
            mForcedRadioToggle = true;
        }

        void DoSkipMergeTrackingArea()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = !mParameters.IsRegularMerge();

                mIgnoreMergeTrackingToggle = GUILayout.Toggle(
                    mIgnoreMergeTrackingToggle,
                    PlasticLocalization.Name.MergeOptionSkipMergeTrackingExplanation.GetString(),
                    UnityStyles.Dialog.CheckBox);

                GUI.enabled = true;

                if (GUILayout.Button(Images.GetInfoIcon(), UnityStyles.Dialog.FlatButton))
                {
                    ShowSkipMergeTrackingHelp();
                }

                GUILayout.FlexibleSpace();
            }
        }

        void ShowSkipMergeTrackingHelp()
        {
            GuiMessage.ShowInformation(
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.MergeOptionSkipMergeTrackingHelpTitle),
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.MergeOptionSkipMergeTrackingHelp));
        }

        void DoAdvancedArea()
        {
            GUI.enabled = mParameters.IsRegularMerge();

            if (GUILayout.Toggle(
                    mAutomaticAncestorRadioToggle,
                    PlasticLocalization.Name.MergeOptionAutomaticCalculateAncestor.GetString(),
                    UnityStyles.Dialog.RadioToggle))
            {
                CheckAutomaticAncestorRadioToggle();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Toggle(
                        mManualAncestorRadioToggle,
                        PlasticLocalization.Name.MergeOptionManualSpecifyAncestor.GetString(),
                        UnityStyles.Dialog.RadioToggle))
                {
                    CheckManualAncestorRadioToggle();
                }

                GUI.enabled = mParameters.IsRegularMerge() && mManualAncestorRadioToggle;

                GUILayout.Space(5);

                mAncestorTextField = GUILayout.TextField(mAncestorTextField, GUILayout.Width(90));

                if (mManualAncestorRadioToggle)
                {
                    GUILayout.Label(PlasticLocalization.Name.
                        MergeOptionManualSpecifyAncestorExample.GetString());
                }

                GUILayout.FlexibleSpace();
            }

            GUI.enabled = true;
        }

        internal void CheckAutomaticAncestorRadioToggle()
        {
            mAutomaticAncestorRadioToggle = true;
            mManualAncestorRadioToggle = false;
        }

        internal void CheckManualAncestorRadioToggle()
        {
            mAutomaticAncestorRadioToggle = false;
            mManualAncestorRadioToggle = true;
        }

        void DoButtonsArea()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    DoSaveButton();
                    DoCancelButton();
                    return;
                }

                DoCancelButton();
                DoSaveButton();
            }
        }

        void DoSaveButton()
        {
            if (!AcceptButton(PlasticLocalization.Name.SaveButton.GetString()))
                return;

            OkButtonWithValidationAction();
        }

        void DoCancelButton()
        {
            if (!NormalButton(PlasticLocalization.Name.CancelButton.GetString()))
                return;

            CancelButtonAction();
        }

        void SetMergeOptions()
        {
            if (mParameters.Options == null)
                return;

            MergeDialogOptions mergeOptions = mParameters.Options;

            SetMergeContributorOptions(mergeOptions);
            SetSkipMergeTrackingOptions(mergeOptions);
            SetConflictResolutionOptions(mergeOptions);
            SetResolutionStrategyOptions(mParameters);
        }

        void SetMergeContributorOptions(MergeDialogOptions mergeOptions)
        {
            switch (mergeOptions.Contributor)
            {
                case MergeContributorType.MergeContributors:
                {
                    mMergeBothRadioToggle = true;
                    break;
                }
                case MergeContributorType.KeepDestination:
                {
                    mMergeFromDestinationRadioToggle = true;
                    break;
                }
                case MergeContributorType.KeepSource:
                {
                    mMergeFromSourceRadioToggle = true;
                    break;
                }
            }
        }

        void SetSkipMergeTrackingOptions(MergeDialogOptions mergeOptions)
        {
            mIgnoreMergeTrackingToggle = mergeOptions.IgnoreMergeTracking;
        }

        void SetConflictResolutionOptions(MergeDialogOptions mergeOptions)
        {
            switch (mergeOptions.ResolutionType)
            {
                case MergeResolutionType.OnlyOne:
                {
                    mOnlyOneRadioToggle = true;
                    break;
                }
                default:
                {
                    mForcedRadioToggle = true;
                    break;
                }
            }
        }

        void SetResolutionStrategyOptions(MergeDialogParameters parameters)
        {
            if (parameters.Strategy == MergeStrategy.Manual && parameters.AncestorSpec != null)
            {
                mManualAncestorRadioToggle = true;
                mAncestorTextField = parameters.AncestorSpec.csNumber.ToString();
            }
            else
            {
                mAutomaticAncestorRadioToggle = true;
            }
        }

        internal void OkButtonWithValidationAction()
        {
            mParameters.SaveParameters(
                mMergeBothRadioToggle,
                mMergeFromDestinationRadioToggle,
                mMergeFromSourceRadioToggle,
                mIgnoreMergeTrackingToggle,
                mOnlyOneRadioToggle,
                mAutomaticAncestorRadioToggle);

            if (mParameters.Strategy != MergeStrategy.Manual)
            {
                OkButtonAction();
                return;
            }

            MergeDialogValidation.AsyncValidation(
                mParameters,
                mAncestorTextField,
                this,
                mProgressControls);
        }

        static void DrawSection(string title, Action drawContent)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label(
                title,
                UnityStyles.Dialog.SectionTitle);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(15);

                using (new EditorGUILayout.VerticalScope())
                {
                    drawContent();
                }

                GUILayout.FlexibleSpace();
            }
        }

        static MergeOptionsDialog Create(
            MergeDialogParameters parameters,
            ProgressControlsForDialogs progressControls)
        {
            var instance = CreateInstance<MergeOptionsDialog>();
            instance.mParameters = parameters;
            instance.mProgressControls = progressControls;
            instance.mEscapeKeyAction = instance.CancelButtonAction;
            return instance;
        }

        class Styles
        {
            internal static GUIContent ManualConflictResolution =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.ManualConflictResolution),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.ManualConflictResolutionTooltip));
            internal static GUIContent AutomaticConflictResolution =
                new GUIContent(PlasticLocalization.GetString(
                        PlasticLocalization.Name.AutomaticConflictResolution),
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.AutomaticConflictResolutionTooltip));
        }

        bool mMergeBothRadioToggle;
        bool mMergeFromDestinationRadioToggle;
        bool mMergeFromSourceRadioToggle;

        bool mOnlyOneRadioToggle;
        bool mForcedRadioToggle;

        bool mIgnoreMergeTrackingToggle;

        bool mAutomaticAncestorRadioToggle;
        bool mManualAncestorRadioToggle;
        string mAncestorTextField;

        ProgressControlsForDialogs mProgressControls;
        MergeDialogParameters mParameters;
    }
}
