#if UNITY_EDITOR


using UnityEditor.Toolbars;
using UnityEditor;
using UnityEngine;


namespace Praenaris.Editor.Toolbars
{
	[InitializeOnLoad]
	internal static class TimeScaleToolbar
	{
		private const float MAX_TIME_SCALE = 4f;
		private const float MIN_TIME_SCALE = 0f;
		private const string RESET_PATH = "Time Scale/Reset";
		private const string SLIDER_PATH = "Time Scale/Slider";


		private static float _shownTimeScale = 1f;


		#region Constructors

			static TimeScaleToolbar()
			{
				EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
				EditorApplication.update += OnEditorUpdate;
			}

		#endregion


		#region Events

			private static void OnPlayModeStateChanged(PlayModeStateChange state)
			{
				if ((state == PlayModeStateChange.EnteredPlayMode) || (state == PlayModeStateChange.EnteredEditMode))
					RefreshElements();
			}

			private static void OnEditorUpdate()
			{
				if (EditorApplication.isPlaying && !Mathf.Approximately(Time.timeScale, _shownTimeScale))
					MainToolbar.Refresh(SLIDER_PATH);
			}

		#endregion


		#region Publics

			[MainToolbarElement(SLIDER_PATH, defaultDockPosition = MainToolbarDockPosition.Middle)]
			public static MainToolbarElement CreateSlider()	// Called again on every refresh
			{
				_shownTimeScale = Time.timeScale;
				return new MainToolbarSlider(new MainToolbarContent("Time Scale", "Time.timeScale, only in Play Mode"), _shownTimeScale, MIN_TIME_SCALE, MAX_TIME_SCALE, SetTimeScale) {
					enabled = EditorApplication.isPlaying,
					populateContextMenu = menu => menu.AppendAction("Reset", _ => ResetTimeScale()),
				};
			}

			[MainToolbarElement(RESET_PATH, defaultDockPosition = MainToolbarDockPosition.Middle)]
			public static MainToolbarElement CreateResetButton() =>	// Called again on every refresh
				new MainToolbarButton(new MainToolbarContent((Texture2D)EditorGUIUtility.IconContent("Refresh").image, "Reset the time scale to 1"), ResetTimeScale) {
					enabled = EditorApplication.isPlaying,
				};

		#endregion


		#region Privates

			private static void SetTimeScale(float timeScale)
			{
				if (!EditorApplication.isPlaying) return;
				Time.timeScale = _shownTimeScale = timeScale;
			}

			private static void ResetTimeScale()
			{
				SetTimeScale(1f);
				MainToolbar.Refresh(SLIDER_PATH);
			}

			private static void RefreshElements()
			{
				MainToolbar.Refresh(SLIDER_PATH);
				MainToolbar.Refresh(RESET_PATH);
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