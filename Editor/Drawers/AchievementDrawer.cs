#if UNITY_EDITOR && ENABLE_INTEGRATION


using Praenaris.Integration;
using UnityEditor;
using UnityEngine;


namespace Praenaris.Editor.Drawers
{
	[CustomPropertyDrawer(typeof(Achievement))]
	public class AchievementDrawer : PropertyDrawer
	{
		private const float SPACING = 2f;
		private const float COLUMN_SPACING = 6f;
		private const float MIN_ICON_SIZE = 40f;
		private const float TITLE_RATIO = 0.6f;
		private const float BUTTONS_RATIO = 0.2f;
		private const int BUTTON_ROWS = 2;	// Achieve and Unachieve buttons
		private const string MISSING_GUID_TEXT = "No GUID yet, update the references to assign one";

		private static readonly (string Name, GUIContent Label)[] Fields = {	// Add any new API key fields here
			(nameof(Achievement.Name), new GUIContent("Name")),
			#if STEAMWORKS_INTEGRATION
			(nameof(Achievement.SteamworksAchievementId), new GUIContent("Steamworks")),
			#endif
		};
		private static readonly GUIContent AchieveLabel = new("Achieve");
		private static readonly GUIContent UnachieveLabel = new("Unachieve");
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
				float lineHeight = EditorGUIUtility.singleLineHeight;
				float rowStep = lineHeight + EditorGUIUtility.standardVerticalSpacing;
				float bodyHeight = GetBodyHeight();
				float bodyY = position.y + rowStep;

				Rect titleRect = new(position.x, position.y, position.width * TITLE_RATIO, lineHeight);
				Rect guidRect = new(titleRect.xMax + SPACING, position.y, position.xMax - titleRect.xMax - SPACING, lineHeight);

				Rect achieveRect = new(position.x, bodyY, position.width * BUTTONS_RATIO, lineHeight);
				Rect unachieveRect = new(position.x, bodyY + rowStep, achieveRect.width, lineHeight);

				Rect unachievedIconRect = new(position.xMax - bodyHeight, bodyY, bodyHeight, bodyHeight);
				Rect achievedIconRect = new(unachievedIconRect.x - SPACING - bodyHeight, bodyY, bodyHeight, bodyHeight);

				float fieldsX = achieveRect.xMax + COLUMN_SPACING;
				float fieldsXMax = achievedIconRect.x - COLUMN_SPACING;
				float fieldLabelWidth = GetFieldLabelWidth();

				int indentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				{
					EditorGUI.LabelField(titleRect, string.IsNullOrEmpty(title) ? label.text : title, EditorStyles.boldLabel);
					EditorGUI.SelectableLabel(guidRect, string.IsNullOrEmpty(guid) ? MISSING_GUID_TEXT : guid, GuidStyle);

					EditorGUI.BeginDisabledGroup(!Application.isPlaying);	// The platform APIs are only initialized in Play Mode
					{
						if (UnityEngine.GUI.Button(achieveRect, AchieveLabel, EditorStyles.miniButton))
							((Achievement)property.boxedValue).Achieve();
						if (UnityEngine.GUI.Button(unachieveRect, UnachieveLabel, EditorStyles.miniButton))
							((Achievement)property.boxedValue).Unachieve();
					}
					EditorGUI.EndDisabledGroup();

					for (int fieldIndex = 0; fieldIndex < Fields.Length; fieldIndex++) {
						float rowY = bodyY + (fieldIndex * rowStep);
						Rect fieldLabelRect = new(fieldsX, rowY, fieldLabelWidth, lineHeight);
						Rect fieldRect = new(fieldLabelRect.xMax + SPACING, rowY, fieldsXMax - fieldLabelRect.xMax - SPACING, lineHeight);

						EditorGUI.LabelField(fieldLabelRect, Fields[fieldIndex].Label, EditorStyles.miniLabel);
						EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative(Fields[fieldIndex].Name), GUIContent.none);
					}

					EditorGUI.ObjectField(achievedIconRect, achievedIconProperty, typeof(Sprite), GUIContent.none);
					EditorGUI.ObjectField(unachievedIconRect, unachievedIconProperty, typeof(Sprite), GUIContent.none);
				}
				EditorGUI.indentLevel = indentLevel;
			}
			EditorGUI.EndProperty();
		}


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + GetBodyHeight();

		private static float GetBodyHeight() => Mathf.Max(GetRowsHeight(Fields.Length), GetRowsHeight(BUTTON_ROWS), MIN_ICON_SIZE);	// Also the icons size

		private static float GetRowsHeight(int rows) =>
			(rows * EditorGUIUtility.singleLineHeight) + ((rows - 1) * EditorGUIUtility.standardVerticalSpacing);

		private static float GetFieldLabelWidth()
		{
			if (_fieldLabelWidth >= 0f) return _fieldLabelWidth;

			foreach ((string _, GUIContent fieldLabel) in Fields)
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