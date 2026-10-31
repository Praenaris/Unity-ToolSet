#if UNITY_EDITOR && ENABLE_UNIBUILDER


using DragonResonance.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
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
		private const string BuildProfilesMenuPath = "File/Build Profiles";
		private const float BuildAllButtonHeight = 32f;
		private const float BrowseButtonWidth = 24f;
		private const float SideColumnWidth = 160f;
		private const int SectionPadding = 12;
		private const int SectionTitleFontSize = 14;

		private static readonly Color IncludedTint = new(0.4f, 0.8f, 0.4f, 0.2f);
		private static readonly Color ExcludedTint = new(0.5f, 0.5f, 0.5f, 0.1f);
		private static readonly Color BuildAllColor = new(0.4f, 0.8f, 0.4f, 1f);
		private static readonly Color SucceededColor = new(0.3f, 0.85f, 0.3f, 1f);
		private static readonly Color FailedColor = new(0.95f, 0.3f, 0.3f, 1f);
		private static readonly GUIContent OpenBuildProfilesLabel = new("Open Build Profiles", "Opens the Build Profiles window to edit them");
		private static readonly GUIContent StopOnFailureLabel = new("Stop the pipeline when a build fails", "Otherwise it carries on with the next profiles");
		private static readonly GUIContent BuildAllLabel = new("Build All", "Builds every profile in the pipeline, in order");
		private static readonly GUIContent MoveUpLabel = new("▲", "Moves it earlier in the pipeline");
		private static readonly GUIContent MoveDownLabel = new("▼", "Moves it later in the pipeline");
		private static readonly GUIContent SelectLabel = new("Select", "Selects the build profile asset");
		private static readonly GUIContent BuildLabel = new("Build", "Builds this profile alone");
		private static readonly GUIContent OutputPathLabel = new("Output path", "Where the build goes, relative to the project folder");
		private static readonly GUIContent BrowseLabel = new("…", "Picks the output folder");
		private static GUIStyle _sectionStyle_internal = null;	// Caching only, use the property instead
		private static GUIStyle _sectionTitleStyle_internal = null;	// Caching only, use the property instead
		private static GUIStyle _buildVersionStyle_internal = null;	// Caching only, use the property instead
		private static GUIStyle _buildTimeStyle_internal = null;	// Caching only, use the property instead
		private static GUIStyle _succeededStyle_internal = null;	// Caching only, use the property instead
		private static GUIStyle _failedStyle_internal = null;	// Caching only, use the property instead

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
					int[] entryIndices = _profiles.Select(profile => FindOrAddEntryIndex(entries, profile)).ToArray();
					int[] order = Enumerable.Range(0, _profiles.Length).OrderBy(profileIndex => entryIndices[profileIndex]).ToArray();	// The pipeline order is the entries' order
					int moveFrom = -1;
					int moveTo = -1;

					DrawHeader(this.SerializedSettings.FindProperty(nameof(UniBuilderSettings.StopPipelineOnFailure)), entryIndices.Count(entryIndex => entries.GetArrayElementAtIndex(entryIndex).FindPropertyRelative(nameof(UniBuildProfileEntry.Included)).boolValue));
					_scroll = EditorGUILayout.BeginScrollView(_scroll);
					{
						for (int position = 0; position < order.Length; position++) {
							int profileIndex = order[position];
							int direction = DrawProfileSection(position, order.Length, _profiles[profileIndex], entries.GetArrayElementAtIndex(entryIndices[profileIndex]));
							if (direction != 0) {
								moveFrom = entryIndices[profileIndex];
								moveTo = entryIndices[order[position + direction]];
							}
						}
					}
					EditorGUILayout.EndScrollView();

					if (moveFrom >= 0)	// After drawing, since moving shifts the entries' property paths
						entries.MoveArrayElement(moveFrom, moveTo);
				}
				this.SerializedSettings.ApplyModifiedProperties();
			}

		#endregion


		#region Publics

			[MenuItem("Window/Praenaris/" + WindowTitle)]
			public static void Open() => GetWindow<UniBuilderPipelineWindow>(WindowTitle);

		#endregion


		#region Privates - Drawing

			private void DrawHeader(SerializedProperty stopOnFailure, int includedCount)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField($"{includedCount} of {_profiles.Length} build profiles in the pipeline", EditorStyles.centeredGreyMiniLabel);

				bool isStoppingOnFailure = EditorGUILayout.ToggleLeft(StopOnFailureLabel, stopOnFailure.boolValue);
				if (isStoppingOnFailure != stopOnFailure.boolValue)
					stopOnFailure.boolValue = isStoppingOnFailure;

				if (GUILayout.Button(OpenBuildProfilesLabel))
					EditorApplication.ExecuteMenuItem(BuildProfilesMenuPath);

				Color previousColor = UnityEngine.GUI.backgroundColor;
				UnityEngine.GUI.backgroundColor = BuildAllColor;
				EditorGUI.BeginDisabledGroup(includedCount == 0);
				{
					if (GUILayout.Button(BuildAllLabel, GUILayout.Height(BuildAllButtonHeight)))
						EditorApplication.delayCall += BuildAll;	// Outside the GUI pass, building takes a while
				}
				EditorGUI.EndDisabledGroup();
				UnityEngine.GUI.backgroundColor = previousColor;
				EditorGUILayout.Space();
			}

			/// <summary>Returns -1 or 1 when moved earlier or later in the pipeline, 0 otherwise.</summary>
			private static int DrawProfileSection(int position, int count, ProfileInfo profile, SerializedProperty entry)
			{
				int direction = 0;
				SerializedProperty included = entry.FindPropertyRelative(nameof(UniBuildProfileEntry.Included));

				Rect sectionRect = EditorGUILayout.BeginHorizontal(SectionStyle);
				{
					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawRect(sectionRect, (included.boolValue ? IncludedTint : ExcludedTint));

					EditorGUILayout.BeginVertical();
					{
						bool isIncluded = EditorGUILayout.ToggleLeft($"{position + 1}. {profile.Profile.name}", included.boolValue, SectionTitleStyle);
						if (isIncluded != included.boolValue)
							included.boolValue = isIncluded;
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
					EditorGUILayout.Space(SectionPadding, false);

					EditorGUILayout.BeginVertical(GUILayout.Width(SideColumnWidth));
					{
						EditorGUILayout.BeginHorizontal();
						{
							GUILayout.FlexibleSpace();
							EditorGUI.BeginDisabledGroup(position == 0);
							{
								if (GUILayout.Button(MoveUpLabel, EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(false)))
									direction = -1;
							}
							EditorGUI.EndDisabledGroup();
							EditorGUI.BeginDisabledGroup(position == (count - 1));
							{
								if (GUILayout.Button(MoveDownLabel, EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
									direction = 1;
							}
							EditorGUI.EndDisabledGroup();
							if (GUILayout.Button(SelectLabel, EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
								Selection.activeObject = profile.Profile;
								EditorGUIUtility.PingObject(profile.Profile);
							}

							Color previousColor = UnityEngine.GUI.backgroundColor;
							UnityEngine.GUI.backgroundColor = BuildAllColor;
							if (GUILayout.Button(BuildLabel, EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
								string outputPath = entry.FindPropertyRelative(nameof(UniBuildProfileEntry.OutputPath)).stringValue;
								EditorApplication.delayCall += () => Build(profile, outputPath);	// Outside the GUI pass, building takes a while
							}
							UnityEngine.GUI.backgroundColor = previousColor;
						}
						EditorGUILayout.EndHorizontal();
						DrawLastBuild(entry);
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space(SectionPadding / 2f);
				return direction;
			}

			private static void DrawLastBuild(SerializedProperty entry)
			{
				string lastBuildTime = entry.FindPropertyRelative(nameof(UniBuildProfileEntry.LastBuildTime)).stringValue;
				if (string.IsNullOrEmpty(lastBuildTime)) {
					GUILayout.Label("Never built", BuildTimeStyle);
					return;
				}

				bool succeeded = entry.FindPropertyRelative(nameof(UniBuildProfileEntry.LastBuildSucceeded)).boolValue;
				GUILayout.Label($"v{entry.FindPropertyRelative(nameof(UniBuildProfileEntry.LastBuildVersion)).stringValue}", BuildVersionStyle);
				GUILayout.Label(lastBuildTime, BuildTimeStyle);
				GUILayout.Label((succeeded ? "Success" : "Fail"), (succeeded ? SucceededStyle : FailedStyle));
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

			private void BuildAll()
			{
				this.SerializedSettings.Update();
				SerializedProperty entries = this.SerializedSettings.FindProperty(nameof(UniBuilderSettings.BuildProfiles));
				bool stopOnFailure = this.SerializedSettings.FindProperty(nameof(UniBuilderSettings.StopPipelineOnFailure)).boolValue;

				List<(ProfileInfo Profile, string OutputPath)> pipeline = new();	// Read up front, building rewrites the entries
				for (int entryIndex = 0; entryIndex < entries.arraySize; entryIndex++) {
					SerializedProperty entry = entries.GetArrayElementAtIndex(entryIndex);
					if (!entry.FindPropertyRelative(nameof(UniBuildProfileEntry.Included)).boolValue) continue;

					string guid = entry.FindPropertyRelative(nameof(UniBuildProfileEntry.ProfileGuid)).stringValue;
					foreach (ProfileInfo profile in _profiles)
						if (profile.Guid == guid)
							pipeline.Add((profile, entry.FindPropertyRelative(nameof(UniBuildProfileEntry.OutputPath)).stringValue));
				}

				Log.Emphasis($"Building the pipeline, {pipeline.Count} profiles ...");
				int builtCount = 0;
				foreach ((ProfileInfo profile, string outputPath) in pipeline) {
					if (Build(profile, outputPath))
						builtCount++;
					else if (stopOnFailure) {
						Log.Error($"Pipeline stopped at {profile.Profile.name}, {builtCount} of {pipeline.Count} profiles built");
						return;
					}
				}
				Log.Emphasis($"Pipeline finished, {builtCount} of {pipeline.Count} profiles built");
			}

			/// <summary>Returns whether it was built.</summary>
			private static bool Build(ProfileInfo profile, string outputPath)
			{
				if (string.IsNullOrWhiteSpace(outputPath)) {
					Log.Error($"{profile.Profile.name} has no output path, set one to build it", profile.Profile);
					return false;
				}

				string locationPath = GetLocationPath(profile.Target, outputPath);
				Log.Info($"Building {profile.Profile.name} into {locationPath} ...", profile.Profile);
				BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions() {
					buildProfile = profile.Profile,
					locationPathName = locationPath,
					options = BuildOptions.None,
				});

				bool succeeded = (report.summary.result == BuildResult.Succeeded);
				RecordBuild(profile, succeeded);
				if (succeeded)
					Log.Emphasis($"{profile.Profile.name} built in {(int)report.summary.totalTime.TotalSeconds} s", profile.Profile);
				else
					Log.Error($"{profile.Profile.name} build {report.summary.result} with {report.summary.totalErrors} errors, see the Console", profile.Profile);
				return succeeded;
			}

			private static string GetLocationPath(BuildTarget target, string outputPath)
			{
				string folder = Path.GetFullPath(Path.Combine(ProjectFolder, outputPath));
				string extension = target switch {
					BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => ".exe",
					BuildTarget.StandaloneLinux64 => ".run",
					BuildTarget.StandaloneOSX => ".app",
					BuildTarget.Android => ".apk",
					_ => null,
				};
				return ((extension != null) ? Path.Combine(folder, (PlayerSettings.productName + extension)) : folder);	// Executables go inside the folder, the rest (Web...) are the folder itself
			}

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

			private static void RecordBuild(ProfileInfo profile, bool succeeded)
			{
				SerializedObject serializedSettings = LoadSerializedSettings();
				if (serializedSettings == null) return;

				SerializedProperty entries = serializedSettings.FindProperty(nameof(UniBuilderSettings.BuildProfiles));
				SerializedProperty entry = entries.GetArrayElementAtIndex(FindOrAddEntryIndex(entries, profile));
				entry.FindPropertyRelative(nameof(UniBuildProfileEntry.LastBuildVersion)).stringValue = PlayerSettings.bundleVersion;
				entry.FindPropertyRelative(nameof(UniBuildProfileEntry.LastBuildTime)).stringValue = DateTime.Now.ToString("s");	// ISO 8601
				entry.FindPropertyRelative(nameof(UniBuildProfileEntry.LastBuildSucceeded)).boolValue = succeeded;
				serializedSettings.ApplyModifiedPropertiesWithoutUndo();
				AssetDatabase.SaveAssetIfDirty(serializedSettings.targetObject);
			}

			private static int FindOrAddEntryIndex(SerializedProperty entries, ProfileInfo profile)
			{
				for (int entryIndex = 0; entryIndex < entries.arraySize; entryIndex++)
					if (entries.GetArrayElementAtIndex(entryIndex).FindPropertyRelative(nameof(UniBuildProfileEntry.ProfileGuid)).stringValue == profile.Guid)
						return entryIndex;

				entries.InsertArrayElementAtIndex(entries.arraySize);	// It copies the last entry, so every field is set below
				SerializedProperty newEntry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
				newEntry.FindPropertyRelative(nameof(UniBuildProfileEntry.ProfileGuid)).stringValue = profile.Guid;
				newEntry.FindPropertyRelative(nameof(UniBuildProfileEntry.Included)).boolValue = false;
				newEntry.FindPropertyRelative(nameof(UniBuildProfileEntry.OutputPath)).stringValue = $"{DefaultOutputFolder}/{profile.Profile.name}";
				newEntry.FindPropertyRelative(nameof(UniBuildProfileEntry.LastBuildVersion)).stringValue = string.Empty;
				newEntry.FindPropertyRelative(nameof(UniBuildProfileEntry.LastBuildTime)).stringValue = string.Empty;
				newEntry.FindPropertyRelative(nameof(UniBuildProfileEntry.LastBuildSucceeded)).boolValue = false;
				return (entries.arraySize - 1);
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

			private static GUIStyle BuildVersionStyle => (_buildVersionStyle_internal ??= new GUIStyle(EditorStyles.boldLabel) {
				alignment = TextAnchor.MiddleRight,
			});

			private static GUIStyle BuildTimeStyle => (_buildTimeStyle_internal ??= new GUIStyle(EditorStyles.miniLabel) {
				alignment = TextAnchor.MiddleRight,
			});

			private static GUIStyle SucceededStyle => (_succeededStyle_internal ??= new GUIStyle(EditorStyles.boldLabel) {
				alignment = TextAnchor.MiddleRight,
				normal = { textColor = SucceededColor },
			});

			private static GUIStyle FailedStyle => (_failedStyle_internal ??= new GUIStyle(EditorStyles.boldLabel) {
				alignment = TextAnchor.MiddleRight,
				normal = { textColor = FailedColor },
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
			public readonly BuildTarget Target;
			public readonly string Platform;
			public readonly string Development;


			public ProfileInfo(string guid, BuildProfile profile)
			{
				SerializedObject serializedProfile = new(profile);	// The target and the platform settings aren't public
				BuildTarget target = (BuildTarget)serializedProfile.FindProperty("m_BuildTarget").intValue;
				SerializedProperty development = serializedProfile.FindProperty("m_PlatformBuildProfile.m_Development");

				Guid = guid;
				Profile = profile;
				Target = target;
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