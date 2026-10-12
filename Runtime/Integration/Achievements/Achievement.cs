#if ENABLE_INTEGRATION


using DragonResonance.Attributes;
using DragonResonance.Extensions;
using System;
using UnityEngine;


namespace Praenaris.Integration
{
	[Serializable]
	public struct Achievement
	{
		public string Name;

		#if STEAMWORKS_INTEGRATION
		public string SteamworksAchievementId;
		public string SteamworksStatId;
		#endif

		public bool IsProgressionStat;
		public float StartingValue;
		public float TargetValue;
		[SpritePreview] public Sprite AchievedIcon;
		[SpritePreview] public Sprite UnachievedIcon;
		[ReadOnly] public string ChannelGuid;


		#region Publics

			public void Progress(float value, bool applyImmediately = true) =>
				Progress((int)Math.Clamp(value + 0.0001d, int.MinValue, int.MaxValue), applyImmediately);

			public void Progress(int value, bool applyImmediately = true)
			{
				#if STEAMWORKS_INTEGRATION
					int minValue = (int)Math.Min(this.StartingValue, this.TargetValue);
					int maxValue = (int)Math.Max(this.StartingValue, this.TargetValue);
					Steamworks.SteamUserStats.SetStat(this.SteamworksStatId, value.Clamp(minValue, maxValue));	// Steam rejects values out of the stat range
					if (applyImmediately)
						Steamworks.SteamUserStats.StoreStats();
				#endif
			}


			public void Achieve(bool applyImmediately = true)
			{
				#if STEAMWORKS_INTEGRATION
					if (this.IsProgressionStat)
						Progress(this.TargetValue, false);
					Steamworks.SteamUserStats.SetAchievement(this.SteamworksAchievementId);
					if (applyImmediately)
						Steamworks.SteamUserStats.StoreStats();
				#endif
			}

			public void Unachieve(bool applyImmediately = true)
			{
				#if STEAMWORKS_INTEGRATION
					if (this.IsProgressionStat)
						Progress(this.StartingValue, false);
					Steamworks.SteamUserStats.ClearAchievement(this.SteamworksAchievementId);
					if (applyImmediately)
						Steamworks.SteamUserStats.StoreStats();
				#endif
			}

		#endregion


		#region Properties

			/// <returns>From 0 (starting value or unachieved) to 1 (target value or achieved)</returns>
			public float NormalizedProgress
			{
				get {
					#if STEAMWORKS_INTEGRATION
						if (this.IsProgressionStat)
							return Steamworks.SteamUserStats.GetStat(this.SteamworksStatId, out float value) ? Mathf.InverseLerp(this.StartingValue, this.TargetValue, value) : 0f;
						return (Steamworks.SteamUserStats.GetAchievement(this.SteamworksAchievementId, out bool achieved) && achieved) ? 1f : 0f;
					#else
						return 0f;
					#endif
				}
			}

			public override string ToString()
			{
				#if STEAMWORKS_INTEGRATION
					string achievementState = Steamworks.SteamUserStats.GetAchievement(this.SteamworksAchievementId, out bool isAchieved) ? (isAchieved ? "ACHIEVED" : "UNACHIEVED") : "UNKNOWN";
					string status = $"Achievement {this.Name} ({this.SteamworksAchievementId}): [{achievementState}]";
					if (!this.IsProgressionStat) return status;

					string statState = Steamworks.SteamUserStats.GetStat(this.SteamworksStatId, out int statValue)
						? $"{statValue}/{this.TargetValue} · {Mathf.InverseLerp(this.StartingValue, this.TargetValue, statValue):P0}"
						: "UNKNOWN";
					return $"{status} | Stat progress ({this.SteamworksStatId}): [{statState}]";
				#else
					return $"Achievement {this.Name} has no integration";
				#endif
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