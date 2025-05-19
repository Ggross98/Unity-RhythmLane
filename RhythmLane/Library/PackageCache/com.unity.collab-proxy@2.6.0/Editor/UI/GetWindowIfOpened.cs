using UnityEditor;

namespace Unity.PlasticSCM.Editor.UI
{
    internal static class GetWindowIfOpened
    {
        internal static PlasticWindow Plastic()
        {
            if (!EditorWindow.HasOpenInstances<PlasticWindow>())
                return null;

            return EditorWindow.GetWindow<PlasticWindow>(null, false);
        }
    }
}
