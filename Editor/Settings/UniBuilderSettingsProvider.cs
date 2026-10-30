#if UNITY_EDITOR


using DragonResonance.Editor.Building;
using UnityEditor;
using UnityEngine;

#if ENABLE_UNIBUILDER
using Praenaris.UniBuilder;
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
		private const int SectionTitleFontSize = 20;

		private static readonly Color StandaloneTint = new(0.95f, 0.95f, 0.95f, 0.2f);
		private static readonly Color SteamTint = new(0.4f, 0.753f, 0.957f, 0.25f);	// Steam #66C0F4
		private static readonly Color ItchIoTint = new(0.98f, 0.361f, 0.361f, 0.25f);	// itch.io #FA5C5C
		private static readonly GUIContent StandaloneLabel = new("Standalone");
		private static readonly GUIContent SteamLabel = new("Steam");
		private static readonly GUIContent ItchIoLabel = new("Itch.io");
		private static readonly GUIContent OpenPipelineLabel = new("Open UniBuilder Pipeline");
	#if ENABLE_UNIBUILDER
		private static readonly string[] StandaloneFields = { nameof(UniBuilderSettings.StandaloneTest1), nameof(UniBuilderSettings.StandaloneTest2), nameof(UniBuilderSettings.StandaloneTest3) };
		private static readonly string[] SteamFields = { nameof(UniBuilderSettings.SteamTest1), nameof(UniBuilderSettings.SteamTest2), nameof(UniBuilderSettings.SteamTest3) };
		private static readonly string[] ItchIoFields = { nameof(UniBuilderSettings.ItchIoTest1), nameof(UniBuilderSettings.ItchIoTest2), nameof(UniBuilderSettings.ItchIoTest3) };
	#endif
		private static GUIStyle _sectionStyle_internal = null;	// Caching only, use the property instead
		private static GUIStyle _sectionTitleStyle_internal = null;	// Caching only, use the property instead

		private Vector2 _sectionsScroll = Vector2.zero;


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
			protected override bool OnPropertyGUI(SerializedProperty property) => (Array.IndexOf(StandaloneFields, property.name) >= 0)
				|| (Array.IndexOf(SteamFields, property.name) >= 0)
				|| (Array.IndexOf(ItchIoFields, property.name) >= 0)	// Drawn in their own sections instead
				|| (property.name == nameof(UniBuilderSettings.BuildProfiles));	// Edited from the pipeline window instead
		#endif

			protected override void OnAfterGUI(string searchContext)
			{
				#if ENABLE_UNIBUILDER
				EditorGUILayout.Space(SmallPadding);
				if (GUILayout.Button(OpenPipelineLabel))
					UniBuilderPipelineWindow.Open();
				EditorGUILayout.Space(SmallPadding);
				_sectionsScroll = EditorGUILayout.BeginScrollView(_sectionsScroll);
				{
					DrawPlatformSection(StandaloneLabel, StandaloneTint, StandaloneFields);
					DrawPlatformSection(SteamLabel, SteamTint, SteamFields);
					DrawPlatformSection(ItchIoLabel, ItchIoTint, ItchIoFields);
				}
				EditorGUILayout.EndScrollView();
				#endif
			}

		#endregion


	#if ENABLE_UNIBUILDER
		#region Privates

			private void DrawPlatformSection(GUIContent title, Color tint, string[] fieldNames)
			{
				Rect sectionRect = EditorGUILayout.BeginVertical(SectionStyle);
				{
					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawRect(sectionRect, tint);

					GUILayout.Label(title, SectionTitleStyle);
					EditorGUILayout.Space(SmallPadding);

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