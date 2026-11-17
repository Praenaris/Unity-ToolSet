#if UNITY_EDITOR && ENABLE_SOUNDER


using UnityEditor;
using UnityEngine;


namespace DragonResonance.Sounder
{
	[CustomEditor(typeof(SounderCue))]
	public class SounderCueEditor : UnityEditor.Editor
	{
		private const string PlayLabel = "Play";
		private const string RestartLabel = "Restart";
		private const string StopLabel = "Stop";


		#region Publics

			public override bool RequiresConstantRepaint() => EditorSounderPreview.IsPlaying;

			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();
				{
					if (GUILayout.Button(PlayLabel))
						Play();
					if (GUILayout.Button(RestartLabel))
						Restart();
					using (new EditorGUI.DisabledScope(!EditorSounderPreview.IsPlaying)) {
						if (GUILayout.Button(StopLabel))
							Stop();
					}
				}
				EditorGUILayout.EndHorizontal();
			}

		#endregion


		#region Privates

			private void Play() => EditorSounderPreview.Play(((SounderCue)base.target).GetConfig());
			private void Stop() => EditorSounderPreview.StopAll();

			private void Restart()
			{
				Stop();
				Play();
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