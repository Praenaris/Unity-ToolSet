#if ENABLE_SAVEDATA


using Tabernero.SimpleJSON;


namespace Praenaris.Savedata
{
	public partial class Savedata	// Getters & Setters
	{
		#region Publics - Getters

			public static bool GetBool(string key, bool fallback = false)
			{
				TryGetBool(key, out bool value, fallback);
				return value;
			}

			public static int GetInt(string key, int fallback = 0)
			{
				TryGetInt(key, out int value, fallback);
				return value;
			}

			public static float GetFloat(string key, float fallback = 0f)
			{
				TryGetFloat(key, out float value, fallback);
				return value;
			}

			public static string GetString(string key, string fallback = "")
			{
				TryGetString(key, out string value, fallback);
				return value;
			}


			public static void GetSavable(string key, ISavableString value) =>
				TryGetSavable(key, value);

			public static void GetSavable(string key, ISavableNode value) =>
				TryGetSavable(key, value);

			public static JSONNode GetNode(string key, JSONNode fallback = null)
			{
				TryGetNode(key, out JSONNode value, fallback);
				return value;
			}

		#endregion




		#region Publics - TryGetters

			public static bool TryGetBool(string key, out bool value, bool fallback = false)
			{
				bool found = TryGetNode(key, out JSONNode node);
				value = found ? node.AsBool : fallback;
				return found;
			}

			public static bool TryGetInt(string key, out int value, int fallback = 0)
			{
				bool found = TryGetNode(key, out JSONNode node);
				value = found ? node.AsInt : fallback;
				return found;
			}

			public static bool TryGetFloat(string key, out float value, float fallback = 0f)
			{
				bool found = TryGetNode(key, out JSONNode node);
				value = found ? node.AsFloat : fallback;
				return found;
			}

			public static bool TryGetString(string key, out string value, string fallback = "")
			{
				bool found = TryGetNode(key, out JSONNode node);
				value = found ? node.Value : fallback;
				return found;
			}


			public static bool TryGetSavable(string key, ISavableString target)
			{
				if (!TryGetString(key, out string savedata)) return false;
				target.FromSavedata(savedata);
				return true;
			}

			public static bool TryGetSavable(string key, ISavableNode target)
			{
				if (!TryGetNode(key, out JSONNode savedata)) return false;
				target.FromSavedata(savedata);
				return true;
			}

			public static bool TryGetNode(string key, out JSONNode value, JSONNode fallback = null)
			{
				value = fallback;
				if (!_isReady || !_data.HasKey(key)) return false;

				value = _data[key];
				return true;
			}

		#endregion




		#region Publics - Setters

			public static bool TrySetBool(string key, bool value) =>
				TrySetNode(key, value);

			public static bool TrySetInt(string key, int value) =>
				TrySetNode(key, value);

			public static bool TrySetFloat(string key, float value) =>
				TrySetNode(key, value);

			public static bool TrySetString(string key, string value) =>
				TrySetNode(key, value);


			public static bool TrySetSavable(string key, ISavableString value) =>
				TrySetString(key, value.ToSavedata());

			public static bool TrySetSavable(string key, ISavableNode value) =>
				TrySetNode(key, value.ToSavedata());

			public static bool TrySetNode(string key, JSONNode value)
			{
				if (!_isReady) return false;

				_data[key] = value;
				return true;
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