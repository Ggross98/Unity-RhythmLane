using System;
using System.ComponentModel;
using System.Threading.Tasks;

using UnityEditor;
using UnityEngine;

using Codice.Client.Common.Connection;
using Codice.CM.Common;
using Codice.LogWrapper;
using Unity.PlasticSCM.Editor.AssetMenu;
using Unity.PlasticSCM.Editor.AssetsOverlays;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.AssetUtils.Processor;
using Unity.PlasticSCM.Editor.Hub;
using Unity.PlasticSCM.Editor.Inspector;
using Unity.PlasticSCM.Editor.SceneView;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor
{
    // Internal usage. This isn't a public API.
    // It supports calls across two different modules "Unity.CollabProxy.Editor".ToolbarButton -> "Unity.PlasticSCM.Editor".PlasticPlugin.Xxx()
    // and the "Unity.CollabProxy.Editor" is itself required as it is hard-coded by the Unity Editor code
    [EditorBrowsable(EditorBrowsableState.Never)]
    [InitializeOnLoad]
    public static class PlasticPlugin
    {
        // Internal usage between two different modules of the package.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static event Action OnNotificationUpdated = delegate { };

        // Internal usage between two different modules of the package.
        // It's pending to rename the OpenPlasticWindowDisablingOfflineModeIfNeeded
        // method to OpenPlasticWindowAndEnablePluginIfNeeded. We cannot do it now
        // because it's a public method and this rename breaks the API validation
        // check. We will do it when we change the major version number to v3.0.0
        // (which is only allowed for a major new version of Unity)
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void OpenPlasticWindowDisablingOfflineModeIfNeeded()
        {
            if (!PlasticPluginIsEnabledPreference.IsEnabled())
            {
                PlasticPluginIsEnabledPreference.Enable();
                Enable();
            }

            ShowWindow.Plastic();
        }

        // Internal usage between two different modules of the package.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Texture GetPluginStatusIcon()
        {
            return PlasticNotification.GetIcon(mNotificationStatus);
        }

        internal static IAssetStatusCache AssetStatusCache
        {
            get { return mAssetStatusCache; }
        }

        internal static WorkspaceOperationsMonitor WorkspaceOperationsMonitor
        {
            get { return mWorkspaceOperationsMonitor; }
        }

        internal static PlasticConnectionMonitor ConnectionMonitor
        {
            get { return mPlasticConnectionMonitor; }
        }

        internal static bool IsUnitTesting { get; set; }

        static PlasticPlugin()
        {
            ProcessCommand.Initialize();
            EditorDispatcher.Initialize();

            if (!FindWorkspace.HasWorkspace(ApplicationDataPath.Get()))
                return;

            if (!PlasticPluginIsEnabledPreference.IsEnabled())
                return;

            CooldownWindowDelayer cooldownInitializeAction = new CooldownWindowDelayer(
                Enable, UnityConstants.PLUGIN_DELAYED_INITIALIZE_INTERVAL);
            cooldownInitializeAction.Ping();
        }

        internal static void Enable()
        {
            if (mIsEnabled)
                return;

            mIsEnabled = true;

            PlasticApp.InitializeIfNeeded();

            mLog.Debug("Enable");

            if (!FindWorkspace.HasWorkspace(ApplicationDataPath.Get()))
                return;

            WorkspaceInfo wkInfo = FindWorkspace.InfoForApplicationPath(
                ApplicationDataPath.Get(), PlasticGui.Plastic.API);

            if (wkInfo == null)
                return;

            EnableForWorkspace(wkInfo);
        }

        internal static void EnableForWorkspace(WorkspaceInfo wkInfo)
        {
            if (mIsEnabledForWorkspace)
                return;

            mIsEnabledForWorkspace = true;

            mLog.Debug("EnableForWorkspace " + wkInfo.ClientPath);

            PlasticApp.SetWorkspace(wkInfo);

            HandleCredsAliasAndServerCert.InitializeHostUnreachableExceptionListener(
                mPlasticConnectionMonitor);

            bool isGluonMode = PlasticGui.Plastic.API.IsGluonWorkspace(wkInfo);

            mAssetStatusCache = new AssetStatusCache(wkInfo, isGluonMode);

            PlasticAssetsProcessor plasticAssetsProcessor = new PlasticAssetsProcessor();

            mWorkspaceOperationsMonitor = BuildWorkspaceOperationsMonitor(
                plasticAssetsProcessor, isGluonMode);
            mWorkspaceOperationsMonitor.Start();

            UnityCloudProjectLinkMonitor.CheckCloudProjectAlignmentAsync(wkInfo);

            AssetsProcessors.Enable(
                wkInfo.ClientPath, plasticAssetsProcessor, mAssetStatusCache);
            AssetMenuItems.Enable(
                wkInfo, mAssetStatusCache);
            DrawAssetOverlay.Enable(
                wkInfo.ClientPath, mAssetStatusCache);
            DrawInspectorOperations.Enable(
                wkInfo.ClientPath, mAssetStatusCache);
            DrawSceneOperations.Enable(
                wkInfo.ClientPath, mWorkspaceOperationsMonitor, mAssetStatusCache);

            Task.Run(() => EnsureServerConnection(wkInfo, mPlasticConnectionMonitor));
        }

        internal static void Shutdown()
        {
            mLog.Debug("Shutdown");

            HandleCredsAliasAndServerCert.CleanHostUnreachableExceptionListener();
            mPlasticConnectionMonitor.Stop();

            Disable();
        }

        internal static void Disable()
        {
            if (!mIsEnabled)
                return;

            try
            {
                mLog.Debug("Disable");

                DisableForWorkspace();

                PlasticApp.Dispose();
            }
            finally
            {
                mIsEnabled = false;
            }
        }

        internal static void SetNotificationStatus(
            PlasticWindow plasticWindow,
            PlasticNotification.Status status)
        {
            mNotificationStatus = status;

            plasticWindow.UpdateWindowIcon(status);

            if (OnNotificationUpdated != null)
                OnNotificationUpdated.Invoke();
        }

        static void DisableForWorkspace()
        {
            if (!mIsEnabledForWorkspace)
                return;

            try
            {
                mWorkspaceOperationsMonitor.Stop();

                AssetsProcessors.Disable();
                AssetMenuItems.Disable();
                DrawAssetOverlay.Disable();
                DrawInspectorOperations.Disable();
                DrawSceneOperations.Disable();
            }
            finally
            {
                mIsEnabledForWorkspace = false;
            }
        }

        internal static PlasticNotification.Status GetNotificationStatus()
        {
            return mNotificationStatus;
        }

        static WorkspaceOperationsMonitor BuildWorkspaceOperationsMonitor(
            PlasticAssetsProcessor plasticAssetsProcessor,
            bool isGluonMode)
        {
            WorkspaceOperationsMonitor result = new WorkspaceOperationsMonitor(
                PlasticGui.Plastic.API, plasticAssetsProcessor, isGluonMode);
            plasticAssetsProcessor.SetWorkspaceOperationsMonitor(result);
            return result;
        }

        static void EnsureServerConnection(
            WorkspaceInfo wkInfo,
            PlasticConnectionMonitor plasticConnectionMonitor)
        {
            if (IsUnitTesting)
                return;

            RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);

            plasticConnectionMonitor.SetRepositorySpecForEventTracking(repSpec);

            try
            {
                // set the PlasticConnectionMonitor initially to have a valid connection
                // then check that the server connection is valid. If failed, we call
                // PlasticConnectionMonitor.OnConnectionError that fires the Plugin disable
                // and the reconnection mechanism

                plasticConnectionMonitor.SetAsConnected();

                if (!PlasticGui.Plastic.API.CheckServerConnection(repSpec.Server))
                    throw new Exception(string.Format("Failed to connect to {0}", repSpec.Server));
            }
            catch (Exception ex)
            {
                plasticConnectionMonitor.OnConnectionError(ex, repSpec.Server);
            }
        }

        static PlasticNotification.Status mNotificationStatus;
        static AssetStatusCache mAssetStatusCache;
        static WorkspaceOperationsMonitor mWorkspaceOperationsMonitor;
        static PlasticConnectionMonitor mPlasticConnectionMonitor = new PlasticConnectionMonitor();
        static bool mIsEnabled;
        static bool mIsEnabledForWorkspace;

        static readonly ILog mLog = PlasticApp.GetLogger("PlasticPlugin");
    }
}
