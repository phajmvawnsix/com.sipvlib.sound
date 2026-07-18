# Sound Module - Complete API Reference

Comprehensive reference for all public APIs in the Sound Module.

## Table of Contents
1. [SoundManager](#soundmanager)
2. [SoundUtility](#soundutility)
3. [ConfigSoundAttribute](#configsoundattribute)
4. [SoundChannel](#soundchannel)
5. [Event Payloads](#event-payloads)
6. [Configuration Classes](#configuration-classes)

---

## SoundManager

Main singleton managing all audio playback.

### Class Declaration
```csharp
public class SoundManager : MonoSingleton<SoundManager>
```

### Properties

```csharp
/// Checks if the SoundManager is initialized and ready
public bool IsInitialized { get; }
```

### Initialization

```csharp
/// Asynchronously initializes the SoundManager
/// @param configLocation - Where to load ConfigSoundGroup assets (default: Local)
/// @returns Task<bool> - True if successful
public async UniTask<bool> Init(ConfigLocation configLocation = ConfigLocation.Local)
```

### Playback - Music

```csharp
/// Plays a music track, stopping any currently playing music
/// @param soundId - ID of the music sound to play
/// @param fadeDuration - Fade in duration (currently immediate, parameter for future use)
public void PlayMusic(string soundId, float fadeDuration = 0f)

/// Stops the currently playing music
public void StopMusic()
```

### Playback - SFX

```csharp
/// Plays a one-shot sound effect
/// @param soundId - ID of the SFX sound to play
public void PlaySfx(string soundId)
```

### Playback - Ambience

```csharp
/// Plays an ambience sound (can have multiple playing simultaneously)
/// @param soundId - ID of the ambience sound to play
public void PlayAmbience(string soundId)

/// Stops playing a specific ambience sound
/// @param soundId - ID of the ambience sound to stop
public void StopAmbience(string soundId)

/// Stops all ambience sounds
public void StopAllAmbience()
```

### Volume Control

```csharp
/// Sets the volume for a sound channel (0 to 1)
/// @param soundType - Music, Sfx, or Ambience
/// @param volume - Volume level (0.0 = silent, 1.0 = full)
public void SetChannelVolume(SoundType soundType, float volume)

/// Gets the current volume for a sound channel
/// @param soundType - Music, Sfx, or Ambience
/// @returns float - Current volume (0-1)
public float GetChannelVolume(SoundType soundType)

/// Sets the mute state for a sound channel
/// @param soundType - Music, Sfx, or Ambience
/// @param muted - True to mute, false to unmute
public void SetChannelMute(SoundType soundType, bool muted)

/// Gets the mute state for a sound channel
/// @param soundType - Music, Sfx, or Ambience
/// @returns bool - True if muted, false if unmuted
public bool IsChannelMuted(SoundType soundType)
```

### Debug

```csharp
/// Logs the current state of the sound manager to console
public void LogStatus()
```

### Events (Constants)

```csharp
/// Fired when a sound starts playing
/// Payload: SoundPlayEvent
public const string EventSoundStarted = "Sound.Started";

/// Fired when a sound stops playing
/// Payload: SoundStopEvent
public const string EventSoundStopped = "Sound.Stopped";

/// Fired when a channel's volume changes
/// Payload: SoundVolumeChangedEvent
public const string EventSoundVolumeChanged = "Sound.VolumeChanged";

/// Fired when a channel's mute state changes
/// Payload: SoundMuteChangedEvent
public const string EventSoundMuteChanged = "Sound.MuteChanged";
```

---

## SoundUtility

Static utility class with convenience methods for common operations.

### Common Sound Effects

```csharp
/// Plays the menu navigation sound
public static void PlayMenuNavigate()

/// Plays the menu click sound
public static void PlayMenuClick()

/// Plays an error/deny sound
public static void PlayError()

/// Plays a success/confirm sound
public static void PlaySuccess()
```

### Generic Playback

```csharp
/// Plays a generic SFX sound
/// @param soundId - ID of the SFX sound
public static void PlaySfx(string soundId)

/// Plays a music track
/// @param soundId - ID of the music
/// @param fadeDuration - Fade duration (currently unused)
public static void PlayMusic(string soundId, float fadeDuration = 0f)

/// Stops the current music
public static void StopMusic()

/// Plays an ambience sound
/// @param soundId - ID of the ambience
public static void PlayAmbience(string soundId)

/// Stops a specific ambience sound
/// @param soundId - ID of the ambience
public static void StopAmbience(string soundId)
```

### Volume Control

```csharp
/// Sets a channel's volume
/// @param soundType - Music, Sfx, or Ambience
/// @param volume - Volume level (0-1)
public static void SetVolume(SoundType soundType, float volume)

/// Sets a channel's mute state
/// @param soundType - Music, Sfx, or Ambience
/// @param muted - True to mute, false to unmute
public static void SetMute(SoundType soundType, bool muted)

/// Toggles a channel's mute state
/// @param soundType - Music, Sfx, or Ambience
public static void ToggleMute(SoundType soundType)

/// Gets a channel's current volume
/// @param soundType - Music, Sfx, or Ambience
/// @returns float - Current volume (0-1)
public static float GetVolume(SoundType soundType)

/// Gets a channel's mute state
/// @param soundType - Music, Sfx, or Ambience
/// @returns bool - True if muted
public static bool IsMuted(SoundType soundType)
```

---

## ConfigSoundAttribute

Attribute for marking string fields as sound ID selections.

### Constructor Overloads

```csharp
/// Empty constructor (no filtering)
public ConfigSoundAttribute()

/// Constructor with SoundType filter
/// @param soundTypeFilter - Only show sounds of this type
/// @param previewOnly - Hide label and ID field (optional)
public ConfigSoundAttribute(SoundType soundTypeFilter, bool previewOnly = false)

/// Constructor with preview-only mode
/// @param previewOnly - Hide label and ID field
public ConfigSoundAttribute(bool previewOnly = false)
```

### Properties

```csharp
/// Optional sound type filter
public SoundType? SoundTypeFilter { get; }

/// Whether to show preview-only mode
public bool PreviewOnly { get; }
```

### Usage Example

```csharp
public class MyClass : MonoBehaviour
{
    // No filtering - shows all sounds
    [ConfigSound]
    public string anySound = "";

    // Only show SFX sounds
    [ConfigSound(SoundType.Sfx)]
    public string sfxSound = "";

    // Only show Music sounds
    [ConfigSound(SoundType.Music)]
    public string musicSound = "";

    // Only show Ambience sounds
    [ConfigSound(SoundType.Ambience)]
    public string ambienceSound = "";
}
```

---

## SoundChannel

Manages per-channel state (volume, mute, mixer group).

### Properties

```csharp
/// The sound type this channel represents
public SoundType Type { get; }

/// Current volume (0-1)
public float Volume { get; }

/// Current mute state
public bool IsMuted { get; }

/// The AudioMixer group this channel routes through
public AudioMixerGroup MixerGroup { get; }
```

### Methods

```csharp
/// Sets the volume for this channel
/// @param volume - Volume level (0-1)
public void SetVolume(float volume)

/// Sets the mute state for this channel
/// @param muted - True to mute, false to unmute
public void SetMute(bool muted)

/// Registers a callback for volume changes
/// @param callback - Function to call when volume changes
public void OnVolumeChanged(Action<float> callback)

/// Registers a callback for mute changes
/// @param callback - Function to call when mute state changes
public void OnMuteChanged(Action<bool> callback)

/// Unregisters a volume change callback
/// @param callback - Function to unregister
public void RemoveVolumeListener(Action<float> callback)

/// Unregisters a mute change callback
/// @param callback - Function to unregister
public void RemoveMuteListener(Action<bool> callback)
```

---

## Event Payloads

All event structures passed to EventManager callbacks.

### SoundPlayEvent

```csharp
[System.Serializable]
public struct SoundPlayEvent
{
    /// ID of the sound that started playing
    public string soundId;

    /// Type of sound (Music, Sfx, or Ambience)
    public SoundType soundType;
}

// Usage
EventManager.Add<SoundPlayEvent>(
    SoundManager.EventSoundStarted,
    evt => Debug.Log($"Playing: {evt.soundId} ({evt.soundType})")
);
```

### SoundStopEvent

```csharp
[System.Serializable]
public struct SoundStopEvent
{
    /// Type of sound that stopped
    public SoundType soundType;
}

// Usage
EventManager.Add<SoundStopEvent>(
    SoundManager.EventSoundStopped,
    evt => Debug.Log($"Stopped: {evt.soundType}")
);
```

### SoundVolumeChangedEvent

```csharp
[System.Serializable]
public struct SoundVolumeChangedEvent
{
    /// Type of channel that changed
    public SoundType soundType;

    /// New volume level (0-1)
    public float volume;
}

// Usage
EventManager.Add<SoundVolumeChangedEvent>(
    SoundManager.EventSoundVolumeChanged,
    evt => Debug.Log($"{evt.soundType}: {evt.volume:F2}")
);
```

### SoundMuteChangedEvent

```csharp
[System.Serializable]
public struct SoundMuteChangedEvent
{
    /// Type of channel that changed
    public SoundType soundType;

    /// New mute state
    public bool isMuted;
}

// Usage
EventManager.Add<SoundMuteChangedEvent>(
    SoundManager.EventSoundMuteChanged,
    evt => Debug.Log($"{evt.soundType} mute: {evt.isMuted}")
);
```

---

## Configuration Classes

### ConfigSoundData

```csharp
[System.Serializable]
public class ConfigSoundData
{
    /// The audio clip to play
    public AudioClip AudioClip { get; }

    /// Playback volume (0-1)
    public float Volume { get; }

    /// Playback pitch (-3 to +3)
    public float Pitch { get; }

    /// Whether to preload this sound during initialization
    public bool PreloadEnabled { get; }
}
```

### ConfigSoundGroup

```csharp
public class ConfigSoundGroup : GameConfig
{
    /// SFX sounds (one-shot)
    #if UNITY_EDITOR
    public Dictionary<string, ConfigSoundData> SfxClips { get; }
    #endif

    /// Music sounds (background)
    #if UNITY_EDITOR
    public Dictionary<string, ConfigSoundData> MusicClips { get; }
    #endif

    /// Ambience sounds (looping)
    #if UNITY_EDITOR
    public Dictionary<string, ConfigSoundData> AmbienceClips { get; }
    #endif

    /// Gets SFX data by sound ID
    public ConfigSoundData GetSfxData(string id)

    /// Gets Music data by sound ID
    public ConfigSoundData GetMusicData(string id)

    /// Gets Ambience data by sound ID
    public ConfigSoundData GetAmbienceData(string id)
}
```

### SoundType (Enum)

```csharp
[System.Serializable]
public enum SoundType
{
    /// Sound effects (one-shot)
    Sfx = 0,

    /// Background music
    Music = 1,

    /// Looping ambient sounds
    Ambience = 2,
}
```

---

## SoundRefsEditor (Editor-Only)

Utility for working with sound data in the editor.

```csharp
#if UNITY_EDITOR

public static class SoundRefsEditor
{
    /// Gets an AudioClip by sound ID
    /// @param soundId - ID of the sound
    /// @returns AudioClip or null
    public static AudioClip GetSoundClip(string soundId)

    /// Gets ConfigSoundData by sound ID
    /// @param soundId - ID of the sound
    /// @returns ConfigSoundData or null
    public static ConfigSoundData GetSoundData(string soundId)

    /// Gets the SoundType of a sound by ID
    /// @param soundId - ID of the sound
    /// @returns SoundType or null
    public static SoundType? GetSoundType(string soundId)

    /// Gets all available sound IDs with optional filtering
    /// @param filterType - Optional: only return sounds of this type
    /// @returns List<string> of sound IDs, sorted alphabetically
    public static List<string> GetAvailableSoundIds(SoundType? filterType = null)

    /// Caches all sound groups and their data
    public static void CacheSoundGroups()

    /// Clears the sound cache
    public static void ClearCache()
}

#endif
```

---

## Common Patterns

### Initialize and Play a Sound

```csharp
public class GameStartup : MonoBehaviour
{
    private async void Start()
    {
        // Initialize
        await SoundManager.Instance.Init();

        // Play music
        SoundManager.Instance.PlayMusic("main_menu");
    }
}
```

### Create a Volume Slider

```csharp
public class SettingsUI : MonoBehaviour
{
    public void OnMusicVolumeChanged(float value)
    {
        SoundManager.Instance.SetChannelVolume(SoundType.Music, value);
    }

    public void OnSfxVolumeChanged(float value)
    {
        SoundManager.Instance.SetChannelVolume(SoundType.Sfx, value);
    }
}
```

### Subscribe to Sound Events

```csharp
public class GameManager : MonoBehaviour
{
    private void Start()
    {
        EventManager.Add<SoundPlayEvent>(
            SoundManager.EventSoundStarted,
            OnSoundPlayed
        );
    }

    private void OnSoundPlayed(SoundPlayEvent evt)
    {
        Debug.Log($"Sound: {evt.soundId}");
    }
}
```

### Use ConfigSoundAttribute

```csharp
public class Button : MonoBehaviour
{
    [ConfigSound(SoundType.Sfx)]
    private string _clickSound = "";

    public void OnClick()
    {
        SoundManager.Instance.PlaySfx(_clickSound);
    }
}
```

---

## Quick Reference Table

| Task | Method |
|------|--------|
| Play Music | `SoundManager.Instance.PlayMusic(id)` |
| Play SFX | `SoundManager.Instance.PlaySfx(id)` or `SoundUtility.PlaySfx(id)` |
| Play Ambience | `SoundManager.Instance.PlayAmbience(id)` |
| Stop Music | `SoundManager.Instance.StopMusic()` |
| Stop Ambience | `SoundManager.Instance.StopAmbience(id)` |
| Set Volume | `SoundManager.Instance.SetChannelVolume(type, vol)` |
| Toggle Mute | `SoundUtility.ToggleMute(type)` |
| Get Volume | `SoundManager.Instance.GetChannelVolume(type)` |
| Listen Event | `EventManager.Add<T>(eventName, callback)` |
| Debug Status | `SoundManager.Instance.LogStatus()` |

---

**API Version**: 1.0  
**Last Updated**: July 5, 2026

For more details, see README.md and QUICK_START.md

