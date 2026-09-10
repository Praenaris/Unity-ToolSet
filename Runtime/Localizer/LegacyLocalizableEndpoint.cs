#if ENABLE_LOCALIZER


using Cysharp.Threading.Tasks;
using DragonResonance.Behaviours;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using UnityEngine;


namespace DragonResonance.Localizer
{
	public class LegacyLocalizableEndpoint : PossumBehaviour
	{
		[SerializeField] private bool _autoTranslateOnEnable = true;
		[SerializeField] private bool _autoTranslateOnLanguageChange = true;
		[SerializeField] private bool _autoWriteBraces = true;
		[SerializeField] protected string _localizationTemplate = "This is a {TEST}";


		private static readonly Regex NoBracesSingleToken = new(@"^[^\s\{\}]+$");
		public UnityEvent<string> OnLocalize = null;


		#region Events

			private void OnValidate()
			{
				if (_autoWriteBraces && !string.IsNullOrWhiteSpace(_localizationTemplate) && NoBracesSingleToken.IsMatch(_localizationTemplate))
					_localizationTemplate = $"{{{_localizationTemplate}}}";
			}


			private void OnEnable()
			{
				if (_autoTranslateOnEnable)
					Localize();
				if (_autoTranslateOnLanguageChange)
					Localizer.OnLanguageChange += Localize;
			}

			private void OnDisable()
			{
				if (_autoTranslateOnLanguageChange)
					Localizer.OnLanguageChange -= Localize;
			}

		#endregion


		#region Publics

			[ContextMenu(nameof(Localize))]
			public virtual void Localize() => Localizer.Localize(_localizationTemplate, OnLocalize).Forget();

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