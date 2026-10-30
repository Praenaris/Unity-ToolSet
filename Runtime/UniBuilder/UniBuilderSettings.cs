#if ENABLE_UNIBUILDER


using DragonResonance.Behaviours;
using UnityEngine;


namespace Praenaris.UniBuilder
{
	[CreateAssetMenu(menuName = "Praenaris/Settings/UniBuilder", fileName = "New UniBuilder Settings")]
	public class UniBuilderSettings : SingletonScriptableObject<UniBuilderSettings>
	{
		public string StandaloneTest1 = "";
		public int StandaloneTest2 = 0;
		public bool StandaloneTest3 = false;

		public string SteamTest1 = "";
		public int SteamTest2 = 0;
		public bool SteamTest3 = false;

		public string ItchIoTest1 = "";
		public int ItchIoTest2 = 0;
		public bool ItchIoTest3 = false;

		public UniBuildProfileEntry[] BuildProfiles = { };
		public bool StopPipelineOnFailure = true;
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