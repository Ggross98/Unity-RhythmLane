using Codice.CM.Common;
using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Explorer;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.Views.Changesets
{
    internal partial class ChangesetsTab
    {
        void SwitchToChangesetForMode()
        {
            if (mIsGluonMode)
            {
                SwitchToChangesetForGluon();
                return;
            }

            SwitchToChangesetForDeveloper();
        }

        void SwitchToChangesetForDeveloper()
        {
            mChangesetOperations.SwitchToChangeset(
                ChangesetsSelection.GetSelectedRepository(mChangesetsListView),
                ChangesetsSelection.GetSelectedChangeset(mChangesetsListView),
                RefreshAsset.BeforeLongAssetOperation,
                items => RefreshAsset.AfterLongAssetOperation(
                    ProjectPackages.ShouldBeResolved(items, mWkInfo, false)));
        }

        void SwitchToChangesetForGluon()
        {
            ChangesetExtendedInfo csetInfo = ChangesetsSelection.GetSelectedChangeset(mChangesetsListView);

            new SwitchToUIOperation().SwitchToChangeset(
                mWkInfo,
                PlasticGui.Plastic.API.GetRepositorySpec(mWkInfo),
                csetInfo.BranchName,
                csetInfo.ChangesetId,
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
