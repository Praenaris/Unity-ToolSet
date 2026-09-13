#if ENABLE_LOCALIZER


using Cysharp.Threading.Tasks;
using DragonResonance.Databases;
using DragonResonance.Extensions;
using DragonResonance.Logging;
using Praenaris;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System;
using UnityEngine.Events;
using UnityEngine.Scripting;
using UnityEngine;


namespace DragonResonance.Localizer
{
	[Preserve]
	public partial class Localizer : ASubsystem<Localizer, LocalizerSettings>
	{
		private static SystemLanguage _currentLanguage = SystemLanguage.Unknown;
		private static readonly HeaderedSheet<string> _dataSheet = new();

		public static event Action OnLanguageChange = null;


		#region Events

			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
			private static void Initialize() => Startup(onStarting: Start);

			private static async Task Start()
			{
				#if ENABLE_UNITYWEBREQUEST && (UNITY_EDITOR || DEVELOPMENT_BUILD)
					await RetrieveOnlineData();
				#endif
				await LoadLocalData();
				_currentLanguage = FirstPreferredLanguage(_currentLanguage);
			}

		#endregion


		#region Publics

			public static async UniTask ChangeLanguage(SystemLanguage language)
			{
				await _starting.Task;
				Log.Emphasis($"Language changed to {language}");
				_currentLanguage = language;
				OnLanguageChange?.Invoke();
			}


			public static async UniTask Localize(string template, UnityEvent<string> handler)
			{
				await _starting.Task;
				handler.Invoke(await Localize(template));
			}

			public static async UniTask<string> Localize(string template)
			{
				await _starting.Task;

				string language = _currentLanguage.ToString();
				foreach (string key in GetKeys(template)) {

					// Simple keys
					if (_dataSheet.TryGet(language, key, out string simpleValue) && !string.IsNullOrWhiteSpace(simpleValue)) {
						template = template.Replace($"{{{key}}}", simpleValue);
						continue;
					}

					// Composite keys
					int partialIndex = 1;
					if (_dataSheet.HeadingColumn.Contains($"{key}:{partialIndex}")) {
						StringBuilder compositeValueBuilder = new();
						while (_dataSheet.TryGet(language, $"{key}:{partialIndex++}", out string partialValue))
							compositeValueBuilder.AppendLine(partialValue);

						string compositeValue = compositeValueBuilder.ToString();
						if (!string.IsNullOrWhiteSpace(compositeValue)) {
							template = template.Replace($"{{{key}}}", compositeValue);
							continue;
						}
					}

					Log.Error($"Key {key} not found or empty in {language} language!");
				}
				return template;
			}

		#endregion


		#region Privates

			private static SystemLanguage FirstPreferredLanguage(SystemLanguage fallback)
			{
				SystemLanguage[] preferredLanguages = _settings.PreferredLanguages.ToArray();
				SystemLanguage[] availableLanguages = Localizer.AvailableLanguages.ToArray();
				return preferredLanguages.FirstMatchOrFallback(availableLanguages, fallback);
			}

			private static IEnumerable<string> GetKeys(string rawText) =>
				Regex.Matches(rawText, @"\{(\w+)\}").Select(match => match.Groups[1].Value);

		#endregion


		#region Properties

			public static SystemLanguage CurrentLanguage => _currentLanguage;
			public static HeaderedSheet<string> DataSheet => _dataSheet;

			public static bool IsDefaultLanguage => (_currentLanguage == _settings.PreferredLanguages.First());
			public static IEnumerable<string> AvailableLanguageNames => Localizer.AvailableLanguages.Select(language => language.ToString());
			public static IEnumerable<SystemLanguage> AvailableLanguages => _dataSheet.HeadingRow
				.Select(cellString => (success: Enum.TryParse<SystemLanguage>(cellString, out var language), language))
				.Where(parsing => parsing.success)
				.Select(parsing => parsing.language);

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