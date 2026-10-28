#if UNITY_EDITOR && ENABLE_CURSORMAN


using Praenaris.Cursorman;
using UnityEditor;
using UnityEngine;


namespace Praenaris.Editor.Drawers
{
	[CustomPropertyDrawer(typeof(CursorPreset))]
	public class CursorPresetDrawer : PropertyDrawer
	{
		private const float SPACING = 4f;
		private const float COLUMN_SPACING = 8f;
		private const float ID_RATIO = 0.35f;
		private const float TOGGLE_WIDTH = 16f;
		private const float DELAY_WIDTH = 40f;

		private static readonly GUIContent AutoHideLabel = new("Auto-hide", "Hides the cursor after some seconds without pointer movement");
		private static readonly GUIContent SecondsLabel = new("s");


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			{
				SerializedProperty idProperty = property.FindPropertyRelative(nameof(CursorPreset.Id));
				SerializedProperty spriteProperty = property.FindPropertyRelative(nameof(CursorPreset.Sprite));
				SerializedProperty autoHideProperty = property.FindPropertyRelative(nameof(CursorPreset.AutoHide));
				SerializedProperty autoHideDelayProperty = property.FindPropertyRelative(nameof(CursorPreset.AutoHideDelay));

				float lineHeight = EditorGUIUtility.singleLineHeight;
				float autoHideLabelWidth = EditorStyles.miniLabel.CalcSize(AutoHideLabel).x;
				float secondsLabelWidth = EditorStyles.miniLabel.CalcSize(SecondsLabel).x;

				Rect secondsLabelRect = new(position.xMax - secondsLabelWidth, position.y, secondsLabelWidth, lineHeight);
				Rect delayRect = new(secondsLabelRect.x - DELAY_WIDTH, position.y, DELAY_WIDTH, lineHeight);
				Rect toggleRect = new(delayRect.x - SPACING - TOGGLE_WIDTH, position.y, TOGGLE_WIDTH, lineHeight);
				Rect autoHideLabelRect = new(toggleRect.x - SPACING - autoHideLabelWidth, position.y, autoHideLabelWidth, lineHeight);
				float fieldsWidth = autoHideLabelRect.x - COLUMN_SPACING - position.x;
				Rect idRect = new(position.x, position.y, (fieldsWidth * ID_RATIO) - SPACING, lineHeight);
				Rect spriteRect = new(idRect.xMax + SPACING, position.y, fieldsWidth - idRect.width - SPACING, lineHeight);

				int indentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				{
					EditorGUI.PropertyField(idRect, idProperty, GUIContent.none);
					EditorGUI.ObjectField(spriteRect, spriteProperty, typeof(Sprite), GUIContent.none);
					EditorGUI.LabelField(autoHideLabelRect, AutoHideLabel, EditorStyles.miniLabel);
					EditorGUI.PropertyField(toggleRect, autoHideProperty, GUIContent.none);
					EditorGUI.BeginDisabledGroup(!autoHideProperty.boolValue);
					{
						EditorGUI.PropertyField(delayRect, autoHideDelayProperty, GUIContent.none);
						EditorGUI.LabelField(secondsLabelRect, SecondsLabel, EditorStyles.miniLabel);
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUI.indentLevel = indentLevel;
			}
			EditorGUI.EndProperty();
		}


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
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