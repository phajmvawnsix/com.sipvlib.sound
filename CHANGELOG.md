# Changelog

## [1.1.3] - 2026-09-02

`ConfigSoundAttributeDrawer` rebuilt its sound-id list on every repaint — a LINQ filter, a `Sort()`
and a `ToArray()` per frame, per `[ConfigSound]` field. The list is now built once per cache
generation, tracked by the new `SoundRefsEditor.CacheVersion` (bumped whenever the sound cache is
populated or cleared).

## [1.1.2] - 2026-09-02

Fix `SoundRefsEditor` background `EditorApplication.update` polling: every 0.5s, forever, regardless
of Editor focus, it ran a full `AssetDatabase.FindAssets("t:ConfigSoundGroup")` scan plus a
`LoadAssetAtPath` per result to detect changes — the main cause of Editor CPU/memory climbing and
the machine getting laggy the longer the Editor stayed open. Cache invalidation is now purely
event-driven via the existing `SoundRefsAssetProcessor.OnPostprocessAllAssets`.

## [1.1.1] - 2026-09-01

`PlayMusic`/`PlaySfx`/`PlayAmbience` load their clip synchronously via `ConfigSoundData.GetAudioClip()`
instead of fire-and-forget wrapping the async variant — blocks on first use if not preloaded, matching
the sync/async split already established by `AssetConfig.GetAsset()`/`GetAssetAsync()`.

## [1.1.0] - 2026-08-31

Detach audio assets from the APK: `ConfigSoundData` now holds an `AudioClipConfig` Id
(`com.sipvlib.config`) instead of a direct `AudioClip` reference, so clips can live in
Resources/Addressable storage instead of being bundled into `ConfigSoundGroup`. Adds
`ConfigSoundData.GetAudioClip()`/`GetAudioClipAsync()` to resolve the clip via `ConfigManager`.
Adds `SoundManager.PlayMusicAsync`/`PlaySfxAsync`/`PlayAmbienceAsync`; the existing sync
`PlayMusic`/`PlaySfx`/`PlayAmbience` are now thin fire-and-forget wrappers over them. Removes
`ConfigSoundData.PreloadEnabled` and the silent-play preload hack — preloading is now handled by
`AudioClipConfig.LoadAssetOnStartup` during `ConfigManager` initialization.

## [1.0.1] - 2026-07-28

Fix compile bugs

## [1.0.0] - 2026-07-18

Initial extraction from SiPVLib monolith.
