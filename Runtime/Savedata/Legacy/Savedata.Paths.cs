#if ENABLE_SAVEDATA


using System.IO;
using System;
using UnityEngine;


namespace Praenaris.SavedataLegacy
{
	public partial class Savedata	// Paths
	{
		#region Publics

			public static string GetOptimizedPersistentDataPath() => GetOptimizedPersistentDataPath(".");
			public static string GetOptimizedPersistentDataPath(string path) => GetOptimizedPersistentDataPath(".", path);
			public static string GetOptimizedPersistentDataPath(string path, string filename)
			{
				string optimizedPersistentDataPath = Application.persistentDataPath;

				#if UNITY_STANDALONE_WIN
					optimizedPersistentDataPath = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
						Application.companyName, Application.productName);
				#elif UNITY_STANDALONE_LINUX
					optimizedPersistentDataPath = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						Application.companyName, Application.productName);
				#elif UNITY_STANDALONE_OSX
					optimizedPersistentDataPath = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						Application.companyName, Application.productName);
				#endif

				return Path.GetFullPath(Path.Combine(optimizedPersistentDataPath, path, filename));
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