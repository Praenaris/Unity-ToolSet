#if UNITY_EDITOR && ENABLE_SOUNDER


using DragonResonance.Extensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace DragonResonance.Sounder
{
	public static class EditorSounderPreview
	{
		private const string PreviewObjectName = "SounderPreviewAudioSource";

		private static readonly List<AudioSource> _audioSources = new();


		#region Constructors

			static EditorSounderPreview()
			{
				AssemblyReloadEvents.beforeAssemblyReload += StopAll;
				EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			}

		#endregion


		#region Events

			private static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
			{
				if ((stateChange == PlayModeStateChange.ExitingEditMode) || (stateChange == PlayModeStateChange.ExitingPlayMode))
					StopAll();
			}

			private static void OnEditorUpdate()
			{
				for (int sourceIndex = _audioSources.LastIndex(); sourceIndex.IsPositiveOrZero(); sourceIndex--) {
					AudioSource audioSource = _audioSources[sourceIndex];
					if ((audioSource == null) || audioSource.HasStopped()) {
						Release(audioSource);
						_audioSources.RemoveAt(sourceIndex);
					}
				}

				if (_audioSources.IsEmpty())
					EditorApplication.update -= OnEditorUpdate;
			}

		#endregion


		#region Publics

			public static AudioSource Play(AudioSourceConfig audioSourceConfig)
			{
				GameObject gameObject = EditorUtility.CreateGameObjectWithHideFlags(PreviewObjectName, HideFlags.HideAndDontSave, typeof(AudioSource));
				AudioSource audioSource = gameObject.GetComponent<AudioSource>();
				audioSource.playOnAwake = false;
				audioSource.resource = audioSourceConfig.AudioResource;
				audioSource.outputAudioMixerGroup = audioSourceConfig.AudioMixerGroup;
				audioSource.Play();

				if (_audioSources.IsEmpty())
					EditorApplication.update += OnEditorUpdate;
				_audioSources.Add(audioSource);
				return audioSource;
			}

			public static void StopAll()
			{
				foreach (AudioSource audioSource in _audioSources)
					Release(audioSource);
				_audioSources.Clear();
				EditorApplication.update -= OnEditorUpdate;
			}

		#endregion


		#region Privates

			private static void Release(AudioSource audioSource)
			{
				if (audioSource != null)
					UnityObject.DestroyImmediate(audioSource.gameObject);
			}

		#endregion


		#region Properties

			public static bool IsPlaying => !_audioSources.IsEmpty();

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