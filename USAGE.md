# SiPVLib Sound Module - Implementation Guide

## Overview

The Sound Module provides a centralized audio management system for games with support for three distinct sound types:
- **Music**: Background music (1 at a time)
- **SFX**: Sound effects (one-shot playback)
- **Ambience**: Looping environmental sounds (multiple simultaneous)

## Core Architecture

### Components

1. **ConfigSoundData** - Serializable class with audio clip + metadata (volume, pitch, preloadEnabled)
2. **ConfigSoundGroup** - GameConfig that groups sounds by type (Music/SFX/Ambience)
3. **SoundManager** - MonoSingleton managing all playback, mixing, and user settings
4. **SoundChannel** - Per-channel state management (volume, mute)
5. **ConfigSoundAttribute** - Editor attribute for sound ID selection with type filtering
6. **ConfigSoundAttributeDrawer** - Odin Inspector drawer for ConfigSoundAttribute

### Design Patterns

- **Single Music/SFX AudioSource**: One shared AudioSource for exclusive Music or SFX playback
- **Pooled Ambience AudioSources**: Up to `_maxAmbienceSounds` simultaneous ambience sounds via separate AudioSources
- **AudioMixer Integration**: Routes each channel (Music/SFX/Ambience) through dedicated mixer groups
- **UserData Persistence**: Saves/loads user volume and mute settings per channel
- **EventManager Integration**: Fires events for UI/game system subscription

## Setup Instructions

### 1. AudioMixer Configuration

Create an AudioMixer with three child groups:
```
AudioMixer
├── Master
├── Music
├── SFX
└── Ambience
```

Assign to SoundManager's `_audioMixer` field in the Inspector.

### 2. ConfigSoundGroup Setup

1. Create a ConfigSoundGroup asset (e.g., `SoundGroup_Main.asset`)
2. Add ConfigSoundData entries for each sound:
   ```
   SFX Clips:
     "ui_click" → ConfigSoundData (clip, volume=0.8, pitch=1.0, preload=true)
     "player_jump" → ConfigSoundData (clip, volume=0.9, pitch=1.0, preload=false)
   
   Music Clips:
     "menu_theme" → ConfigSoundData (clip, volume=0.7, pitch=1.0, preload=true)
     "gameplay" → ConfigSoundData (clip, volume=0.6, pitch=1.0, preload=false)
   
   Ambience Clips:
     "forest_wind" → ConfigSoundData (clip, volume=0.5, pitch=1.0, preload=false)
   ```

### 3. SoundManager Setup

1. Create a GameObject with SoundManager component
2. Mark as DontDestroyOnLoad or use MonoSingleton's Dont Destroy On Load inspector setting
3. Assign AudioMixer and configure mixer group names
4. Set `_maxAmbienceSounds` (default: 8)

### 4. Initialization

```csharp
// In your game initialization code
var soundManager = SoundManager.Instance;
await soundManager.Init(ConfigLocation.Local);
```

## Usage Examples

### Playing Sounds

```csharp
// Play background music
SoundManager.Instance.PlayMusic("menu_theme");

// Play one-shot SFX
SoundManager.Instance.PlaySfx("ui_click");

// Play ambience (can have multiple)
SoundManager.Instance.PlayAmbience("forest_wind");
```

### Volume & Mute Control

```csharp
// Set channel volume (0 to 1)
SoundManager.Instance.SetChannelVolume(SoundType.Music, 0.8f);
SoundManager.Instance.SetChannelVolume(SoundType.Sfx, 0.9f);

// Mute/unmute channels
SoundManager.Instance.SetChannelMute(SoundType.Ambience, true);

// Query current state
float musicVolume = SoundManager.Instance.GetChannelVolume(SoundType.Music);
bool isSfxMuted = SoundManager.Instance.IsChannelMuted(SoundType.Sfx);
```

### Stopping Sounds

