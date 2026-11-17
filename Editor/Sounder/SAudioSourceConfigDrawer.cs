#if UNITY_EDITOR && ENABLE_SOUNDER


using UnityEditor;
using UnityEngine;


namespace DragonResonance.Sounder
{
	[CustomPropertyDrawer(typeof(AudioSourceConfig))]
	public class SAudioSourceConfigDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			{
				SerializedProperty audioResourceProperty = property.FindPropertyRelative(nameof(AudioSourceConfig.AudioResource));
				SerializedProperty audioMixerGroupProperty = property.FindPropertyRelative(nameof(AudioSourceConfig.AudioMixerGroup));

				position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

				float halfWidth = position.width / 2f;
				Rect audioResourcePropertyRect = new(position.x, position.y, halfWidth - 2, EditorGUIUtility.singleLineHeight);
				Rect audioMixerGroupPropertyRect = new(position.x + halfWidth + 2, position.y, halfWidth - 2, EditorGUIUtility.singleLineHeight);

				EditorGUI.PropertyField(audioResourcePropertyRect, audioResourceProperty, GUIContent.none);
				EditorGUI.PropertyField(audioMixerGroupPropertyRect, audioMixerGroupProperty, GUIContent.none);
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