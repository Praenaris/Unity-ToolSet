#if UNITY_EDITOR


using DragonResonance.Editor.Building;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if ENABLE_INTEGRATION
using Praenaris.Integration;
#endif


namespace DragonResonance.Editor.Settings
{
#if ENABLE_INTEGRATION
	public class IntegrationSettingsProvider : AScriptableSettingsProvider<IntegrationSettings>
#else
	public class IntegrationSettingsProvider : AScriptableSettingsProvider
#endif
	{
		private const string SettingsPath = "Project/Praenaris/Integration";
		private const string BuildDefinition = "ENABLE_INTEGRATION";
		private const float SeparatorHeight = 1f;

		private static readonly Color SeparatorColor = new(0.5f, 0.5f, 0.5f, 0.5f);
		private static readonly SortedSet<string> _modules = new();	// Filled by each module provider when Unity creates it


		#region Constructors

			[SettingsProvider]
			public static SettingsProvider Create() => new IntegrationSettingsProvider(SettingsPath, SettingsScope.Project);

			public IntegrationSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

		#endregion


		#region Inheritables

			protected override void OnBeforeGUI(string searchContext)
			{
				#if ENABLE_INTEGRATION
					if (!EditorGUILayout.Toggle("Enabled", true))
						BuildDefines.SetDefinitionState(BuildDefinition, false);
				#else
					if (EditorGUILayout.Toggle("Enabled", false))
						BuildDefines.SetDefinitionState(BuildDefinition, true);
				#endif
			}

			protected override void OnAfterGUI(string searchContext)
			{
				#if ENABLE_INTEGRATION
				EditorGUILayout.Space(SmallPadding);
				EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, SeparatorHeight), SeparatorColor);
				EditorGUILayout.Space(SmallPadding);

				EditorGUILayout.LabelField("Modules", EditorStyles.boldLabel);
				foreach (string module in _modules.Where(module => EditorGUILayout.LinkButton(module)))
					SettingsService.OpenProjectSettings(GetModulePath(module));
				#endif
			}

		#endregion


		#region Publics

			public static void RegisterModule(string moduleName) => _modules.Add(moduleName);
			public static string GetModulePath(string moduleName) => $"{SettingsPath}/{moduleName}";

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