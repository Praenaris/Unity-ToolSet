#if UNITY_EDITOR && ENABLE_SAVEDATA


using DragonResonance.Logging;
using Praenaris.Savedata;
using System.IO;
using System;
using UnityEditor;
using UnityEngine;


namespace DragonResonance.Editor.Drawers
{
	[CustomPropertyDrawer(typeof(SavedataResource))]
	public class SavedataResourceDrawer : PropertyDrawer
	{
		private const float SPACING = 2f;
		private const float PATH_ROOT_PADDING = 12f;
		private const float ARRAY_ELEMENT_LABEL_RATIO = 0.2f;
		private const float OPEN_FOLDER_WIDTH = 24f;
		private const string SLOT_PREVIEW_ID = "{slot}";

		private static readonly GUIContent CompanyLabel = new("Company ");
		private static readonly GUIContent ProductLabel = new("Product ");
		private static readonly GUIContent SlottedLabel = new("Slotted ");
		private static readonly GUIContent OpenFolderContent = new(EditorGUIUtility.IconContent("FolderOpened Icon").image);
		private static float _pathRootWidth = -1f;


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			{
				SerializedProperty rootProperty = property.FindPropertyRelative(nameof(SavedataResource.Root));
				SerializedProperty companyProperty = property.FindPropertyRelative(nameof(SavedataResource.Company));
				SerializedProperty productProperty = property.FindPropertyRelative(nameof(SavedataResource.Product));
				SerializedProperty relativePathProperty = property.FindPropertyRelative(nameof(SavedataResource.RelativePath));
				SerializedProperty slottedProperty = property.FindPropertyRelative(nameof(SavedataResource.Slotted));

				SavedataResource resource = new() {
					Root = (SavedataRootPath)rootProperty.intValue,
					Company = companyProperty.boolValue,
					Product = productProperty.boolValue,
					RelativePath = relativePathProperty.stringValue,
					Slotted = slottedProperty.boolValue,
				};

				string title = GetTitle(resource);
				GUIContent prefixLabel = string.IsNullOrEmpty(title) ? label : new GUIContent(title, label.tooltip);

				float labelWidth = EditorGUIUtility.labelWidth;
				if (IsArrayElement(property))
					EditorGUIUtility.labelWidth = Mathf.Min(labelWidth, position.width * ARRAY_ELEMENT_LABEL_RATIO);
				position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), prefixLabel);
				EditorGUIUtility.labelWidth = labelWidth;

				float lineHeight = EditorGUIUtility.singleLineHeight;
				float secondLineY = position.y + lineHeight + EditorGUIUtility.standardVerticalSpacing;
				float companyWidth = EditorStyles.toggle.CalcSize(CompanyLabel).x;
				float productWidth = EditorStyles.toggle.CalcSize(ProductLabel).x;
				float slottedWidth = EditorStyles.toggle.CalcSize(SlottedLabel).x;

				Rect pathRootRect = new(position.x, position.y, GetPathRootWidth(), lineHeight);
				Rect companyRect = new(pathRootRect.xMax + SPACING, position.y, companyWidth, lineHeight);
				Rect productRect = new(companyRect.xMax + SPACING, position.y, productWidth, lineHeight);
				Rect slottedRect = new(position.xMax - slottedWidth, position.y, slottedWidth, lineHeight);
				Rect relativePathRect = new(productRect.xMax + SPACING, position.y, slottedRect.xMin - productRect.xMax - (SPACING * 2f), lineHeight);
				Rect openFolderRect = new(position.xMax - OPEN_FOLDER_WIDTH, secondLineY, OPEN_FOLDER_WIDTH, lineHeight);
				Rect fullPathRect = new(position.x, secondLineY, openFolderRect.xMin - position.x - SPACING, lineHeight);

				int indentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				{
					EditorGUI.PropertyField(pathRootRect, rootProperty, GUIContent.none);
					DrawToggle(companyRect, companyProperty, CompanyLabel);
					DrawToggle(productRect, productProperty, ProductLabel);
					EditorGUI.PropertyField(relativePathRect, relativePathProperty, GUIContent.none);
					DrawToggle(slottedRect, slottedProperty, SlottedLabel);

					string fullPathPreview = GetFullPathPreview(resource, out bool isValidPath);
					EditorGUI.SelectableLabel(fullPathRect, fullPathPreview, EditorStyles.miniLabel);
					EditorGUI.BeginDisabledGroup(!isValidPath);
					{
						if (UnityEngine.GUI.Button(openFolderRect, OpenFolderContent, EditorStyles.miniButton))
							OpenContainingFolder(fullPathPreview);
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUI.indentLevel = indentLevel;
			}
			EditorGUI.EndProperty();
		}


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			(EditorGUIUtility.singleLineHeight * 2f) + EditorGUIUtility.standardVerticalSpacing;


		private static void DrawToggle(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			bool value = EditorGUI.ToggleLeft(position, label, property.boolValue);
			if (EditorGUI.EndChangeCheck())
				property.boolValue = value;
		}


		private static bool IsArrayElement(SerializedProperty property) => property.propertyPath.EndsWith("]");


		private static string GetTitle(SavedataResource resource)
		{
			try {
				return Path.GetFileName(resource.RelativePath);
			}
			catch (ArgumentException) {
				return null;
			}
		}


		private static string GetFullPathPreview(SavedataResource resource, out bool isValid)
		{
			try {
				isValid = true;
				return resource.FormatFullPath(SLOT_PREVIEW_ID);
			}
			catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException) {
				isValid = false;
				Log.Exception(exception, $"Invalid path ({exception.Message})");
				return $"Invalid path ({exception.Message})";
			}
		}


		private static void OpenContainingFolder(string filePath)
		{
			string folderPath = Path.GetDirectoryName(filePath);
			if (string.IsNullOrEmpty(folderPath)) return;

			try {
				Log.Info($"Opening \"{folderPath}\" ...");
				Directory.CreateDirectory(folderPath);
				EditorUtility.OpenWithDefaultApp(folderPath);
			}
			catch (Exception exception) {
				Log.Exception(exception, $"Unable to open the folder \"{folderPath}\"");
			}
		}


		private static float GetPathRootWidth()
		{
			if (_pathRootWidth >= 0f) return _pathRootWidth;

			foreach (string rootName in Enum.GetNames(typeof(SavedataRootPath)))
				_pathRootWidth = Mathf.Max(_pathRootWidth, EditorStyles.popup.CalcSize(new GUIContent(ObjectNames.NicifyVariableName(rootName))).x);
			_pathRootWidth += PATH_ROOT_PADDING;
			return _pathRootWidth;
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