using System;
using System.Collections.Generic;
using System.Linq;

using Codice.Client.Common;
using Codice.Client.Common.Threading;
using Codice.CM.Common;
using Codice.LogWrapper;
using PlasticGui.WebApi.Responses;
using PlasticGui.WorkspaceWindow.Home;

namespace Unity.PlasticSCM.Editor
{
    internal static class OrganizationsInformation
    {
        internal static bool IsUnityOrganization(string server)
        {
            return ResolveServer.ToDisplayString(server).EndsWith(UnityOrganizationServer);
        }

        internal static void LoadCloudOrganizationsAsync()
        {
            PlasticThreadPool.Run(delegate
            {
                try
                {
                    OrganizationsResponse organizationResponse = PlasticGui.Plastic.WebRestAPI.GetCloudServers();

                    if (organizationResponse.Error != null)
                    {
                        mLog.ErrorFormat(
                            "Unable to retrieve cloud organizations: {0} [code {1}]",
                            organizationResponse.Error.Message,
                            organizationResponse.Error.ErrorCode);

                        return;
                    }

                    UpdateOrganizationSlugs(organizationResponse);
                }
                catch (Exception e)
                {
                    ExceptionsHandler.LogException(typeof(OrganizationsInformation).Name, e);
                }
            });
        }

        internal static void UpdateOrganizationSlugs(OrganizationsResponse organizationsResponse)
        {
            if (organizationsResponse == null || organizationsResponse.OrganizationInfo == null)
            {
                return;
            }

            List<string> genesisOrgIds = new List<string>();
            List<string> slugs = new List<string>();

            foreach (string organization in organizationsResponse.Organizations)
            {
                OrganizationsResponse.OrganizationInformation info;

                if (!organizationsResponse.OrganizationInfo.TryGetValue(organization, out info))
                {
                    continue;
                }

                if (info.Type != OrganizationsResponse.OrganizationInformation.ORGANIZATION_TYPE_UNITY)
                {
                    continue;
                }

                genesisOrgIds.Add(organization);
                slugs.Add(info.Slug);
            }

            PlasticGui.Plastic.API.SetUnityOrganizationsSlugData(genesisOrgIds, slugs);
        }

        internal static List<OrganizationInfo> FromServersOrdered(List<string> serverNames)
        {
            List<OrganizationInfo> organizationsInfo = new List<OrganizationInfo>();

            foreach (var organizationServer in serverNames)
            {
                organizationsInfo.Add(FromServer(organizationServer));
            }

            organizationsInfo.Sort((x, y) =>
                string.Compare(x.DisplayName, y.DisplayName, StringComparison.CurrentCulture));

            return organizationsInfo;
        }

        internal static OrganizationInfo FromServer(string organizationServer)
        {
            return new OrganizationInfo(
                CloudServer.GetOrganizationName(organizationServer),
                ResolveServer.ToDisplayString(organizationServer),
                organizationServer,
                CloudServer.IsUnityOrganization(organizationServer) ?
                    OrganizationInfo.OrganizationType.Unity :
                    OrganizationInfo.OrganizationType.Cloud );
        }

        internal static List<string> GetOrganizationProjects(string organizationServer)
        {
            RepositoryInfo[] allServerProjects = CmConnection.Get().GetRepositoryHandler(organizationServer)
                .GetRepositoryList(RepositoryType.Project);

            List<string> serverProjects = allServerProjects
                .Where(project => !RepositoryInfo.IsDeleted(project))
                .Select(project => project.Name)
                .ToList();

            serverProjects.Sort();

            return serverProjects;
        }

        internal static string TryResolveServerFromInput(string userInputServer)
        {
            try
            {
                // This method talks to the cloud servers if needed, and can throw unexpected exceptions we need to control
                return ResolveServer.FromUserInput(userInputServer, CmConnection.Get().UnityOrgResolver);
            }
            catch (Exception e)
            {
                mLog.ErrorFormat("Could not resolve the server {0}: {1}", userInputServer, e.Message);
                return null;
            }
        }

        internal static RepositorySpec TryResolveRepositorySpecFromInput(string userInputRepSpec)
        {
            try
            {
                // This method talks to the cloud servers if needed, and can throw unexpected exceptions we need to control
                return new SpecGenerator().GenRepositorySpec(false, userInputRepSpec, CmConnection.Get().UnityOrgResolver);
            }
            catch (Exception e)
            {
                mLog.ErrorFormat("Could not resolve the repSpec {0}: {1}", userInputRepSpec, e.Message);
                return null;
            }
        }

        static readonly string UnityOrganizationServer = "@unity";

        static readonly ILog mLog = PlasticApp.GetLogger("OrganizationsInformation");
    }
}
