#if UNITY_EDITOR


using DragonResonance.Editor.Building;
using UnityEditor;


namespace Praenaris.Editor.Building
{
	[InitializeOnLoad]
	public static class FilemanForcePlayerPrefsToggler
	{
		public const string FORCE_PLAYERPREFS_DEFINE = "_FILEMAN_FORCE_PLAYERPREFS";


		#region Constructors

			static FilemanForcePlayerPrefsToggler() => BuildDefines.SetDefinition(FORCE_PLAYERPREFS_DEFINE, false);

		#endregion


		#region Publics

			#if FILEMAN_FORCE_PLAYERPREFS
				[MenuItem("Subsystems/Fileman Force PlayerPrefs [ON]/Stop forcing PlayerPrefs on Fileman")]
			#else
				[MenuItem("Subsystems/Fileman Force PlayerPrefs [OFF]/Force PlayerPrefs on Fileman")]
			#endif
			public static void Toggle() => BuildDefines.ToggleBuildDefinition(FORCE_PLAYERPREFS_DEFINE);

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