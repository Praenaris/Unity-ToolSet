# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

**Praenaris ToolSet** (`com.praenaris.toolset`, published on OpenUPM): a Unity 6 package of larger, opt-in runtime subsystems with their Project Settings pages, built on top of **Praenaris DevKit** (`com.praenaris.devkit`). It is a generic, publicly distributed package: nothing in it (code, comments, docs, this file) may reference or depend on any specific project that uses it.

For development the repo is usually cloned as an **embedded package** into some Unity project's `Packages/` folder (often next to an embedded DevKit clone), so edits compile in that project's open Editor immediately.

Dependencies (`package.json`): DevKit, `dev.tabernero.simplejson` (namespace `Tabernero.SimpleJSON`), `com.cysharp.unitask`; **Steamworks.NET is optional**. When ToolSet starts using a DevKit API that isn't published yet, the `com.praenaris.devkit` pin in `package.json` has to move to the DevKit release that ships it.

## Assembly layout

There is **one asmdef**, `Praenaris.ToolSet.asmdef` (`rootNamespace: Praenaris`, `autoReferenced`), at the repo root. There is **no Editor asmdef**: everything under `Editor/` compiles into the same runtime assembly, so **every Editor file is wrapped whole in `#if UNITY_EDITOR` … `#endif`** (before the usings, after the namespace). New editor code must do the same, or player builds break.

`versionDefines` in the asmdef turn installed packages into defines: `SIMPLEJSON`, `UNITASK`, and `STEAMWORKS_INTEGRATION` (only when `com.rlabrecque.steamworks.net` is installed). Steamworks calls are always behind `#if STEAMWORKS_INTEGRATION`, and the code must compile without it.

```
Runtime/
├── ASubsystem.cs          base of the static subsystems
├── Fileman/               async local file read/write + UnityWebRequest fetch (Praenaris.Tools)
├── Savedata/              JSON savedata in slots and resources (+ Legacy/)
├── Localizer/             string tables from local resources / streaming assets / online sheets
├── Prefabricator/         spawns prefabs from settings on game start
├── Sounder/               pooled AudioSource player (+ Animator/ state hooks)
└── Integration/           platform façade: Achievements, RichPresence, Steamworks/SteamManager
Editor/
├── Building/              Subsystems/ menu togglers (one per subsystem) + Steamworks disabler
├── Drawers/               property drawers / custom editors for the subsystem data
├── Settings/              Project Settings providers under Project/Praenaris/…
└── Sounder/               SAudioSourceConfig drawer
Resources/[SteamManager].prefab
```

## Subsystem defines

Every subsystem is compiled out unless its define is on: `ENABLE_SAVEDATA`, `ENABLE_LOCALIZER`, `ENABLE_PREFABRICATOR`, `ENABLE_SOUNDER`, `ENABLE_INTEGRATION`. **Each file of a subsystem is wrapped whole in its define.**

