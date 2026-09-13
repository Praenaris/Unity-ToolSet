using DragonResonance.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine.Networking;
using UnityEngine;


namespace Praenaris.Fileman
{
	public class Fileman
	{
		#region Local

		#if UNITY_EDITOR
			public static async Task WriteToEditorAsset(string content, TextAsset targetAsset, CancellationToken cancellationToken = default) =>
				await WriteToEditorAsset(content, targetAsset, Encoding.UTF8, cancellationToken);
			public static async Task WriteToEditorAsset(string content, TextAsset targetAsset, Encoding encoding, CancellationToken cancellationToken = default) =>
				await WriteToFile(content, UnityEditor.AssetDatabase.GetAssetPath(targetAsset), encoding, cancellationToken);
		#endif

			public static async Task WriteToFile(string content, string targetPath, CancellationToken cancellationToken = default) =>
				await WriteToFile(content, targetPath, Encoding.UTF8, cancellationToken);
			public static async Task WriteToFile(string content, string targetPath, Encoding encoding, CancellationToken cancellationToken = default) =>
				await File.WriteAllTextAsync(targetPath, content, encoding, cancellationToken);

		#endregion


		#region Online

		#if UNITY_EDITOR
			public static async Task FetchToEditorAsset(string url, TextAsset targetAsset, CancellationToken cancellationToken = default) =>
				await FetchToEditorAsset(url, targetAsset, Encoding.UTF8, cancellationToken);
			public static async Task FetchToEditorAsset(string url, TextAsset targetAsset, Encoding encoding, CancellationToken cancellationToken = default) =>
				await FetchToFile(url, UnityEditor.AssetDatabase.GetAssetPath(targetAsset), encoding, cancellationToken);
		#endif

			public static async Task FetchToFile(string url, string targetPath, CancellationToken cancellationToken = default) =>
				await FetchToFile(url, targetPath, Encoding.UTF8, cancellationToken);
			public static async Task FetchToFile(string url, string targetPath, Encoding encoding, CancellationToken cancellationToken = default)
			{
				string content = await FetchWebResource(url, cancellationToken);
				if (content == null) return;
				await WriteToFile(content, targetPath, encoding, cancellationToken);
			}


			public static async Task<string> FetchWebResource(string url, CancellationToken cancellationToken = default)
			{
				Log.Info($"Retrieving source {url} ...");
				using UnityWebRequest request = UnityWebRequest.Get(url);
				await using CancellationTokenRegistration registration = cancellationToken.Register(request.Abort);
				try {
					await request.SendWebRequest();
					cancellationToken.ThrowIfCancellationRequested();
					if (request.result == UnityWebRequest.Result.Success)
						return request.downloadHandler.text;
					Log.Error($"Error {request.result} fetching the resource \"{url}\"");
					return null;
				}
				catch (Exception exception) {
					Log.Exception(exception, $"Exception fetching source {url}");
					return null;
				}
			}

		#endregion
	}
}


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