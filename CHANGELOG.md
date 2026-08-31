# Changelog

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
