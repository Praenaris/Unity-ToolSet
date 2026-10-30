#if UNITY_EDITOR && ENABLE_UNIBUILDER


using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build.Profile;
using UnityEditor;
using UnityEngine;


namespace Praenaris.UniBuilder
{
	/// <summary>Lists the project's build profiles, each one marked in or out of the pipeline, with its output path and the profile's main settings.</summary>
	public class UniBuilderPipelineWindow : EditorWindow
	{
		private const string WindowTitle = "UniBuilder Pipeline";
		private const string SettingsPath = "Project/Praenaris/UniBuilder";
		private const string DefaultOutputFolder = "Builds";
		private const float BuildAllButtonHeight = 32f;
		private const float BrowseButtonWidth = 24f;
		private const int SectionPadding = 12;
		private const int SectionTitleFontSize = 14;

		private static readonly Color IncludedTint = new(0.4f, 0.8f, 0.4f, 0.2f);
		private static readonly Color ExcludedTint = new(0.5f, 0.5f, 0.5f, 0.1f);
		private static readonly Color BuildAllColor = new(0.4f, 0.8f, 0.4f, 1f);
		private static readonly GUIContent BuildAllLabel = new("Build All", "Builds every profile in the pipeline, in order");
		private static readonly GUIContent ActiveLabel = new("Active", "The active build profile");
		private static readonly GUIContent SelectLabel = new("Select", "Selects the build profile asset");
		private static readonly GUIContent OutputPathLabel = new("Output path", "Where the build goes, relative to the project folder");
		private static readonly GUIContent BrowseLabel = new("…", "Picks the output folder");
		private static GUIStyle _sectionStyle_internal = null;	// Caching only, use the property instead
		private static GUIStyle _sectionTitleStyle_internal = null;	// Caching only, use the property instead

		private ProfileInfo[] _profiles = { };
		private SerializedObject _serializedSettings_internal = null;	// Caching only, use the property instead
		private Vector2 _scroll = Vector2.zero;


		#region Events

			private void OnEnable() => RefreshProfiles();
			private void OnFocus() => RefreshProfiles();	// The profiles may have been edited in the Build Profiles window

			private void OnProjectChange()
			{
				RefreshProfiles();
				Repaint();
			}

			private void OnGUI()
			{
				if (this.SerializedSettings == null) {
					EditorGUILayout.HelpBox("There are no UniBuilder settings yet, open its settings page to create them.", MessageType.Warning);
					if (GUILayout.Button("Open UniBuilder settings"))
						SettingsService.OpenProjectSettings(SettingsPath);
					return;
				}

				this.SerializedSettings.Update();
				{
					SerializedProperty entries = this.SerializedSettings.FindProperty(nameof(UniBuilderSettings.BuildProfiles));
					SerializedProperty[] profileEntries = _profiles.Select(profile => FindOrAddEntry(entries, profile)).ToArray();

					DrawHeader(profileEntries.Count(entry => entry.FindPropertyRelative(nameof(UniBuildProfileEntry.Included)).boolValue));
					_scroll = EditorGUILayout.BeginScrollView(_scroll);
					{
						for (int profileIndex = 0; profileIndex < _profiles.Length; profileIndex++)
							DrawProfileSection(_profiles[profileIndex], profileEntries[profileIndex]);
					}
					EditorGUILayout.EndScrollView();
				}
				this.SerializedSettings.ApplyModifiedProperties();
			}

		#endregion


		#region Publics

			[MenuItem("Window/Praenaris/" + WindowTitle)]
			public static void Open() => GetWindow<UniBuilderPipelineWindow>(WindowTitle);

		#endregion


		#region Privates - Drawing

			private void DrawHeader(int includedCount)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField($"{includedCount} of {_profiles.Length} build profiles in the pipeline", EditorStyles.centeredGreyMiniLabel);

				Color previousColor = UnityEngine.GUI.backgroundColor;
				UnityEngine.GUI.backgroundColor = BuildAllColor;
				EditorGUI.BeginDisabledGroup(includedCount == 0);
				{
					if (GUILayout.Button(BuildAllLabel, GUILayout.Height(BuildAllButtonHeight)))
						BuildAll();
				}
				EditorGUI.EndDisabledGroup();
				UnityEngine.GUI.backgroundColor = previousColor;
				EditorGUILayout.Space();
			}

