#if UNITY_EDITOR


using DragonResonance.Editor.Building;
using DragonResonance.Extensions;
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
					_serializedSettings.Update();
					OnBeforeGUI(searchContext);

					SerializedProperty property = _serializedSettings.GetIterator();
					if (property.NextVisible(true)) {
						do {
							if (property.name == "m_Script") continue;
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


		#region Privates

			private void LoadOrCreateSettings()
			{
				string settingsName = typeof(TSettings).Name;

				string[] guids = AssetDatabase.FindAssets($"t:{settingsName}");
				if (guids.Length.IsZero()) {
					_settings = ScriptableObject.CreateInstance<TSettings>();
					AssetDatabase.CreateAsset(_settings, $"Assets/{settingsName}.asset");
					AssetDatabase.SaveAssets();
				}
				else {
					string path = AssetDatabase.GUIDToAssetPath(guids.First());
					_settings = AssetDatabase.LoadAssetAtPath<TSettings>(path);
				}

				PreloadedAssets.Add(_settings);
				_serializedSettings = new SerializedObject(_settings);
			}

		#endregion


		#region Properties

			protected TSettings Settings => _settings;

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