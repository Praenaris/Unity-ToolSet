#if ENABLE_SAVEDATA


using System.Collections.Generic;
using System;
using Tabernero.SimpleJSON;
using UnityEngine;


namespace Praenaris.SavedataLegacy
{
	public partial class Savedata	// Wrappers
	{
		private static readonly Dictionary<Delegate, Action<JSONNode>> _wrappers = new();


		#region Publics - Data

			public static bool Get<T>(out T data, T fallback = default) where T : struct, ISavableData
			{
				data = fallback;

				if (Get(fallback.Key, out JSONNode json)) {
					data = JsonUtility.FromJson<T>(json.ToString());
					return true;
				}

				return false;
			}

			public static void Set<T>(T data) where T : struct, ISavableData
			{
				Set(data.Key, JSONNode.Parse(JsonUtility.ToJson(data)));
			}

		#endregion


		#region Publics - Events

			public static void SubscribeAndReload<T>(Action<T> handler) where T : struct, ISavableData => SubscribeAndReload(typeof(T).Name, handler);
			public static void SubscribeAndReload<T>(string key, Action<T> handler) where T : struct, ISavableData
			{
				Subscribe(key, handler);
				if (Get(out T data))
					Set(data);
			}

			public static void Subscribe<T>(Action<T> handler) where T : struct, ISavableData => Subscribe(typeof(T).Name, handler);
			public static void Subscribe<T>(string key, Action<T> handler) where T : struct, ISavableData
			{
				void Wrapper(JSONNode json) => handler.Invoke(JsonUtility.FromJson<T>(json.ToString()));

				_wrappers[handler] = Wrapper;

				if (_events.TryGetValue(key, out Action<JSONNode> current))
					_events[key] = current + Wrapper;
				else
					_events[key] = Wrapper;
			}

			public static void Unsubscribe<T>(Action<T> handler) where T : struct, ISavableData => Unsubscribe(typeof(T).Name, handler);
			public static void Unsubscribe<T>(string key, Action<T> handler) where T : struct, ISavableData
			{
				if (_events.TryGetValue(key, out Action<JSONNode> current)) {
					if (_wrappers.TryGetValue(handler, out Action<JSONNode> wrapper))
						_events[key] = current - wrapper;

					_wrappers.Remove(handler);
				}
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