using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

using Codice.Client.Common;
using Codice.Client.Common.Threading;
using Codice.CM.Common;
using Codice.LogWrapper;
using PlasticGui.WorkspaceWindow.Home;
using PlasticGui;

namespace Unity.PlasticSCM.Editor.AssetUtils.Processor
{
    internal class UnityCloudProjectLinkMonitor : AssetModificationProcessor
    {
        internal class CloudSettings
        {
            internal string OrganizationId;
            internal string OrganizationName;
            internal string ProjectId;
            internal string ProjectName;
        }

        /// <summary>
        /// Factory class to retrieve the cloud settings for the current project.
        /// </summary>
        internal static Func<CloudSettings> CloudSettingsFactory = () => new CloudSettings()
        {
            OrganizationId = CloudProjectSettings.organizationId,
            OrganizationName = CloudProjectSettings.organizationName,
            ProjectId = CloudProjectSettings.projectId,
            ProjectName = CloudProjectSettings.projectName
        };

        /// <summary>
        /// Checks if the workspace project is aligned with the linked project in the cloud settings (if any).
        /// If not, logs a warning message for users to inform about the issue and give indications on how to re-link.
        /// </summary>
        internal static void CheckCloudProjectAlignmentAsync(WorkspaceInfo wkInfo)
        {
            mLog.Debug("Checking for cloud project alignment...");

            mCachedWkInfo = wkInfo;

            CloudSettings cloudSettings = CloudSettingsFactory();

            if (string.IsNullOrEmpty(cloudSettings.OrganizationId) || string.IsNullOrEmpty(cloudSettings.ProjectId))
            {
                mCachedCloudSettings = cloudSettings;

                mLog.Debug("The project is not connected to Unity Cloud");
                return;
            }

            if (cloudSettings.OrganizationId.Equals(mCachedCloudSettings.OrganizationId) &&
                cloudSettings.ProjectId.Equals(mCachedCloudSettings.ProjectId))
            {
                mLog.Debug("The linked project didn't change");
                return;
            }

            mCachedCloudSettings = cloudSettings;

            CheckCloudProjectAlignmentAsync();
        }

        static void CheckCloudProjectAlignmentAsync()
        {
            PlasticThreadPool.Run(delegate
            {
                try
                {
                    RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(mCachedWkInfo);
                    if (repSpec == null || !OrganizationsInformation.IsUnityOrganization(repSpec.Server))
                    {
                        mLog.Debug("Skipping check for not covered organization");
                        return;
                    }

                    RepositoryInfo repositoryProject = ProjectInfo.ForRepSpec(repSpec);

                    if (repositoryProject == null || !repositoryProject.GUID.ToString().Equals(mCachedCloudSettings.ProjectId))
                    {
                        string repositoryOrganizationName = CloudServer.GetOrganizationName(ResolveServer.ToDisplayString(repSpec.Server));
                        string repositoryProjectName = CloudProject.GetProjectName(repSpec.Name);

                        LogMismatchingProject(
                            mCachedCloudSettings.OrganizationName, mCachedCloudSettings.ProjectName,
                            repositoryOrganizationName, repositoryProjectName);
                    }
                    else
                    {
                        mLog.Debug("The linked cloud project is properly aligned");
                    }
                }
                catch (Exception e)
                {
                    ExceptionsHandler.LogException(typeof(UnityCloudProjectLinkMonitor).Name, e);
                }
            });
        }

        static void LogMismatchingProject(string cloudOrganization, string cloudProject, string repoOrganization, string repoProject)
        {
            string localOrganizationAndProject = string.Format("{0}/{1}", repoOrganization, repoProject);
            string cloudOrganizationAndProject = string.Format("{0}/{1}", cloudOrganization, cloudProject);

            string mismatchingProjectMessage = string.Format(
                PlasticLocalization.Name.MismatchingRepositoryProjectMessage.GetString(),
                cloudOrganizationAndProject,
                localOrganizationAndProject,
                localOrganizationAndProject,
                repoOrganization,
                localOrganizationAndProject,
                cloudOrganizationAndProject
            );

            Debug.LogWarning(mismatchingProjectMessage);
            mLog.Warn(mismatchingProjectMessage);
        }

        static string[] OnWillSaveAssets(string[] paths)
        {
            if (mCachedWkInfo != null && paths.Any(path => path.Equals(ProjectSettingsAsset)))
            {
                CheckCloudProjectAlignmentAsync(mCachedWkInfo);
            }

            return paths;
        }

        static WorkspaceInfo mCachedWkInfo;
        static CloudSettings mCachedCloudSettings = new CloudSettings();

        static readonly string ProjectSettingsAsset = "ProjectSettings/ProjectSettings.asset";

        static readonly ILog mLog = PlasticApp.GetLogger("UnityCloudProjectLinkMonitor");
    }
}
