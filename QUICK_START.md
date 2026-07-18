# Sound Module - Quick Start Guide

Get the Sound Module up and running in 5 minutes!

## Step 1: Create AudioMixer (2 minutes)

1. In Project window, right-click → Audio → Mixer
2. Name it `MainAudioMixer`
3. In the Mixer:
   - Right-click Master group → Add child group → Name: "Music"
   - Right-click Master group → Add child group → Name: "SFX"
   - Right-click Master group → Add child group → Name: "Ambience"

Result:
```
MainAudioMixer
├── Master
├── Music
├── SFX
└── Ambience
```

## Step 2: Create ConfigSoundGroup (2 minutes)

1. Create folder: `Assets/Resources/Configs/Sounds/` (or similar)
2. Right-click → Create → SiPVLib → ConfigSoundGroup
3. Name it `SoundGroup_Main`
4. In Inspector, add some test sounds:

**SFX Clips:**
- ID: "click"
  - Audio Clip: (any click sound)
  - Volume: 0.8
  - Pitch: 1.0
  - Preload Enabled: ✓

**Music Clips:**
- ID: "menu"
  - Audio Clip: (any music)
  - Volume: 0.7
  - Pitch: 1.0
  - Preload Enabled: ✓

**Ambience Clips:**
- ID: "wind"
  - Audio Clip: (any ambient sound)
  - Volume: 0.5
  - Pitch: 1.0
  - Preload Enabled: ✗

## Step 3: Setup SoundManager (1 minute)

1. Create empty GameObject → Name: "SoundManager"
2. Add component: SoundManager (SiPVLib.Sound)
3. Drag MainAudioMixer to the `_audioMixer` field
4. Leave other fields as defaults
5. Mark DontDestroyOnLoad = true

## Step 4: Initialize in Code (1 minute)

```csharp
// In your game bootstrap/startup script
public class GameBootstrap : MonoBehaviour
{
    private async void Start()
    {
        // Initialize Sound Manager
        var success = await SoundManager.Instance.Init();
        if (success)
        {
            Debug.Log("Sound system ready!");
            
            // Play test sounds
            SoundManager.Instance.PlayMusic("menu");
            SoundManager.Instance.PlaySfx("click");
            SoundManager.Instance.PlayAmbience("wind");
        }
    }
}
```

## Usage Examples

### Play Sounds
```csharp
// Music (one at a time)
SoundManager.Instance.PlayMusic("menu");

// SFX (one-shot, oneshot multiple allowed)
SoundManager.Instance.PlaySfx("click");

// Ambience (loop, multiple allowed)
SoundManager.Instance.PlayAmbience("wind");
```

### Control Volume
```csharp
// Set volume (0 to 1)
SoundManager.Instance.SetChannelVolume(SoundType.Music, 0.8f);
SoundManager.Instance.SetChannelVolume(SoundType.Sfx, 0.9f);

// Mute/unmute
SoundManager.Instance.SetChannelMute(SoundType.Music, false);

// Quick helpers
SoundUtility.SetVolume(SoundType.Music, 0.8f);
SoundUtility.ToggleMute(SoundType.Sfx);
```

### Stop Sounds
```csharp
// Stop music
SoundManager.Instance.StopMusic();

// Stop specific ambience
SoundManager.Instance.StopAmbience("wind");

// Stop all ambience
SoundManager.Instance.StopAllAmbience();
```

### Subscribe to Events
```csharp
// Listen for sounds playing
EventManager.Add<SoundPlayEvent>(
    SoundManager.EventSoundStarted,
    evt => Debug.Log($"Playing: {evt.soundId}")
);

// Listen for volume changes
EventManager.Add<SoundVolumeChangedEvent>(
    SoundManager.EventSoundVolumeChanged,
    evt => Debug.Log($"{evt.soundType} volume: {evt.volume}")
);
```

### Use ConfigSoundAttribute
```csharp
public class MyUIButton : MonoBehaviour
{
    [ConfigSound(SoundType.Sfx)]
    [SerializeField] private string _clickSound = "";

    public void OnClick()
    {
        SoundManager.Instance.PlaySfx(_clickSound);
    }
}
```

The `_clickSound` field will show a dropdown in the editor with all available SFX sounds!

## Common Tasks

### Add New Sound
1. Open your ConfigSoundGroup asset
2. Add new entry to desired dictionary (SFX/Music/Ambience)
3. Set ID, AudioClip, Volume, Pitch
4. Done!

### Change Volume from UI
```csharp
public void OnMusicVolumeSlider(float value)
{
    SoundManager.Instance.SetChannelVolume(SoundType.Music, value);
}
```

### Debug Sound Issues
```csharp
SoundManager.Instance.LogStatus();
// Output: Initialized, channel volumes/mutes, active ambience count
```

## Useful Static Methods (SoundUtility)

```csharp
// Quick sound effects
SoundUtility.PlayMenuClick();
SoundUtility.PlayMenuNavigate();
SoundUtility.PlayError();
SoundUtility.PlaySuccess();

// Quick control
SoundUtility.SetVolume(SoundType.Music, 0.8f);
SoundUtility.ToggleMute(SoundType.Sfx);
SoundUtility.IsMuted(SoundType.Music); // Returns bool
```

## Next Steps

1. **Read the Full Guide**: `README.md` for comprehensive documentation
2. **Check Examples**: `SoundManagerExample.cs` for complete implementation
3. **Integration Checklist**: `INTEGRATION_CHECKLIST.md` to verify setup
4. **Implementation Details**: `IMPLEMENTATION_SUMMARY.md` for architecture

## Keyboard Debug Shortcuts (Optional)

Add this to test quickly:

```csharp
private void Update()
{
    if (Input.GetKeyDown(KeyCode.M))
        SoundUtility.ToggleMute(SoundType.Music);
    
    if (Input.GetKeyDown(KeyCode.S))
        SoundUtility.ToggleMute(SoundType.Sfx);
    
    if (Input.GetKeyDown(KeyCode.A))
        SoundUtility.ToggleMute(SoundType.Ambience);
}
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Sounds not playing | Check SoundManager initialized (`IsInitialized`) |
| No dropdown in editor | Make sure Odin Inspector is installed |
| Volume not persisting | Verify UserDataManager is initialized |
| Audio not in mixer | Check mixer group names match (Music/SFX/Ambience) |

## That's It!

Your Sound Module is now ready. Happy coding! 🎵

For more details, see README.md and IMPLEMENTATION_SUMMARY.md

