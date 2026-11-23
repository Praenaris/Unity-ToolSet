#if ENABLE_SAVEDATA


using Praenaris.Tools;
using System.IO;
using System;
using UnityEngine;


namespace Praenaris.Savedata
{
	[Serializable]
	public struct SavedataResource
	{
		public SavedataRootPath Root;
		public bool Company;
		public bool Product;
		public string RelativePath;
		public bool Slotted;
		public string[] Keys;


		public string GetFullPath(int slot) => FormatFullPath(slot.ToString());

		internal string FormatFullPath(string slotId)
		{
			string relativePath = Path.Join(
				this.Company ? Application.companyName : null,
				this.Product ? Application.productName : null,
				this.RelativePath);
			if (this.Slotted)
				relativePath = Path.Join(
					Path.GetDirectoryName(relativePath),
					$"{Path.GetFileNameWithoutExtension(relativePath)}_{slotId}{Path.GetExtension(relativePath)}");
			return Fileman.ResolvePath(this.Root, relativePath);
		}
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