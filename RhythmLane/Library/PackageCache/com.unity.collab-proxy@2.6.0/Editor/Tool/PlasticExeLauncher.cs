using Codice.Client.Commands;
using Codice.Client.Common.EventTracking;
using Codice.CM.Common;

namespace Unity.PlasticSCM.Editor.Tool
{
    internal class PlasticExeLauncher : IToolLauncher
    {
        internal interface ICheckIsExeAvailable
        {
            bool ForMode(bool isGluonMode);
        }

        internal interface IProcessExecutor
        {
            bool Execute(string tool, string arguments, bool bWait);
        }

        internal static PlasticExeLauncher BuildForDiffContributors(
            WorkspaceInfo wkInfo,
            bool isGluonMode,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            ICheckIsExeAvailable checkIsExeAvailable = null,
            IProcessExecutor processExecutor = null)
        {
            return new PlasticExeLauncher(
                wkInfo,
                isGluonMode,
                TrackFeatureUseEvent.Features.InstallPlasticCloudFromDiffContributors,
                TrackFeatureUseEvent.Features.InstallPlasticEnterpriseFromDiffContributors,
                TrackFeatureUseEvent.Features.CancelPlasticInstallationFromDiffContributors,
                false,
                showDownloadPlasticExeWindow,
                checkIsExeAvailable ?? new CheckIsExeAvailable(),
                processExecutor ?? new ProcessExecutor());
        }

        internal static PlasticExeLauncher BuildForDiffWorkspaceContent(
            WorkspaceInfo wkInfo,
            bool isGluonMode,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            ICheckIsExeAvailable checkIsExeAvailable = null,
            IProcessExecutor processExecutor = null)
        {
            return new PlasticExeLauncher(
                wkInfo,
                isGluonMode,
                TrackFeatureUseEvent.Features.InstallPlasticCloudFromDiffWorkspaceContent,
                TrackFeatureUseEvent.Features.InstallPlasticEnterpriseFromDiffWorkspaceContent,
                TrackFeatureUseEvent.Features.CancelPlasticInstallationFromDiffWorkspaceContent,
                false,
                showDownloadPlasticExeWindow,
                checkIsExeAvailable ?? new CheckIsExeAvailable(),
                processExecutor ?? new ProcessExecutor());
        }

        internal static PlasticExeLauncher BuildForShowDiff(
            WorkspaceInfo wkInfo,
            bool isGluonMode,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            ICheckIsExeAvailable checkIsExeAvailable = null,
            IProcessExecutor processExecutor = null)
        {
            return new PlasticExeLauncher(
                wkInfo,
                isGluonMode,
                TrackFeatureUseEvent.Features.InstallPlasticCloudFromShowDiff,
                TrackFeatureUseEvent.Features.InstallPlasticEnterpriseFromFromShowDiff,
                TrackFeatureUseEvent.Features.CancelPlasticInstallationFromFromShowDiff,
                false,
                showDownloadPlasticExeWindow,
                checkIsExeAvailable ?? new CheckIsExeAvailable(),
                processExecutor ?? new ProcessExecutor());
        }

        internal static PlasticExeLauncher BuildForDiffRevision(
            WorkspaceInfo wkInfo,
            bool isGluonMode,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            ICheckIsExeAvailable checkIsExeAvailable = null,
            IProcessExecutor processExecutor = null)
        {
            return new PlasticExeLauncher(
                wkInfo,
                isGluonMode,
                TrackFeatureUseEvent.Features.InstallPlasticCloudFromDiffRevision,
                TrackFeatureUseEvent.Features.InstallPlasticEnterpriseFromDiffRevision,
                TrackFeatureUseEvent.Features.CancelPlasticInstallationFromDiffRevision,
                false,
                showDownloadPlasticExeWindow,
                checkIsExeAvailable ?? new CheckIsExeAvailable(),
                processExecutor ?? new ProcessExecutor());
        }

        internal static PlasticExeLauncher BuildForDiffRevision(
            RepositorySpec repSpec,
            bool isGluonMode,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            ICheckIsExeAvailable checkIsExeAvailable = null,
            IProcessExecutor processExecutor = null)
        {
            return new PlasticExeLauncher(
                repSpec,
                isGluonMode,
                TrackFeatureUseEvent.Features.InstallPlasticCloudFromDiffRevision,
                TrackFeatureUseEvent.Features.InstallPlasticEnterpriseFromDiffRevision,
                TrackFeatureUseEvent.Features.CancelPlasticInstallationFromDiffRevision,
                false,
                showDownloadPlasticExeWindow,
                checkIsExeAvailable ?? new CheckIsExeAvailable(),
                processExecutor ?? new ProcessExecutor());
        }

        internal static PlasticExeLauncher BuildForDiffSelectedRevisions(
            RepositorySpec repSpec,
            bool isGluonMode,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            ICheckIsExeAvailable checkIsExeAvailable = null,
            IProcessExecutor processExecutor = null)
        {
            return new PlasticExeLauncher(
                repSpec,
                isGluonMode,
                TrackFeatureUseEvent.Features.InstallPlasticCloudFromDiffRevision,
                TrackFeatureUseEvent.Features.InstallPlasticEnterpriseFromDiffRevision,
                TrackFeatureUseEvent.Features.CancelPlasticInstallationFromDiffRevision,
                false,
                showDownloadPlasticExeWindow,
                checkIsExeAvailable ?? new CheckIsExeAvailable(),
                processExecutor ?? new ProcessExecutor());
        }

