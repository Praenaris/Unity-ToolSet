#if ENABLE_SAVEDATA


using DragonResonance.Extensions;
using DragonResonance.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;
using Tabernero.SimpleJSON;
using UnityEngine.Scripting;
using UnityEngine;


namespace Praenaris.Savedata
{
	[Preserve]
	public partial class Savedata : ASubsystem<Savedata, SavedataSettings>
	{
		private static readonly Dictionary<string, JSONNode> _data = new();


		#region Events

			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
			private static void Initialize() => Startup(onStarted: Start);

			private static async Task Start()
			{
				if (_settings.LoadOnStart)
					await Load();
			}

		#endregion


		#region Publics

			public static async Task Load()
			{
				// TODO
			}


			public static async Task Save()
			{
				// TODO
			}


			public static async Task SaveAndReload()
			{
				await Save();
				await Load();
			}

		#endregion


		#region Privates

			private static void Test()
			{
				//
			}

		#endregion


		#region Properties

			public static Dictionary<string, JSONNode> Data => _data;

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