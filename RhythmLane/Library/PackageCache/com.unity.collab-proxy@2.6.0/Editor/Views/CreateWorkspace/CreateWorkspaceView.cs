using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Codice.Client.Common;
using Codice.Client.Common.Threading;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.Configuration.CloudEdition;
using PlasticGui.WorkspaceWindow.Home.Repositories;
using PlasticGui.WorkspaceWindow.Home.Workspaces;
using PlasticGui.WebApi;
using Unity.PlasticSCM.Editor.AssetUtils.Processor;
using Unity.PlasticSCM.Editor.UI.Progress;

namespace Unity.PlasticSCM.Editor.Views.CreateWorkspace
{
    internal class CreateWorkspaceView :
        IPlasticDialogCloser,
        IWorkspacesRefreshableView
    {
        internal interface ICreateWorkspaceListener
        {
            void OnWorkspaceCreated(WorkspaceInfo wkInfo, bool isGluonMode);
        }

        internal CreateWorkspaceView(
            PlasticWindow parentWindow,
            ICreateWorkspaceListener listener,
            IPlasticAPI plasticApi,
            IPlasticWebRestApi plasticWebRestApi,
            string workspacePath)
        {
            mParentWindow = parentWindow;
            mCreateWorkspaceListener = listener;
            mWorkspacePath = workspacePath;
            mPlasticWebRestApi = plasticWebRestApi;

            mProgressControls = new ProgressControlsForViews();
            mWorkspaceOperations = new WorkspaceOperations(this, mProgressControls, null);
            mCreateWorkspaceState = CreateWorkspaceViewState.BuildForProjectDefaults();

            Initialize(plasticApi, plasticWebRestApi);
        }

        internal void Update()
        {
            mProgressControls.UpdateProgress(mParentWindow);
        }

        internal void OnGUI()
        {
            if (Event.current.type == EventType.Layout)
            {
                mProgressControls.ProgressData.CopyInto(
                    mCreateWorkspaceState.ProgressData);
            }

            string repository = mCreateWorkspaceState.Repository;

            DrawCreateWorkspace.ForState(
                SelectRepository,
                CreateRepository,
                ValidateAndCreateWorkspace,
                mParentWindow,
                mPlasticWebRestApi,
                mDefaultServer,
                ref mCreateWorkspaceState);

            if (repository == mCreateWorkspaceState.Repository)
                return;

            SynchronizeRepositoryAndWorkspace();
        }

        void Initialize(IPlasticAPI plasticApi, IPlasticWebRestApi plasticWebRestApi)
        {
            ((IProgressControls)mProgressControls).ShowProgress(string.Empty);

            WorkspaceInfo[] allWorkspaces = null;
            IList allRepositories = null;
            string repositoryProject = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    mDefaultServer = GetDefaultServer.ToCreateWorkspace(plasticWebRestApi);

                    allWorkspaces = plasticApi.GetAllWorkspacesArray();
                    allRepositories = plasticApi.GetAllRepositories(mDefaultServer, true);

                    if (OrganizationsInformation.IsUnityOrganization(mDefaultServer))
                    {
                        List<string> serverProjects = OrganizationsInformation.GetOrganizationProjects(mDefaultServer);

                        if (serverProjects.Count > 0)
                        {
                            repositoryProject = serverProjects.First();
                        }
                    }
                },
                /*afterOperationDelegate*/ delegate
                {
                    ((IProgressControls) mProgressControls).HideProgress();

                    if (waiter.Exception != null)
                    {
                        DisplayException(mProgressControls, waiter.Exception);
                        return;
                    }

                    string serverSpecPart = string.Format("@{0}", ResolveServer.ToDisplayString(mDefaultServer));

                    mCreateWorkspaceState.Repository = ValidRepositoryName.Get(
                        string.Format("{0}{1}", mCreateWorkspaceState.Repository, serverSpecPart),
                        allRepositories);

                    if (repositoryProject != null)
                    {
                        mCreateWorkspaceState.Repository = string.Format("{0}/{1}", repositoryProject, mCreateWorkspaceState.Repository);
                    }

                    string proposedWorkspaceName = mCreateWorkspaceState.Repository.Replace(serverSpecPart, string.Empty);

                    mCreateWorkspaceState.WorkspaceName = CreateWorkspaceDialogUserAssistant.GetNonExistingWkNameForName(
                        proposedWorkspaceName, allWorkspaces);

                    mDialogUserAssistant = CreateWorkspaceDialogUserAssistant.ForWkPathAndName(
                        mWorkspacePath,
                        allWorkspaces);

                    SynchronizeRepositoryAndWorkspace();
                });
        }

        void SynchronizeRepositoryAndWorkspace()
        {
            if (mDialogUserAssistant == null)
            {
                return;
            }

            mDialogUserAssistant.RepositoryChanged(
                mCreateWorkspaceState.Repository,
                mCreateWorkspaceState.WorkspaceName,
                mWorkspacePath);

            mCreateWorkspaceState.WorkspaceName = mDialogUserAssistant.GetProposedWorkspaceName();
        }

        void SelectRepository(string repository)
        {
            mCreateWorkspaceState.Repository = repository;
            SynchronizeRepositoryAndWorkspace();
        }

        void CreateRepository(RepositoryCreationData data)
        {
            if (!data.Result)
                return;

            ((IProgressControls)mProgressControls).ShowProgress(
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.CreatingRepository,
                    data.RepName));

            RepositoryInfo createdRepository = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter();
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    createdRepository = PlasticGui.Plastic.API.CreateRepository(
                        data.ServerName, data.RepName);
                },
                /*afterOperationDelegate*/ delegate
                {
                    ((IProgressControls)mProgressControls).HideProgress();

                    if (waiter.Exception != null)
                    {
                        DisplayException(mProgressControls, waiter.Exception);
                        return;
                    }

                    if (createdRepository == null)
                        return;

                    mCreateWorkspaceState.Repository = createdRepository.GetRepSpec().ToDisplayString();

                    SynchronizeRepositoryAndWorkspace();
                });
        }

        void ValidateAndCreateWorkspace(CreateWorkspaceViewState state)
        {
            // We need a conversion here because the actual repSpec is used during the AsyncValidation
            // to call UpdateLastUsedRepositoryPreference. Also, it is the one to be used during wk creation
            RepositorySpec repSpec = OrganizationsInformation.TryResolveRepositorySpecFromInput(state.Repository);

            if (repSpec == null)
            {
                ((IProgressControls) mProgressControls).ShowError(
                    PlasticLocalization.Name.UnresolvedRepository.GetString(state.Repository));

                return;
            }

            mWkCreationData = BuildCreationDataFromState(state, mWorkspacePath, repSpec);

            // AsyncValidation calls IPlasticDialogCloser.CloseDialog() when the validation is ok
            WorkspaceCreationValidation.AsyncValidation(mWkCreationData, this, mProgressControls);
        }

        void IPlasticDialogCloser.CloseDialog()
        {
            ((IProgressControls) mProgressControls).ShowProgress(string.Empty);

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    RepositorySpec repSpec = new SpecGenerator().GenRepositorySpec(
                        false, mWkCreationData.Repository, CmConnection.Get().UnityOrgResolver);

                    bool repositoryExist = PlasticGui.Plastic.API.CheckRepositoryExists(
                        repSpec.Server, repSpec.Name);

                    if (!repositoryExist)
                        PlasticGui.Plastic.API.CreateRepository(repSpec.Server, repSpec.Name);
                },
                /*afterOperationDelegate*/ delegate
                {
                    ((IProgressControls)mProgressControls).HideProgress();

                    if (waiter.Exception != null)
                    {
                        DisplayException(mProgressControls, waiter.Exception);
                        return;
                    }

                    mWkCreationData.Result = true;
                    mWorkspaceOperations.CreateWorkspace(mWkCreationData);

                    // the operation calls IWorkspacesRefreshableView.RefreshAndSelect
                    // when the workspace is created
                });
        }

        void IWorkspacesRefreshableView.RefreshAndSelect(WorkspaceInfo wkInfo)
        {
            UnityCloudProjectLinkMonitor.CheckCloudProjectAlignmentAsync(wkInfo);

            PerformInitialCheckin.IfRepositoryIsEmpty(
                wkInfo,
                mWkCreationData.Repository,
                mWkCreationData.IsGluonWorkspace,
                PlasticGui.Plastic.API,
                mProgressControls,
                mCreateWorkspaceListener,
                mParentWindow);
        }

        static WorkspaceCreationData BuildCreationDataFromState(
            CreateWorkspaceViewState state,
            string workspacePath,
            RepositorySpec repSpec)
        {
            return new WorkspaceCreationData(
                state.WorkspaceName,
                workspacePath,
                repSpec.ToString(),
                state.WorkspaceMode == CreateWorkspaceViewState.WorkspaceModes.Gluon,
                false);
        }

        static void DisplayException(
            IProgressControls progressControls,
            Exception ex)
        {
            ExceptionsHandler.LogException(
                "CreateWorkspaceView", ex);

            progressControls.ShowError(
                ExceptionsHandler.GetCorrectExceptionMessage(ex));
        }

        class GetDefaultServer
        {
            internal static string ToCreateWorkspace(IPlasticWebRestApi plasticWebRestApi)
            {
                string clientConfServer = PlasticGui.Plastic.ConfigAPI.GetClientConfServer();

                if (!EditionToken.IsCloudEdition())
                    return clientConfServer;

                string cloudServer = PlasticGuiConfig.Get().
                    Configuration.DefaultCloudServer;

                if (!string.IsNullOrEmpty(cloudServer))
                    return cloudServer;

                CloudEditionCreds.Data config =
                    CloudEditionCreds.GetFromClientConf();

                cloudServer = GetFirstCloudServer.
                    GetCloudServer(plasticWebRestApi, config.Email, config.Password);

                if (string.IsNullOrEmpty(cloudServer))
                    return clientConfServer;

                SaveCloudServer.ToPlasticGuiConfig(cloudServer);

                return cloudServer;
            }
        }

        WorkspaceCreationData mWkCreationData;
        CreateWorkspaceViewState mCreateWorkspaceState;

        CreateWorkspaceDialogUserAssistant mDialogUserAssistant;

        string mDefaultServer;

        readonly WorkspaceOperations mWorkspaceOperations;
        readonly ProgressControlsForViews mProgressControls;
        readonly string mWorkspacePath;
        readonly ICreateWorkspaceListener mCreateWorkspaceListener;
        readonly PlasticWindow mParentWindow;
        readonly IPlasticWebRestApi mPlasticWebRestApi;
    }
}
