namespace Unity.PlasticSCM.Editor.Views.Merge
{
    internal interface IIncomingChangesTab
    {
        bool IsVisible
        {
            get; set;
        }

        void OnEnable();
        void OnDisable();
        void Update();
        void OnGUI();
        void DrawSearchFieldForTab();
        void AutoRefresh();
    }
}
