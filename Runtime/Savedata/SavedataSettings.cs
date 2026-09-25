#if ENABLE_SAVEDATA


using DragonResonance.Attributes;
using DragonResonance.Behaviours;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Praenaris.Savedata
{
	[CreateAssetMenu(menuName = "Praenaris/Settings/Savedata", fileName = "New Savedata Settings")]
	public class SavedataSettings : SingletonScriptableObject<SavedataSettings>
	{
		public bool LoadOnGameStart = true;
		public bool SaveCompactData = false;

		[Header("Slots")]
		public bool LimitSlots = false;
		[ShowIf(nameof(LimitSlots))] [SerializeField] [Min(1)] private int _maxSlots = 3;

		[Header("Fallback Resource")] public SavedataResource FallbackResource = new() {
			Root = SavedataRootPath.AppData,
			Company = true,
			Product = true,
			RelativePath = "savedata.json",
			Slotted = false,
		};
		public SavedataResource[] ResourceOverrides = { };


		public int MaxSlots => LimitSlots ? _maxSlots : -1;
		public IEnumerable<SavedataResource> Resources => ResourceOverrides.Prepend(FallbackResource);
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