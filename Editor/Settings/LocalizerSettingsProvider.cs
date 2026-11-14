#if UNITY_EDITOR


using DragonResonance.Editor.Building;
using UnityEditor;
using UnityEngine;

#if ENABLE_LOCALIZER
using DragonResonance.Databases;
using DragonResonance.Editor.Drawers;
using DragonResonance.Extensions;
using DragonResonance.Localizer;
using UnityEngine.UIElements;
#endif


namespace DragonResonance.Editor.Settings
{
#if ENABLE_LOCALIZER
	public class LocalizerSettingsProvider : AScriptableSettingsProvider<LocalizerSettings>
#else
	public class LocalizerSettingsProvider : AScriptableSettingsProvider
#endif
	{
		private const string SettingsPath = "Project/Praenaris/Localizer";
		private const string BuildDefinition = "ENABLE_LOCALIZER";
		private const float SeparatorHeight = 1f;
		private const float DataRowHeight = 24f;
		private const float DataButtonWidth = 64f;
		private const int DataTitleFontSize = 16;
		private const float DataAreaHeight = 400f;

		private static readonly Color SeparatorColor = new(0.5f, 0.5f, 0.5f, 0.5f);
		private static readonly GUIContent DataLabel = new("Data");
		private static readonly GUIContent RefreshLabel = new("Refresh");
		private static readonly GUIContent UpdateLabel = new("Update");
		private static GUIStyle _dataTitleStyle_internal = null;	// Caching only, use the property instead

		#if ENABLE_LOCALIZER
		private readonly LocalizationSheetView _sheetView = new();
		#endif
		private bool _isBusy = false;


		#region Constructors

			[SettingsProvider]
			public static SettingsProvider Create() => new LocalizerSettingsProvider(SettingsPath, SettingsScope.Project);

			public LocalizerSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

		#endregion


		#region Events

		#if ENABLE_LOCALIZER
			public override void OnActivate(string searchContext, VisualElement rootElement)
			{
				base.OnActivate(searchContext, rootElement);
				HeaderedSheet<string> sheet = Localizer.Localizer.DataSheet;
				if ((this.Settings != null) && ((sheet?.Data == null) || sheet.Data.IsEmpty()))	// Emptied by domain reloads, the Localizer itself only starts in Play Mode
					RefreshData();
			}
		#endif

		#endregion


		#region Inheritables

			protected override void OnBeforeGUI(string searchContext)
			{
				#if ENABLE_LOCALIZER
					if (!EditorGUILayout.Toggle("Enabled", true))
						BuildDefines.SetDefinitionState(BuildDefinition, false);
				#else
					if (EditorGUILayout.Toggle("Enabled", false))
						BuildDefines.SetDefinitionState(BuildDefinition, true);
				#endif
			}

			protected override void OnAfterGUI(string searchContext)
			{
				EditorGUILayout.Space(SmallPadding);
				EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, SeparatorHeight), SeparatorColor);
				EditorGUILayout.Space(SmallPadding);

				#if ENABLE_LOCALIZER
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label(DataLabel, DataTitleStyle, GUILayout.Height(DataRowHeight));
					GUILayout.FlexibleSpace();

					EditorGUI.BeginDisabledGroup(_isBusy);
					{
						if (GUILayout.Button(RefreshLabel, GUILayout.Width(DataButtonWidth), GUILayout.Height(DataRowHeight)))
							RefreshData();
						#if ENABLE_UNITYWEBREQUEST
						if (GUILayout.Button(UpdateLabel, GUILayout.Width(DataButtonWidth), GUILayout.Height(DataRowHeight)))
							UpdateData();
						#endif
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space(SmallPadding);
				HeaderedSheet<string> sheet = Localizer.Localizer.DataSheet;
				if ((sheet?.Data == null) || sheet.Data.IsEmpty())
					EditorGUILayout.LabelField("No data, Refresh to load it", EditorStyles.centeredGreyMiniLabel);
				else
					_sheetView.OnGUI(GUILayoutUtility.GetRect(0f, DataAreaHeight, GUILayout.ExpandWidth(true)), sheet);
				#endif
			}

		#endregion


		#region Privates

		#if ENABLE_LOCALIZER
			private async void RefreshData()
			{
				_isBusy = true;
				try {
					Localizer.Localizer.Settings = this.Settings;
					await Localizer.Localizer.Reload();
				}
				finally {
					_isBusy = false;
					Repaint();
				}
			}

			#if ENABLE_UNITYWEBREQUEST
			private async void UpdateData()
			{
				_isBusy = true;
				try {
					Localizer.Localizer.Settings = this.Settings;
					await Localizer.Localizer.UpdateOnlineData();
				}
				finally {
					_isBusy = false;
					Repaint();
				}
			}
			#endif
		#endif

		#endregion


		#region Properties

			private static GUIStyle DataTitleStyle => (_dataTitleStyle_internal ??= new GUIStyle(EditorStyles.boldLabel) {
				fontSize = DataTitleFontSize,
				alignment = TextAnchor.MiddleLeft,
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