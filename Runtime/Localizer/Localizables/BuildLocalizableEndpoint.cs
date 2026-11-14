#if ENABLE_LOCALIZER


using DragonResonance.Attributes;
using UnityEngine;


namespace DragonResonance.Localizer
{
	public class BuildLocalizableEndpoint : LocalizableEvent
	{
		[SerializeField] private bool _enableEditorTemplate = false;
		[ShowIf(nameof(_enableEditorTemplate))] [SerializeField] private string _localizationEditorTemplate = "This is a editor {TEST}";

		[SerializeField] private bool _enableDevelopmentTemplate = false;
		[ShowIf(nameof(_enableDevelopmentTemplate))] [SerializeField] private string _localizationDevelopmentTemplate = "This is a development {TEST}";

		[SerializeField] private bool _enableDemoTemplate = false;
		[ShowIf(nameof(_enableDemoTemplate))] [SerializeField] private string _localizationDemoTemplate = "This is a demo {TEST}";


		#region Properties

		#if UNITY_EDITOR
			protected override string LocalizationTemplate => (_enableEditorTemplate ? _localizationEditorTemplate : _localizationTemplate);
		#elif DEVELOPMENT_BUILD
			protected override string LocalizationTemplate => (_enableDevelopmentTemplate ? _localizationDevelopmentTemplate : _localizationTemplate);
		#elif DEMO_BUILD
			protected override string LocalizationTemplate => (_enableDemoTemplate ? _localizationDemoTemplate : _localizationTemplate);
		#endif

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