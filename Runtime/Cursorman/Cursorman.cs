#if ENABLE_CURSORMAN


using Cysharp.Threading.Tasks;
using DragonResonance.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Scripting;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


namespace Praenaris.Cursorman
{
	[Preserve]
	public class Cursorman : ASubsystem<Cursorman, CursormanSettings>
	{
		private static readonly Dictionary<string, SCachedCursor> _cache = new();	// Filled progressively, as each preset is first used
		private static string _currentId = null;
		private static CancellationTokenSource _autoHideCancellation = null;
		private static float _lastMovementTime = 0f;
		private static bool _isAutoHidden = false;


		#region Events

			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
			private static void Initialize() => Startup(onStarting: Start);

			private static Task Start()
			{
				_cache.Clear();
				ListenPointer();
				Apply(null);	// Before the startup completes, so any Set() awaiting it is applied after the default
				return Task.CompletedTask;
			}

			#if ENABLE_INPUT_SYSTEM
			private static void OnPointerPerformed(InputAction.CallbackContext context) => _lastMovementTime = Time.unscaledTime;
			#endif

		#endregion


		#region Publics

			/// <param name="id">The preset ID, or null to reset to the default cursor</param>
			public static async void Set(string id)
			{
				await _starting.Task;
				Apply(id);
			}

			public static void Reset() => Set(null);

		#endregion


		#region Privates

			private static void Apply(string id)
			{
				if (string.IsNullOrEmpty(id))
					id = _settings.DefaultCursor;

				if (string.IsNullOrEmpty(id)) {
					StopAutoHide();
					Cursor.SetCursor(null, Vector2.zero, _settings.CursorMode);
					_currentId = null;
					return;
				}

				if (!TryGetCursor(id, out SCachedCursor cursor)) {
					Log.Error($"The cursor preset \"{id}\" does not exist");
					return;
				}

				StopAutoHide();
				Cursor.SetCursor(cursor.Texture, cursor.Hotspot, _settings.CursorMode);
				_currentId = id;

				if (cursor.Preset.AutoHide)
					StartAutoHide(cursor.Preset.AutoHideDelay);
			}

			private static bool TryGetCursor(string id, out SCachedCursor cursor)
			{
				if (_cache.TryGetValue(id, out cursor)) return true;

				foreach (CursorPreset preset in _settings.Presets) {
					if (preset.Id != id) continue;

					cursor = new SCachedCursor(preset, GetTexture(preset.Sprite), GetHotspot(preset.Sprite));
					_cache.Add(id, cursor);
					return true;
				}

				return false;
			}


			private static Texture2D GetTexture(Sprite sprite)
			{
				if (sprite == null) return null;

				Texture2D texture = sprite.texture;
				if (!texture.isReadable) {
					Log.Error($"Enable Read/Write in the import settings of \"{texture.name}\" to use \"{sprite.name}\" as a cursor");
					return null;
				}

				Rect rect = sprite.textureRect;
				Texture2D cursorTexture = new((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false) { name = sprite.name };	// The format the hardware cursor needs
				cursorTexture.SetPixels(texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
				cursorTexture.Apply();
				return cursorTexture;
			}

			private static Vector2 GetHotspot(Sprite sprite) =>
				((sprite == null) ? Vector2.zero : new Vector2(sprite.pivot.x, sprite.rect.height - sprite.pivot.y));	// Pivot from the bottom, hotspot from the top


			private static void ListenPointer()
			{
				#if ENABLE_INPUT_SYSTEM
				if (_settings.PointerInput == null) return;

				InputAction action = _settings.PointerInput.action;
				action.performed -= OnPointerPerformed;
				action.performed += OnPointerPerformed;
				if (!action.enabled)
					action.Enable();
				#endif
			}

			private static void StartAutoHide(float delay)
			{
				#if ENABLE_INPUT_SYSTEM
				if (_settings.PointerInput == null) {
					Log.Warning($"The cursor preset \"{_currentId}\" auto-hides, but there is no Pointer Input in the Cursorman settings to show it again");
					return;
				}

				_autoHideCancellation = CancellationTokenSource.CreateLinkedTokenSource(Application.exitCancellationToken);
				AutoHide(delay, _autoHideCancellation.Token).Forget();
				#else
				Log.Warning($"The cursor preset \"{_currentId}\" auto-hides, but that needs the Input System active");
				#endif
			}

			private static void StopAutoHide()
			{
				if (_autoHideCancellation != null) {
					_autoHideCancellation.Cancel();
					_autoHideCancellation.Dispose();
					_autoHideCancellation = null;
				}

				if (_isAutoHidden) {
					_isAutoHidden = false;
					Cursor.visible = true;
				}
			}

			private static async UniTaskVoid AutoHide(float delay, CancellationToken cancellationToken)
			{
				_lastMovementTime = Time.unscaledTime;
				while (!await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow()) {
					bool isIdle = ((Time.unscaledTime - _lastMovementTime) >= delay);
					if (isIdle == _isAutoHidden) continue;

					_isAutoHidden = isIdle;
					Cursor.visible = !isIdle;
				}
			}

		#endregion


		#region Properties

			public static string CurrentId => _currentId;

		#endregion




		private readonly struct SCachedCursor
		{
			public readonly CursorPreset Preset;
			public readonly Texture2D Texture;
			public readonly Vector2 Hotspot;

			public SCachedCursor(CursorPreset preset, Texture2D texture, Vector2 hotspot)
			{
				this.Preset = preset;
				this.Texture = texture;
				this.Hotspot = hotspot;
			}
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