			private static void DrawProfileSection(ProfileInfo profile, SerializedProperty entry)
			{
				SerializedProperty included = entry.FindPropertyRelative(nameof(UniBuildProfileEntry.Included));

				Rect sectionRect = EditorGUILayout.BeginVertical(SectionStyle);
				{
					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawRect(sectionRect, (included.boolValue ? IncludedTint : ExcludedTint));

					EditorGUILayout.BeginHorizontal();
					{
						bool isIncluded = EditorGUILayout.ToggleLeft(profile.Profile.name, included.boolValue, SectionTitleStyle);
						if (isIncluded != included.boolValue)
							included.boolValue = isIncluded;
						if (BuildProfile.GetActiveBuildProfile() == profile.Profile)
							GUILayout.Label(ActiveLabel, EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(false));
						if (GUILayout.Button(SelectLabel, EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
							Selection.activeObject = profile.Profile;
							EditorGUIUtility.PingObject(profile.Profile);
						}
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space(SectionPadding / 2f);

					EditorGUI.BeginDisabledGroup(!included.boolValue);
					{
						EditorGUILayout.LabelField("Platform", profile.Platform);
						EditorGUILayout.LabelField("Development", profile.Development);
						EditorGUILayout.LabelField("Scenes", GetScenesSummary(profile.Profile));
						EditorGUILayout.LabelField("Scripting defines", GetDefinesSummary(profile.Profile), EditorStyles.wordWrappedLabel);
						DrawOutputPathField(profile.Profile, entry.FindPropertyRelative(nameof(UniBuildProfileEntry.OutputPath)));
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space(SectionPadding / 2f);
			}

			private static void DrawOutputPathField(BuildProfile profile, SerializedProperty outputPath)
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.PropertyField(outputPath, OutputPathLabel);
					if (GUILayout.Button(BrowseLabel, EditorStyles.miniButton, GUILayout.Width(BrowseButtonWidth))) {
						string folder = EditorUtility.OpenFolderPanel($"Output folder for {profile.name}", Path.GetFullPath(Path.Combine(ProjectFolder, outputPath.stringValue)), string.Empty);
						if (!string.IsNullOrEmpty(folder)) {
							outputPath.stringValue = Path.GetRelativePath(ProjectFolder, folder).Replace(Path.DirectorySeparatorChar, '/');
							outputPath.serializedObject.ApplyModifiedProperties();
						}
						GUIUtility.ExitGUI();	// The folder panel breaks the current layout pass
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			private static string GetScenesSummary(BuildProfile profile) =>
				$"{profile.GetScenesForBuild().Count(scene => scene.enabled)} ({(profile.overrideGlobalScenes ? "own list" : "global list")})";

			private static string GetDefinesSummary(BuildProfile profile) =>
				(profile.scriptingDefines.Any() ? string.Join(", ", profile.scriptingDefines) : "None");

		#endregion


		#region Privates - Data

			private void BuildAll() { }	// Pending

			private void RefreshProfiles()
			{
				List<ProfileInfo> profiles = new();
				foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(BuildProfile)}")) {
					BuildProfile profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(AssetDatabase.GUIDToAssetPath(guid));
					if (profile != null)
						profiles.Add(new ProfileInfo(guid, profile));
				}
				_profiles = profiles.ToArray();
			}

			private static SerializedProperty FindOrAddEntry(SerializedProperty entries, ProfileInfo profile)
			{
				for (int entryIndex = 0; entryIndex < entries.arraySize; entryIndex++) {
					SerializedProperty entry = entries.GetArrayElementAtIndex(entryIndex);
					if (entry.FindPropertyRelative(nameof(UniBuildProfileEntry.ProfileGuid)).stringValue == profile.Guid)
						return entry;
				}

				entries.InsertArrayElementAtIndex(entries.arraySize);
				SerializedProperty newEntry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
				newEntry.FindPropertyRelative(nameof(UniBuildProfileEntry.ProfileGuid)).stringValue = profile.Guid;
				newEntry.FindPropertyRelative(nameof(UniBuildProfileEntry.Included)).boolValue = false;
				newEntry.FindPropertyRelative(nameof(UniBuildProfileEntry.OutputPath)).stringValue = $"{DefaultOutputFolder}/{profile.Profile.name}";
				return newEntry;
			}

			private static SerializedObject LoadSerializedSettings()
			{
				string guid = AssetDatabase.FindAssets($"t:{nameof(UniBuilderSettings)}").FirstOrDefault();
				UniBuilderSettings settings = ((guid != null) ? AssetDatabase.LoadAssetAtPath<UniBuilderSettings>(AssetDatabase.GUIDToAssetPath(guid)) : null);
				return ((settings != null) ? new SerializedObject(settings) : null);
			}

		#endregion


		#region Properties

			private SerializedObject SerializedSettings => (((_serializedSettings_internal != null) && (_serializedSettings_internal.targetObject != null))
				? _serializedSettings_internal
				: (_serializedSettings_internal = LoadSerializedSettings()));

			private static string ProjectFolder => Directory.GetParent(Application.dataPath).FullName;

			private static GUIStyle SectionStyle => (_sectionStyle_internal ??= new GUIStyle() {
				padding = new RectOffset(SectionPadding, SectionPadding, SectionPadding, SectionPadding),
			});

			private static GUIStyle SectionTitleStyle => (_sectionTitleStyle_internal ??= new GUIStyle(EditorStyles.boldLabel) {
				fontSize = SectionTitleFontSize,
			});

		#endregion


		/// <summary>What the window shows of a build profile, read once per refresh.</summary>
		private readonly struct ProfileInfo
		{
			public readonly string Guid;
			public readonly BuildProfile Profile;
			public readonly string Platform;
			public readonly string Development;


			public ProfileInfo(string guid, BuildProfile profile)
			{
				SerializedObject serializedProfile = new(profile);	// The target and the platform settings aren't public
				BuildTarget target = (BuildTarget)serializedProfile.FindProperty("m_BuildTarget").intValue;
				SerializedProperty development = serializedProfile.FindProperty("m_PlatformBuildProfile.m_Development");

				Guid = guid;
				Profile = profile;
				Platform = ((BuildPipeline.GetBuildTargetGroup(target) == BuildTargetGroup.Standalone)
					? $"{target} ({(StandaloneBuildSubtarget)serializedProfile.FindProperty("m_Subtarget").intValue})"
					: target.ToString());
				Development = ((development == null) ? "Unknown" : (development.boolValue ? "Yes" : "No"));
			}
		}
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