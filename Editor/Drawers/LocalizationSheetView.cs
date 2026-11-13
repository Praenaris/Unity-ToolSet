#if UNITY_EDITOR && ENABLE_LOCALIZER


using DragonResonance.Databases;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;


namespace DragonResonance.Editor.Drawers
{
	public class LocalizationSheetView : TreeView<int>
	{
		private const float KeyColumnWidth = 160f;
		private const float LanguageColumnWidth = 240f;
		private const float MinColumnWidth = 40f;

		private static readonly Color EmptyCellColor = new(0.9f, 0.2f, 0.2f, 0.25f);

		private readonly SearchField _searchField = new();
		private HeaderedSheet<string> _sheet = null;
		private List<string> _headingRow = null;
		private int _columnCount = 0;
		private int _rowCount = 0;


		#region Constructors

			public LocalizationSheetView() : base(new TreeViewState<int>())
			{
				base.showAlternatingRowBackgrounds = true;
				base.showBorder = true;
				_searchField.downOrUpArrowKeyPressed += base.SetFocusAndEnsureSelectedItem;
			}

		#endregion


		#region Publics

			public void OnGUI(Rect position, HeaderedSheet<string> sheet)
			{
				if (HasChanged(sheet))
					Rebuild(sheet);

				float searchHeight = EditorGUIUtility.singleLineHeight;
				float tableY = (position.y + searchHeight + EditorGUIUtility.standardVerticalSpacing);
				base.searchString = _searchField.OnGUI(new Rect(position.x, position.y, position.width, searchHeight), base.searchString);
				base.OnGUI(new Rect(position.x, tableY, position.width, (position.yMax - tableY)));
			}

		#endregion


		#region Inheritables

			protected override TreeViewItem<int> BuildRoot()
			{
				TreeViewItem<int> root = new(-1, -1, "Root");
				List<TreeViewItem<int>> items = new();
				for (int rowIndex = 1; rowIndex < _sheet.RowCount; rowIndex++)	// The first row is the heading
					items.Add(new TreeViewItem<int>(rowIndex, 0, (_sheet.Data[rowIndex].FirstOrDefault() ?? string.Empty)));

				if (items.Count > 0)
					SetupParentsAndChildrenFromDepths(root, items);
				else
					root.children = new List<TreeViewItem<int>>();	// The tree view needs it even when empty
				return root;
			}

			protected override void RowGUI(RowGUIArgs args)
			{
				List<string> row = _sheet.Data[args.item.id];
				for (int visibleIndex = 0; visibleIndex < args.GetNumVisibleColumns(); visibleIndex++) {
					int columnIndex = args.GetColumn(visibleIndex);
					Rect cellRect = args.GetCellRect(visibleIndex);
					string cell = ((columnIndex < row.Count) ? row[columnIndex] : null);

					if (string.IsNullOrWhiteSpace(cell)) {
						EditorGUI.DrawRect(cellRect, EmptyCellColor);
						continue;
					}

					base.CenterRectUsingSingleLineHeight(ref cellRect);
					EditorGUI.LabelField(cellRect, new GUIContent(ToSingleLine(cell), cell));	// The full text as tooltip
				}
			}

			protected override bool DoesItemMatchSearch(TreeViewItem<int> item, string search) =>
				_sheet.Data[item.id].Any(cell => ((cell != null) && cell.Contains(search, StringComparison.OrdinalIgnoreCase)));

		#endregion


		#region Privates

			private bool HasChanged(HeaderedSheet<string> sheet) =>
				(!ReferenceEquals(_sheet, sheet) || !ReferenceEquals(_headingRow, sheet.HeadingRow) ||
				(_columnCount != (sheet.HeadingRow?.Count ?? 0)) || (_rowCount != sheet.RowCount));

			private void Rebuild(HeaderedSheet<string> sheet)
			{
				_sheet = sheet;
				_headingRow = sheet.HeadingRow;
				_columnCount = (_headingRow?.Count ?? 0);
				_rowCount = sheet.RowCount;

				MultiColumnHeaderState.Column[] columns = (_headingRow ?? new List<string>())
					.Select((heading, columnIndex) => new MultiColumnHeaderState.Column {
						headerContent = new GUIContent(heading),
						width = ((columnIndex == 0) ? KeyColumnWidth : LanguageColumnWidth),
						minWidth = MinColumnWidth,
						autoResize = false,
						allowToggleVisibility = (columnIndex > 0),	// The keys always stay
						canSort = false,
					})
					.ToArray();
				base.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(columns));
				base.Reload();
			}


			private static string ToSingleLine(string text) => text.Replace("\r", string.Empty).Replace("\n", " \u21B5 ");

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