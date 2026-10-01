#if ENABLE_SAVEDATA


using DragonResonance.Extensions;
using DragonResonance.Logging;
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
	public partial class Savedata : ASubsystem<Savedata, SavedataSettings>
	{
		public const int DefaultSlot = 0;
		public const string CurrentSlotKey = "SAVEDATA_CURRENTSLOT";

		private static JSONNode[] _resourcesData = { };
		private static JSONNode _data = default;
		private static bool _isReady = false;
		private static readonly SemaphoreSlim _filesSemaphore = new(1, 1);


		public static Action<SavedataLoadState> OnLoaded = null;
		public static Action OnSaved = null;


		#region Events

			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
			private static void Initialize() => Startup(onStarted: Start);

			private static async Task Start()
			{
				if (_settings.LoadOnGameStart)
					await Load();
			}

		#endregion


		#region Publics

			public static async Task Load() => await Load(CurrentSlot);
			public static async Task Load(int slot)
			{
				//await _starting.Task;
				await _filesSemaphore.WaitAsync();
				try {
					Log.Info($"Loading slot {slot}...");
					SetCurrentSlot(slot);
					_isReady = false;

					string[] filePaths = _settings.Resources.Select(resource => resource.GetFullPath(slot)).ToArray();
					JSONNode[] resourcesData = await Task.WhenAll(filePaths.Select(LoadResource));
					int dataVersion = CheckResourcesDataVersion(resourcesData);
					_data = dataVersion.IsNegative() ? JSONNode.New() : MergeResourcesData(resourcesData);

					_isReady = true;
					OnLoaded?.Invoke(EvaluateLoadState(dataVersion));
					Log.Info($"Slot {slot} loaded!");
				}
				finally {
					_filesSemaphore.Release();
				}
			}


			public static async Task Save() => await Save(CurrentSlot);
			public static async Task Save(int slot)
			{
				//await _starting.Task;
				await _filesSemaphore.WaitAsync();
				try {
					Log.Info($"Saving slot {slot}...");
					SetCurrentSlot(slot);
					_isReady = false;

					_data[_settings.SavedataVersionKey] = _settings.SavedataVersion;	// Stamp the current savedata version
					SavedataResource[] resources = _settings.Resources.ToArray();
					JSONNode[] resourcesData = SplitData(_data, resources);
					string[] filePaths = resources.Select(resource => resource.GetFullPath(slot)).ToArray();
					await Task.WhenAll(filePaths.Select((filePath, resourceIndex) => SaveResource(filePath, resourcesData[resourceIndex])));

					_isReady = true;
					OnSaved?.Invoke();
					Log.Info($"Slot {slot} saved!");
				}
				finally {
					_filesSemaphore.Release();
				}
			}


			public static async Task SaveAndReload() => await SaveAndReload(CurrentSlot);
			public static async Task SaveAndReload(int slot)
			{
				await Save(slot);
				await Load(slot);
			}

		#endregion


		#region Privates

			private static int GetCurrentSlot() => PlayerPrefs.GetInt(CurrentSlotKey, DefaultSlot);
			private static void SetCurrentSlot(int slot) => PlayerPrefs.SetInt(CurrentSlotKey, slot);


			private static async Task<JSONNode> LoadResource(string filePath)
			{
				Log.Info($"Reading {filePath} ...");
				try {
					string content = await Fileman.ReadFromFile(filePath);
					if (string.IsNullOrWhiteSpace(content)) {
						Log.Info($"No savedata found at \"{filePath}\", starting empty");
						return JSONNode.New();
					}
					else {
						JSONNode json = JSONNode.Parse(content);
						if (json is JSONObject)
							return json;

						Log.Error($"The savedata at \"{filePath}\" is not a JSON object");
						return null;
					}
				}
				catch (Exception exception) {
					Log.Exception(exception, $"Exception loading the savedata at \"{filePath}\"");
					return null;
				}
			}

			private static async Task SaveResource(string filePath, JSONNode json)
			{
				Log.Info($"Writing {filePath} ...");
				try {
					await Fileman.WriteToFile(json.ToString(_settings.SaveCompactData), filePath);
				}
				catch (Exception exception) {
					Log.Exception(exception, $"Exception saving the savedata at \"{filePath}\"");
				}
			}


			private static JSONNode MergeResourcesData(IEnumerable<JSONNode> resourcesData)
			{
				JSONNode mergedData = JSONNode.New();
				foreach (JSONNode resourceData in resourcesData.Where(resourceData => (resourceData != null)))
					foreach (KeyValuePair<string, JSONNode> entry in resourceData)
						mergedData[entry.Key] = entry.Value;	// Later resources (the overrides) win over earlier ones (the fallback)
				return mergedData;
			}

			private static JSONNode[] SplitData(JSONNode data, SavedataResource[] resources)
			{
				JSONNode[] resourcesData = resources.Select(_ => JSONNode.New()).ToArray();
				foreach (KeyValuePair<string, JSONNode> entry in data) {
					int resourceIndex = Array.FindLastIndex(resources, resource => (resource.Keys != null) && resource.Keys.Contains(entry.Key));
					resourcesData[Math.Max(resourceIndex, 0)][entry.Key] = entry.Value;	// Unrequested keys go to the fallback, the first resource
				}
				return resourcesData;
			}


			private static int CheckResourcesDataVersion(IEnumerable<JSONNode> resourcesData)
			{
				foreach (JSONNode resourceData in resourcesData.Where(resourceData => (resourceData != null)))
					if (resourceData.HasKey(_settings.SavedataVersionKey))
						return resourceData[_settings.SavedataVersionKey].AsInt;
				return -1;
			}

			private static SavedataLoadState EvaluateLoadState(int dataVersion)
			{
				if (dataVersion.IsNegative()) return SavedataLoadState.FreshNew;
				return (dataVersion < _settings.SavedataVersion) ? SavedataLoadState.OlderVersion : SavedataLoadState.LatestVersion;
			}

		#endregion


		#region Properties

			public static bool IsReady => _isReady;
			public static int CurrentSlot => GetCurrentSlot();

			public static JSONNode Data
			{
				get => _data;
				internal set => _data = value;	// The settings editor can edit it directly
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