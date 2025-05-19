using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Packages.Rider.Editor.Debugger
{
  internal class RiderDebuggerProvider
  {
    private const string UnityProjectIl2CPPDebugFlagSettingsName = "unity_project_il2cpp_debug_flag";
    private const string UnityProjectUseDebugLinkDuringTheBuild = "unity_project_use_debug_link_flag";
    private const int RequiredRiderVersion = 243;
    public const string RequiredRiderVersionName = "2024.3";

    private Il2CppDebugSupport m_Il2CppDebugSupportFlag =
      (Il2CppDebugSupport)EditorPrefs.GetInt(UnityProjectIl2CPPDebugFlagSettingsName,
        (int)Il2CppDebugSupport.PreserveUnityEngineDlls);

    private bool m_useDebugLinkDuringTheBuild = EditorPrefs.GetBool(UnityProjectUseDebugLinkDuringTheBuild, true);

    private RiderDebuggerProvider()
    {
    }

    public static readonly RiderDebuggerProvider Instance = new RiderDebuggerProvider();

    public Il2CppDebugSupport Il2CppDebugSupport
    {
      get => m_Il2CppDebugSupportFlag;
      private set
      {
        if (m_Il2CppDebugSupportFlag != value)
        {
          EditorPrefs.SetInt(UnityProjectIl2CPPDebugFlagSettingsName, (int)value);
          m_Il2CppDebugSupportFlag = value;
        }
      }
    }

    public void ToggleIl2CppSupport(Il2CppDebugSupport preference)
    {
      if (Il2CppDebugSupport.HasFlag(preference))
        Il2CppDebugSupport ^= preference;
      else
        Il2CppDebugSupport |= preference;
    }

    public bool UseDebugLinkDuringTheBuild
    {
      get => m_useDebugLinkDuringTheBuild;
      private set
      {
        EditorPrefs.SetBool(UnityProjectUseDebugLinkDuringTheBuild, value);
        m_useDebugLinkDuringTheBuild = value;
      }
    }
    public void ToggleUseDebugLinkDuringTheBuild(bool value)
    {
      if (UseDebugLinkDuringTheBuild != value)
        UseDebugLinkDuringTheBuild = value;
    }

    public static bool IsIl2CppScriptingBackend([CanBeNull] BuildReport report)
    {
#if UNITY_2023_1_OR_NEWER
      var summaryPlatformGroup = NamedBuildTarget.FromBuildTargetGroup(report == null 
        ? EditorUserBuildSettings.selectedBuildTargetGroup
        : report.summary.platformGroup);
#else
      var summaryPlatformGroup = report == null 
        ? EditorUserBuildSettings.selectedBuildTargetGroup
        : report.summary.platformGroup;
#endif

      return PlayerSettings.GetScriptingBackend(summaryPlatformGroup) == ScriptingImplementation.IL2CPP;

    }

    public static bool IsScriptDebuggingEnable([CanBeNull] BuildReport report)
    {
      if(report != null)
        return report.summary.options.HasFlag(BuildOptions.AllowDebugging);

      return EditorUserBuildSettings.allowDebugging;
    }

    public static bool IsSupportedRiderVersion()
    {
      return RiderScriptEditorData.instance != null && RiderScriptEditorData.instance.editorBuildNumber != null  && RiderScriptEditorData.instance.editorBuildNumber.Major >= RequiredRiderVersion;
    }
  }
}