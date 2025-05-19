using System;

using UnityEditor;
using UnityEngine;

using Codice.Client.Common;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WebApi;
using PlasticGui.WorkspaceWindow.Home.Repositories;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;
using Unity.PlasticSCM.Editor.Views.CreateWorkspace.Dialogs;

namespace Unity.PlasticSCM.Editor.Views.CreateWorkspace
{
    internal static class DrawCreateWorkspace
    {
        internal static void ForState(
            Action<string> selectRepositoryAction,
            Action<RepositoryCreationData> createRepositoryAction,
            Action<CreateWorkspaceViewState> createWorkspaceAction,
            EditorWindow parentWindow,
            IPlasticWebRestApi plasticWebRestApi,
            string defaultServer,
            ref CreateWorkspaceViewState state)
        {
            DoTitle();

            GUILayout.Space(15);

            DoFieldsArea(
                selectRepositoryAction,
                createRepositoryAction,
                parentWindow,
                plasticWebRestApi,
                defaultServer,
                ref state);

            GUILayout.Space(10);

            DoRadioButtonsArea(ref state);

            GUILayout.Space(3);

            DoHelpLabel();

            GUILayout.Space(10);

            DoCreateWorkspaceButton(
                createWorkspaceAction,
                ref state);

            GUILayout.Space(5);

            DoNotificationArea(state.ProgressData);
        }

        static void DoTitle()
        {
            GUILayout.Label(
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.NewWorkspace),
                UnityStyles.Dialog.MessageTitle);

            GUILayout.Label(
                PlasticLocalization.GetString(PlasticLocalization.Name.WorkspacesExplanationLabel),
                EditorStyles.wordWrappedLabel);
        }

        static void DoFieldsArea(
            Action<string> selectRepositoryAction,
            Action<RepositoryCreationData> createRepositoryAction,
            EditorWindow parentWindow,
            IPlasticWebRestApi plasticWebRestApi,
            string defaultServer,
            ref CreateWorkspaceViewState state)
        {
            DoRepositoryField(
                selectRepositoryAction,
                createRepositoryAction,
                parentWindow,
                plasticWebRestApi,
                defaultServer,
                ref state);

            DoWorkspaceField(ref state);
        }

        static void DoRepositoryField(
            Action<string> selectRepositoryAction,
            Action<RepositoryCreationData> createRepositoryAction,
            EditorWindow parentWindow,
            IPlasticWebRestApi plasticWebRestApi,
            string defaultServer,
            ref CreateWorkspaceViewState state)
        {
            EditorGUILayout.BeginHorizontal();

            DoLabel(PlasticLocalization.GetString(PlasticLocalization.Name.Repository));

            state.Repository = DoTextField(
                state.Repository,
                !state.ProgressData.IsOperationRunning,
                LABEL_WIDTH,
                TEXTBOX_WIDTH - BROWSE_BUTTON_WIDTH);

            float browseButtonX =
                LABEL_WIDTH + TEXTBOX_WIDTH + BUTTON_MARGIN -
                BROWSE_BUTTON_WIDTH;
            float browseButtonWidth =
                BROWSE_BUTTON_WIDTH - BUTTON_MARGIN;

            if (DoButton(
                    "...",
                    !state.ProgressData.IsOperationRunning,
                    browseButtonX,
                    browseButtonWidth))
            {
                DoBrowseRepositoryButton(
                    selectRepositoryAction,
                    parentWindow,
                    plasticWebRestApi,
                    defaultServer);

                EditorGUIUtility.ExitGUI();
            }

            float newButtonX =
                LABEL_WIDTH + TEXTBOX_WIDTH + BUTTON_MARGIN;
            float newButtonWidth =
                NEW_BUTTON_WIDTH - BUTTON_MARGIN;

            if (DoButton(
                    PlasticLocalization.GetString(PlasticLocalization.Name.NewButton),
                    !state.ProgressData.IsOperationRunning,
                    newButtonX, newButtonWidth))
            {
                DoNewRepositoryButton(
                    createRepositoryAction,
                    parentWindow,
                    plasticWebRestApi,
                    state.Repository,
                    defaultServer);

                EditorGUIUtility.ExitGUI();
            }

            ValidationResult validationResult = ValidateRepository(state.Repository);

            if (!validationResult.IsValid)
                DoWarningLabel(validationResult.ErrorMessage,
                    LABEL_WIDTH + TEXTBOX_WIDTH + NEW_BUTTON_WIDTH + LABEL_MARGIN);

            EditorGUILayout.EndHorizontal();
        }

        static void DoWorkspaceField(
            ref CreateWorkspaceViewState state)
        {
            EditorGUILayout.BeginHorizontal();

            DoLabel(
                PlasticLocalization.GetString(PlasticLocalization.Name.WorkspaceName));

            state.WorkspaceName = DoTextField(
                state.WorkspaceName,
                !state.ProgressData.IsOperationRunning,
                LABEL_WIDTH,
                TEXTBOX_WIDTH - BROWSE_BUTTON_WIDTH);

            ValidationResult validationResult = ValidateWorkspaceName(
                state.WorkspaceName);

            if (!validationResult.IsValid)
                DoWarningLabel(validationResult.ErrorMessage,
                    LABEL_WIDTH + TEXTBOX_WIDTH - BROWSE_BUTTON_WIDTH + LABEL_MARGIN);

            EditorGUILayout.EndHorizontal();
        }

