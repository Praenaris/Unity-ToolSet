#if ENABLE_SAVEDATA


namespace Praenaris.Savedata
{
	public enum SavedataRootPath
	{
		AppData,	// %APPDATA% (AppData/Roaming) · ~/.config
		CommonAppData,	// %PROGRAMDATA% · /usr/share
		Desktop,	// %USERPROFILE%/Desktop · ~/Desktop
		GameFolder,	// The folder containing the game's executable (the project root in the Editor)
		LocalAppData,	// %LOCALAPPDATA% (AppData/Local) · ~/.local/share
		MyDocuments,	// %USERPROFILE%/Documents · ~/Documents
		PersistentData,	// Unity's Application.persistentDataPath
		TemporaryCache,	// Unity's Application.temporaryCachePath
		UserProfile,	// %USERPROFILE% · ~
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