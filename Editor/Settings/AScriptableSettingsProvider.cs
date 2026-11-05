#if UNITY_EDITOR


using DragonResonance.Editor.Building;
using DragonResonance.Extensions;
using DragonResonance.Logging;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace DragonResonance.Editor.Settings
{
	public abstract class AScriptableSettingsProvider : SettingsProvider
	{
		protected const int LargePadding = 24;
		protected const int SmallPadding = 12;


		#region Constructors

			protected AScriptableSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

		#endregion


		#region Publics

			public override void OnGUI(string searchContext)
			{
				GUIStyle paddedSection = new() { padding = new RectOffset(LargePadding, LargePadding, SmallPadding, SmallPadding) };

				EditorGUILayout.BeginVertical(paddedSection);
				GUILayout.FlexibleSpace();
				{
					OnBeforeGUI(searchContext);
					OnAfterGUI(searchContext);
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndVertical();
			}

		#endregion


		#region Inheritables

			protected virtual void OnBeforeGUI(string searchContext) { }
			protected virtual void OnAfterGUI(string searchContext) { }

		#endregion
	}




	public abstract class AScriptableSettingsProvider<TSettings> : AScriptableSettingsProvider where TSettings : ScriptableObject
	{
		private TSettings _settings = null;
		private SerializedObject _serializedSettings = null;


		#region Constructors

			protected AScriptableSettingsProvider(string path, SettingsScope scope) : base(path, scope) => LoadOrCreateSettings();

		#endregion


		#region Publics

			public override void OnGUI(string searchContext)
			{
				GUIStyle paddedSection = new() { padding = new RectOffset(LargePadding, LargePadding, SmallPadding, SmallPadding) };

				EditorGUILayout.BeginVertical(paddedSection);
				{
					if (_serializedSettings == null) {
						DrawUnloadedSettings();
						EditorGUILayout.EndVertical();
						return;
					}

					_serializedSettings.Update();
					OnBeforeGUI(searchContext);

					SerializedProperty property = _serializedSettings.GetIterator();
					if (property.NextVisible(true)) {
						do {
							if (property.name == "m_Script") continue;
							if (!OnPropertyGUI(property))
								EditorGUILayout.PropertyField(property, true);
						}
						while (property.NextVisible(false));
					}

					OnAfterGUI(searchContext);
					_serializedSettings.ApplyModifiedProperties();
				}
				EditorGUILayout.EndVertical();
			}

		#endregion


		#region Inheritables

			/// <returns>True if the property was drawn here, false to draw it by default</returns>
			protected virtual bool OnPropertyGUI(SerializedProperty property) => false;

		#endregion


		#region Privates

			private void LoadOrCreateSettings()
			{
				string[] guids = AssetDatabase.FindAssets($"t:{SettingsName}");
				if (!guids.Length.IsZero()) {
					_settings = AssetDatabase.LoadAssetAtPath<TSettings>(AssetDatabase.GUIDToAssetPath(guids.First()));
				}
				else if (File.Exists(DefaultSettingsPath)) {	// There but not indexed as that type yet (its define was off, a pending reimport...), it's never overwritten
					AssetDatabase.ImportAsset(DefaultSettingsPath, ImportAssetOptions.ForceUpdate);
					_settings = AssetDatabase.LoadAssetAtPath<TSettings>(DefaultSettingsPath);
					if (_settings == null) {
						Log.Error($"{DefaultSettingsPath} exists but can't be loaded as {SettingsName} (missing script?), fix or remove it, it won't be overwritten");
						return;
					}
				}
				else {
					_settings = ScriptableObject.CreateInstance<TSettings>();
					AssetDatabase.CreateAsset(_settings, DefaultSettingsPath);
					AssetDatabase.SaveAssets();
				}

				PreloadedAssets.Add(_settings);
				_serializedSettings = new SerializedObject(_settings);
			}

			private void DrawUnloadedSettings()
			{
				EditorGUILayout.HelpBox($"{DefaultSettingsPath} couldn't be loaded as {SettingsName}, see the Console. Fix it and retry, or remove it to start over with a new one.", MessageType.Error);
				if (GUILayout.Button("Retry"))
					LoadOrCreateSettings();
				if (GUILayout.Button("Remove it") && EditorUtility.DisplayDialog($"Remove {DefaultSettingsPath}?",
					$"It goes to the system trash, and a new {SettingsName} with the default settings replaces it.",
					"Yes, remove",
					"No, cancel")) {
					AssetDatabase.MoveAssetToTrash(DefaultSettingsPath);
					LoadOrCreateSettings();
				}
			}

		#endregion


		#region Properties

			private static string SettingsName => typeof(TSettings).Name;
			private static string DefaultSettingsPath => $"Assets/{SettingsName}.asset";

			protected TSettings Settings => _settings;
			protected SerializedObject SerializedSettings => _serializedSettings;

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