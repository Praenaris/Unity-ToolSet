#if UNITY_EDITOR && ENABLE_INTEGRATION


using Praenaris.Integration;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Praenaris.Editor.Drawers
{
	[CustomPropertyDrawer(typeof(Achievement))]
	public class AchievementDrawer : PropertyDrawer
	{
		private const float HORIZONTAL_PADDING = 8f;
		private const float VERTICAL_PADDING = 4f;
		private const float SPRITES_SPACING = 4f;
		private const float COLUMN_SPACING = 8f;
		private const float ICON_SIZE = 60f;
		private const float TOGGLE_WIDTH = 16f;
		private const float ACTION_WIDTH = ICON_SIZE;	// Each button right under its icon
		private const float VALUE_WIDTH = 48f;
		private const float SET_WIDTH = 40f;
		private const int FIXED_ROWS = 2;	// Name and progress rows, the store IDs come after them
		private const string MISSING_GUID_TEXT = "No GUID yet, update the references to assign one";

		private static readonly (string AchievementName, string StatName, GUIContent Label)[] StoreIdFields = {	// Add any new store ID fields here
			#if (STEAMWORKS_INTEGRATION && !DISABLESTEAMWORKS)
			(nameof(Achievement.SteamworksAchievementId), nameof(Achievement.SteamworksStatId), new GUIContent("Steamworks")),
			#endif
		};
		private static readonly GUIContent NameLabel = new("Name");
		private static readonly GUIContent AchievementIdLabel = new("Ach. ID");
		private static readonly GUIContent StatIdLabel = new("Stat. ID");
		private static readonly GUIContent ProgressLabel = new("Progress");
		private static readonly GUIContent ProgressiveToggleLabel = new(string.Empty);
		private static readonly GUIContent StartingLabel = new("From");
		private static readonly GUIContent TargetLabel = new("To");
		private static readonly GUIContent TestLabel = new("Test");
		private static readonly GUIContent SetLabel = new("Set");
		private static readonly GUIContent AchieveLabel = new("Achieve");
		private static readonly GUIContent UnachieveLabel = new("Unachieve");
		private static readonly GUIContent CheckStatusLabel = new("Check Status");
		private static readonly Dictionary<string, float> TestValues = new();	// Testing slider values, by property path
		private static float _fieldLabelWidth = -1f;
		private static GUIStyle _guidStyle_internal = null;	// Caching only, use the property instead


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			{
				SerializedProperty nameProperty = property.FindPropertyRelative(nameof(Achievement.Name));
				SerializedProperty achievedIconProperty = property.FindPropertyRelative(nameof(Achievement.AchievedIcon));
				SerializedProperty unachievedIconProperty = property.FindPropertyRelative(nameof(Achievement.UnachievedIcon));
				SerializedProperty guidProperty = property.FindPropertyRelative(nameof(Achievement.ChannelGuid));

				string title = nameProperty.stringValue;
				string guid = guidProperty.stringValue;

				position = EditorGUI.IndentedRect(position);
				Rect content = new(position.x + HORIZONTAL_PADDING, position.y + VERTICAL_PADDING, position.width - (2f * HORIZONTAL_PADDING), position.height - (2f * VERTICAL_PADDING));
				float lineHeight = EditorGUIUtility.singleLineHeight;
				float rowStep = lineHeight + EditorGUIUtility.standardVerticalSpacing;
				float bodyHeight = GetBodyHeight();
				float bodyY = content.y + rowStep;
				float footerY = bodyY + bodyHeight + EditorGUIUtility.standardVerticalSpacing;

				Rect titleRect = new(content.x, content.y, content.width * 0.6f, lineHeight);
				Rect guidRect = new(titleRect.xMax + SPRITES_SPACING, content.y, content.xMax - titleRect.xMax - SPRITES_SPACING, lineHeight);

				Rect achievedIconRect = new(content.x, bodyY, ICON_SIZE, ICON_SIZE);
				Rect unachievedIconRect = new(achievedIconRect.xMax + SPRITES_SPACING, bodyY, ICON_SIZE, ICON_SIZE);

				float fieldLabelWidth = GetFieldLabelWidth();
				float labelsX = unachievedIconRect.xMax + COLUMN_SPACING;
				float valuesX = labelsX + fieldLabelWidth + SPRITES_SPACING;
				float valuesWidth = content.xMax - valuesX;

				int indentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				{
					UnityEngine.GUI.Box(position, GUIContent.none, EditorStyles.helpBox);

					// Header
					EditorGUI.LabelField(titleRect, string.IsNullOrEmpty(title) ? label.text : title, EditorStyles.boldLabel);
					EditorGUI.SelectableLabel(guidRect, string.IsNullOrEmpty(guid) ? MISSING_GUID_TEXT : guid, GuidStyle);

					// Icons
					EditorGUI.ObjectField(achievedIconRect, achievedIconProperty, typeof(Sprite), GUIContent.none);
					EditorGUI.ObjectField(unachievedIconRect, unachievedIconProperty, typeof(Sprite), GUIContent.none);

					// Name row
					EditorGUI.LabelField(new Rect(labelsX, bodyY, fieldLabelWidth, lineHeight), NameLabel, EditorStyles.miniLabel);
					EditorGUI.PropertyField(new Rect(valuesX, bodyY, valuesWidth, lineHeight), nameProperty, GUIContent.none);

					// Progress row
					float progressY = bodyY + rowStep;
					EditorGUI.LabelField(new Rect(labelsX, progressY, fieldLabelWidth, lineHeight), ProgressLabel, EditorStyles.miniLabel);
					DrawProgressSettings(new Rect(valuesX, progressY, valuesWidth, lineHeight), property);

					// Store IDs rows, achievement and stat
					bool isProgressionStat = property.FindPropertyRelative(nameof(Achievement.IsProgressionStat)).boolValue;
					float achievementIdLabelWidth = EditorStyles.miniLabel.CalcSize(AchievementIdLabel).x;
					float statIdLabelWidth = EditorStyles.miniLabel.CalcSize(StatIdLabel).x;
					float idWidth = (valuesWidth - achievementIdLabelWidth - statIdLabelWidth - COLUMN_SPACING - (2f * SPRITES_SPACING)) * 0.5f;
					for (int fieldIndex = 0; fieldIndex < StoreIdFields.Length; fieldIndex++) {
						float rowY = bodyY + ((FIXED_ROWS + fieldIndex) * rowStep);
						Rect achievementIdLabelRect = new(valuesX, rowY, achievementIdLabelWidth, lineHeight);
						Rect achievementIdRect = new(achievementIdLabelRect.xMax + SPRITES_SPACING, rowY, idWidth, lineHeight);
						Rect statIdLabelRect = new(achievementIdRect.xMax + COLUMN_SPACING, rowY, statIdLabelWidth, lineHeight);
						Rect statIdRect = new(statIdLabelRect.xMax + SPRITES_SPACING, rowY, content.xMax - statIdLabelRect.xMax - SPRITES_SPACING, lineHeight);

						EditorGUI.LabelField(new Rect(labelsX, rowY, fieldLabelWidth, lineHeight), StoreIdFields[fieldIndex].Label, EditorStyles.miniLabel);
						EditorGUI.LabelField(achievementIdLabelRect, AchievementIdLabel, EditorStyles.miniLabel);
						EditorGUI.PropertyField(achievementIdRect, property.FindPropertyRelative(StoreIdFields[fieldIndex].AchievementName), GUIContent.none);
						EditorGUI.BeginDisabledGroup(!isProgressionStat);
						{
							EditorGUI.LabelField(statIdLabelRect, StatIdLabel, EditorStyles.miniLabel);
							EditorGUI.PropertyField(statIdRect, property.FindPropertyRelative(StoreIdFields[fieldIndex].StatName), GUIContent.none);
						}
						EditorGUI.EndDisabledGroup();
					}

					// Footer
					DrawActions(new Rect(content.x, footerY, content.width, lineHeight), property);

					EditorGUI.BeginDisabledGroup(!Application.isPlaying);	// The platform APIs are only initialized in Play Mode
					{
						Rect checkStatusRect = new(content.x, footerY + rowStep, (2f * ACTION_WIDTH) + SPRITES_SPACING, lineHeight);	// Under both action buttons
						if (UnityEngine.GUI.Button(checkStatusRect, CheckStatusLabel, EditorStyles.miniButton))
							Debug.Log(((Achievement)property.boxedValue).ToString());
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUI.indentLevel = indentLevel;
			}
			EditorGUI.EndProperty();
		}


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			(2f * VERTICAL_PADDING) + (3f * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing)) + GetBodyHeight();	// Header, body and two footer rows

		private static void DrawProgressSettings(Rect rect, SerializedProperty property)
		{
			SerializedProperty isProgressionStatProperty = property.FindPropertyRelative(nameof(Achievement.IsProgressionStat));
			SerializedProperty startingValueProperty = property.FindPropertyRelative(nameof(Achievement.StartingValue));
			SerializedProperty targetValueProperty = property.FindPropertyRelative(nameof(Achievement.TargetValue));

			float startingLabelWidth = EditorStyles.miniLabel.CalcSize(StartingLabel).x;
			float targetLabelWidth = EditorStyles.miniLabel.CalcSize(TargetLabel).x;
			float valueWidth = (rect.width - TOGGLE_WIDTH - startingLabelWidth - targetLabelWidth - COLUMN_SPACING - (4f * SPRITES_SPACING)) * 0.5f;

			Rect toggleRect = new(rect.x, rect.y, TOGGLE_WIDTH, rect.height);
			Rect startingLabelRect = new(toggleRect.xMax + SPRITES_SPACING, rect.y, startingLabelWidth, rect.height);
			Rect startingValueRect = new(startingLabelRect.xMax + SPRITES_SPACING, rect.y, valueWidth, rect.height);
			Rect targetLabelRect = new(startingValueRect.xMax + COLUMN_SPACING, rect.y, targetLabelWidth, rect.height);
			Rect targetValueRect = new(targetLabelRect.xMax + SPRITES_SPACING, rect.y, rect.xMax - targetLabelRect.xMax - SPRITES_SPACING, rect.height);

			EditorGUI.PropertyField(toggleRect, isProgressionStatProperty, ProgressiveToggleLabel);

			EditorGUI.BeginDisabledGroup(!isProgressionStatProperty.boolValue);
			{
				EditorGUI.LabelField(startingLabelRect, StartingLabel, EditorStyles.miniLabel);
				EditorGUI.PropertyField(startingValueRect, startingValueProperty, GUIContent.none);
				EditorGUI.LabelField(targetLabelRect, TargetLabel, EditorStyles.miniLabel);
				EditorGUI.PropertyField(targetValueRect, targetValueProperty, GUIContent.none);
			}
			EditorGUI.EndDisabledGroup();
		}

		private static void DrawActions(Rect rect, SerializedProperty property)
		{
			SerializedProperty isProgressionStatProperty = property.FindPropertyRelative(nameof(Achievement.IsProgressionStat));
			SerializedProperty startingValueProperty = property.FindPropertyRelative(nameof(Achievement.StartingValue));
			SerializedProperty targetValueProperty = property.FindPropertyRelative(nameof(Achievement.TargetValue));

			Rect achieveRect = new(rect.x, rect.y, ACTION_WIDTH, rect.height);
			Rect unachieveRect = new(achieveRect.xMax + SPRITES_SPACING, rect.y, ACTION_WIDTH, rect.height);
			Rect testLabelRect = new(unachieveRect.xMax + (2f * COLUMN_SPACING), rect.y, EditorStyles.miniLabel.CalcSize(TestLabel).x, rect.height);
			Rect setRect = new(rect.xMax - SET_WIDTH, rect.y, SET_WIDTH, rect.height);
			Rect testValueRect = new(setRect.x - SPRITES_SPACING - VALUE_WIDTH, rect.y, VALUE_WIDTH, rect.height);
			Rect sliderRect = new(testLabelRect.xMax + SPRITES_SPACING, rect.y, testValueRect.x - testLabelRect.xMax - COLUMN_SPACING, rect.height);

			bool isPlaying = Application.isPlaying;	// The platform APIs are only initialized in Play Mode

			EditorGUI.BeginDisabledGroup(!isPlaying);
			{
				if (UnityEngine.GUI.Button(achieveRect, AchieveLabel, EditorStyles.miniButtonLeft))
					((Achievement)property.boxedValue).Achieve();
				if (UnityEngine.GUI.Button(unachieveRect, UnachieveLabel, EditorStyles.miniButtonRight))
					((Achievement)property.boxedValue).Unachieve();
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!isProgressionStatProperty.boolValue);
			{
				float startingValue = startingValueProperty.floatValue;
				float targetValue = targetValueProperty.floatValue;
				float minValue = Mathf.Min(startingValue, targetValue);
				float maxValue = Mathf.Max(startingValue, targetValue);

				string key = property.propertyPath;
				float testValue = TestValues.TryGetValue(key, out float storedValue) ? Mathf.Clamp(storedValue, minValue, maxValue) : startingValue;	// Defaults at the starting value

				EditorGUI.LabelField(testLabelRect, TestLabel, EditorStyles.miniLabel);
				testValue = UnityEngine.GUI.HorizontalSlider(sliderRect, testValue, minValue, maxValue);	// Bar only, EditorGUI.Slider hides it when narrow
				TestValues[key] = Mathf.Clamp(EditorGUI.FloatField(testValueRect, testValue), minValue, maxValue);

				EditorGUI.BeginDisabledGroup(!isPlaying);
				{
					if (UnityEngine.GUI.Button(setRect, SetLabel, EditorStyles.miniButton)) {
						Achievement achievement = (Achievement)property.boxedValue;
						achievement.Unachieve(false);	// Testing only, relocks it so the stat can go down and unlock again
						achievement.Progress(TestValues[key]);
					}
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUI.EndDisabledGroup();
		}

		private static float GetBodyHeight() => Mathf.Max(GetRowsHeight(FIXED_ROWS + StoreIdFields.Length), ICON_SIZE);

		private static float GetRowsHeight(int rows) =>
			(rows * EditorGUIUtility.singleLineHeight) + ((rows - 1) * EditorGUIUtility.standardVerticalSpacing);

		private static float GetFieldLabelWidth()
		{
			if (_fieldLabelWidth >= 0f) return _fieldLabelWidth;

			_fieldLabelWidth = Mathf.Max(EditorStyles.miniLabel.CalcSize(NameLabel).x, EditorStyles.miniLabel.CalcSize(ProgressLabel).x);
			foreach ((string _, string _, GUIContent fieldLabel) in StoreIdFields)
				_fieldLabelWidth = Mathf.Max(_fieldLabelWidth, EditorStyles.miniLabel.CalcSize(fieldLabel).x);
			return _fieldLabelWidth;
		}


		private static GUIStyle GuidStyle => (_guidStyle_internal ??= new GUIStyle(EditorStyles.miniLabel) {
			alignment = TextAnchor.MiddleRight,
		});
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
