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
		private const float CHECK_STATUS_WIDTH = (2f * ICON_SIZE) + SPRITES_SPACING;
		private const float TESTING_ROW_HEIGHT = 24f;	// Slightly taller than a line, they are the important controls
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
		private static readonly GUIContent StartingLabel = new("From");
		private static readonly GUIContent TargetLabel = new("To");
		private static readonly GUIContent TestLabel = new("Test");
		private static readonly GUIContent SetLabel = new("Set");
		private static readonly GUIContent AchieveLabel = new("Achieve");
		private static readonly GUIContent UnachieveLabel = new("Unachieve");
		private static readonly GUIContent CheckStatusLabel = new("Check Status");
		private static readonly Color AchieveColor = new(0.6f, 0.85f, 1f);
		private static readonly Color UnachieveColor = new(1f, 0.65f, 0.65f);
		private static readonly Color TestingColor = new(1.6f, 1.6f, 1.6f);	// The slider, Set and Check Status, above 1 brightens them towards white
		private static readonly Dictionary<string, float> TestValues = new();	// Testing slider values, by property path
		private static float _fieldLabelWidth_internal = -1f;	// Caching only, use the property instead
		private static GUIStyle _guidStyle_internal = null;	// Caching only, use the property instead
		private static GUIStyle _testingButtonStyle_internal = null;	// Caching only, use the property instead


		#region Publics

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				EditorGUI.BeginProperty(position, label, property);
				{
					position = EditorGUI.IndentedRect(position);
					Rect content = new(position.x + HORIZONTAL_PADDING, position.y + VERTICAL_PADDING, position.width - (2f * HORIZONTAL_PADDING), position.height - (2f * VERTICAL_PADDING));
					float lineHeight = EditorGUIUtility.singleLineHeight;
					float rowStep = lineHeight + EditorGUIUtility.standardVerticalSpacing;
					float bodyY = content.y + rowStep;
					float footerY = bodyY + GetBodyHeight() + EditorGUIUtility.standardVerticalSpacing;

					// Left column, the icons, and right column, the fields and testing controls
					Rect achievedIconRect = new(content.x, bodyY, ICON_SIZE, ICON_SIZE);
					Rect unachievedIconRect = new(achievedIconRect.xMax + SPRITES_SPACING, bodyY, ICON_SIZE, ICON_SIZE);
					float labelsX = unachievedIconRect.xMax + COLUMN_SPACING;
					float valuesX = labelsX + FieldLabelWidth + SPRITES_SPACING;
					float valuesWidth = content.xMax - valuesX;
					bool isProgressionStat = property.FindPropertyRelative(nameof(Achievement.IsProgressionStat)).boolValue;

					int indentLevel = EditorGUI.indentLevel;
					EditorGUI.indentLevel = 0;
					{
						UnityEngine.GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
						DrawHeader(new Rect(content.x, content.y, content.width, lineHeight), property, label);
						EditorGUI.ObjectField(achievedIconRect, property.FindPropertyRelative(nameof(Achievement.AchievedIcon)), typeof(Sprite), GUIContent.none);
						EditorGUI.ObjectField(unachievedIconRect, property.FindPropertyRelative(nameof(Achievement.UnachievedIcon)), typeof(Sprite), GUIContent.none);

						DrawFieldLabel(labelsX, bodyY, NameLabel);
						EditorGUI.PropertyField(new Rect(valuesX, bodyY, valuesWidth, lineHeight), property.FindPropertyRelative(nameof(Achievement.Name)), GUIContent.none);

						DrawFieldLabel(labelsX, bodyY + rowStep, ProgressLabel);
						DrawProgressSettings(new Rect(valuesX, bodyY + rowStep, valuesWidth, lineHeight), property, isProgressionStat);

						for (int fieldIndex = 0; fieldIndex < StoreIdFields.Length; fieldIndex++) {
							float rowY = bodyY + ((FIXED_ROWS + fieldIndex) * rowStep);
							DrawFieldLabel(labelsX, rowY, StoreIdFields[fieldIndex].Label);
							DrawStoreIds(new Rect(valuesX, rowY, valuesWidth, lineHeight), property, StoreIdFields[fieldIndex], isProgressionStat);
						}

						float testingWidth = content.xMax - labelsX;
						DrawProgressTesting(new Rect(labelsX, footerY, testingWidth, TESTING_ROW_HEIGHT), property, isProgressionStat);
						DrawActions(new Rect(labelsX, footerY + TESTING_ROW_HEIGHT + EditorGUIUtility.standardVerticalSpacing, testingWidth, TESTING_ROW_HEIGHT), property);
					}
					EditorGUI.indentLevel = indentLevel;
				}
				EditorGUI.EndProperty();
			}

			public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>	// Header, body and the two testing rows
				(2f * VERTICAL_PADDING) + EditorGUIUtility.singleLineHeight + GetBodyHeight() + (2f * TESTING_ROW_HEIGHT) + (3f * EditorGUIUtility.standardVerticalSpacing);

		#endregion


		#region Privates - Fields

			private static void DrawHeader(Rect rect, SerializedProperty property, GUIContent label)
			{
				string title = property.FindPropertyRelative(nameof(Achievement.Name)).stringValue;
				string guid = property.FindPropertyRelative(nameof(Achievement.ChannelGuid)).stringValue;
				Rect titleRect = new(rect.x, rect.y, rect.width * 0.6f, rect.height);
				Rect guidRect = new(titleRect.xMax + SPRITES_SPACING, rect.y, rect.xMax - titleRect.xMax - SPRITES_SPACING, rect.height);

				EditorGUI.LabelField(titleRect, string.IsNullOrEmpty(title) ? label.text : title, EditorStyles.boldLabel);
				EditorGUI.SelectableLabel(guidRect, string.IsNullOrEmpty(guid) ? MISSING_GUID_TEXT : guid, GuidStyle);
			}

			private static void DrawFieldLabel(float x, float y, GUIContent label) =>
				EditorGUI.LabelField(new Rect(x, y, FieldLabelWidth, EditorGUIUtility.singleLineHeight), label, EditorStyles.miniLabel);

			private static void DrawProgressSettings(Rect rect, SerializedProperty property, bool isProgressionStat)
			{
				float startingLabelWidth = EditorStyles.miniLabel.CalcSize(StartingLabel).x;
				float targetLabelWidth = EditorStyles.miniLabel.CalcSize(TargetLabel).x;
				float valueWidth = (rect.width - TOGGLE_WIDTH - startingLabelWidth - targetLabelWidth - COLUMN_SPACING - (4f * SPRITES_SPACING)) * 0.5f;

				Rect toggleRect = new(rect.x, rect.y, TOGGLE_WIDTH, rect.height);
				Rect startingLabelRect = new(toggleRect.xMax + SPRITES_SPACING, rect.y, startingLabelWidth, rect.height);
				Rect startingValueRect = new(startingLabelRect.xMax + SPRITES_SPACING, rect.y, valueWidth, rect.height);
				Rect targetLabelRect = new(startingValueRect.xMax + COLUMN_SPACING, rect.y, targetLabelWidth, rect.height);
				Rect targetValueRect = new(targetLabelRect.xMax + SPRITES_SPACING, rect.y, rect.xMax - targetLabelRect.xMax - SPRITES_SPACING, rect.height);

				EditorGUI.PropertyField(toggleRect, property.FindPropertyRelative(nameof(Achievement.IsProgressionStat)), GUIContent.none);

				EditorGUI.BeginDisabledGroup(!isProgressionStat);
				{
					EditorGUI.LabelField(startingLabelRect, StartingLabel, EditorStyles.miniLabel);
					EditorGUI.PropertyField(startingValueRect, property.FindPropertyRelative(nameof(Achievement.StartingValue)), GUIContent.none);
					EditorGUI.LabelField(targetLabelRect, TargetLabel, EditorStyles.miniLabel);
					EditorGUI.PropertyField(targetValueRect, property.FindPropertyRelative(nameof(Achievement.TargetValue)), GUIContent.none);
				}
				EditorGUI.EndDisabledGroup();
			}

			private static void DrawStoreIds(Rect rect, SerializedProperty property, (string AchievementName, string StatName, GUIContent Label) storeIdField, bool isProgressionStat)
			{
				float achievementIdLabelWidth = EditorStyles.miniLabel.CalcSize(AchievementIdLabel).x;
				float statIdLabelWidth = EditorStyles.miniLabel.CalcSize(StatIdLabel).x;
				float idWidth = (rect.width - achievementIdLabelWidth - statIdLabelWidth - COLUMN_SPACING - (2f * SPRITES_SPACING)) * 0.5f;

				Rect achievementIdLabelRect = new(rect.x, rect.y, achievementIdLabelWidth, rect.height);
				Rect achievementIdRect = new(achievementIdLabelRect.xMax + SPRITES_SPACING, rect.y, idWidth, rect.height);
				Rect statIdLabelRect = new(achievementIdRect.xMax + COLUMN_SPACING, rect.y, statIdLabelWidth, rect.height);
				Rect statIdRect = new(statIdLabelRect.xMax + SPRITES_SPACING, rect.y, rect.xMax - statIdLabelRect.xMax - SPRITES_SPACING, rect.height);

				EditorGUI.LabelField(achievementIdLabelRect, AchievementIdLabel, EditorStyles.miniLabel);
				EditorGUI.PropertyField(achievementIdRect, property.FindPropertyRelative(storeIdField.AchievementName), GUIContent.none);

				EditorGUI.BeginDisabledGroup(!isProgressionStat);
				{
					EditorGUI.LabelField(statIdLabelRect, StatIdLabel, EditorStyles.miniLabel);
					EditorGUI.PropertyField(statIdRect, property.FindPropertyRelative(storeIdField.StatName), GUIContent.none);
				}
				EditorGUI.EndDisabledGroup();
			}

		#endregion


		#region Privates - Testing

			private static void DrawProgressTesting(Rect rect, SerializedProperty property, bool isProgressionStat)
			{
				float lineHeight = EditorGUIUtility.singleLineHeight;
				float lineY = rect.y + ((rect.height - lineHeight) * 0.5f);	// The single-line controls, centered in the taller row
				Rect checkStatusRect = new(rect.xMax - CHECK_STATUS_WIDTH, rect.y, CHECK_STATUS_WIDTH, rect.height);
				Rect setRect = new(checkStatusRect.x - COLUMN_SPACING - SET_WIDTH, rect.y, SET_WIDTH, rect.height);
				Rect testValueRect = new(setRect.x - SPRITES_SPACING - VALUE_WIDTH, lineY, VALUE_WIDTH, lineHeight);
				Rect testLabelRect = new(rect.x, lineY, EditorStyles.miniLabel.CalcSize(TestLabel).x, lineHeight);
				Rect sliderRect = new(testLabelRect.xMax + SPRITES_SPACING, lineY, testValueRect.x - testLabelRect.xMax - COLUMN_SPACING, lineHeight);

				float startingValue = property.FindPropertyRelative(nameof(Achievement.StartingValue)).floatValue;
				float targetValue = property.FindPropertyRelative(nameof(Achievement.TargetValue)).floatValue;
				float minValue = Mathf.Min(startingValue, targetValue);
				float maxValue = Mathf.Max(startingValue, targetValue);
				string key = property.propertyPath;
				float testValue = TestValues.TryGetValue(key, out float storedValue) ? Mathf.Clamp(storedValue, minValue, maxValue) : startingValue;	// Defaults at the starting value
				bool isPlaying = Application.isPlaying;	// The platform APIs are only initialized in Play Mode

				EditorGUI.BeginDisabledGroup(!isProgressionStat);
				{
					EditorGUI.LabelField(testLabelRect, TestLabel, EditorStyles.miniLabel);
					Color backgroundColor = UnityEngine.GUI.backgroundColor;
					UnityEngine.GUI.backgroundColor = TestingColor;
					testValue = UnityEngine.GUI.HorizontalSlider(sliderRect, testValue, minValue, maxValue);	// Bar only, EditorGUI.Slider hides it when narrow
					UnityEngine.GUI.backgroundColor = backgroundColor;
					TestValues[key] = testValue = Mathf.Clamp(EditorGUI.FloatField(testValueRect, testValue), minValue, maxValue);
				}
				EditorGUI.EndDisabledGroup();

				EditorGUI.BeginDisabledGroup(!isProgressionStat || !isPlaying);
				{
					if (DrawTintedButton(setRect, SetLabel, TestingColor)) {
						Achievement achievement = GetAchievement(property);
						achievement.Unachieve(false);	// Testing only, relocks it so the stat can go down and unlock again
						achievement.Progress(testValue);
					}
				}
				EditorGUI.EndDisabledGroup();

				EditorGUI.BeginDisabledGroup(!isPlaying);
				{
					if (DrawTintedButton(checkStatusRect, CheckStatusLabel, TestingColor))
						Debug.Log(GetAchievement(property).ToString());
				}
				EditorGUI.EndDisabledGroup();
			}

			private static void DrawActions(Rect rect, SerializedProperty property)
			{
				float buttonWidth = (rect.width - SPRITES_SPACING) * 0.5f;	// Both share the whole row
				Rect achieveRect = new(rect.x, rect.y, buttonWidth, rect.height);
				Rect unachieveRect = new(achieveRect.xMax + SPRITES_SPACING, rect.y, buttonWidth, rect.height);

				EditorGUI.BeginDisabledGroup(!Application.isPlaying);	// The platform APIs are only initialized in Play Mode
				{
					if (DrawTintedButton(achieveRect, AchieveLabel, AchieveColor))
						GetAchievement(property).Achieve();
					if (DrawTintedButton(unachieveRect, UnachieveLabel, UnachieveColor))
						GetAchievement(property).Unachieve();
				}
				EditorGUI.EndDisabledGroup();
			}

			private static bool DrawTintedButton(Rect rect, GUIContent label, Color color)
			{
				Color backgroundColor = UnityEngine.GUI.backgroundColor;
				UnityEngine.GUI.backgroundColor = color;
				bool clicked = UnityEngine.GUI.Button(rect, label, TestingButtonStyle);
				UnityEngine.GUI.backgroundColor = backgroundColor;
				return clicked;
			}

		#endregion


		#region Privates - Layout

			private static Achievement GetAchievement(SerializedProperty property) => (Achievement)property.boxedValue;

			private static float GetBodyHeight() => Mathf.Max(GetRowsHeight(FIXED_ROWS + StoreIdFields.Length), ICON_SIZE);

			private static float GetRowsHeight(int rows) =>
				(rows * EditorGUIUtility.singleLineHeight) + ((rows - 1) * EditorGUIUtility.standardVerticalSpacing);

		#endregion


		#region Properties

			private static float FieldLabelWidth
			{
				get {
					if (_fieldLabelWidth_internal >= 0f) return _fieldLabelWidth_internal;

					_fieldLabelWidth_internal = Mathf.Max(EditorStyles.miniLabel.CalcSize(NameLabel).x, EditorStyles.miniLabel.CalcSize(ProgressLabel).x);
					foreach ((string _, string _, GUIContent fieldLabel) in StoreIdFields)
						_fieldLabelWidth_internal = Mathf.Max(_fieldLabelWidth_internal, EditorStyles.miniLabel.CalcSize(fieldLabel).x);
					return _fieldLabelWidth_internal;
				}
			}

			private static GUIStyle GuidStyle => (_guidStyle_internal ??= new GUIStyle(EditorStyles.miniLabel) {
				alignment = TextAnchor.MiddleRight,
			});

			private static GUIStyle TestingButtonStyle => (_testingButtonStyle_internal ??= new GUIStyle(EditorStyles.miniButton) {
				fixedHeight = 0f,	// The mini button has a fixed height, it would ignore the taller testing rows
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