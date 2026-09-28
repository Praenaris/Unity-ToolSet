#if UNITY_EDITOR


using DragonResonance.Editor.Building;
using UnityEditor;
using UnityEngine;

#if ENABLE_SAVEDATA
using DragonResonance.Logging;
using Praenaris.Savedata;
using Tabernero.SimpleJSON;
#endif


namespace DragonResonance.Editor.Settings
{
#if ENABLE_SAVEDATA
	public partial class SavedataSettingsProvider : AScriptableSettingsProvider<SavedataSettings>
#else
	public partial class SavedataSettingsProvider : AScriptableSettingsProvider
#endif
	{
		private const string SettingsPath = "Project/Praenaris/Savedata";
		private const string BuildDefinition = "ENABLE_SAVEDATA";
		private const float SeparatorHeight = 1f;
		private const float SlotRowHeight = 24f;
		private const float SlotArrowWidth = 24f;
		private const float FileButtonWidth = 64f;
		private const int SlotFontSize = 16;
		private const float DataAreaHeight = 400f;

		private static readonly Color SeparatorColor = new(0.5f, 0.5f, 0.5f, 0.5f);
		private static readonly GUIContent PreviousSlotLabel = new("◀");
		private static readonly GUIContent NextSlotLabel = new("▶");
		private static readonly GUIContent LoadLabel = new("Load");
		private static readonly GUIContent SaveLabel = new("Save");
		private static GUIStyle _slotStyle_internal = null;	// Caching only, use the property instead
		private static GUIStyle _dataStyle_internal = null;	// Caching only, use the property instead

		private string _dataText = string.Empty;
		private Vector2 _dataScroll = Vector2.zero;


		#region Constructors

			[SettingsProvider]
			public static SettingsProvider Create() => new SavedataSettingsProvider(SettingsPath, SettingsScope.Project);

			public SavedataSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

		#endregion


		#region Inheritables

			protected override void OnBeforeGUI(string searchContext)
			{
				#if ENABLE_SAVEDATA
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

				#if ENABLE_SAVEDATA
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Button(PreviousSlotLabel, GUILayout.Width(SlotArrowWidth), GUILayout.Height(SlotRowHeight));	// TODO
					GUILayout.Label($"Slot {Savedata.CurrentSlot}", SlotStyle, GUILayout.Height(SlotRowHeight));
					GUILayout.Button(NextSlotLabel, GUILayout.Width(SlotArrowWidth), GUILayout.Height(SlotRowHeight));	// TODO

					GUILayout.FlexibleSpace();

					if (GUILayout.Button(LoadLabel, GUILayout.Width(FileButtonWidth), GUILayout.Height(SlotRowHeight))) {
						LoadData();
					}

					EditorGUI.BeginDisabledGroup(!Savedata.IsReady);
					{
						if (GUILayout.Button(SaveLabel, GUILayout.Width(FileButtonWidth), GUILayout.Height(SlotRowHeight)))
							SaveData();
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space(SmallPadding);
				_dataScroll = EditorGUILayout.BeginScrollView(_dataScroll, GUILayout.Height(DataAreaHeight));
				{
					_dataText = EditorGUILayout.TextArea(_dataText, DataStyle, GUILayout.ExpandHeight(true));
				}
				EditorGUILayout.EndScrollView();
				#endif
			}

		#endregion


		#region Privates

		#if ENABLE_SAVEDATA
			private async void LoadData()
			{
				Savedata.Settings = this.Settings;
				await Savedata.Load();

				GUIUtility.keyboardControl = 0;	// Otherwise a focused text area keeps showing its old buffer
				_dataText = (Savedata.Data != null) ? Savedata.Data.ToString(false) : string.Empty;
				Repaint();
			}

			private async void SaveData()
			{
				JSONNode data = JSONNode.Parse(_dataText);
				if (data is not JSONObject) {
					Log.Error("The savedata text is not a JSON object, nothing was saved");
					return;
				}

				Savedata.Data = data;
				await Savedata.Save();
			}
		#endif

		#endregion


		#region Properties

			private static GUIStyle SlotStyle => (_slotStyle_internal ??= new GUIStyle(EditorStyles.boldLabel) {
				fontSize = SlotFontSize,
				alignment = TextAnchor.MiddleCenter,
			});

			private static GUIStyle DataStyle => (_dataStyle_internal ??= new GUIStyle(EditorStyles.textArea) {
				font = EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf") as Font,
				wordWrap = false,
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