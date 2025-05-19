﻿using System;
using System.Collections.Generic;
using System.IO;

using Codice.Client.Common;
using Codice.LogWrapper;
using Codice.Utils;

namespace Unity.PlasticSCM.Editor.Tool
{
    internal static class IsExeAvailable
    {
        internal static bool ForMode(bool isGluonMode)
        {
            string toolPath = isGluonMode ?
                PlasticInstallPath.GetGluonExePath() :
                PlasticInstallPath.GetPlasticExePath();

            return !string.IsNullOrEmpty(toolPath);
        }

        internal static bool ForLocalServer()
        {
            return !string.IsNullOrEmpty(PlasticInstallPath.GetLocalPlasticServerExePath());
        }
    }

    internal static class PlasticInstallPath
    {
        internal static string GetClientBinDir()
        {
            if (PlatformIdentifier.IsWindows())
            {
                string plasticExePath = GetPlasticExePath();

                if (plasticExePath == null)
                    return null;

                return Path.GetDirectoryName(plasticExePath);
            }

            if (PlatformIdentifier.IsMac())
            {
                string path = GetToolCommand(Plastic.NEW_GUI_MACOS);
                if (path != null)
                    return GetExistingDir(ToolConstants.NEW_MACOS_BINDIR);

                return GetExistingDir(ToolConstants.LEGACY_MACOS_BINDIR);
            }

            return null;
        }

        internal static string GetPlasticExePath()
        {
            if (PlatformIdentifier.IsWindows())
                return FindTool.ObtainToolCommand(
                Plastic.GUI_WINDOWS,
                    new List<String>() { GetWindowsClientInstallationFolder() });

            if (PlatformIdentifier.IsMac())
            {
                string path = GetToolCommand(Plastic.NEW_GUI_MACOS);
                if(path != null)
                    return path;

                return GetToolCommand(Plastic.LEGACY_GUI_MACOS);
            }

            return null;
        }

        internal static string GetGluonExePath()
        {
            if (PlatformIdentifier.IsWindows())
                return FindTool.ObtainToolCommand(
                    Gluon.GUI_WINDOWS,
                    new List<String>() { GetWindowsClientInstallationFolder() });

            if (PlatformIdentifier.IsMac())
            {
                string path = GetToolCommand(Gluon.NEW_GUI_MACOS);
                if (path != null)
                    return path;

                return GetToolCommand(Gluon.LEGACY_GUI_MACOS);
            }

            return null;
        }

        internal static string GetLocalPlasticServerExePath()
        {
            if (PlatformIdentifier.IsWindows())
            {
                return FindTool.ObtainToolCommand(
                    Plastic.LOCAL_SERVER_WINDOWS,
                    new List<String>() { GetWindowsServerInstallationFolder() });
            }

            if (PlatformIdentifier.IsMac())
            {
                return GetToolCommand(Plastic.LOCAL_SERVER_MACOS);
            }

            return null;
        }

        internal static void LogInstallationInfo()
        {
            string plasticClientBinDir = GetClientBinDir();

            if (string.IsNullOrEmpty(plasticClientBinDir))
            {
                mLog.DebugFormat("No installation found, behaving as {0} Edition",
                    EditionToken.IsCloudEdition() ? "Cloud" : "Enterprise");
            }
            else
            {
                bool isCloudPlasticInstall = File.Exists(Path.Combine(plasticClientBinDir, EditionToken.CLOUD_EDITION_FILE_NAME));

                mLog.DebugFormat("{0} Edition detected - installation directory: {1}",
                    isCloudPlasticInstall ? "Cloud" : "Enterprise",
                    plasticClientBinDir);
                mLog.DebugFormat("Local token: {0} Edition",
                    EditionToken.IsCloudEdition() ? "Cloud" : "Enterprise");
            }
        }

        static string GetToolCommand(string tool)
        {
            return File.Exists(tool) ? tool : null;
        }

        static string GetExistingDir(string directory)
        {
            return Directory.Exists(directory) ? directory : null;
        }

        static string GetWindowsClientInstallationFolder()
        {
            string programFilesFolder = Environment.GetFolderPath(
                Environment.SpecialFolder.ProgramFiles);

            return Path.Combine(Path.Combine(programFilesFolder,
                PLASTICSCM_FOLDER), PLASTICSCM_CLIENT_SUBFOLDER);
        }

        static string GetWindowsServerInstallationFolder()
        {
            string programFilesFolder = Environment.GetFolderPath(
                Environment.SpecialFolder.ProgramFiles);

            return Path.Combine(Path.Combine(programFilesFolder,
                PLASTICSCM_FOLDER), PLASTICSCM_SERVER_SUBFOLDER);
        }

        const string PLASTICSCM_FOLDER = "PlasticSCM5";
        const string PLASTICSCM_CLIENT_SUBFOLDER = "client";
        const string PLASTICSCM_SERVER_SUBFOLDER = "server";

        static readonly ILog mLog = PlasticApp.GetLogger("PlasticInstallPath");

        class Plastic
        {
            internal const string GUI_WINDOWS = "plastic.exe";
            internal const string LOCAL_SERVER_WINDOWS = "plasticd.exe";
            internal const string LEGACY_GUI_MACOS = "/Applications/PlasticSCM.app/Contents/MacOS/PlasticSCM";
            internal const string NEW_GUI_MACOS = "/Applications/PlasticSCM.app/Contents/MacOS/macplasticx";
            internal const string LOCAL_SERVER_MACOS = "/Applications/PlasticSCMServer.app/Contents/MacOS/plasticd";
        }

        class Gluon
        {
            internal const string GUI_WINDOWS = "gluon.exe";
            internal const string LEGACY_GUI_MACOS = "/Applications/Gluon.app/Contents/MacOS/Gluon";
            internal const string NEW_GUI_MACOS = "/Applications/Gluon.app/Contents/MacOS/macgluonx";
        }
    }
}
