using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.UI
{
    internal static class DrawSearchField
    {
        internal static void For(
            SearchField searchField,
            TreeView treeView,
            float width)
        {
            treeView.searchString = searchField.OnToolbarGUI(
                treeView.searchString, GUILayout.MaxWidth(width / 2f));
        }
    }
}
