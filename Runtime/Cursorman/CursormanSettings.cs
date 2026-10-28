#if ENABLE_CURSORMAN


using DragonResonance.Behaviours;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


namespace Praenaris.Cursorman
{
	[CreateAssetMenu(menuName = "Praenaris/Settings/Cursorman", fileName = "New Cursorman Settings")]
	public class CursormanSettings : SingletonScriptableObject<CursormanSettings>
	{
		public CursorMode CursorMode = CursorMode.Auto;
		[Tooltip("Preset applied on startup and when resetting, none uses the system cursor")]
		[Cursor] public string DefaultCursor = string.Empty;
		#if ENABLE_INPUT_SYSTEM
		[Tooltip("Pointer position or delta action, it shows the cursor again when it moves while auto-hidden")]
		public InputActionReference PointerInput = null;
		#endif

		[Header("Presets")]
		public List<CursorPreset> Presets = new();
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