        static void DoRadioButtonsArea(
            ref CreateWorkspaceViewState state)
        {
            EditorGUILayout.BeginVertical();
            DoLabel(
                PlasticLocalization.GetString(PlasticLocalization.Name.WorkPreferenceQuestion));

            if (DoRadioButton(
                PlasticLocalization.GetString(PlasticLocalization.Name.WorkPreferenceAnswerUnityVCS),
                state.WorkspaceMode == CreateWorkspaceViewState.WorkspaceModes.Developer,
                !state.ProgressData.IsOperationRunning,
                RADIO_BUTTON_MARGIN))
                state.WorkspaceMode = CreateWorkspaceViewState.WorkspaceModes.Developer;

            if (DoRadioButton(
                PlasticLocalization.GetString(PlasticLocalization.Name.WorkPreferenceAnswerGluon),
                state.WorkspaceMode == CreateWorkspaceViewState.WorkspaceModes.Gluon,
                !state.ProgressData.IsOperationRunning,
                RADIO_BUTTON_MARGIN))
                state.WorkspaceMode = CreateWorkspaceViewState.WorkspaceModes.Gluon;

            EditorGUILayout.EndVertical();
        }

        static void DoCreateWorkspaceButton(
            Action<CreateWorkspaceViewState> createWorkspaceAction,
            ref CreateWorkspaceViewState state)
        {
            EditorGUILayout.BeginHorizontal();

            bool isButtonEnabled =
                IsValidState(state) &&
                !state.ProgressData.IsOperationRunning;

            string buttonText = PlasticLocalization.GetString(
                PlasticLocalization.Name.CreateWorkspace);

            bool isButtonClicked = DoButton(buttonText, isButtonEnabled,
                CREATE_WORKSPACE_BUTTON_MARGIN, CREATE_WORKSPACE_BUTTON_WIDTH);

            GUI.enabled = true;

            if (state.ProgressData.IsOperationRunning)
            {
                DoProgress(state.ProgressData,
                    CREATE_WORKSPACE_BUTTON_MARGIN +
                    PROGRESS_MARGIN +
                    CREATE_WORKSPACE_BUTTON_WIDTH);
            }

            EditorGUILayout.EndHorizontal();

            if (isButtonClicked)
                createWorkspaceAction(state);
        }

        static void DoBrowseRepositoryButton(
            Action<string> selectRepositoryAction,
            EditorWindow parentWindow,
            IPlasticWebRestApi plasticWebRestApi,
            string defaultServer)
        {
            string selectedRepository = RepositoryExplorerDialog.BrowseRepository(
                parentWindow,
                plasticWebRestApi,
                defaultServer);

            if (string.IsNullOrEmpty(selectedRepository))
                return;

            selectRepositoryAction(selectedRepository);
        }

        static void DoNewRepositoryButton(
            Action<RepositoryCreationData> createRepositoryAction,
            EditorWindow parentWindow,
            IPlasticWebRestApi plasticWebRestApi,
            string repositorySpecInput,
            string defaultServer)
        {
            string proposedRepositoryName;
            string proposedServer;

            RepositorySpec repSpec = OrganizationsInformation.TryResolveRepositorySpecFromInput(repositorySpecInput);

            if (repSpec != null)
            {
                proposedRepositoryName = repSpec.Name;
                proposedServer = repSpec.Server;
            }
            else
            {
                proposedRepositoryName = ExtractRepositoryName(repositorySpecInput);
                proposedServer = defaultServer;
            }

            RepositoryCreationData creationData = CreateRepositoryDialog.CreateRepository(
                parentWindow,
                plasticWebRestApi,
                proposedRepositoryName,
                proposedServer,
                defaultServer,
                ClientConfig.Get().GetWorkspaceServer());

            createRepositoryAction(creationData);
        }

        static string ExtractRepositoryName(string repositorySpec)
        {
            string[] repositoryParts = repositorySpec.Split('@');

            return repositoryParts.Length > 0 ? repositoryParts[0] : string.Empty;
        }

        static void DoHelpLabel()
        {
            string linkText =PlasticLocalization.Name.HereLink.GetString();
            string labelText = PlasticLocalization.Name.LearnMoreDifferencesUnityVCS.GetString();

            EditorGUILayout.BeginHorizontal();

            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
            labelStyle.richText = true;
            labelStyle.stretchWidth = false;
            labelStyle.margin = new RectOffset();
            labelStyle.padding.left = LEARN_MORE_LABEL_LEFT_PADDING;

            GUILayout.Label(labelText, labelStyle);

            GUIStyle linkStyle = new GUIStyle(UnityStyles.LinkLabel);
            linkStyle.fontSize = EditorStyles.miniLabel.fontSize;
            linkStyle.stretchWidth = false;
            linkStyle.margin = new RectOffset();

            if (GUILayout.Button(linkText, linkStyle))
            {
                Application.OpenURL(PlasticLocalization.Name.PlasticSCMFullVsPartialWorkspaceLink.GetString());
            }

            EditorGUIUtility.AddCursorRect(
               GUILayoutUtility.GetLastRect(), MouseCursor.Link);

            EditorGUILayout.EndHorizontal();
        }

