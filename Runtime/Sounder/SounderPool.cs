#if ENABLE_SOUNDER


using Cysharp.Threading.Tasks;
using DragonResonance.Behaviours;
using DragonResonance.Extensions;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Pool;
using UnityEngine;


namespace DragonResonance.Sounder
{
	public class SounderPool : PersistentSingletonPossumBehaviour<SounderPool>
	{
		private const string POOLED_AUDIOSOURCE_NAME = "PooledAudioSource";

		[SerializeField] private SounderSettings _settings = null;

		private ObjectPool<AudioSource> _pool = null;


		#region Events

			protected override void LateAwake() => InitializePool();

		#endregion


		#region Publics

			public AudioSource Get() => this.Pool.Get();
			public void Release(AudioSource item) => this.Pool.Release(item);

			public void ReleaseWhenDone(AudioSource item) => ReleaseWhenDoneAsync(item).Forget();
			public async UniTask ReleaseWhenDoneAsync(AudioSource audioSourceItem)
			{
				CancellationToken cancellationToken = audioSourceItem.GetCancellationTokenOnDestroy();
				await UniTask.WaitUntil(audioSourceItem.HasStopped, cancellationToken:cancellationToken);
				Release(audioSourceItem);
			}

		#endregion


		#region Privates

			private void InitializePool()
			{
				if (_pool != null) return;
				Log($"Generating {_settings.StartingPoolAmount} items...");
				_pool = new ObjectPool<AudioSource>(
					createFunc: CreateItem,
					actionOnGet: OnItemGet,
					actionOnRelease: OnItemRelease,
					actionOnDestroy: OnItemDestroy,
					defaultCapacity: _settings.StartingPoolSize,
					maxSize: _settings.MaxPoolSize,
					collectionCheck: false
				);
				Populate(_settings.StartingPoolAmount);
			}


			private void Populate(int amount)
			{
				Queue<AudioSource> populatedItems = new(amount);
				for (int itemIndex = 0; itemIndex < amount; itemIndex++)
					populatedItems.Enqueue(this.Pool.Get());
				while (populatedItems.TryDequeue(out AudioSource item))
					this.Pool.Release(item);
			}

			private AudioSource CreateItem()
			{
				GameObject gameObject = new(POOLED_AUDIOSOURCE_NAME);
				gameObject.transform.SetParent(this.transform);

				AudioSource audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.enabled = false;
				audioSource.playOnAwake = false;

				UpdatePooledAudioSourceName(audioSource);
				return audioSource;
			}

			private void OnItemGet(AudioSource audioSource)
			{
				audioSource.enabled = true;
				UpdatePooledAudioSourceName(audioSource);
			}

			private void OnItemRelease(AudioSource audioSource)
			{
				audioSource.enabled = false;
				UpdatePooledAudioSourceName(audioSource);
			}

			private void OnItemDestroy(AudioSource audioSource)
			{
				DestroyDynamically(audioSource.gameObject);
			}

			private void UpdatePooledAudioSourceName(AudioSource audioSource)
			{
				audioSource.name = $"{POOLED_AUDIOSOURCE_NAME} ({(audioSource.enabled ? "Playing" : "Idle")})";
			}

		#endregion


		#region Properties

			public ObjectPool<AudioSource> Pool
			{
				get {
					InitializePool();
					return _pool;
				}
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