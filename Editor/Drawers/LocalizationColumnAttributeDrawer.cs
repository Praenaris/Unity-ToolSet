#if UNITY_EDITOR && ENABLE_LOCALIZER


using DragonResonance.Databases;
using DragonResonance.Localizer;
using Praenaris.Editor.Attributes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;


namespace DragonResonance.Editor.Drawers
{
	[CustomPropertyDrawer(typeof(LocalizationColumnAttribute))]
	public class LocalizationColumnAttributeDrawer : AComboBoxAttributeDrawer
	{
		#region Inheritables

			protected override IEnumerable<string> GetItems(SerializedProperty property)
			{
				HeaderedSheet<string> sheet = Localizer.Localizer.DataSheet;
				if (sheet.RowCount == 0) {
					LocalizationKeyAttributeDrawer.LoadData();
					return Enumerable.Empty<string>();	// The items will be ready in the next editor update
				}

				string keysHeading = Localizer.Localizer.Settings.KeysHeading;
				return sheet.HeadingRow.Where(heading => !string.IsNullOrWhiteSpace(heading) && (heading != keysHeading));
			}

		#endregion


		#region Properties

			protected override string UnknownItemTooltip => "No column of the localization data in the text.";

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