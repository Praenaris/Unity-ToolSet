#if UNITY_EDITOR && ENABLE_CURSORMAN


using Praenaris.Cursorman;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Praenaris.Editor.Drawers
{
	[CustomPropertyDrawer(typeof(CursorAttribute))]
	public class CursorAttributeDrawer : PropertyDrawer
	{
		private const string NoneOption = "None";
		private const string MissingSuffix = " (Missing)";

		private static CursormanSettings _settings_internal = null;	// Caching only, use the property instead


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.String) {
				EditorGUI.HelpBox(position, $"{nameof(CursorAttribute)} works just on strings.", MessageType.Error);
				return;
			}

			EditorGUI.BeginProperty(position, label, property);
			{
				List<string> ids = GetIds();
				string currentId = property.stringValue;
				int currentIndex = string.IsNullOrEmpty(currentId) ? 0 : ids.IndexOf(currentId);

				List<GUIContent> options = ids.Select(id => new GUIContent(string.IsNullOrEmpty(id) ? NoneOption : id)).ToList();
				if (currentIndex < 0) {	// Kept visible instead of silently replacing it
					currentIndex = ids.Count;
					ids.Add(currentId);
					options.Add(new GUIContent(currentId + MissingSuffix));
				}

				EditorGUI.BeginChangeCheck();
				EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
				int selectedIndex = EditorGUI.Popup(position, label, currentIndex, options.ToArray());
				EditorGUI.showMixedValue = false;
				if (EditorGUI.EndChangeCheck())
					property.stringValue = ids[selectedIndex];
			}
			EditorGUI.EndProperty();
		}


		private static List<string> GetIds()
		{
			List<string> ids = new() { string.Empty };
			if (Settings == null) return ids;

			foreach (CursorPreset preset in Settings.Presets) {
				if (!string.IsNullOrEmpty(preset.Id) && !ids.Contains(preset.Id))
					ids.Add(preset.Id);
			}
			return ids;
		}


		private static CursormanSettings Settings
		{
			get {
				if (_settings_internal != null) return _settings_internal;
				if (CursormanSettings.TryGetInstance(out _settings_internal)) return _settings_internal;

				string guid = AssetDatabase.FindAssets($"t:{nameof(CursormanSettings)}").FirstOrDefault();
				if (guid != null)
					_settings_internal = AssetDatabase.LoadAssetAtPath<CursormanSettings>(AssetDatabase.GUIDToAssetPath(guid));
				return _settings_internal;
			}
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