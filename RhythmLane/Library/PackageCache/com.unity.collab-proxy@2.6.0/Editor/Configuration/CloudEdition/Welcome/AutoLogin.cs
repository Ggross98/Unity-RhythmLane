using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Codice.Client.Common.Threading;
using Codice.CM.Common;
using Codice.LogWrapper;
using PlasticGui;
using PlasticGui.Configuration.OAuth;
using PlasticGui.WebApi.Responses;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;
using Unity.PlasticSCM.Editor.WebApi;

namespace Unity.PlasticSCM.Editor.Configuration.CloudEdition.Welcome
{
    internal class AutoLogin : OAuthSignIn.INotify
    {
        internal enum State : byte
        { 
            Off = 0,
            Started = 1,
            Running = 2,
            ResponseInit = 3,
            ResponseEnd = 6,
            ResponseSuccess = 7,
            OrganizationChoosed = 8,
            InitializingPlastic = 9,
            ErrorNoToken = 20,
            ErrorTokenException = 21,
            ErrorResponseNull = 22,
            ErrorResponseError = 23,
            ErrorTokenEmpty = 24,
            ErrorResponseCancel = 25
        }

        internal void Run()
        {
            mPlasticWindow = GetPlasticWindow();

            if (!string.IsNullOrEmpty(CloudProjectSettings.accessToken))
            {
                ExchangeTokensAndJoinOrganization(CloudProjectSettings.accessToken);
                return;
            }
            
            mPlasticWindow.GetWelcomeView().autoLoginState = AutoLogin.State.ErrorNoToken;
        }

        void ExchangeTokensAndJoinOrganization(string unityAccessToken)
        {
            int ini = Environment.TickCount;

            TokenExchangeResponse tokenExchangeResponse = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
            /*threadOperationDelegate*/ delegate
            {
                mPlasticWindow.GetWelcomeView().autoLoginState = AutoLogin.State.ResponseInit;
                tokenExchangeResponse = WebRestApiClient.PlasticScm.TokenExchange(unityAccessToken);
            },
            /*afterOperationDelegate*/ delegate
            {
                mLog.DebugFormat(
                    "TokenExchange time {0} ms",
                    Environment.TickCount - ini);

                if (waiter.Exception != null)
                {
                    mPlasticWindow.GetWelcomeView().autoLoginState = AutoLogin.State.ErrorTokenException;
                    ExceptionsHandler.LogException(
                        "TokenExchangeSetting",
                        waiter.Exception);
                    Debug.LogWarning(waiter.Exception.Message);
                    return;
                }

                if (tokenExchangeResponse == null)
                {
                    mPlasticWindow.GetWelcomeView().autoLoginState = AutoLogin.State.ErrorResponseNull;
                    var warning = PlasticLocalization.GetString(PlasticLocalization.Name.TokenExchangeResponseNull);
                    mLog.Warn(warning);
                    Debug.LogWarning(warning);
                    return;
                }
                   
                if (tokenExchangeResponse.Error != null)
                {
                    mPlasticWindow.GetWelcomeView().autoLoginState = AutoLogin.State.ErrorResponseError;
                    var warning = string.Format(
                        PlasticLocalization.GetString(PlasticLocalization.Name.TokenExchangeResponseError),
                        tokenExchangeResponse.Error.Message, tokenExchangeResponse.Error.ErrorCode);
                    mLog.ErrorFormat(warning);
                    Debug.LogWarning(warning);
                    return;
                }

                if (string.IsNullOrEmpty(tokenExchangeResponse.AccessToken))
                {
                    mPlasticWindow.GetWelcomeView().autoLoginState = AutoLogin.State.ErrorTokenEmpty;
                    var warning = string.Format(
                        PlasticLocalization.GetString(PlasticLocalization.Name.TokenExchangeAccessEmpty), 
                        tokenExchangeResponse.User);
                    mLog.InfoFormat(warning);
                    Debug.LogWarning(warning);
                    return;
                }

                mPlasticWindow.GetWelcomeView().autoLoginState = AutoLogin.State.ResponseEnd;

                GetOrganizationList(tokenExchangeResponse.User, tokenExchangeResponse.AccessToken);
            });
        }

        void GetOrganizationList(string userName, string accessToken)
        {
            OAuthSignIn.GetOrganizationsFromAccessToken(
                SsoProvider.UNITY_URL_ACTION,
                userName,
                accessToken,
                OAuthSignIn.Mode.UnityPackage,
                new ProgressControlsForDialogs(),
                this,
                PlasticGui.Plastic.WebRestAPI
            );
        }

        void OAuthSignIn.INotify.SuccessForUnityPackage(OrganizationsResponse organizationResponse, string userName, string accessToken)
        {
            mPlasticWindow.GetWelcomeView().autoLoginState = AutoLogin.State.ResponseSuccess;
            ChooseOrganization(userName, accessToken, organizationResponse);
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
            mPlasticWindow.GetWelcomeView().autoLoginState = AutoLogin.State.ErrorResponseCancel;
        }

        void ChooseOrganization(string userName, string accessToken, OrganizationsResponse organizationResponse)
        {
            mPlasticWindow = GetPlasticWindow();

            CloudEditionWelcomeWindow.ShowWindow(
                PlasticGui.Plastic.WebRestAPI,
                mPlasticWindow.CmConnectionForTesting, null, true);

            mCloudEditionWelcomeWindow = CloudEditionWelcomeWindow.GetWelcomeWindow();

            mCloudEditionWelcomeWindow.ShowOrganizationsPanelFromAuthResponse(
                userName,
                accessToken,
                organizationResponse,
                SEIDWorkingMode.SSOWorkingMode);

            mCloudEditionWelcomeWindow.Focus();
        }

        static PlasticWindow GetPlasticWindow()
        {
            var windows = Resources.FindObjectsOfTypeAll<PlasticWindow>();
            PlasticWindow plasticWindow = windows.Length > 0 ? windows[0] : null;

            if (plasticWindow == null)
                plasticWindow = ShowWindow.Plastic();

            return plasticWindow;
        }

        PlasticWindow mPlasticWindow;
        CloudEditionWelcomeWindow mCloudEditionWelcomeWindow;

        static readonly ILog mLog = PlasticApp.GetLogger("TokensExchange");
    }
}
