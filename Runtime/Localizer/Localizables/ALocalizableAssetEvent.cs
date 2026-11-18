#if ENABLE_LOCALIZER


using UnityEngine.Events;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace DragonResonance.Localizer
{
	public abstract class ALocalizableAssetEvent<TAsset> : ALocalizable where TAsset : UnityObject
	{
		public UnityEvent<TAsset> Target = null;

		private int _lastRequest = 0;


		#region Inheritables

			protected override void OnLocalized(string localizedText)	// The asset path inside any Resources folder without extension
			{
				string resourcePath = localizedText.Trim();
				int request = ++_lastRequest;

				if (!Application.isPlaying) {	// Edit Mode has no player loop to complete an async load
					OnResourceLoaded(request, resourcePath, Resources.Load<TAsset>(resourcePath));
					return;
				}

				ResourceRequest loading = Resources.LoadAsync<TAsset>(resourcePath);
				loading.completed += _ => OnResourceLoaded(request, resourcePath, (loading.asset as TAsset));
			}


			protected virtual void OnResourceLoaded(int request, string resourcePath, TAsset asset)
			{
				if (request != _lastRequest) return;	// A newer request superseded this one

				if (asset == null)
					Error($"No {typeof(TAsset).Name} at Resources/{resourcePath} for the {base.LocalizationColumn} column");
				else
					this.Target?.Invoke(asset);
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