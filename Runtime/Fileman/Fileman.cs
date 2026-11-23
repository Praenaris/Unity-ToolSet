#if (UNITY_SWITCH)
	#define FILEMAN_FORCE_PLAYERPREFS
#endif


using DragonResonance.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine.Networking;
using UnityEngine;


namespace Praenaris.Tools
{
	public static class Fileman
	{
		public const string PlayerPrefsRoot = "<PlayerPrefs>";
		public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);	// UTF-8 without BOM

	#if FILEMAN_FORCE_PLAYERPREFS
		public static readonly bool ForcePlayerPrefs = true;
	#else
		public static readonly bool ForcePlayerPrefs = false;
	#endif


		#region Local

			public static bool Exists(SavedataRootPath root, string relativePath) => Exists(ResolvePath(root, relativePath));
			public static bool Exists(string path) => IsPlayerPrefsPath(path) ? PlayerPrefs.HasKey(path) : File.Exists(path);

			public static void Delete(SavedataRootPath root, string relativePath) => Delete(ResolvePath(root, relativePath));
			public static void Delete(string path)
			{
				if (IsPlayerPrefsPath(path)) {
					PlayerPrefs.DeleteKey(path);
					PlayerPrefs.Save();
				}
				else if (File.Exists(path))
					File.Delete(path);
			}


			public static async Task<string> ReadFromStorage(SavedataRootPath root, string relativePath, CancellationToken cancellationToken = default) =>
				await ReadFromStorage(root, relativePath, DefaultEncoding, cancellationToken);
			public static async Task<string> ReadFromStorage(SavedataRootPath root, string relativePath, Encoding encoding, CancellationToken cancellationToken = default) =>
				await ReadFromStorage(ResolvePath(root, relativePath), encoding, cancellationToken);
			public static async Task<string> ReadFromStorage(string sourcePath, CancellationToken cancellationToken = default) =>
				await ReadFromStorage(sourcePath, DefaultEncoding, cancellationToken);
			public static async Task<string> ReadFromStorage(string sourcePath, Encoding encoding, CancellationToken cancellationToken = default)
			{
				if (IsPlayerPrefsPath(sourcePath))
					return (PlayerPrefs.HasKey(sourcePath) ? PlayerPrefs.GetString(sourcePath) : null);

				if (File.Exists(sourcePath))
					return await File.ReadAllTextAsync(sourcePath, encoding, cancellationToken);

				return null;
			}

		#if UNITY_EDITOR
			public static async Task<string> ReadFromAsset(TextAsset sourceAsset, CancellationToken cancellationToken = default) =>
				await ReadFromAsset(sourceAsset, DefaultEncoding, cancellationToken);
			public static async Task<string> ReadFromAsset(TextAsset sourceAsset, Encoding encoding, CancellationToken cancellationToken = default) =>
				await ReadFromStorage(UnityEditor.AssetDatabase.GetAssetPath(sourceAsset), encoding, cancellationToken);
		#endif


			public static async Task WriteToStorage(string content, SavedataRootPath root, string relativePath, CancellationToken cancellationToken = default) =>
				await WriteToStorage(content, root, relativePath, DefaultEncoding, cancellationToken);
			public static async Task WriteToStorage(string content, SavedataRootPath root, string relativePath, Encoding encoding, CancellationToken cancellationToken = default) =>
				await WriteToStorage(content, ResolvePath(root, relativePath), encoding, cancellationToken);
			public static async Task WriteToStorage(string content, string targetPath, CancellationToken cancellationToken = default) =>
				await WriteToStorage(content, targetPath, DefaultEncoding, cancellationToken);
			public static async Task WriteToStorage(string content, string targetPath, Encoding encoding, CancellationToken cancellationToken = default)
			{
				if (IsPlayerPrefsPath(targetPath)) {
					PlayerPrefs.SetString(targetPath, content);
					PlayerPrefs.Save();
				}
				else {
					await File.WriteAllTextAsync(targetPath, content, encoding, cancellationToken);
				}
			}

		#if UNITY_EDITOR
			public static async Task WriteToAsset(string content, TextAsset targetAsset, CancellationToken cancellationToken = default) =>
				await WriteToAsset(content, targetAsset, DefaultEncoding, cancellationToken);
			public static async Task WriteToAsset(string content, TextAsset targetAsset, Encoding encoding, CancellationToken cancellationToken = default) =>
				await WriteToStorage(content, UnityEditor.AssetDatabase.GetAssetPath(targetAsset), encoding, cancellationToken);
		#endif

		#endregion


		#region Online

			public static async Task FetchToFile(string url, string targetPath, CancellationToken cancellationToken = default) =>
				await FetchToFile(url, targetPath, DefaultEncoding, cancellationToken);
			public static async Task FetchToFile(string url, string targetPath, Encoding encoding, CancellationToken cancellationToken = default)
			{
				string content = await FetchWebResource(url, cancellationToken);
				if (content == null) return;
				await WriteToStorage(content, targetPath, encoding, cancellationToken);
			}

		#if UNITY_EDITOR
			public static async Task FetchToEditorAsset(string url, TextAsset targetAsset, CancellationToken cancellationToken = default) =>
				await FetchToEditorAsset(url, targetAsset, DefaultEncoding, cancellationToken);
			public static async Task FetchToEditorAsset(string url, TextAsset targetAsset, Encoding encoding, CancellationToken cancellationToken = default) =>
				await FetchToFile(url, UnityEditor.AssetDatabase.GetAssetPath(targetAsset), encoding, cancellationToken);
		#endif


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


		#region Paths

			public static bool IsPlayerPrefsPath(string path) => path.StartsWith(PlayerPrefsRoot, StringComparison.Ordinal);

			public static string ResolvePath(SavedataRootPath root, string relativePath)
			{
				if (ForcePlayerPrefs || (root == SavedataRootPath.PlayerPrefs))
					return Path.Join(PlayerPrefsRoot, relativePath).Replace('\\', '/');	// Standard path naming
				return Path.GetFullPath(Path.Join(root.Resolve(), ".", relativePath));
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