        internal static PlasticExeLauncher BuildForResolveConflicts(
            WorkspaceInfo wkInfo,
            bool isGluonMode,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            ICheckIsExeAvailable checkIsExeAvailable = null,
            IProcessExecutor processExecutor = null)
        {
            return new PlasticExeLauncher(
                wkInfo,
                    isGluonMode,
                    TrackFeatureUseEvent.Features.InstallPlasticCloudFromResolveConflicts,
                    TrackFeatureUseEvent.Features.InstallPlasticEnterpriseFromResolveConflicts,
                    TrackFeatureUseEvent.Features.CancelPlasticInstallationFromResolveConflicts,
                true,
                showDownloadPlasticExeWindow,
                checkIsExeAvailable ?? new CheckIsExeAvailable(),
                processExecutor ?? new ProcessExecutor());
        }

        internal static PlasticExeLauncher BuildForMergeSelectedFiles(
            WorkspaceInfo wkInfo,
            bool isGluonMode,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            ICheckIsExeAvailable checkIsExeAvailable = null,
            IProcessExecutor processExecutor = null)
        {
            return new PlasticExeLauncher(
                wkInfo,
                isGluonMode,
                TrackFeatureUseEvent.Features.InstallPlasticCloudFromMergeSelectedFiles,
                TrackFeatureUseEvent.Features.InstallPlasticEnterpriseFromMergeSelectedFiles,
                TrackFeatureUseEvent.Features.CancelPlasticInstallationFromMergeSelectedFiles,
                true,
                showDownloadPlasticExeWindow,
                checkIsExeAvailable ?? new CheckIsExeAvailable(),
                processExecutor ?? new ProcessExecutor());
        }

        PlasticExeLauncher(
            WorkspaceInfo wkInfo,
            bool isGluonMode,
            string installCloudFrom,
            string installEnterpriseFrom,
            string cancelInstallFrom,
            bool bWait,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            ICheckIsExeAvailable checkIsExeAvailable,
            IProcessExecutor processExecutor)
            : this(
                PlasticGui.Plastic.API.GetRepositorySpec(wkInfo),
                isGluonMode,
                installCloudFrom,
                installEnterpriseFrom,
                cancelInstallFrom,
                bWait,
                showDownloadPlasticExeWindow,
                checkIsExeAvailable,
                processExecutor)
        {
        }

        PlasticExeLauncher(
            RepositorySpec repSpec,
            bool isGluonMode,
            string installCloudFrom,
            string installEnterpriseFrom,
            string cancelInstallFrom,
            bool bWait,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            ICheckIsExeAvailable checkIsExeAvailable,
            IProcessExecutor processExecutor)
        {
            mRepSpec = repSpec;
            mIsGluonMode = isGluonMode;
            mInstallCloudFrom = installCloudFrom;
            mInstallEnterpriseFrom = installEnterpriseFrom;
            mCancelInstallFrom = cancelInstallFrom;
            mbWait = bWait;
            mShowDownloadPlasticExeWindow = showDownloadPlasticExeWindow;
            mCheckIsExeAvailable = checkIsExeAvailable;
            mProcessExecutor = processExecutor;
        }

        bool IToolLauncher.Launch(
            string tool, string arguments, string fileExtensions)
        {
            if (mAskedUser)
                return false;

            if (mCheckIsExeAvailable.ForMode(mIsGluonMode))
                return mProcessExecutor.Execute(tool, arguments, mbWait);

            mShowDownloadPlasticExeWindow.Show(
                mRepSpec,
                mIsGluonMode,
                mInstallCloudFrom,
                mInstallEnterpriseFrom,
                mCancelInstallFrom);

            mAskedUser = true;
            return false;
        }

        bool mAskedUser = false;

        readonly RepositorySpec mRepSpec;
        readonly bool mIsGluonMode;
        readonly string mInstallCloudFrom;
        readonly string mInstallEnterpriseFrom;
        readonly string mCancelInstallFrom;
        readonly bool mbWait;

        readonly LaunchTool.IShowDownloadPlasticExeWindow mShowDownloadPlasticExeWindow;
        readonly ICheckIsExeAvailable mCheckIsExeAvailable;
        readonly IProcessExecutor mProcessExecutor;

        internal class CheckIsExeAvailable : ICheckIsExeAvailable
        {
            bool ICheckIsExeAvailable.ForMode(bool isGluonMode)
            {
                return IsExeAvailable.ForMode(isGluonMode);
            }
        }

        internal class ProcessExecutor : IProcessExecutor
        {
            bool IProcessExecutor.Execute(string tool, string arguments, bool bWait)
            {
                return Codice.Client.BaseCommands.ProcessExecutor.Execute(
                    tool,
                    arguments,
                    bWait,
                    bNoWindow: false);
            }
        }
    }
}
