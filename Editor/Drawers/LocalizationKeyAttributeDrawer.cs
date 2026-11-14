#if UNITY_EDITOR && ENABLE_LOCALIZER


using DragonResonance.Databases;
using DragonResonance.Localizer;
using Praenaris.Editor.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;


namespace DragonResonance.Editor.Drawers
{
	[CustomPropertyDrawer(typeof(LocalizationKeyAttribute))]
	public class LocalizationKeyAttributeDrawer : AComboBoxAttributeDrawer
	{
		private static readonly Regex CompositePartSuffix = new(@":\d+$");
		private static bool _loadRequested = false;	// Once per domain, a domain reload empties the data again


		#region Inheritables

			protected override IEnumerable<string> GetItems(SerializedProperty property)
			{
				HeaderedSheet<string> sheet = Localizer.Localizer.DataSheet;
				if (sheet.RowCount == 0) {
					LoadData();
					return Enumerable.Empty<string>();
				}

				int keysColumn = sheet.GetHeadingColumnIndex(Localizer.Localizer.Settings.KeysHeading);
				if (keysColumn == -1) return Enumerable.Empty<string>();

				return sheet.Data
					.Where(row => (keysColumn < row.Count))
					.Select(row => CompositePartSuffix.Replace(row[keysColumn] ?? string.Empty, string.Empty))	// Composite keys are used whole
					.Where(key => !string.IsNullOrWhiteSpace(key))
					.Select(key => $"{{{key}}}");
			}

			protected override string GetItemPath(string item)
			{
				int prefixLength = item.IndexOf('_');
				return ((prefixLength > 1) ? $"{item.Substring(1, (prefixLength - 1))}/{item}" : item);	// Grouped by prefix, MENU_TITLE goes under MENU
			}

		#endregion


		#region Privates

			private static async void LoadData()
			{
				if (_loadRequested) return;
				_loadRequested = true;

				if (Localizer.Localizer.Settings == null) {	// Not started, as in Edit Mode
					string settingsGuid = AssetDatabase.FindAssets($"t:{nameof(LocalizerSettings)}").FirstOrDefault();
					if (settingsGuid == null) return;
					Localizer.Localizer.Settings = AssetDatabase.LoadAssetAtPath<LocalizerSettings>(AssetDatabase.GUIDToAssetPath(settingsGuid));
				}

				await Localizer.Localizer.Reload();
				InternalEditorUtility.RepaintAllViews();
			}

		#endregion


		#region Properties

			protected override string UnknownItemTooltip => "Not a single key of the localization data.";

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