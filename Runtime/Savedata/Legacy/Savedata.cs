#if ENABLE_SAVEDATA


using Cysharp.Threading.Tasks;
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


namespace Praenaris.SavedataLegacy
{
	[Preserve]
	public partial class Savedata : ASubsystem<Savedata, SavedataLegacySettings>
	{
		private static readonly Dictionary<string, JSONNode> _data = new();
		private static readonly Dictionary<string, Action<JSONNode>> _events = new();
		private static readonly UniTaskCompletionSource _loading = new();
		private static readonly SemaphoreSlim _saveSemaphore = new(1, 1);


		#region Events

			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
			private static void Initialize() => Startup(onStarted: Start);

			private static async Task Start()
			{
				if (_settings.LoadOnStart)
					await Load();
			}

		#endregion


		#region Publics - Files

			public static async UniTask Load()
			{
				Log.Info("Loading...");
				await _starting.Task;

				_data.Clear();
				foreach (string filePath in Savedata.FilePaths) {
					string dataFilePath = GetOptimizedPersistentDataPath(filePath);
					if (!File.Exists(dataFilePath)) continue;

					string content = await File.ReadAllTextAsync(dataFilePath, Encoding.UTF8);
					JSONNode jsonNode = JSONNode.Parse(content);

					foreach (KeyValuePair<string, JSONNode> jsonDataKeyValuePair in jsonNode)
						Set(jsonDataKeyValuePair.Key, jsonDataKeyValuePair.Value);
				}

				_loading.TrySetResult();
				Log.Info("Loaded!");
			}


			public static async UniTask Save()
			{
				//await _saveSemaphore.WaitAsync();
				if (!await _saveSemaphore.WaitAsync(_settings.ThreadTimeoutMilliseconds)) return;
				try {
					Log.Info("Saving...");
					await _loading.Task;

					HashSet<string> processedKeys = new();
					JSONNode temporalJsonNode = null;
					string persistentDataPath = GetOptimizedPersistentDataPath();
					if (!Directory.CreateDirectory(persistentDataPath).Exists) return;

					// Overrides
					foreach (SFilePathOverride savableOverride in _settings.Overrides) {
						temporalJsonNode = JSONNode.New();
						foreach (string key in savableOverride.Keys) {
							if (_data.ContainsKey(key))
								temporalJsonNode.Add(key, _data[key]);
							processedKeys.Add(key);
						}
						await File.WriteAllTextAsync(
							Path.Combine(persistentDataPath, savableOverride.FilePath),
							temporalJsonNode.ToString(_settings.UseCompactData));
					}

					// Default
					temporalJsonNode = JSONNode.New();
					foreach (KeyValuePair<string, JSONNode> keyValuePair in _data.Where(dataEntryPair => !processedKeys.Contains(dataEntryPair.Key))) {
						temporalJsonNode.Add(keyValuePair.Key, keyValuePair.Value);
					}
					await File.WriteAllTextAsync(
						Path.Combine(persistentDataPath, _settings.DefaultFilePath),
						temporalJsonNode.ToString(_settings.UseCompactData));
				}
				finally {
					_saveSemaphore.Release();
				}

				Log.Info("Saved!");
			}


			[ContextMenu(nameof(SaveReload))]
			public static async UniTask SaveReload()
			{
				await Save();
				await Load();
			}

		#endregion


		#region Publics - Data

			public static bool Get(string key, out JSONNode json)
			{
				return _data.TryGetValue(key, out json);
			}

			public static void Set(string key, JSONNode json)
			{
				_data.AddOrSet(key, json);
				Publish(key, json);
			}

		#endregion


		#region Publics - Events

			public static void Subscribe(string key, Action<JSONNode> handler)
			{
				if (_events.TryGetValue(key, out Action<JSONNode> current))
					_events[key] = current + handler;
				else
					_events[key] = handler;
			}

			public static void Unsubscribe(string key, Action<JSONNode> handler)
			{
				if (_events.TryGetValue(key, out Action<JSONNode> current))
					_events[key] = current - handler;
			}

		#endregion


		#region Privates

			private static void Publish(string key, JSONNode eventData)
			{
				if (_events.TryGetValue(key, out Action<JSONNode> current))
					current?.Invoke(eventData);
			}

		#endregion


		#region Properties

			public static Dictionary<string, JSONNode> Data => _data;
			public static Dictionary<string, Action<JSONNode>> Events => _events;

			public static UniTaskCompletionSource Loading => _loading;

			public static IEnumerable<string> FilePaths => _settings.Overrides
				.Select(savable => savable.FilePath)
				.Prepend(_settings.DefaultFilePath);

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