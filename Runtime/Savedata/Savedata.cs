#if ENABLE_SAVEDATA


using DragonResonance.Extensions;
using DragonResonance.Logging;
using DragonResonance.Serializables;
using Praenaris.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using Tabernero.SimpleJSON;
using UnityEngine.Scripting;
using UnityEngine;


namespace Praenaris.Savedata
{
	[Preserve]
	public class Savedata : ASubsystem<Savedata, SavedataSettings>
	{
		private const string CurrentSlotKey = "SAVEDATA_CURRENTSLOT";


		private static JSONNode[] _resourcesData = { };
		private static bool _isReady = false;
		private static readonly SemaphoreSlim _filesSemaphore = new(1, 1);


		#region Events

			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
			private static void Initialize() => Startup(onStarted: Start);

			private static async Task Start()
			{
				if (_settings.LoadOnGameStart)
					await Load();
			}

		#endregion


		#region Publics - ????

			public static async Task Load() => await Load(CurrentSlot);
			public static async Task Load(int slot)
			{
				//await _starting.Task;
				await _filesSemaphore.WaitAsync();
				try {
					Log.Info($"Loading slot {slot}...");
					_isReady = false;
					SetCurrentSlot(slot);

					string[] filePaths = _settings.Resources.Select(resource => resource.GetFullPath(slot)).ToArray();
					_resourcesData = await Task.WhenAll(filePaths.Select(LoadResource));

					_isReady = true;
					Log.Info($"Slot {slot} loaded!");
				}
				finally {
					_filesSemaphore.Release();
				}
			}


			public static async Task Save() => await Save(CurrentSlot);
			public static async Task Save(int slot)
			{
				// TODO - use GetSlotData ?
			}


			public static async Task SaveAndReload() => await SaveAndReload(CurrentSlot);
			public static async Task SaveAndReload(int slot)
			{
				await Save(slot);
				await Load(slot);
			}

		#endregion


		#region Publics - ????

			public static bool Get<T>(string key, out T data, T fallback = default)
			{
				data = default;

				// TODO

				return false;	// False if the savedata is not ready (not loaded)
			}


			public static bool Set<T>(string key, T data)
			{
				// TODO

				return false;	// False if the savedata is not ready (not loaded)
			}

		#endregion


		#region Privates

			private static int GetCurrentSlot() => PlayerPrefs.GetInt(CurrentSlotKey, 0);
			private static void SetCurrentSlot(int slot) => PlayerPrefs.SetInt(CurrentSlotKey, slot);

			private static async Task<JSONNode> LoadResource(string filePath)
			{
				Log.Info($"Reading {filePath} ...");
				try {
					/*string content = await Fileman.ReadFromFile(filePath);
					if (string.IsNullOrWhiteSpace(content)) {
						Log.Info($"No savedata found at \"{filePath}\", starting empty");
						return JSONNode.New();
					}

					JSONNode json = JSONNode.Parse(content);
					if (json is JSONObject) return json;

					Log.Error($"The savedata at \"{filePath}\" is not a JSON object");*/
					return null;
				}
				catch (Exception exception) {
					Log.Exception(exception, $"Exception loading the savedata at \"{filePath}\"");
					return null;
				}
			}

		#endregion


		#region Properties

			public static bool IsReady => _isReady;
			public static int CurrentSlot => GetCurrentSlot();
			public static IReadOnlyList<JSONNode> ResourcesData => _resourcesData;

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