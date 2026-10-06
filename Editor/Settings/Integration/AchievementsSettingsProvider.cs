#if UNITY_EDITOR && ENABLE_INTEGRATION


using Praenaris.Editor.Drawers;
using Praenaris.Integration;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Praenaris.Editor.Settings
{
	public class AchievementsSettingsProvider : AIntegrationModuleSettingsProvider<AchievementsSettings>
	{
		private const string ModuleName = "Achievements";
		private const float ButtonHeight = 24f;

		private static readonly GUIContent AchieveAllLabel = new("Achieve All");
		private static readonly GUIContent UnachieveAllLabel = new("Unachieve All");
		private static readonly GUIContent UpdateReferencesLabel = new(AchievementsSettingsEditor.UpdateChannelsLabel);


		#region Constructors

			[SettingsProvider]
			public static SettingsProvider Create() => new AchievementsSettingsProvider();

			public AchievementsSettingsProvider() : base(ModuleName) { }

		#endregion


		#region Inheritables

			protected override void OnAfterGUI(string searchContext)
			{
				EditorGUILayout.Separator();

				EditorGUILayout.BeginHorizontal();
				{
					if (GUILayout.Button(AchieveAllLabel, GUILayout.Height(ButtonHeight)))
						AchieveAll();
					if (GUILayout.Button(UnachieveAllLabel, GUILayout.Height(ButtonHeight)))
						UnachieveAll();
				}
				EditorGUILayout.EndHorizontal();

				if (GUILayout.Button(UpdateReferencesLabel, GUILayout.Height(ButtonHeight)))
					AchievementsSettingsEditor.UpdateChannels(this.Settings);
			}

		#endregion


		#region Privates

			private void AchieveAll()
			{
				List<Achievement> achievements = this.Settings.Achievements;
				for (int achievementIndex = 0; achievementIndex < achievements.Count; achievementIndex++)
					achievements[achievementIndex].Achieve(achievementIndex == (achievements.Count - 1));	// Applies once, with the last one
			}

			private void UnachieveAll()
			{
				List<Achievement> achievements = this.Settings.Achievements;
				for (int achievementIndex = 0; achievementIndex < achievements.Count; achievementIndex++)
					achievements[achievementIndex].Unachieve(achievementIndex == (achievements.Count - 1));	// Applies once, with the last one
			}

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