- The togglers in `Editor/Building/*SystemToggler.cs` are `[InitializeOnLoad]`: their static constructor registers the define in its *off* form (`_ENABLE_X`, leading underscore = off) through DevKit's `BuildDefines.SetDefinition` if the project has neither form yet, and their `Subsystems/<Name> [ON|OFF]` menu item flips it. A new subsystem gets its own toggler copied from these.
- `DisableSteamworksToggler` flips `DISABLESTEAMWORKS` (Steamworks.NET's own switch) from the `Integration/Steamworks` menu.
- Settings providers must compile **in both states**: under `#if ENABLE_X` they derive from `AScriptableSettingsProvider<XSettings>`, otherwise from the non-generic `AScriptableSettingsProvider`, and they show an "Enabled" toggle that calls `BuildDefines.SetDefinitionState` (see `SavedataSettingsProvider`).

## How a subsystem is built

- **Static class + settings asset.** `ASubsystem<TSelf, TSettings>` (`Runtime/ASubsystem.cs`, namespace `Praenaris`) holds the static `_settings` and a `_starting` `TaskCompletionSource`. The subsystem marks itself `[Preserve]`, hooks a `[RuntimeInitializeOnLoadMethod]` that calls `Startup(onStarting, onStarted)`, and public entry points `await _starting.Task` before touching state. `Startup` awaits `SingletonScriptableObject<TSettings>.GetInstanceAsync()`.
- **Settings are a DevKit `SingletonScriptableObject<T>`** (`[CreateAssetMenu(menuName = "Praenaris/Settings/<Name>")]`, public fields). Its instance is only set in the asset's `Awake`/`OnValidate`, and `GetInstanceAsync` polls every frame until then, so **the asset must be loaded at startup**: `AScriptableSettingsProvider<TSettings>` finds the first asset of that type (or creates `Assets/<TypeName>.asset`) and adds it to **Preloaded Assets** via DevKit's `PreloadedAssets`. If the settings page was never opened, a subsystem can wait forever.
- `ASubsystem.Settings` (and e.g. `Savedata.Data`) have an `internal` setter so the settings editors can drive the subsystem from Edit Mode.
- **Settings pages** derive from `AScriptableSettingsProvider<TSettings>` at `Project/Praenaris/<Name>` (Integration modules nest under `Project/Praenaris/Integration/<Module>` through `AIntegrationModuleSettingsProvider`). It draws every visible property; override `OnPropertyGUI` (return `true` if you drew it), `OnBeforeGUI`, `OnAfterGUI`. `Project/Praenaris` itself is `PraenarisEditorSettingsProvider` (banner, links), backed by a `ScriptableSingleton` in `ProjectSettings/PraenarisSettings.asset`.
- **Async**: `ASubsystem`, Savedata and Fileman use `System.Threading.Tasks`; Sounder and Localizer use UniTask. Match the subsystem you're in.

## Subsystems

**Savedata** (`Praenaris.Savedata`, the reworked one): one in-memory `JSONNode` (`Savedata.Data`) split across **resources** (`SavedataResource`: root folder enum `SavedataRootPath`, optional company/product subfolders, relative path, `Slotted` adds `_<slot>` to the file name, `Keys`). `SavedataSettings.Resources` is the `FallbackResource` followed by the `ResourceOverrides`. On load all files are read in parallel and merged, later resources winning; on save each top-level key goes to the **last** resource listing it, unlisted keys go to the fallback. The settings' `SavedataVersion` is stamped under `SavedataVersionKey` on save, and `OnLoaded` reports `SavedataLoadState` (`FreshNew` / `OlderVersion` / `LatestVersion`); migrating older versions is up to the consuming code. The current slot persists in PlayerPrefs (`SAVEDATA_CURRENTSLOT`); `LoadOnGameStart` loads it at `AfterAssembliesLoaded`. Load/Save are serialized by a semaphore and clear `IsReady` while running. Accessors live in `Savedata.GettersSetters.cs`: `Get*`/`TryGet*` with fallbacks, `Set*` aliases of `TrySet*`, which **return `false` and drop the value while `IsReady` is false**; whole objects go through `ISavableNode` (`JSONNode`) or `ISavableString`. The settings page has a slot browser that loads, edits and saves the raw JSON.

**Savedata Legacy** (`Praenaris.SavedataLegacy`, `Runtime/Savedata/Legacy/`, `ISavableData` + wrappers, `SavedataLegacySettings` and its own settings page): the previous API, kept compiling under the same `ENABLE_SAVEDATA` for projects still on it. Fix it if it breaks, don't extend it.

**Fileman** (`Praenaris.Tools`, always compiled): `ReadFromFile` / `WriteToFile` (UTF-8 default, `null` when missing), `FetchWebResource` / `FetchToFile`, plus Editor-only `TextAsset` variants.

**Prefabricator** (`DragonResonance.Prefabricator`): at `AfterSceneLoad` instantiates each `SPrefabricable` (`Prefab`, `Amount`, `Persistent` → `DontDestroyOnLoad`), named after the prefab. Await `Prefabricator.SpawningTask` to know they exist.

**Sounder** (`DragonResonance.Sounder`): `Sounder.Play` / `PlayAndAwait` / `PlayAndGet` take an `SAudioSourceConfig` (an `AudioResource` + mixer group) and use a pooled `AudioSource` from `SounderPool`, a DevKit `PersistentSingletonPossumBehaviour` that releases sources when they finish; `AnimatorStateSounderPlayer` plays configs from Animator states. Sounder predates `ASubsystem` and still has its own UniTask startup.

**Localizer** (`DragonResonance.Localizer`, partial `Localizer` + `.Local` + `.Online`): loads string tables into a DevKit `HeaderedSheet<string>` (languages as columns, keys as rows) from `LocalizerSettings` sources; online sources are fetched first only in the Editor and development builds. `Localize(template)` replaces `{Key}`, joining `Key:1`, `Key:2`… lines for composite keys; the language is the first of system language + `LanguageFallbacks` present in the sheet; `OnLanguageChange` fires on `ChangeLanguage`.

**Integration** (`Praenaris.Integration`): `IntegrationSettings`, plus modules with their own settings and pages. **Achievements**: `AchievementsSettings.Achievements` is a list of `Achievement` structs (name, Steam achievement/stat IDs, optional progression stat range, icons); the custom editor (`Editor/Drawers/AchievementsSettingsEditor.cs`) keeps one `AchievementReference` sub-asset per entry inside the settings asset, matched by the `[ReadOnly] ChannelGuid` it generates, so game objects can reference a single achievement and call `Progress` / `Achieve` / `Unachieve`. Steam rejects stat values outside the stat's range, hence the clamping in `Achievement.Progress`. **RichPresence**: settings page only so far. `Integration/Steamworks/SteamManager.cs` is the community Steamworks.NET `SteamManager` (global namespace, its own style; leave it as is), spawned from `Resources/[SteamManager].prefab`.

## Code style

The DevKit/ToolSet source *is* the maintainer's house style; read a file of the same subsystem end-to-end before adding one. In short:

- `.editorconfig`: tabs, LF, UTF-8, **no trailing newline**.
- Allman braces for namespaces, types and methods; K&R for control flow, with `else` on its own line after `}`. Single-statement bodies drop braces onto the next indented line; guard clauses stay inline (`if (_pool != null) return;`).
- `#region` order **Constructors → Events → Publics → Inheritables → Privates → Properties**, contents indented one extra level; split with suffixes (`#region Publics - Setters`). Two blank lines between usings and the namespace, around regions and between member groups.
- `using` ordinal-alphabetical with children before their parent (`System.Threading.Tasks;` then `System.Threading;` then `System;`). Usings only needed under a subsystem define go in a separate `#if` block after the others.
- Private fields `_camelCase`, always explicitly initialized; settings assets use public PascalCase fields. Lazy caches `_x_internal` with a `// Caching only, use the property instead` comment, and **`[NonSerialized]` when they live on a ScriptableObject or EditorWindow**. Read own properties through `this.`. Explicit types, no `var`. Prefixes `A` abstract, `S` serializable struct, `I` interface. Expression bodies where they fit. Inline comments after a tab.
- Use the DevKit primitives: `Log.Info/Emphasis/Warning/Error/Exception` (`DragonResonance.Logging`, conditional on `ENABLE_LOGGING`) instead of `Debug.Log`, the DevKit extensions (`IsNegative()`, `IsZero()`, `Clamp`…) and attributes (`ShowIf`, `ReadOnly`, `SpritePreview`). Something generic that ToolSet needs belongs in **DevKit**, not here.
- **Dependencies always sorted alphabetically**, wherever they are listed: `dependencies` in `package.json`, `references` in the asmdef, `using` directives (with the children-before-parent rule above). Keep the order when adding one.
- **Copyright footer on every code file**: every `.cs` ends, two blank lines after the last `#endif`/`}`, with the Praenaris ASCII-art Apache footer (11 lines, `Licensed under the Apache License, Version 2.0` … `Copyright © 2026`) and **no newline after it**. New files get it too. Copy it from an existing file, never retype it, and never use any other copyright notice here.
- **Namespaces are mid-rename**: older code is `DragonResonance.*` (Localizer, Prefabricator, Sounder, `DragonResonance.Editor.*`), newer code `Praenaris.*` (`ASubsystem`, Savedata, Integration, Fileman). Match the file or subsystem you're in, and don't rename one file's namespace in isolation; new subsystems use `Praenaris.<Name>`.
- Every asset/folder needs its `.meta` (Unity generates them when the Editor of the project embedding the package refocuses; commit them).

## Compile-checking

There is no CLI build. Check edits with Roslyn from the Unity install (`<Editor>/Data/DotNetSdkRoslyn/csc.dll` under `dotnet`), feeding it the embedding project's generated `Praenaris.ToolSet.csproj`: its `<HintPath>`s, its `<DefineConstants>`, every `<Compile Include>`, `-langversion:9.0 -noconfig -nostdlib+ -target:library`, and `Library/ScriptAssemblies/Praenaris.DevKit.dll` (a fresh build of it if DevKit changed too) for the `<ProjectReference>`. Pass `-noconfig` on the command line, not in the rsp. `-nowarn:CS0436,CS0169,CS0414,CS0649,CS0108,CS0219,CS0162,CS0067,CS1998,CS4014` keeps the log readable.

- The csproj defines are **the embedding project's**: a subsystem that project has switched off (`_ENABLE_SOUNDER` etc.) is not compiled at all. When editing such a subsystem, add its `ENABLE_*` define to the rsp by hand, and when touching shared code (`ASubsystem`, settings providers, drawers) check with the defines both on and off.
- `STEAMWORKS_INTEGRATION` code can only be checked in a project with Steamworks.NET installed; say so when it's left unverified.
- Files added outside the Editor aren't in the csproj yet: append them to the rsp.

## Git and releases

- Branches: work on **`develop`** (or `feature/<name>` merged into `develop` with `--no-ff`). `master` is only written by the release workflow.
- **Cloud sessions** (Claude Code on the web / remote): at the **start of every cloud session**, merge the development branch (`develop`) into the session's branch to stay up to date. After that, **stay on the session's own branch** unless told otherwise: work, commit and push freely there, but don't check out, create, merge into, push to, open PRs against or otherwise manage any other branch unless explicitly asked.
- Commit messages: one imperative, capitalized line, no period, usually no body ("Fix status check for achievements", "Add refresh button to the Savedata settings provider").
- **Bumping `version` in `package.json` and pushing is a release.** `.github/workflows/auto-merge-and-tag.yml` runs on every push: if no `v<version>` tag exists, it merges the pushed branch into `master` and tags it; the tag triggers `auto-sign.yml`, which signs the package with the UPM CLI and creates the GitHub Release, and OpenUPM picks the tag up. Never change `version` unless asked to release. Outside a cloud session's own branch, never push without being asked.
- Everything in the repo root ships in the package, so keep non-package files out of it (or make sure they're intended to ship).