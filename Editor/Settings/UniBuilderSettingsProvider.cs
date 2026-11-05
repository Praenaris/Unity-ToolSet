#if UNITY_EDITOR


using DragonResonance.Editor.Building;
using UnityEditor;
using UnityEngine;

#if ENABLE_UNIBUILDER
using Praenaris.UniBuilder;
using System.Collections.Generic;
using System.IO;
using System;
#endif


namespace DragonResonance.Editor.Settings
{
#if ENABLE_UNIBUILDER
	public class UniBuilderSettingsProvider : AScriptableSettingsProvider<UniBuilderSettings>
#else
	public class UniBuilderSettingsProvider : AScriptableSettingsProvider
#endif
	{
		private const string SettingsPath = "Project/Praenaris/UniBuilder";
		private const string BuildDefinition = "ENABLE_UNIBUILDER";
		private const int GroupTitleFontSize = 20;
		private const int SectionTitleFontSize = 14;	// Like the pipeline window items
		private const float SeparatorThickness = 1f;
		private const float OpenPipelineButtonHeight = 32f;	// Like the pipeline window main buttons

		private static readonly Color FoundColor = new(0.3f, 0.85f, 0.3f, 1f);
		private static readonly Color MissingColor = new(0.95f, 0.3f, 0.3f, 1f);
		private static readonly Color SeparatorColor = new(0.5f, 0.5f, 0.5f, 0.5f);
		private static readonly Color ShipyardTint = new(1f, 0.9f, 0.3f, 0.25f);
		private static readonly Color SteamTint = new(0.4f, 0.753f, 0.957f, 0.25f);	// Steam #66C0F4
		private static readonly Color ItchIoTint = new(0.98f, 0.361f, 0.361f, 0.25f);	// itch.io #FA5C5C
		private static readonly GUIContent IntegrationsLabel = new("Integrations");
		private static readonly GUIContent StoresLabel = new("Stores");
		private static readonly GUIContent ShipyardLabel = new("Shipyard");
		private static readonly GUIContent CheckPythonLabel = new("Check Python");
		private static readonly GUIContent ShipyardToolsPathLabel = new("Tools path");
		private static readonly GUIContent SteamLabel = new("Steam");
		private static readonly GUIContent CheckSteamCmdLabel = new("Check steamcmd");
		private static readonly GUIContent ItchIoLabel = new("Itch.io");
		private static readonly GUIContent CheckButlerLabel = new("Check butler");
		private static readonly GUIContent OpenPipelineLabel = new("Open UniBuilder Pipeline");
	#if ENABLE_UNIBUILDER
		private static readonly string[] SteamFields = { nameof(UniBuilderSettings.SteamTest1), nameof(UniBuilderSettings.SteamTest2), nameof(UniBuilderSettings.SteamTest3) };
		private static readonly string[] ItchIoFields = { nameof(UniBuilderSettings.ItchIoTest1), nameof(UniBuilderSettings.ItchIoTest2), nameof(UniBuilderSettings.ItchIoTest3) };
		private static readonly Dictionary<GUIContent, (bool IsFound, string Result)> ToolChecks = new();	// By check button, empty until checked
	#endif
		private static GUIStyle _sectionStyle_internal = null;	// Caching only, use the property instead
		private static GUIStyle _groupTitleStyle_internal = null;	// Caching only, use the property instead
		private static GUIStyle _sectionTitleStyle_internal = null;	// Caching only, use the property instead

		private static GUIStyle _foundStyle_internal = null;	// Caching only, use the property instead
		private static GUIStyle _missingStyle_internal = null;	// Caching only, use the property instead


		#region Constructors

			[SettingsProvider]
			public static SettingsProvider Create() => new UniBuilderSettingsProvider(SettingsPath, SettingsScope.Project);

			public UniBuilderSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

		#endregion


		#region Inheritables

			protected override void OnBeforeGUI(string searchContext)
			{
				#if ENABLE_UNIBUILDER
					if (!EditorGUILayout.Toggle("Enabled", true))
						BuildDefines.SetDefinitionState(BuildDefinition, false);
				#else
					if (EditorGUILayout.Toggle("Enabled", false))
						BuildDefines.SetDefinitionState(BuildDefinition, true);
				#endif
			}

		#if ENABLE_UNIBUILDER
			protected override bool OnPropertyGUI(SerializedProperty property) => (Array.IndexOf(SteamFields, property.name) >= 0)
				|| (Array.IndexOf(ItchIoFields, property.name) >= 0)	// Drawn in their own sections instead
				|| (property.name == nameof(UniBuilderSettings.BuildProfiles))
				|| (property.name == nameof(UniBuilderSettings.StopPipelineOnFailure))
				|| (property.name == nameof(UniBuilderSettings.ConfirmDoNotsRemoval))
				|| (property.name == nameof(UniBuilderSettings.RecentBuildDays));	// Edited from the pipeline window instead
		#endif

			protected override void OnAfterGUI(string searchContext)
			{
				#if ENABLE_UNIBUILDER
				EditorGUILayout.Space(SmallPadding);
				GUILayout.Label(IntegrationsLabel, GroupTitleStyle);
				EditorGUILayout.Space(SmallPadding);
				DrawShipyardSection();
				if (GUILayout.Button(OpenPipelineLabel, GUILayout.Height(OpenPipelineButtonHeight)))
					UniBuilderPipelineWindow.Open();
				EditorGUILayout.Space(SmallPadding);
				DrawSeparator();
				GUILayout.Label(StoresLabel, GroupTitleStyle);
				EditorGUILayout.Space(SmallPadding);
				DrawPlatformSection(SteamLabel, SteamTint, SteamFields, CheckSteamCmdLabel, () => CheckOnPath("steamcmd"));
				DrawPlatformSection(ItchIoLabel, ItchIoTint, ItchIoFields, CheckButlerLabel, () => CheckOnPath("butler"));
				#endif
			}

		#endregion


	#if ENABLE_UNIBUILDER
		#region Privates

			private static void DrawSeparator()
			{
				EditorGUILayout.Space(SmallPadding / 2f);
				EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, SeparatorThickness), SeparatorColor);
				EditorGUILayout.Space(SmallPadding / 2f);
			}

			/// <summary>A check button with its result next to it, green when found and red otherwise.</summary>
			private static void DrawToolCheck(GUIContent buttonLabel, Func<(bool IsFound, string Result)> check)
			{
				EditorGUILayout.BeginHorizontal();
				{
					if (GUILayout.Button(buttonLabel, GUILayout.ExpandWidth(false)))
						ToolChecks[buttonLabel] = check();
					if (ToolChecks.TryGetValue(buttonLabel, out (bool IsFound, string Result) toolCheck))
						GUILayout.Label(toolCheck.Result, (toolCheck.IsFound ? FoundStyle : MissingStyle));
				}
				EditorGUILayout.EndHorizontal();
			}

			private static (bool IsFound, string Result) CheckOnPath(string executable)
			{
				string[] extensions = { string.Empty, ".exe", ".bat", ".cmd" };	// Bare elsewhere, any of these on Windows
				foreach (string folder in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator)) {
					foreach (string extension in extensions) {
						string path = Path.Combine(folder.Trim('"'), (executable + extension));
						if (File.Exists(path))
							return (true, $"{executable} found at {path}");
					}
				}
				return (false, $"{executable} isn't installed or isn't on the PATH");
			}

			private static void DrawShipyardSection()
			{
				Rect sectionRect = EditorGUILayout.BeginVertical(SectionStyle);
				{
					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawRect(sectionRect, ShipyardTint);

					GUILayout.Label(ShipyardLabel, SectionTitleStyle);
					EditorGUILayout.Space(SmallPadding);

					DrawToolCheck(CheckPythonLabel, () => (Shipyard.IsPythonInstalled(out string version)
						? (true, $"{version} found on the PATH")
						: (false, "Python isn't installed or isn't on the PATH")));
					DrawSeparator();

					string toolsPath = EditorGUILayout.DelayedTextField(ShipyardToolsPathLabel, Shipyard.ToolsPath);
					if (toolsPath != Shipyard.ToolsPath)
						Shipyard.ToolsPath = toolsPath;	// Stored on this machine only
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space(SmallPadding);
			}

			private void DrawPlatformSection(GUIContent title, Color tint, string[] fieldNames, GUIContent checkLabel, Func<(bool IsFound, string Result)> check)
			{
				Rect sectionRect = EditorGUILayout.BeginVertical(SectionStyle);
				{
					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawRect(sectionRect, tint);

					GUILayout.Label(title, SectionTitleStyle);
					EditorGUILayout.Space(SmallPadding);

					DrawToolCheck(checkLabel, check);
					DrawSeparator();

					foreach (string fieldName in fieldNames)
						EditorGUILayout.PropertyField(this.SerializedSettings.FindProperty(fieldName), true);
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space(SmallPadding);
			}

		#endregion
	#endif


		#region Properties

			private static GUIStyle SectionStyle => (_sectionStyle_internal ??= new GUIStyle() {
				padding = new RectOffset(SmallPadding, SmallPadding, SmallPadding, SmallPadding),
			});

			private static GUIStyle FoundStyle => (_foundStyle_internal ??= new GUIStyle(EditorStyles.boldLabel) {
				normal = { textColor = FoundColor },
			});

			private static GUIStyle MissingStyle => (_missingStyle_internal ??= new GUIStyle(EditorStyles.boldLabel) {
				normal = { textColor = MissingColor },
			});

			private static GUIStyle GroupTitleStyle => (_groupTitleStyle_internal ??= new GUIStyle(EditorStyles.boldLabel) {
				fontSize = GroupTitleFontSize,
			});

			private static GUIStyle SectionTitleStyle => (_sectionTitleStyle_internal ??= new GUIStyle(EditorStyles.boldLabel) {
				fontSize = SectionTitleFontSize,
			});

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