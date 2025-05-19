#if UNITY_2019_3_OR_NEWER
using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;
using UnityEngine;

namespace Packages.Rider.Editor.Debugger
{
  internal class LinkXmlInstaller : IUnityLinkerProcessor
  {
    public int callbackOrder => 0;

    public string GenerateAdditionalLinkXmlFile([CanBeNull] BuildReport report, UnityLinkerBuildPipelineData data)
    {
      if (!RiderScriptEditor.IsRiderOrFleetInstallation(RiderScriptEditor.CurrentEditor))
        return string.Empty;
      
      if (!RiderDebuggerProvider.IsScriptDebuggingEnable(report))
        return string.Empty;
      if (!RiderDebuggerProvider.IsIl2CppScriptingBackend(report))
        return string.Empty;
      
      var il2CppDebugSupport = RiderDebuggerProvider.Instance.Il2CppDebugSupport;
      
      if (il2CppDebugSupport == Il2CppDebugSupport.None)
        return string.Empty;

      try
      {
        var preserveUnityEngineDlls = il2CppDebugSupport.HasFlag(Il2CppDebugSupport.PreserveUnityEngineDlls);
        var preservePlayerDlls = il2CppDebugSupport.HasFlag(Il2CppDebugSupport.PreservePlayerDlls);
      
        var path = EditorPluginInterop.GenerateAdditionalLinkXmlFile(report, data, preserveUnityEngineDlls, preservePlayerDlls);
        return path;
      }
      catch (Exception e)
      {
        Debug.LogError(e);
      }
      
      return string.Empty;
    }


    //Unity Editor 2019 IUnityLinkerProcessor interface methods
    public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data) {}
    public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data) {}
  }
}
#endif