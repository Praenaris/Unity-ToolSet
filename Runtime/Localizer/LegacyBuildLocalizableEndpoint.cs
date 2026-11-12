#if ENABLE_LOCALIZER


using DragonResonance.Attributes;
using DragonResonance.Localizer;
using UnityEngine;


public class LegacyBuildLocalizableEndpoint : LegacyLocalizableEndpoint
{
	[SerializeField] private bool _enableEditorTemplate = false;
	[ShowIf(nameof(_enableEditorTemplate))] [SerializeField] protected string _localizationEditorTemplate = "This is a editor {TEST}";

	[SerializeField] private bool _enableDevelopmentTemplate = false;
	[ShowIf(nameof(_enableDevelopmentTemplate))] [SerializeField] protected string _localizationDevelopmentTemplate = "This is a development {TEST}";

	[SerializeField] private bool _enableDemoTemplate = false;
	[ShowIf(nameof(_enableDemoTemplate))] [SerializeField] protected string _localizationDemoTemplate = "This is a demo {TEST}";


	#if UNITY_EDITOR

		public override void Localize()
		{
			if (_enableEditorTemplate)
				Localizer.Localize(_localizationEditorTemplate, OnLocalize.Invoke);
			else
				base.Localize();
		}

	#elif DEVELOPMENT_BUILD

		public override void Localize()
		{
			if (_enableDevelopmentTemplate)
				Localizer.Localize(_localizationDevelopmentTemplate, OnLocalize.Invoke);
			else
				base.Localize();
		}

	#elif DEMO_BUILD

		public override void Localize()
		{
			if (_enableDemoTemplate)
				Localizer.Localize(_localizationDemoTemplate, OnLocalize.Invoke);
			else
				base.Localize();
		}

	#endif
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