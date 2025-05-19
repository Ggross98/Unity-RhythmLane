using System;
using System.Collections.Generic;

using UnityEditor.IMGUI.Controls;
using UnityEngine;

using PlasticGui;
using Unity.PlasticSCM.Editor.UI.Tree;

namespace Unity.PlasticSCM.Editor.Views.Merge.Developer
{
    internal enum MergeTreeColumn
    {
        Path,
        Size,
        Author,
        Details,
        Resolution,
        DateModified,
        Comment
    }

    [Serializable]
    internal class MergeTreeHeaderState : MultiColumnHeaderState, ISerializationCallbackReceiver
    {
        internal static MergeTreeHeaderState GetDefault()
        {
            MergeTreeHeaderState headerState =
                new MergeTreeHeaderState(BuildColumns());

            headerState.visibleColumns = GetDefaultVisibleColumns();

            return headerState;
        }

        internal static List<string> GetColumnNames()
        {
            List<string> result = new List<string>();
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.PathColumn));
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.SizeColumn));
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.CreatedByColumn));
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.DetailsColumn));
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.ResolutionMethodColumn));
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.DateModifiedColumn));
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.CommentColumn));
            return result;
        }

        internal static string GetColumnName(MergeTreeColumn column)
        {
            switch (column)
            {
                case MergeTreeColumn.Path:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.PathColumn);
                case MergeTreeColumn.Size:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.SizeColumn);
                case MergeTreeColumn.Author:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.AuthorColumn);
                case MergeTreeColumn.Details:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.DetailsColumn);
                case MergeTreeColumn.Resolution:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.ResolutionMethodColumn);
                case MergeTreeColumn.DateModified:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.DateModifiedColumn);
                case MergeTreeColumn.Comment:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.CommentColumn);
                default:
                    return null;
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (mHeaderTitles != null)
                TreeHeaderColumns.SetTitles(columns, mHeaderTitles);

            if (mColumsAllowedToggleVisibility != null)
                TreeHeaderColumns.SetVisibilities(columns, mColumsAllowedToggleVisibility);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        static int[] GetDefaultVisibleColumns()
        {
            List<int> result = new List<int>();
            result.Add((int)MergeTreeColumn.Path);
            result.Add((int)MergeTreeColumn.Size);
            result.Add((int)MergeTreeColumn.Author);
            result.Add((int)MergeTreeColumn.Resolution);
            result.Add((int)MergeTreeColumn.DateModified);
            result.Add((int)MergeTreeColumn.Comment);
            return result.ToArray();
        }

        static Column[] BuildColumns()
        {
            return new Column[]
            {
                new Column()
                {
                    width = 450,
                    headerContent = new GUIContent(
                        GetColumnName(MergeTreeColumn.Path)),
                    minWidth = 200,
                    allowToggleVisibility = false,
                    sortingArrowAlignment = TextAlignment.Right
                },
                new Column()
                {
                    width = 150,
                    headerContent = new GUIContent(
                        GetColumnName(MergeTreeColumn.Size)),
                    minWidth = 45,
                    sortingArrowAlignment = TextAlignment.Right
                },
                new Column()
                {
                    width = 150,
                    headerContent = new GUIContent(
                        GetColumnName(MergeTreeColumn.Author)),
                    minWidth = 80,
                    sortingArrowAlignment = TextAlignment.Right
                },
                new Column()
                {
                    width = 200,
                    headerContent = new GUIContent(
                        GetColumnName(MergeTreeColumn.Details)),
                    minWidth = 100,
                    sortingArrowAlignment = TextAlignment.Right
                },
                new Column()
                {
                    width = 250,
                    headerContent = new GUIContent(
                        GetColumnName(MergeTreeColumn.Resolution)),
                    minWidth = 120,
                    sortingArrowAlignment = TextAlignment.Right
                },
                new Column()
                {
                    width = 330,
                    headerContent = new GUIContent(
                        GetColumnName(MergeTreeColumn.DateModified)),
                    minWidth = 100,
                    sortingArrowAlignment = TextAlignment.Right
                },
                new Column()
                {
                    width = 400,
                    headerContent = new GUIContent(
                        GetColumnName(MergeTreeColumn.Comment)),
                    minWidth = 100,
                    sortingArrowAlignment = TextAlignment.Right
                }
            };
        }

        MergeTreeHeaderState(Column[] columns)
            : base(columns)
        {
            if (mHeaderTitles == null)
                mHeaderTitles = TreeHeaderColumns.GetTitles(columns);

            if (mColumsAllowedToggleVisibility == null)
                mColumsAllowedToggleVisibility = TreeHeaderColumns.GetVisibilities(columns);
        }

        [SerializeField]
        string[] mHeaderTitles;

        [SerializeField]
        bool[] mColumsAllowedToggleVisibility;
    }
}
