using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Codice.CM.Common;
using Codice.Client.Common.OAuth;
using Codice.Client.Common.Connection;
using PlasticGui;
using PlasticGui.Configuration.CloudEdition;
using PlasticGui.Configuration.CloudEdition.Welcome;
using PlasticGui.Configuration.OAuth;
using PlasticGui.WebApi.Responses;
using PlasticGui.WorkspaceWindow.Home;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;

namespace Unity.PlasticSCM.Editor.Configuration
{
    internal class SSOCredentialsDialog : PlasticDialog, OAuthSignIn.INotify, Login.INotify
    {
        protected override Rect DefaultRect
        {
            get
            {
                var baseRect = base.DefaultRect;
                return new Rect(baseRect.x, baseRect.y, 525, 450);
            }
        }

        internal static AskCredentialsToUser.DialogData RequestCredentials(
            string cloudServer,
            EditorWindow parentWindow)
        {
            SSOCredentialsDialog dialog = Create(
                cloudServer, new ProgressControlsForDialogs());

            ResponseType dialogResult = dialog.RunModal(parentWindow);

            return dialog.BuildCredentialsDialogData(dialogResult);
        }

        protected override string GetTitle()
        {
            return PlasticLocalization.Name.CredentialsDialogTitle.GetString();
        }

        protected override void OnModalGUI()
        {
            Title(PlasticLocalization.Name.CredentialsDialogTitle.GetString());

            Paragraph(PlasticLocalization.Name.CredentialsDialogExplanation.GetString(mOrganizationInfo.DisplayName));

            GUILayout.Space(20);

            DoEntriesArea();

            GUILayout.Space(10);

            DrawProgressForDialogs.For(
                mProgressControls.ProgressData);

            GUILayout.Space(10);

            DoButtonsArea();
        }

        void DoEntriesArea()
        {
            Paragraph("Sign in with Unity ID");
            GUILayout.Space(5);

            DoUnityIDButton();

            GUILayout.Space(25);
            Paragraph("    --or--    ");

            Paragraph("Sign in with email");

            mEmail = TextEntry(
                PlasticLocalization.Name.Email.GetString(),
                mEmail, ENTRY_WIDTH, ENTRY_X);

            GUILayout.Space(5);

            mPassword = PasswordEntry(
                PlasticLocalization.Name.Password.GetString(),
                mPassword, ENTRY_WIDTH, ENTRY_X);
        }

        void DoUnityIDButton()
        {
            if (NormalButton("Sign in with Unity ID"))
            {
                Guid state = Guid.NewGuid();

                OAuthSignInForUnityPackage(
                    GetCloudSsoProviders.BuildAuthInfoForUnityId(string.Empty, state).SignInUrl,
                    state,
                    new GetCloudSsoToken(PlasticGui.Plastic.WebRestAPI));
            }
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
            Login.Run(
                PlasticGui.Plastic.WebRestAPI,
                new SaveCloudEditionCreds(),
                mEmail,
                mPassword,
                string.Empty,
                string.Empty,
                Login.Mode.UnityPackage,
                mProgressControls,
                this);
        }

        void OAuthSignInForUnityPackage(Uri signInUrl, Guid state, IGetOauthToken getOauthToken)
        {
            OAuthSignIn mSignIn = new OAuthSignIn();

            mSignIn.ForUnityPackage(
                SEIDWorkingMode.SSOWorkingMode,
                signInUrl,
                state,
                string.Empty,
                string.Empty,
                mProgressControls,
                this,
                GetWindow<PlasticWindow>().CmConnectionForTesting,
                new OAuthSignIn.Browser(),
                getOauthToken,
                PlasticGui.Plastic.WebRestAPI);
        }

        void OAuthSignIn.INotify.SuccessForUnityPackage(OrganizationsResponse organizationResponse, string userName, string accessToken)
        {
            if (!organizationResponse.CloudServers.Contains(mOrganizationInfo.Server))
            {
                CancelButtonAction();
                return;
            }

            mEmail = userName;
            mPassword = accessToken;

            ClientConfiguration.Save(
                mOrganizationInfo.Server,
                SEIDWorkingMode.SSOWorkingMode,
                userName,
                accessToken);

            GetWindow<PlasticWindow>().InitializePlastic();
            OkButtonAction();
        }

        void OAuthSignIn.INotify.SuccessForSSO(string organization)
        {
            OkButtonAction();
        }

        void OAuthSignIn.INotify.SuccessForProfile(string email)
        {
            OkButtonAction();
        }

        void OAuthSignIn.INotify.SuccessForCredentials(
            string email,
            string accessToken)
        {
            OkButtonAction();
        }

        void OAuthSignIn.INotify.SuccessForHomeView(string usrName)
        {
            OkButtonAction();
        }

        void OAuthSignIn.INotify.SuccessForConfigure(
            List<string> organizations,
            bool canCreateAnOrganization,
            string userName,
            string accessToken)
        {
            OkButtonAction();
        }

        void OAuthSignIn.INotify.Cancel(string errorMessage)
        {
            CancelButtonAction();
        }

        void Login.INotify.SuccessForUnityPackage(
            OrganizationsResponse organizationResponse,
            string userName,
            string accessToken)
        {
            if (!organizationResponse.CloudServers.Contains(mOrganizationInfo.Server))
            {
                CancelButtonAction();
                return;
            }

            mEmail = userName;
            mPassword = accessToken;

            ClientConfiguration.Save(
                mOrganizationInfo.Server,
                SEIDWorkingMode.LDAPWorkingMode,
                userName,
                accessToken);

            GetWindow<PlasticWindow>().InitializePlastic();
            OkButtonAction();
        }

        void Login.INotify.SuccessForConfigure(
            List<string> organizations,
            bool canCreateAnOrganization,
            string userName,
            string password)
        {
            OkButtonAction();
        }

        void Login.INotify.SuccessForSSO(
            string organization)
        {
            OkButtonAction();
        }

        void Login.INotify.SuccessForCredentials(string userName, string password)
        {
            OkButtonAction();
        }

        void Login.INotify.SuccessForProfile(
            string userName)
        {
            OkButtonAction();
        }

        void Login.INotify.SuccessForHomeView(string userName)
        {
            OkButtonAction();
        }

        void Login.INotify.ValidationFailed(
            Login.ValidationResult validationResult)
        {
            CancelButtonAction();
        }

        void Login.INotify.SignUpNeeded(
            Login.Data loginData)
        {
            CancelButtonAction();
        }

        void Login.INotify.Error(
            string message)
        {
            CancelButtonAction();
        }

        AskCredentialsToUser.DialogData BuildCredentialsDialogData(
            ResponseType dialogResult)
        {
            return new AskCredentialsToUser.DialogData(
                dialogResult == ResponseType.Ok,
                mEmail, mPassword, false, SEIDWorkingMode.SSOWorkingMode);
        }

        static SSOCredentialsDialog Create(
            string server,
            ProgressControlsForDialogs progressControls)
        {
            var instance = CreateInstance<SSOCredentialsDialog>();
            instance.mOrganizationInfo = OrganizationsInformation.FromServer(server);
            instance.mProgressControls = progressControls;
            instance.mEnterKeyAction = instance.OkButtonWithValidationAction;
            instance.mEscapeKeyAction = instance.CancelButtonAction;
            return instance;
        }

        string mEmail;
        string mPassword = string.Empty;

        ProgressControlsForDialogs mProgressControls;

        OrganizationInfo mOrganizationInfo;

        const float ENTRY_WIDTH = 345f;
        const float ENTRY_X = 150f;
    }
}
