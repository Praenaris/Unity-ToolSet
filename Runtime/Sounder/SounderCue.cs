#if ENABLE_SOUNDER


using DragonResonance.Extensions;
using UnityEngine.Audio;
using UnityEngine;


namespace DragonResonance.Sounder
{
	[CreateAssetMenu(menuName = "Praenaris/Sounder/Cue", fileName = "New Sounder Cue")]
	public class SounderCue : ScriptableObject
	{
		//[SerializeField] private AudioResource[] _audioResources = { };
		[SerializeField] private AudioResource _audioResource = null;
		[SerializeField] private AudioMixerGroup _audioMixerGroup = null;


		#region Publics

			public void Play() => Sounder.Play(GetConfig());

			/*public AudioSourceConfig GetConfig() => new() {
				AudioResource = (_audioResources.IsEmpty() ? null : _audioResources.Random()),
				AudioMixerGroup = _audioMixerGroup,
			};*/

			public AudioSourceConfig GetConfig() => new() {
				AudioResource = _audioResource,
				AudioMixerGroup = _audioMixerGroup,
			};

		#endregion


		#region Properties

			//public AudioResource[] AudioResources => _audioResources;
			public AudioResource AudioResource => _audioResource;
			public AudioMixerGroup AudioMixerGroup => _audioMixerGroup;

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