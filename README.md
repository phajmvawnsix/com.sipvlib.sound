# com.sipvlib.sound

Part of [SiPVLib](https://github.com/phajmvawnsix/SiPVLib). A central `SoundManager` for music, SFX, and ambience playback with AudioMixer channel volume/mute control, `ConfigSoundGroup`-based clip authoring, and `UserDataManager`-backed persistence of user volume/mute settings.

## Install

Add to your project's `Packages/manifest.json`:

```json
"com.sipvlib.sound": "https://github.com/phajmvawnsix/com.sipvlib.sound.git",
"com.sipvlib.config": "https://github.com/phajmvawnsix/com.sipvlib.config.git",
"com.sipvlib.debugging": "https://github.com/phajmvawnsix/com.sipvlib.debugging.git",
"com.sipvlib.event": "https://github.com/phajmvawnsix/com.sipvlib.event.git",
"com.sipvlib.userdata": "https://github.com/phajmvawnsix/com.sipvlib.userdata.git",
"com.sipvlib.utilities": "https://github.com/phajmvawnsix/com.sipvlib.utilities.git",
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
```

UPM does not automatically resolve nested git dependencies — you must add the `com.sipvlib.*` and UniTask entries above yourself alongside this package.

## Optional: Odin Inspector

This package integrates with [Odin Inspector](https://odininspector.com) (Sirenix) if you have it installed, but does NOT require it and does NOT bundle it — Odin is a paid Unity Asset Store asset and cannot be redistributed here.

- **Without Odin installed**:
  - `ConfigSoundGroup`'s SFX/Music/Ambience clip dictionaries render with the plain Unity Inspector (no `DictionaryDrawerSettings` foldout/labels UI) — you can still author entries via the default `Dictionary` field rendering.
  - `SoundEffectPlayer` and `AmbiencePlayer`'s conditional fields (`_randomPitchAmount`, `_fadeInDuration`, `_fadeOutDuration`) are always shown in the inspector instead of only appearing when their corresponding toggle is enabled (no `[ShowIf]` support).
  - `ConfigSoundAttributeDrawer` (the dropdown sound-picker UI for fields marked `[ConfigSound]`) is excluded from compilation — `[ConfigSound]`-marked string fields fall back to a plain text field where you type the sound ID manually.
  - All runtime playback (`SoundManager`, `SoundChannel`, `SoundUtility`, `MusicPlayer`, etc.) is fully functional regardless of Odin.
- **With Odin installed** (purchase + import from the Asset Store, which auto-defines the `ODIN_INSPECTOR` scripting define symbol): all of the above light up automatically — dictionary foldout UI, conditional field visibility, and the sound-ID dropdown picker.

No manual setup is needed beyond installing Odin itself — detection is automatic via the `ODIN_INSPECTOR` define.

## Documentation
- [Module overview](00_SOUND_MODULE_COMPLETE.md)
- [API reference](API_REFERENCE.md)
- [Architecture](ARCHITECTURE.md)
- [Quick start](QUICK_START.md)
- [Usage guide](USAGE.md) — original module documentation carried over from the SiPVLib monolith