```csharp
// Stop music (any type)
SoundManager.Instance.StopMusic();

// Stop specific ambience
SoundManager.Instance.StopAmbience("forest_wind");

// Stop all ambience
SoundManager.Instance.StopAllAmbience();
```

### Event Subscription

```csharp
// Listen for sound events
EventManager.Add<SoundPlayEvent>(SoundManager.EventSoundStarted, HandleSoundStarted);
EventManager.Add<SoundStopEvent>(SoundManager.EventSoundStopped, HandleSoundStopped);
EventManager.Add<SoundVolumeChangedEvent>(SoundManager.EventSoundVolumeChanged, HandleVolumeChanged);
EventManager.Add<SoundMuteChangedEvent>(SoundManager.EventSoundMuteChanged, HandleMuteChanged);

private void HandleSoundStarted(SoundPlayEvent evt)
{
    Debug.Log($"Playing {evt.soundType}: {evt.soundId}");
}

private void HandleVolumeChanged(SoundVolumeChangedEvent evt)
{
    Debug.Log($"{evt.soundType} volume: {evt.volume}");
}
```

### Using ConfigSoundAttribute

```csharp
public class UIButton : MonoBehaviour
{
    [ConfigSound(SoundType.Sfx)]
    private string _clickSoundId = "";

    public void OnClick()
    {
        SoundManager.Instance.PlaySfx(_clickSoundId);
    }
}
```

## User Settings Persistence

Volume and mute state are automatically saved to UserDataManager for each sound type:
- `Sound_Volume_Music`, `Sound_Mute_Music`
- `Sound_Volume_Sfx`, `Sound_Mute_Sfx`
- `Sound_Volume_Ambience`, `Sound_Mute_Ambience`

These settings persist across game sessions and are automatically restored on SoundManager.Init().

## Advanced Features

### Preloading Sounds

Sounds with `preloadEnabled = true` are preloaded during SoundManager.Init():

```csharp
// In ConfigSoundData editor, check "Preload Enabled" for frequently-used sounds
// e.g., UI_Click, Menu_Theme
```

### ConfigLocation Support

Initialize SoundManager with different config locations:

```csharp
// Load from Local config (default)
await SoundManager.Instance.Init(ConfigLocation.Local);

// Load from Resources
await SoundManager.Instance.Init(ConfigLocation.Resources);

// Load from Addressables
await SoundManager.Instance.Init(ConfigLocation.Addressable);
```

### Debugging

```csharp
// Log current sound manager status
SoundManager.Instance.LogStatus();
```

## Limitations & Considerations

1. **Music/SFX Exclusive**: Only one Music or SFX sound plays at a time (shared AudioSource)
2. **Ambience Limit**: Max simultaneous ambience sounds configurable via `_maxAmbienceSounds`
3. **AudioMixer Required**: AudioMixer assignment is optional but recommended for volume control
4. **UserDataManager Required**: For settings persistence (gracefully degrades if not available)

## Event Payloads

### SoundPlayEvent
```csharp
public string soundId;      // ID of the sound played
public SoundType soundType; // Music, Sfx, or Ambience
```

### SoundStopEvent
```csharp
public SoundType soundType; // Music, Sfx, or Ambience
```

### SoundVolumeChangedEvent
```csharp
public SoundType soundType; // Affected channel
public float volume;        // New volume (0-1)
```

### SoundMuteChangedEvent
```csharp
public SoundType soundType; // Affected channel
public bool isMuted;        // New mute state
```

## Troubleshooting

### Sounds Not Playing
- Check ConfigSoundGroup is assigned and contains the sound ID
- Verify SoundManager.IsInitialized is true
- Check SoundManager.LogStatus() for debug info

### Volume Not Changing
- Verify AudioMixer and groups are correctly configured
- Check UserDataManager is initialized (settings persistence)
- Ensure channel is not muted

### Ambience Overlapping
- Ambience sounds mix together; this is by design
- To prevent overlapping, manually StopAmbience() before playing new ones

---

**Version**: 1.0  
**Last Updated**: July 5, 2026

