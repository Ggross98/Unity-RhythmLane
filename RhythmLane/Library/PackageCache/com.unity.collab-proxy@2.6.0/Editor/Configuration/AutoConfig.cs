using System;

using UnityEngine;

using Codice.CM.Common;
using Codice.LogWrapper;
using PlasticGui;
using Unity.PlasticSCM.Editor.WebApi;

namespace Unity.PlasticSCM.Editor.Configuration
{
    internal static class AutoConfig
    {
        internal static TokenExchangeResponse PlasticCredentials(
            string unityAccessToken,
            string serverName)
        {
            var startTick = Environment.TickCount;

            var tokenExchangeResponse = WebRestApiClient.PlasticScm.TokenExchange(unityAccessToken);

            mLog.DebugFormat("TokenExchange time {0} ms", Environment.TickCount - startTick);

            if (tokenExchangeResponse == null)
            {
                var warning = PlasticLocalization.GetString(PlasticLocalization.Name.TokenExchangeResponseNull);
                mLog.Warn(warning);
                Debug.LogWarning(warning);
                return null;
            }

            if (tokenExchangeResponse.Error != null)
            {
                var warning = string.Format(
                    PlasticLocalization.GetString(PlasticLocalization.Name.TokenExchangeResponseError),
                    tokenExchangeResponse.Error.Message, tokenExchangeResponse.Error.ErrorCode);
                mLog.ErrorFormat(warning);
                Debug.LogWarning(warning);
                return tokenExchangeResponse;
            }

            if (string.IsNullOrEmpty(tokenExchangeResponse.AccessToken))
            {
                var warning = string.Format(
                    PlasticLocalization.GetString(PlasticLocalization.Name.TokenExchangeAccessEmpty), 
                    tokenExchangeResponse.User);
                mLog.InfoFormat(warning);
                Debug.LogWarning(warning);
                return tokenExchangeResponse;
            }

            ClientConfiguration.Save(
                serverName,
                SEIDWorkingMode.SSOWorkingMode,
                tokenExchangeResponse.User,
                tokenExchangeResponse.AccessToken);

            return tokenExchangeResponse;
        }

        static readonly ILog mLog = PlasticApp.GetLogger("AutoConfig");
    }
}

