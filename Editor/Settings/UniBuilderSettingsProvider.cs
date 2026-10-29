#if UNITY_EDITOR


using DragonResonance.Editor.Building;
using UnityEditor;
using UnityEngine;

#if ENABLE_UNIBUILDER
using Praenaris.UniBuilder;
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
			protected override bool OnPropertyGUI(SerializedProperty property) => property.name is
				nameof(UniBuilderSettings.Standalone) or
				nameof(UniBuilderSettings.Steam) or
				nameof(UniBuilderSettings.ItchIo);	// Drawn in their own sections instead
		#endif

			protected override void OnAfterGUI(string searchContext)
			{
				#if ENABLE_UNIBUILDER
				EditorGUILayout.Space(SmallPadding);
				_sectionsScroll = EditorGUILayout.BeginScrollView(_sectionsScroll);
				{
					DrawPlatformSection(StandaloneLabel, StandaloneTint, this.SerializedSettings.FindProperty(nameof(UniBuilderSettings.Standalone)));
					DrawPlatformSection(SteamLabel, SteamTint, this.SerializedSettings.FindProperty(nameof(UniBuilderSettings.Steam)));
					DrawPlatformSection(ItchIoLabel, ItchIoTint, this.SerializedSettings.FindProperty(nameof(UniBuilderSettings.ItchIo)));
				}
				EditorGUILayout.EndScrollView();
				#endif
			}

		#endregion


		#region Privates

			private static void DrawPlatformSection(GUIContent title, Color tint, SerializedProperty property)
			{
				Rect sectionRect = EditorGUILayout.BeginVertical(SectionStyle);
				{
					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawRect(sectionRect, tint);

					GUILayout.Label(title, SectionTitleStyle);
					EditorGUILayout.Space(SmallPadding);

					SerializedProperty child = property.Copy();
					SerializedProperty end = property.GetEndProperty();
					if (child.NextVisible(true)) {
						do {
							if (SerializedProperty.EqualContents(child, end)) break;
							EditorGUILayout.PropertyField(child, true);
						}
						while (child.NextVisible(false));
					}
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space(SmallPadding);
			}

		#endregion


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