        static void DoNotificationArea(ProgressControlsForViews.Data progressData)
        {
            if (string.IsNullOrEmpty(progressData.NotificationMessage))
                return;

            DrawProgressForViews.ForNotificationArea(progressData);
        }

        static void DoLabel(string labelText)
        {
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);

            Rect rect = GUILayoutUtility.GetRect(
                new GUIContent(labelText),
                labelStyle);

            GUI.Label(rect, labelText, labelStyle);
        }

        static string DoTextField(
            string entryValue,
            bool enabled,
            float textBoxLeft,
            float textBoxWidth)
        {
            GUI.enabled = enabled;

            var rect = GUILayoutUtility.GetRect(
                new GUIContent(entryValue),
                UnityStyles.Dialog.EntryLabel);
            rect.width = textBoxWidth;
            rect.x = textBoxLeft;

            string result = GUI.TextField(rect, entryValue);

            GUI.enabled = true;

            return result;
        }

        static bool DoButton(
            string text,
            bool isEnabled,
            float buttonLeft,
            float buttonWidth)
        {
            GUI.enabled = isEnabled;

            var rect = GUILayoutUtility.GetRect(
                new GUIContent(text),
                UnityStyles.Dialog.EntryLabel);

            rect.width = buttonWidth;
            rect.x = buttonLeft;

            bool result = GUI.Button(rect, text);
            GUI.enabled = true;
            return result;
        }

        static bool DoRadioButton(
            string text,
            bool isChecked,
            bool isEnabled,
            float buttonLeft)
        {
            GUI.enabled = isEnabled;

            GUIStyle radioButtonStyle = new GUIStyle(EditorStyles.radioButton);
            radioButtonStyle.padding.left = RADIO_BUTTON_LEFT_PADDING;

            var rect = GUILayoutUtility.GetRect(
                new GUIContent(text),
                radioButtonStyle);

            rect.x = buttonLeft;

            bool result = GUI.Toggle(
                rect,
                isChecked,
                text,
                radioButtonStyle);

            GUI.enabled = true;

            return result;
        }

        static void DoWarningLabel(
            string labelText,
            float labelLeft)
        {
            Rect rect = GUILayoutUtility.GetRect(
                new GUIContent(labelText),
                EditorStyles.label);

            rect.x = labelLeft;

            GUI.Label(rect,
                new GUIContent(labelText, Images.GetWarnIcon()),
                UnityStyles.HeaderWarningLabel);
        }

        static void DoProgress(
            ProgressControlsForViews.Data data,
            float progressLeft)
        {
            if (string.IsNullOrEmpty(data.ProgressMessage))
                return;

            var rect = GUILayoutUtility.GetRect(
                new GUIContent(data.ProgressMessage),
                UnityStyles.Dialog.EntryLabel);

            rect.x = progressLeft;

            GUI.Label(rect, data.ProgressMessage);
        }

        static bool IsValidState(
            CreateWorkspaceViewState state)
        {
            if (!ValidateRepository(state.Repository).IsValid)
                return false;

            if (!ValidateWorkspaceName(state.WorkspaceName).IsValid)
                return false;

            return true;
        }

        static ValidationResult ValidateRepository(string repository)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(repository))
            {
                result.ErrorMessage = PlasticLocalization.GetString(PlasticLocalization.Name.RepositoryNameEmpty);
                result.IsValid = false;
                return result;
            }

            result.IsValid = true;
            return result;
        }

        static ValidationResult ValidateWorkspaceName(string workspaceName)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(workspaceName))
            {
                result.ErrorMessage = PlasticLocalization.GetString(PlasticLocalization.Name.WorkspaceNameEmpty);
                result.IsValid = false;
                return result;
            }

            result.IsValid = true;
            return result;
        }

        class ValidationResult
        {
            internal string ErrorMessage;
            internal bool IsValid;
        }

        const float LABEL_WIDTH = 150;
        const float TEXTBOX_WIDTH = 400;
        const float BROWSE_BUTTON_WIDTH = 25;
        const float NEW_BUTTON_WIDTH = 60;
        const float BUTTON_MARGIN = 2;
        const float LABEL_MARGIN = 2;
        const float RADIO_BUTTON_MARGIN = 38;
        const int RADIO_BUTTON_LEFT_PADDING = 20;
        const float PROGRESS_MARGIN = 5;
        const float CREATE_WORKSPACE_BUTTON_MARGIN = 32;
        const float CREATE_WORKSPACE_BUTTON_WIDTH = 160;
        const int LEARN_MORE_LABEL_LEFT_PADDING = 10;
    }
}
