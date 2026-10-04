#if UNITY_EDITOR && ENABLE_INTEGRATION


using Praenaris.Integration;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace Praenaris.Editor.Drawers
{
	[CustomEditor(typeof(AchievementsSettings))]
	public class AchievementsSettingsEditor : UnityEditor.Editor
	{
		public const string UpdateChannelsLabel = "Update References";


		#region Publics

			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				EditorGUILayout.Separator();
				if (GUILayout.Button(UpdateChannelsLabel))
					UpdateChannels((AchievementsSettings)base.target);
			}


			public static void UpdateChannels(AchievementsSettings settings)
			{
				string path = AssetDatabase.GetAssetPath(settings);

				Dictionary<string, AchievementReference> existingChannels = new();
				List<AchievementReference> unlinkedChannels = new();
				foreach (AchievementReference channel in AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<AchievementReference>()) {
					string guid = channel.Data.ChannelGuid;
					if (string.IsNullOrEmpty(guid) || !existingChannels.TryAdd(guid, channel))	// Empty or duplicated in the list
						unlinkedChannels.Add(channel);
				}

				HashSet<string> updatedGuids = new();
				for (int achievementIndex = 0; achievementIndex < settings.Achievements.Count; achievementIndex++) {
					Achievement achievement = settings.Achievements[achievementIndex];
					if (string.IsNullOrEmpty(achievement.ChannelGuid) || !updatedGuids.Add(achievement.ChannelGuid)) {	// New or duplicated in the list
						achievement.ChannelGuid = Guid.NewGuid().ToString("N");
						updatedGuids.Add(achievement.ChannelGuid);
						settings.Achievements[achievementIndex] = achievement;
					}

					if (!existingChannels.Remove(achievement.ChannelGuid, out AchievementReference channel)) {
						channel = ScriptableObject.CreateInstance<AchievementReference>();
						AssetDatabase.AddObjectToAsset(channel, settings);
					}

					channel.name = string.IsNullOrEmpty(achievement.Name) ? achievement.ChannelGuid : achievement.Name;
					channel.Data = achievement;
					EditorUtility.SetDirty(channel);
				}

				foreach (AchievementReference channel in unlinkedChannels.Concat(existingChannels.Values)) {
					AssetDatabase.RemoveObjectFromAsset(channel);
					UnityObject.DestroyImmediate(channel, true);
				}

				EditorUtility.SetDirty(settings);
				AssetDatabase.SaveAssets();
				AssetDatabase.ImportAsset(path);	// Refreshes the children names in the Project window
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