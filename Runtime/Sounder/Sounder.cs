#if ENABLE_SOUNDER


using Cysharp.Threading.Tasks;
using DragonResonance.Extensions;
using UnityEngine.Scripting;
using UnityEngine;


namespace DragonResonance.Sounder
{
	[Preserve]
	public class Sounder
	{
		private static SounderSettings _settings = null;
		private static readonly UniTaskCompletionSource _starting = new();


		#region Events

			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
			private static void Initialize() => OnStartup();

			private static async void OnStartup()
			{
				Logging.Log.Info("Starting up...");

				_settings = await SounderSettings.GetInstanceAsync();
				_starting.TrySetResult();

				Logging.Log.Info("Started!");
			}

		#endregion


		#region Publics

			public static void Play(AudioSourceConfig audioSourceConfig) =>
				PlayAndAwait(audioSourceConfig).Forget();

			public static void Play(AudioSourceConfig audioSourceConfig, AudioSource audioSource) =>
				SetupPooledAudioSource(audioSourceConfig, audioSource);


			public static async UniTask PlayAndAwait(AudioSourceConfig audioSourceConfig)
			{
				await _starting.Task;
				AudioSource audioSource = SetupPooledAudioSource(audioSourceConfig);
				await SounderPool.Current.ReleaseWhenDoneAsync(audioSource);
			}

			public static async UniTask PlayAndAwait(AudioSourceConfig audioSourceConfig, AudioSource audioSource)
			{
				SetupPooledAudioSource(audioSourceConfig, audioSource);
				if (audioSourceConfig.AudioResource is AudioClip)
					await UniTask.WaitUntil(audioSource.HasAudioClipStopped);
				else
					await UniTask.WaitUntil(audioSource.HasAudioResourceStopped);
			}


			public static async UniTask<AudioSource> PlayAndGet(AudioSourceConfig audioSourceConfig, bool autoReleaseWhenDone = true)
			{	// ReSharper disable MethodHasAsyncOverload
				await _starting.Task;
				AudioSource audioSource = SetupPooledAudioSource(audioSourceConfig);
				if (autoReleaseWhenDone)
					SounderPool.Current.ReleaseWhenDone(audioSource);
				return audioSource;
			}	// ReSharper restore MethodHasAsyncOverload


			public static void Stop(AudioSource audioSource) => audioSource.Stop();
			public static void Release(AudioSource audioSource) => SounderPool.Current.Release(audioSource);

		#endregion


		#region Privates

			private static AudioSource SetupPooledAudioSource(AudioSourceConfig audioSourceConfig) =>
				SetupPooledAudioSource(audioSourceConfig, SounderPool.Instance.Get());
			private static AudioSource SetupPooledAudioSource(AudioSourceConfig audioSourceConfig, AudioSource audioSource)
			{
				audioSource.resource = audioSourceConfig.AudioResource;
				audioSource.outputAudioMixerGroup = audioSourceConfig.AudioMixerGroup;
				audioSource.Play();
				return audioSource;
			}

		#endregion


		#region Properties

			public static UniTaskCompletionSource Starting => _starting;

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