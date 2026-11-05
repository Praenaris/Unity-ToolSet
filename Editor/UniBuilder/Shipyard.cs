#if UNITY_EDITOR && ENABLE_UNIBUILDER


using DragonResonance.Logging;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System;
using UnityEditor;
using UnityEngine;


namespace Praenaris.UniBuilder
{
	public static class Shipyard
	{
		private const string PythonExecutable = "python";	// Found through the PATH
		private const string DefaultToolsPath = "../Shipyard/tools";	// Relative to the project folder
		private const string ToolsPathPrefKey = "Praenaris.UniBuilder.{0}.ShipyardToolsPath";	// Project GUID, EditorPrefs are shared by every project


		#region Publics

			public static bool IsPythonInstalled(out string version)
			{
				version = null;
				try {
					using Process process = Process.Start(CreateStartInfo("--version"));
					version = (process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd()).Trim();	// Older versions print it to the error output
					process.WaitForExit();
					return (process.ExitCode == 0);
				}
				catch (Win32Exception) {
					return false;	// Not found
				}
			}

			public static int Run(string script, params string[] arguments)
			{
				string scriptPath = Path.Combine(ToolsFolder, script);
				if (!File.Exists(scriptPath)) {
					Log.Error($"Shipyard script {scriptPath} not found, check the Shipyard tools path in the UniBuilder settings");
					return -1;
				}

				Log.Info($"Running Shipyard {script} ...");
				try {
					using Process process = new() { StartInfo = CreateStartInfo(new[] { scriptPath }.Concat(arguments).ToArray()) };
					process.OutputDataReceived += (sender, line) => { if (!string.IsNullOrEmpty(line.Data)) Log.Info(line.Data); };
					process.ErrorDataReceived += (sender, line) => { if (!string.IsNullOrEmpty(line.Data)) Log.Error(line.Data); };
					process.Start();
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();
					process.WaitForExit();

					if (process.ExitCode == 0)
						Log.Emphasis($"Shipyard {script} finished");
					else
						Log.Error($"Shipyard {script} failed with exit code {process.ExitCode}");
					return process.ExitCode;
				}
				catch (Win32Exception exception) {
					Log.Exception(exception, $"Couldn't run Shipyard {script}, is Python installed and on the PATH?");
					return -1;
				}
			}

		#endregion


		#region Privates

			private static ProcessStartInfo CreateStartInfo(params string[] arguments)
			{
				ProcessStartInfo startInfo = new(PythonExecutable, string.Join(" ", arguments.Select(argument => $"\"{argument.Replace("\"", "\\\"")}\""))) {
					CreateNoWindow = true,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					StandardErrorEncoding = Encoding.UTF8,
					StandardOutputEncoding = Encoding.UTF8,
					UseShellExecute = false,
					WorkingDirectory = ProjectFolder,
				};
				startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
				startInfo.EnvironmentVariables["PYTHONUNBUFFERED"] = "1";	// Each line reaches the Console as it's printed
				return startInfo;
			}

		#endregion


		#region Properties

			public static string ToolsPath
			{
				get => EditorPrefs.GetString(string.Format(ToolsPathPrefKey, PlayerSettings.productGUID), DefaultToolsPath);
				set => EditorPrefs.SetString(string.Format(ToolsPathPrefKey, PlayerSettings.productGUID), value);
			}

			public static string ToolsFolder => Path.GetFullPath(Path.Combine(ProjectFolder, ToolsPath));
			private static string ProjectFolder => Directory.GetParent(Application.dataPath).FullName;

		#endregion
	}
}


#endif


/*                                                                                                                */
/*       `7MM"""Mq.`7MM"""Mq.       db     `7MM"""YMM  `7MN.   `7MF'     db     `7MM"""Mq. `7MMF' .M"""bgd        */
/*         MM   `MM. MM   `MM.     ;MM:      MM    `7    MMN.    M      ;MM:      MM   `MM.  MM  ,MI    "Y        */
/*         MM   ,M9  MM   ,M9     ,V^MM.     MM   d      M YMb   M     ,V^MM.     MM   ,M9   MM  `MMb.            */
/*         MMmmdM9   MMmmdM9     ,M  `MM     MMmmMM      M  `MN. M    ,M  `MM     MMmmdM9    MM    `YMMNq.        */
/*         MM        MM  YM.     AbmmmqMA    MM   Y  ,   M   `MM.M    AbmmmqMA    MM  YM.    MM  .     `MM        */
/*         MM        MM   `Mb.  A'     VML   MM     ,M   M     YMM   A'     VML   MM   `Mb.  MM  Mb     dM        */
/*       .JMML.    .JMML. .JMM.AMA.   .AMMA.JMMmmmmMMM .JML.    YM .AMA.   .AMMA.JMML. .JMM.JMML.P"Ybmmd"         */
/*                                                                                                                */
/*                 Licensed under the Apache License, Version 2.0.  See LICENSE.md for more info.                 */
/*                                     Copyright © 2026. All rights reserved.                                     */
/*                                                                                                                */