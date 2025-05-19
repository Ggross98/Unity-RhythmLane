using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Codice.Client.Common;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.Configuration.OAuth;
using PlasticGui.WebApi;
using PlasticGui.WebApi.Responses;
using Unity.PlasticSCM.Editor.Views.Welcome;

namespace Unity.PlasticSCM.Editor.Configuration.CloudEdition.Welcome
{
    internal interface IWelcomeWindowNotify
    {
        void ProcessLoginResponse(OrganizationsResponse organizationResponse, string userName, string accessToken);
        void Back();
    }

    internal class CloudEditionWelcomeWindow :
        EditorWindow,
        OAuthSignIn.INotify,
        IWelcomeWindowNotify
    {
        internal static void ShowWindow(
            IPlasticWebRestApi restApi,
            CmConnection cmConnection,
            WelcomeView welcomeView,
            bool autoLogin = false)
        {
            sRestApi = restApi;
            sCmConnection = cmConnection;
            sAutoLogin = autoLogin;
            CloudEditionWelcomeWindow window = GetWindow<CloudEditionWelcomeWindow>();

            window.titleContent = new GUIContent(
                PlasticLocalization.GetString(PlasticLocalization.Name.SignInToUnityVCS));
            window.minSize = window.maxSize = new Vector2(450, 300);

            window.mWelcomeView = welcomeView;

            window.Show();
        }

        internal static CloudEditionWelcomeWindow GetWelcomeWindow()
        {
            return GetWindow<CloudEditionWelcomeWindow>();
        }

        internal void CancelJoinOrganization()
        {
            if (sAutoLogin)
            {
                GetWindow<PlasticWindow>().GetWelcomeView().autoLoginState = AutoLogin.State.Started;
            }
        }

        internal void ReplaceRootPanel(VisualElement panel)
        {
            rootVisualElement.Clear();
            rootVisualElement.Add(panel);
        }

        internal SignInPanel GetSignInPanel()
        {
            return mSignInPanel;
        }

        internal void ShowOrganizationsPanelFromAuthResponse(
            string userName,
            string accessToken,
            OrganizationsResponse organizationResponse,
            SEIDWorkingMode workingMode)
        {
            mOrganizationPanel = new OrganizationPanel(
                this,
                organizationResponse,
                joinedOrganization =>
                {
                    ClientConfiguration.Save(joinedOrganization, workingMode, userName, accessToken);
                },
                GetWindowTitle());
            
            ReplaceRootPanel(mOrganizationPanel);
        }

        string GetWindowTitle()
        {
            return PlasticLocalization.Name.SignInToUnityVCS.GetString();
        }

        void OnEnable()
        {
            BuildComponents();
        }

        void OnDestroy()
        {
            Dispose();

            if (mWelcomeView != null)
                mWelcomeView.OnUserClosedConfigurationWindow();
        }

        void Dispose()
        {
            if (mSignInPanel != null)
                mSignInPanel.Dispose();

            if (mOrganizationPanel != null)
                mOrganizationPanel.Dispose();
        }

        void OAuthSignIn.INotify.SuccessForUnityPackage(
            OrganizationsResponse organizationResponse,
            string userName,
            string accessToken)
        {
            ShowOrganizationsPanelFromAuthResponse(userName,
                accessToken,
                organizationResponse,
                SEIDWorkingMode.SSOWorkingMode);
        }

        void OAuthSignIn.INotify.SuccessForConfigure(
            List<string> organizations,
            bool canCreateAnOrganization,
            string userName,
            string accessToken)
        {
            // empty implementation
        }

        void OAuthSignIn.INotify.SuccessForSSO(string organization)
        {
            // empty implementation
        }

        void OAuthSignIn.INotify.SuccessForProfile(string email)
        {
            // empty implementation
        }

        void OAuthSignIn.INotify.SuccessForHomeView(string userName)
        {
            // empty implementation
        }

        void OAuthSignIn.INotify.SuccessForCredentials(
            string email,
            string accessToken)
        {
            // empty implementation
        }

        void OAuthSignIn.INotify.Cancel(string errorMessage)
        {
            Focus();
        }

        void IWelcomeWindowNotify.ProcessLoginResponse(OrganizationsResponse organizationResponse, string userName, string accessToken)
        {
            ShowOrganizationsPanelFromAuthResponse(
                userName,
                accessToken,
                organizationResponse,
                SEIDWorkingMode.LDAPWorkingMode);
        }

        void IWelcomeWindowNotify.Back()
        {
            rootVisualElement.Clear();
            rootVisualElement.Add(mSignInPanel);
        }

        void BuildComponents()
        {
            VisualElement root = rootVisualElement;

            root.Clear();

            mSignInPanel = new SignInPanel(
                this,
                sRestApi,
                sCmConnection);

            titleContent = new GUIContent(GetWindowTitle());

            root.Add(mSignInPanel);

            if (sAutoLogin)
            {
                mSignInPanel.SignInWithUnityIdButtonAutoLogin();
            }
        }

        string mUserName;

        OrganizationPanel mOrganizationPanel;
        SignInPanel mSignInPanel;
        WelcomeView mWelcomeView;

        static IPlasticWebRestApi sRestApi;
        static CmConnection sCmConnection;
        static bool sAutoLogin = false;
    }
}