using UnityEditor;
using UnityEngine;

using Codice.Utils;
using PlasticGui;
using PlasticGui.WorkspaceWindow.Home;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.Configuration
{
    internal class EncryptionConfigurationDialog : PlasticDialog
    {
        protected override Rect DefaultRect
        {
            get
            {
                var baseRect = base.DefaultRect;
                return new Rect(baseRect.x, baseRect.y, 650, 425);
            }
        }

        internal static EncryptionConfigurationDialogData RequestEncryptionPassword(
            string server,
            EditorWindow parentWindow)
        {
            EncryptionConfigurationDialog dialog = Create(server);

            ResponseType dialogResult = dialog.RunModal(parentWindow);

            EncryptionConfigurationDialogData result =
                dialog.BuildEncryptionConfigurationData();

            result.Result = dialogResult == ResponseType.Ok;
            return result;
        }

        protected override void OnModalGUI()
        {
            Title(PlasticLocalization.Name.EncryptionConfiguration.GetString());

            GUILayout.Space(20);

            Paragraph(PlasticLocalization.Name.EncryptionConfigurationExplanation.GetString(mOrganizationInfo.DisplayName));

            DoPasswordArea();

            Paragraph(PlasticLocalization.Name.EncryptionConfigurationRemarks.GetString(mOrganizationInfo.DisplayName));

            GUILayout.Space(10);

            DoNotificationArea();

            GUILayout.Space(10);

            DoButtonsArea();
        }

        protected override string GetTitle()
        {
            return PlasticLocalization.Name.EncryptionConfiguration.GetString();
        }

        EncryptionConfigurationDialogData BuildEncryptionConfigurationData()
        {
            return new EncryptionConfigurationDialogData(
                CryptoServices.GetEncryptedPassword(mPassword.Trim()));
        }

        void DoPasswordArea()
        {
            Paragraph(PlasticLocalization.Name.EncryptionConfigurationEnterPassword.GetString());

            GUILayout.Space(5);

            mPassword = PasswordEntry(PlasticLocalization.Name.Password.GetString(),
                mPassword, PASSWORD_TEXT_WIDTH, PASSWORD_TEXT_X);

            GUILayout.Space(5);

            mRetypePassword = PasswordEntry(PlasticLocalization.Name.RetypePassword.GetString(),
                mRetypePassword, PASSWORD_TEXT_WIDTH, PASSWORD_TEXT_X);

            GUILayout.Space(18f);
        }

        void DoNotificationArea()
        {
            if (string.IsNullOrEmpty(mErrorMessage))
                return;

            var rect = GUILayoutUtility.GetRect(
                GUILayoutUtility.GetLastRect().width, 30);

            EditorGUI.HelpBox(rect, mErrorMessage, MessageType.Error);
        }

        void DoButtonsArea()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    DoOkButton();
                    DoCancelButton();
                    return;
                }

                DoCancelButton();
                DoOkButton();
            }
        }

        void DoOkButton()
        {
            if (!AcceptButton(PlasticLocalization.Name.OkButton.GetString()))
                return;

            OkButtonWithValidationAction();
        }

        void DoCancelButton()
        {
            if (!NormalButton(PlasticLocalization.Name.CancelButton.GetString()))
                return;

            CancelButtonAction();
        }

        void OkButtonWithValidationAction()
        {
            if (IsValidPassword(
                    mPassword.Trim(), mRetypePassword.Trim(),
                    out mErrorMessage))
            {
                mErrorMessage = string.Empty;
                OkButtonAction();
                return;
            }

            mPassword = string.Empty;
            mRetypePassword = string.Empty;
        }

        static bool IsValidPassword(
            string password, string retypePassword,
            out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(password))
            {
                errorMessage = PlasticLocalization.Name.InvalidEmptyPassword.GetString();
                return false;
            }

            if (!password.Equals(retypePassword))
            {
                errorMessage = PlasticLocalization.Name.PasswordDoesntMatch.GetString();
                return false;
            }

            return true;
        }

        static EncryptionConfigurationDialog Create(string server)
        {
            var instance = CreateInstance<EncryptionConfigurationDialog>();
            instance.mOrganizationInfo = OrganizationsInformation.FromServer(server);
            instance.mEnterKeyAction = instance.OkButtonWithValidationAction;
            instance.mEscapeKeyAction = instance.CancelButtonAction;
            return instance;
        }

        string mPassword = string.Empty;
        string mRetypePassword = string.Empty;
        string mErrorMessage = string.Empty;

        OrganizationInfo mOrganizationInfo;

        const float PASSWORD_TEXT_WIDTH = 250f;
        const float PASSWORD_TEXT_X = 200f;
    }
}

