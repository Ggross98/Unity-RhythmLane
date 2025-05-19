#if UNITY_2019_3_OR_NEWER
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;
using UnityEngine;

namespace Packages.Rider.Editor.Debugger
{
  internal class RiderDebugLinkXmlProcessor : IUnityLinkerProcessor
  {
    public const string DebugLinkFileName = "debug_link";
    public int callbackOrder { get; }

    public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
    {
      if (!RiderScriptEditor.IsRiderOrFleetInstallation(RiderScriptEditor.CurrentEditor))
        return string.Empty;

      if (!RiderDebuggerProvider.IsScriptDebuggingEnable(report))
        return string.Empty;
      if (!RiderDebuggerProvider.IsIl2CppScriptingBackend(report))
        return string.Empty;

      var debugLinkXmlPaths = FindLinkDebugXmlFilePaths();

      if (debugLinkXmlPaths.Length == 0)
        return string.Empty;

      if (debugLinkXmlPaths.Length == 1)
        return debugLinkXmlPaths[0];

      //create a file in the random folder in the TEMP directory
      var filePath = Path.Combine(CreateRandomFolderInTempDirectory(), "linker.xml");

      var linker = new XElement("linker");
      var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), linker);
      MergeXMLFiles(debugLinkXmlPaths, linker);
      doc.Save(filePath);
      return filePath;
    }

    private static string CreateRandomFolderInTempDirectory()
    {
      // Get the path of the Temp directory
      var tempPath = Path.GetTempPath();

      // Generate a random folder name
      var randomFolderName = Path.GetRandomFileName();

      // Combine the Temp path with the random folder name
      var randomFolderPath = Path.Combine(tempPath, randomFolderName);

      // Create the random folder
      Directory.CreateDirectory(randomFolderPath);

      return randomFolderPath;
    }

    private static void MergeXMLFiles(string[] filePaths, XElement linker)
    {
      foreach (var filePath in filePaths)
      {
        try
        {
          var tempDoc = XDocument.Load(filePath);
          if (tempDoc.Root == null) continue;
          foreach (var node in tempDoc.Root.Nodes())
            linker.Add(node);
        }
        catch (Exception e)
        {
          Debug.LogError(filePath);
          Debug.LogException(e);
        }
      }
    }

    private static string[] FindLinkDebugXmlFilePaths()
    {
      var projectPath = Path.GetDirectoryName(Application.dataPath);

      var assetsPaths = AssetDatabase.FindAssets(DebugLinkFileName)
        .Select(AssetDatabase.GUIDToAssetPath)
        .Where(p => Path.GetExtension(p) == ".xml")
        .Select(p => Path.Combine(projectPath, p))
        .ToArray();

      return assetsPaths;
    }

    //Unity Editor 2019 IUnityLinkerProcessor interface methods
    public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
    {
    }

    public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
    {
    }

    public static void GenerateTemplateDebugLinkXml()
    {
      var filePath =
        EditorUtility.SaveFilePanel($"Save {DebugLinkFileName}", Application.dataPath, DebugLinkFileName, "xml");

      if (string.IsNullOrEmpty(filePath))
        return;

      var linker = new XElement("linker");
      var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), linker);
      linker.Add(new XComment($"Preserve Unity Engine assemblies"));
      linker.Add(new XElement("assembly", new XAttribute("fullname", "UnityEngine"),
        new XAttribute("preserve", "all")));
      linker.Add(new XElement("assembly", new XAttribute("fullname", "UnityEngine.CoreModule"),
        new XAttribute("preserve", "all")));
      linker.Add(new XComment($"Preserve users assemblies"));
      linker.Add(new XElement("assembly", new XAttribute("fullname", "Assembly-CSharp"),
        new XAttribute("preserve", "all")));
      doc.Save(filePath);
    }
  }
}
#endif