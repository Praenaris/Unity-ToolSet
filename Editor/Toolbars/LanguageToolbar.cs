#if UNITY_EDITOR && ENABLE_LOCALIZER


using DragonResonance.Localizer;
using UnityEditor.Toolbars;
using UnityEditor;
using UnityEngine;


namespace Praenaris.Editor.Toolbars
{
	[InitializeOnLoad]
	internal static class LanguageToolbar
	{
		private const string DROPDOWN_PATH = "Language/Dropdown";


		#region Constructors

			static LanguageToolbar()
			{
				EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
				Localizer.OnLanguageChange += RefreshDropdown;
			}

		#endregion


		#region Events

			private static void OnPlayModeStateChanged(PlayModeStateChange state)
			{
				if ((state == PlayModeStateChange.EnteredPlayMode) || (state == PlayModeStateChange.EnteredEditMode))
					RefreshDropdown();
			}

		#endregion


		#region Publics

			[MainToolbarElement(DROPDOWN_PATH, defaultDockPosition = MainToolbarDockPosition.Right)]
			public static MainToolbarElement CreateDropdown() =>	// Called again on every refresh
				new MainToolbarDropdown(new MainToolbarContent((EditorApplication.isPlaying ? Localizer.CurrentLanguage.ToString() : "Language"), "Localizer.ChangeLanguage, only in Play Mode"), ShowMenu) {
					enabled = EditorApplication.isPlaying,
				};

		#endregion


		#region Privates

			private static void ShowMenu(Rect dropdownRect)
			{
				GenericMenu menu = new();
				foreach (SystemLanguage language in Localizer.AvailableLanguages)
					menu.AddItem(new GUIContent(language.ToString()), (language == Localizer.CurrentLanguage), () => ChangeLanguage(language));
				if (menu.GetItemCount() == 0)
					menu.AddDisabledItem(new GUIContent("No languages"));
				menu.DropDown(dropdownRect);
			}

			private static void ChangeLanguage(SystemLanguage language)
			{
				if (!EditorApplication.isPlaying) return;
				Localizer.ChangeLanguage(language);
				RefreshDropdown();
			}

			private static void RefreshDropdown() => MainToolbar.Refresh(DROPDOWN_PATH);

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