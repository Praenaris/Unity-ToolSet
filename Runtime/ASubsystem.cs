using DragonResonance.Behaviours;
using System.Threading.Tasks;
using System;


namespace Praenaris
{
	public abstract class ASubsystem<TSelf, TSettings>
		where TSelf : ASubsystem<TSelf, TSettings>
		where TSettings : SingletonScriptableObject<TSettings>
	{
		protected static TSettings _settings = null;
		// ReSharper disable once StaticMemberInGenericType
		protected static readonly TaskCompletionSource<bool> _starting = new();


		#region Inheritables

			protected static async void Startup(Func<Task> onStarting = null, Func<Task> onStarted = null)
			{
				_settings = await SingletonScriptableObject<TSettings>.GetInstanceAsync();
				if (onStarting != null) await onStarting();
				_starting.TrySetResult(true);
				if (onStarted != null) await onStarted();
			}

		#endregion


		#region Properties

			public static Task<bool> StartingTask => _starting.Task;

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