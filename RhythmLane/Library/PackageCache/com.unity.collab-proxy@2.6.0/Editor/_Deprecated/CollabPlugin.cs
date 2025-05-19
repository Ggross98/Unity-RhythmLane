using System.ComponentModel;

namespace Unity.PlasticSCM.Editor
{
    // Internal usage. This isn't a public API.
    [EditorBrowsable(EditorBrowsableState.Never)] 
    public static class CollabPlugin
    {
        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool IsEnabled()
        {
            return false;
        }
    }
}
