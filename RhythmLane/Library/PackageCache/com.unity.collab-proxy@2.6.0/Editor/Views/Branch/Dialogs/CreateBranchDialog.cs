using UnityEditor;
using UnityEngine;

using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.QueryViews.Branches;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;

namespace Unity.PlasticSCM.Editor.Views.Branches.Dialogs
{
    class CreateBranchDialog : PlasticDialog
    {
        protected override Rect DefaultRect
        {
            get
            {
                var baseRect = base.DefaultRect;
                return new Rect(baseRect.x, baseRect.y, 710, 290);
            }
        }

        protected override string GetTitle()
        {
            return PlasticLocalization.Name.CreateChildBranchTitle.GetString();
        }

        protected override void OnModalGUI()
        {
            DoTitleArea();

            DoFieldsArea();

            DoButtonsArea();
        }

        internal static BranchCreationData CreateBranchFromLastParentBranchChangeset(
            EditorWindow parentWindow,
            RepositorySpec repSpec,
            BranchInfo parentBranchInfo)
        {
            string changesetStr = PlasticLocalization.Name.LastChangeset.GetString();

            string explanation = BranchCreationUserInfo.GetFromObjectString(
                repSpec, parentBranchInfo, changesetStr);

            CreateBranchDialog dialog = Create(repSpec, parentBranchInfo, -1 , explanation);
            ResponseType dialogueResult = dialog.RunModal(parentWindow);

            BranchCreationData result = dialog.BuildCreationData();
            result.Result = dialogueResult == ResponseType.Ok;
            return result;
        }

        void DoTitleArea()
        {
            GUILayout.BeginVertical();

            Title(PlasticLocalization.Name.CreateChildBranchTitle.GetString());

            GUILayout.Space(5);

            Paragraph(string.Format("{0} {1}",
                PlasticLocalization.Name.CreateChildBranchExplanation.GetString(), mExplanation));

            GUILayout.EndVertical();
        }

        void DoFieldsArea()
        {
            GUILayout.BeginVertical();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(
                    PlasticLocalization.Name.BranchNameEntry.GetString(),
                    GUILayout.Width(100));

                GUI.SetNextControlName(NAME_FIELD_CONTROL_NAME);
                mNewBranchName = GUILayout.TextField(mNewBranchName);

                if (!mWasNameFieldFocused)
                {
                    EditorGUI.FocusTextInControl(NAME_FIELD_CONTROL_NAME);
                    mWasNameFieldFocused = true;
                }

                GUILayout.Space(5);
            }

            GUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(100)))
                {
                    GUILayout.Space(49);
                    GUILayout.Label(
                        PlasticLocalization.Name.CommentsEntry.GetString(),
                        GUILayout.Width(100));
                }
                mComment = GUILayout.TextArea(mComment, GUILayout.Height(100));
                GUILayout.Space(5);
            }

            GUILayout.Space(5);

            mSwitchToBranch = GUILayout.Toggle(mSwitchToBranch, PlasticLocalization.Name.SwitchToBranchCheckButton.GetString());

            GUILayout.Space(5);

            GUILayout.EndVertical();
        }

        void DoButtonsArea()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.HorizontalScope(GUILayout.MinWidth(500)))
                {
                    GUILayout.Space(2);
                    DrawProgressForDialogs.For(
                        mProgressControls.ProgressData);
                    GUILayout.Space(2);
                }

                GUILayout.FlexibleSpace();

                DoCreateButton();
                DoCancelButton();
            }
        }

        void DoCancelButton()
        {
            if (!NormalButton(PlasticLocalization.Name.CancelButton.GetString()))
                return;

            CancelButtonAction();
        }

        void DoCreateButton()
        {
            if (!NormalButton(PlasticLocalization.Name.CreateButton.GetString()))
                return;

            BranchCreationValidation.AsyncValidation(
                BuildCreationData(), this, mProgressControls);
        }

        static CreateBranchDialog Create(
            RepositorySpec repSpec, BranchInfo parentBranchInfo, long changesetId, string explanation)
        {
            var instance = CreateInstance<CreateBranchDialog>();
            instance.IsResizable = false;
            instance.mEscapeKeyAction = instance.CloseButtonAction;
            instance.mRepositorySpec = repSpec;
            instance.mParentBranchInfo = parentBranchInfo;
            instance.mNewBranchName = "";
            instance.mComment = "";
            instance.mSwitchToBranch = true;
            instance.mProgressControls = new ProgressControlsForDialogs();
            instance.mExplanation = explanation;
            instance.mChangesetId = changesetId;
            return instance;
        }

        BranchCreationData BuildCreationData()
        {
            return new BranchCreationData(
                mRepositorySpec,
                mParentBranchInfo,
                mChangesetId,
                mNewBranchName,
                mComment,
                null,
                mSwitchToBranch);
        }

        ProgressControlsForDialogs mProgressControls;

        RepositorySpec mRepositorySpec;
        BranchInfo mParentBranchInfo;
        long mChangesetId;

        string mNewBranchName;
        string mComment;
        bool mSwitchToBranch;
        string mExplanation;

        bool mWasNameFieldFocused;
        const string NAME_FIELD_CONTROL_NAME = "CreateBranchNameField";
    }
}