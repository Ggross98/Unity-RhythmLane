using Codice.CM.Common;
using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Explorer;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.Views.Branches
{
    internal partial class BranchesTab
    {
        private void SwitchToBranchForMode()
        {
            if (mIsGluonMode)
            {
                SwitchToBranchForGluon();
                return;
            }

            SwitchToBranchForDeveloper();
        }

        private void SwitchToBranchForDeveloper()
        {
            RepositorySpec repSpec = BranchesSelection.GetSelectedRepository(mBranchesListView);
            BranchInfo branchInfo = BranchesSelection.GetSelectedBranch(mBranchesListView);

            mBranchOperations.SwitchToBranch(
                repSpec,
                branchInfo,
                RefreshAsset.BeforeLongAssetOperation,
                items => RefreshAsset.AfterLongAssetOperation(
                    ProjectPackages.ShouldBeResolved(items, mWkInfo, false)));
        }

        private void SwitchToBranchForGluon()
        {
            BranchInfo branchInfo = BranchesSelection.GetSelectedBranch(mBranchesListView);

            new SwitchToUIOperation().SwitchToBranch(
                mWkInfo,
                PlasticGui.Plastic.API.GetRepositorySpec(mWkInfo),
                branchInfo,
                mViewHost,
                null,
                new UnityPlasticGuiMessage(),
                mProgressControls,
                mWorkspaceWindow.GluonProgressOperationHandler,
                mGluonUpdateReport,
                mWorkspaceWindow,
                null,
                null,
                null,
                RefreshAsset.BeforeLongAssetOperation,
                items => RefreshAsset.AfterLongAssetOperation(
                    ProjectPackages.ShouldBeResolved(items, mWkInfo, true)));
        }
    }
}
