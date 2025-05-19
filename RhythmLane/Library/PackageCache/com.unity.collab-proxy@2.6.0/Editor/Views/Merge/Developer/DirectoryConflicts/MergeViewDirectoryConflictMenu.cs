using UnityEditor;

using Codice.CM.Common.Merge;
using PlasticGui.WorkspaceWindow.Merge;

namespace Unity.PlasticSCM.Editor.Views.Merge.Developer.DirectoryConflicts
{
    internal class MergeViewDirectoryConflictMenu
    {
        internal interface IDirectoryConflictMenu
        {
            GenericMenu Menu { get; }
            void Popup();
        }

        internal GenericMenu Menu { get { return mDirectoryConflictMenu != null ? mDirectoryConflictMenu.Menu : null; } }

        internal MergeViewDirectoryConflictMenu(IMergeViewMenuOperations mergeViewMenuOperations)
        {
            mMergeViewMenuOperations = mergeViewMenuOperations;
        }

        internal void Popup()
        {
            mDirectoryConflictMenu = GetMenu();

            if (mDirectoryConflictMenu == null)
                return;

            mDirectoryConflictMenu.Popup();
        }

        IDirectoryConflictMenu GetMenu()
        {
            DirectoryConflict conflict = mMergeViewMenuOperations.
                GetSelectedMergeChangesGroupInfo().SelectedConflict.DirectoryConflict;

            if (conflict is AddMoveConflict)
            {
                AddMoveConflict addMove = conflict as AddMoveConflict;

                return addMove.IsAddMove ? 
                    GetAddMoveMenu() :
                    GetMoveAddMenu();
            }

            if (conflict is ChangeDeleteConflict)
            {
                ChangeDeleteConflict changeDelete = conflict as ChangeDeleteConflict;

                return changeDelete.IsChangeDelete ? 
                    GetChangeDeletetMenu() :
                    GetDeleteChangeMenu();
            }

            if (conflict is MoveDeleteConflict)
            {
                MoveDeleteConflict moveDelete = conflict as MoveDeleteConflict;

                return moveDelete.IsMoveDelete ?
                    GetMoveDeleteMenu() :
                    GetDeleteMoveMenu();
            }

            if (conflict is EvilTwinConflict)
            {
                EvilTwinConflict evilTwin = conflict as EvilTwinConflict;

                return evilTwin.IsMovedEvilTwin ?
                    GetMovedEvilTwinMenu() :
                    GetEvilTwinMenu();
            }

            if (conflict is CycleMoveConflict)
                return GetCycleMoveMenu();

            if (conflict is DivergentMoveConflict)
                return GetDivergentMoveMenu();

            if (conflict is LoadedTwiceConflict)
                return GetLoadedTwiceMenu();

            return null;
        }

        IDirectoryConflictMenu GetAddMoveMenu()
        {
            if (mAddMoveMenu == null)
                mAddMoveMenu = new AddMoveMenu(mMergeViewMenuOperations);

            return mAddMoveMenu;
        }

        IDirectoryConflictMenu GetChangeDeletetMenu()
        {
            if (mChangeDeleteMenu == null)
                mChangeDeleteMenu = new ChangeDeleteMenu(mMergeViewMenuOperations);

            return mChangeDeleteMenu;
        }

        IDirectoryConflictMenu GetCycleMoveMenu()
        {
            if (mCycleMoveMenu == null)
                mCycleMoveMenu = new CycleMoveMenu(mMergeViewMenuOperations);

            return mCycleMoveMenu;
        }

        IDirectoryConflictMenu GetDeleteChangeMenu()
        {
            if (mDeleteChangeMenu == null)
                mDeleteChangeMenu = new DeleteChangeMenu(mMergeViewMenuOperations);

            return mDeleteChangeMenu;
        }

        IDirectoryConflictMenu GetDeleteMoveMenu()
        {
            if (mDeleteMoveMenu == null)
                mDeleteMoveMenu = new DeleteMoveMenu(mMergeViewMenuOperations);

            return mDeleteMoveMenu;
        }

        IDirectoryConflictMenu GetDivergentMoveMenu()
        {
            if (mDivergentMoveMenu == null)
                mDivergentMoveMenu = new DivergentMoveMenu(mMergeViewMenuOperations);

            return mDivergentMoveMenu;
        }

        IDirectoryConflictMenu GetEvilTwinMenu()
        {
            if (mEvilTwinMenu == null)
                mEvilTwinMenu = new EvilTwinMenu(mMergeViewMenuOperations);

            return mEvilTwinMenu;
        }

        IDirectoryConflictMenu GetLoadedTwiceMenu()
        {
            if (mLoadedTwiceMenu == null)
                 mLoadedTwiceMenu = new LoadedTwiceMenu(mMergeViewMenuOperations);
 
            return mLoadedTwiceMenu;
        }

        IDirectoryConflictMenu GetMoveAddMenu()
        {
            if (mMoveAddMenu == null)
                mMoveAddMenu = new MoveAddMenu(mMergeViewMenuOperations);

            return mMoveAddMenu;
        }

        IDirectoryConflictMenu GetMoveDeleteMenu()
        {
            if (mMoveDeleteMenu == null)
                mMoveDeleteMenu = new MoveDeleteMenu(mMergeViewMenuOperations);

            return mMoveDeleteMenu;
        }

        IDirectoryConflictMenu GetMovedEvilTwinMenu()
        {
            if (mMovedEvilTwinMenu == null)
                mMovedEvilTwinMenu = new MovedEvilTwinMenu(mMergeViewMenuOperations);

            return mMovedEvilTwinMenu;
        }

        IDirectoryConflictMenu mDirectoryConflictMenu;

        AddMoveMenu mAddMoveMenu;
        ChangeDeleteMenu mChangeDeleteMenu;
        CycleMoveMenu mCycleMoveMenu;
        DeleteChangeMenu mDeleteChangeMenu;
        DeleteMoveMenu mDeleteMoveMenu;
        DivergentMoveMenu mDivergentMoveMenu;
        EvilTwinMenu mEvilTwinMenu;
        LoadedTwiceMenu mLoadedTwiceMenu;
        MoveAddMenu mMoveAddMenu;
        MoveDeleteMenu mMoveDeleteMenu;
        MovedEvilTwinMenu mMovedEvilTwinMenu;

        readonly IMergeViewMenuOperations mMergeViewMenuOperations;
    }
}
