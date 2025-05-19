using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.UI
{
    internal static class DrawTextBlockWithEndLink
    {
        internal static void For(
            ExternalLink externalLink,
            string explanation,
            GUIStyle textBlockStyle)
        {
            GUILayout.Label(explanation, textBlockStyle);

            GUIStyle linkStyle = new GUIStyle(UnityStyles.LinkLabel);
            linkStyle.fontSize = textBlockStyle.fontSize;
            linkStyle.stretchWidth = false;

            if (GUILayout.Button(externalLink.Label, linkStyle))
                Application.OpenURL(externalLink.Url);

            EditorGUIUtility.AddCursorRect(
                GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }
    }
}
