using System;

using UnityEditor;

namespace Unity.PlasticSCM.Editor.UI
{
    internal static class ShowWindow
    {
        internal static PlasticWindow Plastic()
        {
            PlasticWindow window = EditorWindow.GetWindow<PlasticWindow>(
                UnityConstants.PLASTIC_WINDOW_TITLE,
                true,
                mConsoleWindowType,
                mProjectBrowserType);

            window.UpdateWindowIcon(PlasticPlugin.GetNotificationStatus());

            return window;
        }

        static Type mConsoleWindowType = typeof(EditorWindow).
            Assembly.GetType("UnityEditor.ConsoleWindow");
        static Type mProjectBrowserType = typeof(EditorWindow).
            Assembly.GetType("UnityEditor.ProjectBrowser");
    }
}