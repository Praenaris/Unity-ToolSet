#if UNITY_EDITOR && ENABLE_LOCALIZER


using DragonResonance.Localizer;
using System.IO;
using System;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace DragonResonance.Editor.Drawers
{
	[CustomPropertyDrawer(typeof(LocalizationDataSource))]
	public class LocalizationDataSourceDrawer : PropertyDrawer
	{
		private const float SPACING = 2f;
		private const float TYPE_PADDING = 16f;

		private static readonly GUIContent UrlLabel = new("URL ");
		private static float _typeWidth_internal = -1f;	// Caching only, use the property instead


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			{
				SerializedProperty typeProperty = property.FindPropertyRelative(nameof(LocalizationDataSource.Type));
				SerializedProperty assetProperty = property.FindPropertyRelative(nameof(LocalizationDataSource._asset));
				SerializedProperty pathProperty = property.FindPropertyRelative(nameof(LocalizationDataSource._path));
				SerializedProperty urlProperty = property.FindPropertyRelative(nameof(LocalizationDataSource.Url));

				string title = GetTitle((LocalizationDataType)typeProperty.intValue, assetProperty, pathProperty);
				GUIContent prefixLabel = string.IsNullOrEmpty(title) ? label : new GUIContent(title, label.tooltip);

				position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), prefixLabel);

				float lineHeight = EditorGUIUtility.singleLineHeight;
				float secondLineY = position.y + lineHeight + EditorGUIUtility.standardVerticalSpacing;
				float urlLabelWidth = EditorStyles.label.CalcSize(UrlLabel).x;

				Rect typeRect = new(position.x, position.y, GetTypeWidth(), lineHeight);
				Rect fieldRect = new(typeRect.xMax + SPACING, position.y, position.xMax - typeRect.xMax - SPACING, lineHeight);
				Rect urlLabelRect = new(position.x, secondLineY, urlLabelWidth, lineHeight);
				Rect urlRect = new(urlLabelRect.xMax, secondLineY, position.xMax - urlLabelRect.xMax, lineHeight);

				int indentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				{
					EditorGUI.PropertyField(typeRect, typeProperty, GUIContent.none);
					switch ((LocalizationDataType)typeProperty.intValue) {
						case LocalizationDataType.TextAsset:
							EditorGUI.PropertyField(fieldRect, assetProperty, GUIContent.none);
							break;
						case LocalizationDataType.StreamingAsset:
							EditorGUI.PropertyField(fieldRect, pathProperty, GUIContent.none);
							break;
						default:
							EditorGUI.LabelField(fieldRect, "Type not supported by the drawer", EditorStyles.centeredGreyMiniLabel);
							break;
					}
					EditorGUI.LabelField(urlLabelRect, UrlLabel);
					EditorGUI.PropertyField(urlRect, urlProperty, GUIContent.none);
				}
				EditorGUI.indentLevel = indentLevel;
			}
			EditorGUI.EndProperty();
		}


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			(EditorGUIUtility.singleLineHeight * 2f) + EditorGUIUtility.standardVerticalSpacing;


		private static string GetTitle(LocalizationDataType type, SerializedProperty assetProperty, SerializedProperty pathProperty)
		{
			switch (type) {
				case LocalizationDataType.TextAsset:
					UnityObject asset = assetProperty.objectReferenceValue;
					return (asset == null) ? null : asset.name;
				case LocalizationDataType.StreamingAsset:
					return Path.GetFileName(pathProperty.stringValue);
				default:
					return null;
			}
		}


		private static float GetTypeWidth()
		{
			if (_typeWidth_internal >= 0f) return _typeWidth_internal;

			foreach (string typeName in Enum.GetNames(typeof(LocalizationDataType)))
				_typeWidth_internal = Mathf.Max(_typeWidth_internal, EditorStyles.popup.CalcSize(new GUIContent(ObjectNames.NicifyVariableName(typeName))).x);
			_typeWidth_internal += TYPE_PADDING;
			return _typeWidth_internal;
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