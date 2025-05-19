using Codice.Client.Common;
using Codice.Client.Common.Servers;
using Codice.CM.Common;
using Codice.Utils;
using PlasticGui;
using PlasticGui.Configuration.TeamEdition;
using PlasticGui.WorkspaceWindow.Home;

namespace Unity.PlasticSCM.Editor.Configuration
{
    internal static class ClientConfiguration
    {
        internal static void Save(
            string server,
            SEIDWorkingMode workingMode,
            string user,
            string accessToken
        )
        {
            SetupUnityEditionToken.CreateCloudEditionTokenIfNeeded();

            // We observed that UserAccounts.SaveAccount skips the client.conf update when there is auth info saved.
            // However, we want to make sure the configuration is always updated, so we add the skipped saving.
            if (ClientConfig.HasAuthInfoConfigured())
            {
                ConfigurationActions.SaveClientConfig(
                    server,
                    workingMode,
                    user,
                    accessToken,
                    null);
            }

            // This creates the client.conf if needed but doesn't overwrite it if it exists already,
            // and it also updates the profiles.conf and tokens.conf with the new AccessToken
            UserAccounts.SaveAccount(
                server,
                workingMode,
                user,
                accessToken,
                null,
                null,
                null);

            SaveDefaultCloudServer(server);
        }

        // Save the Default Server in the config files of all clients, so they are already configured.
        // Avoids having the Desktop application asking the user again later.
        static void SaveDefaultCloudServer(string cloudServer)
        {
            SaveCloudServer.ToPlasticGuiConfig(cloudServer);
            SaveCloudServer.ToPlasticGuiConfigFile(
                cloudServer, GetPlasticConfigFileToSaveOrganization());
            SaveCloudServer.ToPlasticGuiConfigFile(
                cloudServer, GetGluonConfigFileToSaveOrganization());

            KnownServers.ServersFromCloud.InitializeForWindows(
                PlasticGuiConfig.Get().Configuration.DefaultCloudServer);
        }

        static string GetPlasticConfigFileToSaveOrganization()
        {
            if (PlatformIdentifier.IsMac())
            {
                return "macgui.conf";
            }

            return "plasticgui.conf";
        }

        static string GetGluonConfigFileToSaveOrganization()
        {
            if (PlatformIdentifier.IsMac())
            {
                return "gluon.conf";
            }

            return "gameui.conf";
        }
    }
}
