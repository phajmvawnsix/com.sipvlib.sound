# Sound Module - Architecture Diagram

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        GAME APPLICATION                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    EventManager                          │  │
│  │  (Global event bus for all state changes)               │  │
│  └──────────────────────────────────────────────────────────┘  │
│           ▲                    ▲                    ▲            │
│           │                    │                    │            │
│    SoundStarted         VolumeChanged         MuteChanged        │
│    SoundStopped                                                 │
│           │                    │                    │            │
│           └────────────────────┼────────────────────┘            │
│                                │                                 │
│                        ┌───────▼──────────┐                     │
│                        │   SoundManager   │                     │
│                        │  (Singleton)     │                     │
│                        └───────┬──────────┘                     │
│                                │                                 │
│        ┌───────────────────────┼───────────────────────┐        │
│        │                       │                       │        │
│    ┌───▼────┐         ┌────────▼────────┐      ┌──────▼───┐   │
│    │ Channels│         │  Audio Sources  │      │ User     │   │
│    │         │         │                 │      │ Settings │   │
│    │ Music   │         │ Music/SFX:      │      │ Persist- │   │
│    │ SFX     │         │ 1 Shared AS     │      │ ence     │   │
│    │ Ambience│         │                 │      │ (via UM) │   │
│    │         │         │ Ambience:       │      │          │   │
│    │         │         │ Pool (max 8)    │      │          │   │
│    └─────────┘         │                 │      └──────────┘   │
│        ▲               └────────┬────────┘                      │
│        │                        │                               │
│        └────────────────────────┼───────────────────────┐       │
│                                 │                       │       │
└─────────────────────────────────┼───────────────────────┼───────┘
                                  │                       │
                      ┌───────────▼───────────┐  ┌───────▼──────┐
                      │  AudioMixer Groups    │  │ ConfigManager│
                      │                       │  │              │
                      │ ├─ Master             │  │ Loads configs│
                      │ ├─ Music              │  │ from various │
                      │ ├─ SFX                │  │ locations    │
                      │ └─ Ambience           │  └───────┬──────┘
                      │                       │          │
                      └───────────────────────┘  ┌───────▼────────┐
                                                │ConfigSoundGroup│
                                                │ (GameConfig)   │
                                                │                │
                                                │ ├─ SFX Sounds  │
                                                │ ├─ Music       │
                                                │ └─ Ambience    │
                                                └────────────────┘
```

---

## Data Flow - Playing a Sound

```
User calls PlaySfx("click")
         │
         ▼
┌─────────────────────────────┐
│ SoundManager.PlaySfx(id)    │
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────────────────────┐
│ Find ConfigSoundData from ConfigSoundGroup  │
│ (via ConfigManager.GetAll)                  │
└──────────┬──────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────┐
│ Get AudioClip from ConfigSoundData  │
│ Get Volume/Pitch from ConfigSoundData
└──────────┬────────────────────────────┘
           │
           ▼
┌──────────────────────────────────────────┐
│ Setup AudioSource:                       │
│  - Set clip                              │
│  - Set volume (channel vol * data vol)   │
│  - Set pitch                             │
│  - Set mixer group                       │
│  - Set playOnAwake = false               │
└──────────┬───────────────────────────────┘
           │
           ▼
┌──────────────────────────┐
│ Call AudioSource.Play()  │
└──────────┬───────────────┘
           │
           ▼
┌────────────────────────────────────────┐
│ Fire EventManager event:               │
│ - EventSoundStarted (SoundPlayEvent)   │
└──────────┬─────────────────────────────┘
           │
           ▼
     ┌─────────────┐
     │ Game/UI     │
     │ listens and │
     │ reacts      │
     └─────────────┘
```

---

## Component Interaction - Volume Change

```
User/UI requests volume change
         │
         ▼
┌──────────────────────────────────────┐
│ SoundManager.SetChannelVolume(type, vol)
└──────────┬───────────────────────────┘
           │
           ▼
┌──────────────────────────────────┐
│ Find SoundChannel for type       │
│ (Music/SFX/Ambience)            │
└──────────┬───────────────────────┘
           │
           ▼
┌──────────────────────────────────┐
│ SoundChannel.SetVolume(vol)      │
└──────────┬───────────────────────┘
           │
           ├─────────────────────────────────────┐
           │                                     │
           ▼                                     ▼
    Update _volume          Invoke volume change callbacks
           │                             │
           │ ┌───────────────────────────┤
           │ │                           │
           ▼ ▼                           ▼
    ┌─────────────────────┐    ┌──────────────────────┐
    │ SaveChannelSettings │    │ Fire EventManager    │
    │ (async to UserData) │    │ EventSoundVolumeChanged
    └─────────────────────┘    └──────────┬───────────┘
           │                              │
           ▼                              ▼
    ┌────────────────────────┐  ┌──────────────────────┐
    │ UserDataManager.SetAsync│  │ UI/Game systems     │
    │ (persist to storage)   │  │ listen and update   │
    └────────────────────────┘  │ sliders/displays    │
                                └──────────────────────┘
```

---

## Audio Routing Architecture

```
┌───────────────────────────────────────────────────────────────┐
│                      AUDIO OUTPUT                             │
└────────────────────────┬──────────────────────────────────────┘
                         │
                         ▲
                         │
        ┌────────────────┼────────────────┐
        │                │                │
    ┌───┴────┐    ┌──────┴─────┐   ┌──────┴─────┐
    │  Music  │    │    SFX     │   │  Ambience  │
    │ Mixer   │    │   Mixer    │   │   Mixer    │
    │ Group   │    │   Group    │   │   Group    │
    └────┬────┘    └──────┬─────┘   └──────┬─────┘
         │                │                │
    ┌────▼────┐    ┌──────▼─────┐   ┌──────▼──────────┐
    │ 1 Shared │    │  1 Shared  │   │ Pool AudioSrc  │
    │AudioSrc  │    │ AudioSrc   │   │ (up to 8)      │
    │          │    │            │   │                │
    │for Music │    │for SFX     │   │AS1-AS8        │
    └────┬────┘    └──────┬─────┘   └──────┬──────────┘
         │                │                │
         └────────────┬───┴────────────────┘
                      │
              ┌───────▼────────┐
              │  Master Mixer  │
              └───────┬────────┘
                      │
              ┌───────▼────────┐
              │ System Speaker │
              └────────────────┘

Volume Control:
┌─────────────────────────────────────────────────┐
│ SoundChannel.IsMuted ? 0 : Volume * Data.Volume │
└─────────────────────────────────────────────────┘
                        │
                        ▼
                  AudioSource.volume
```

---

## Configuration System

```
┌──────────────────────────────────────────────┐
│           ConfigSoundGroup (Asset)            │
│                 (GameConfig)                  │
├──────────────────────────────────────────────┤
│                                              │
│  SFX Clips Dictionary:                      │
│  ┌────────────────────────────────────────┐ │
│  │ "ui_click"     → ConfigSoundData       │ │
│  │   ├─ AudioClip: ui_click.wav           │ │
│  │   ├─ Volume: 0.8                       │ │
│  │   ├─ Pitch: 1.0                        │ │
│  │   └─ PreloadEnabled: true              │ │
│  │                                        │ │
│  │ "player_jump"  → ConfigSoundData       │ │
│  │   ├─ AudioClip: player_jump.wav        │ │
│  │   ├─ Volume: 0.9                       │ │
│  │   ├─ Pitch: 1.0                        │ │
│  │   └─ PreloadEnabled: false             │ │
│  └────────────────────────────────────────┘ │
│                                              │
│  Music Clips Dictionary:                     │
│  ┌────────────────────────────────────────┐ │
│  │ "menu_theme"   → ConfigSoundData       │ │
│  │ "gameplay"     → ConfigSoundData       │ │
│  │ ...                                    │ │
│  └────────────────────────────────────────┘ │
│                                              │
│  Ambience Clips Dictionary:                  │
│  ┌────────────────────────────────────────┐ │
│  │ "forest_wind"  → ConfigSoundData       │ │
│  │ "ocean_waves"  → ConfigSoundData       │ │
│  │ ...                                    │ │
│  └────────────────────────────────────────┘ │
│                                              │
└──────────────────────────────────────────────┘
```

---

## Editor Attribute Flow

```
Inspector Field:
┌─────────────────────────────────────┐
│ [ConfigSound(SoundType.Sfx)]         │
│ public string _soundId = "";         │
└──────────────┬──────────────────────┘
               │
               ▼
    ┌──────────────────────────────┐
    │ ConfigSoundAttributeDrawer   │
    │ (Odin Inspector)             │
    └──────────┬───────────────────┘
               │
               ├─────────────────────────┐
               │                         │
               ▼                         ▼
    ┌──────────────────────┐   ┌───────────────────┐
    │ SoundRefsEditor      │   │ SoundType Filter  │
    │ .CacheSoundGroups()  │   │ (Sfx selected)    │
    │ .GetAvailable        │   │                   │
    │  SoundIds(SoundType) │   │ Filter to Sfx     │
    │                      │   │ only              │
    └──────────┬───────────┘   └───────────────────┘
               │                         │
               └────────┬────────────────┘
                        │
                        ▼
          ┌─────────────────────────┐
          │ Dropdown UI:            │
          │ ┌─────────────────────┐ │
          │ │ ui_click      ◄     │ │
          │ │ player_jump         │ │
          │ │ ... (Sfx only)      │ │
          │ └─────────────────────┘ │
          └─────────────────────────┘
                        │
                        ▼
          ┌─────────────────────────┐
          │ Store selected ID in    │
          │ string field            │
          └─────────────────────────┘
```

---

## Event System Architecture

```
┌────────────────────────────────────────────────────────────┐
│                    SoundManager                            │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  Fires Events:                                             │
│                                                            │
│  ┌──────────────────┐      EventManager.Invoke            │
│  │ SoundPlayEvent   │ ──────────────────►  EventSoundStarted
│  └──────────────────┘                                     │
│                                                            │
│  ┌──────────────────┐      EventManager.Invoke            │
│  │ SoundStopEvent   │ ──────────────────►  EventSoundStopped
│  └──────────────────┘                                     │
│                                                            │
│  ┌──────────────────────────┐   EventManager.Invoke       │
│  │ SoundVolumeChangedEvent  │ ──────────────────►         │
│  └──────────────────────────┘    EventSoundVolumeChanged  │
│                                                            │
│  ┌──────────────────────────┐   EventManager.Invoke       │
│  │ SoundMuteChangedEvent    │ ──────────────────►         │
│  └──────────────────────────┘    EventSoundMuteChanged    │
│                                                            │
└────────────────────────────────────────────────────────────┘
              │
              │ Broadcasting
              │
    ┌─────────┴──────────┬──────────────────┐
    │                    │                  │
    ▼                    ▼                  ▼
┌────────────┐    ┌─────────────┐   ┌──────────────┐
│ UI Manager │    │ Game Logic  │   │ Analytics    │
│            │    │             │   │              │
│ Update     │    │ Trigger     │   │ Log events   │
│ displays   │    │ game state  │   │              │
└────────────┘    └─────────────┘   └──────────────┘
```

---

## Data Persistence Flow

```
┌─────────────────────────┐
│  User Changes Volume    │
│  SetChannelVolume(vol)  │
└──────────┬──────────────┘
           │
           ▼
    ┌──────────────────────────┐
    │ Update SoundChannel      │
    │ _volume = new_volume     │
    └──────────┬───────────────┘
               │
               ▼
    ┌──────────────────────────────────────┐
    │ SaveChannelSettings(soundType) async │
    └──────────┬───────────────────────────┘
               │
               ▼
    ┌──────────────────────────────────────┐
    │ UserDataManager.SetAsync(key, value) │
    │                                      │
    │ Key: "Sound_Volume_{Type}"          │
    │ Value: volume (float)                │
    └──────────┬───────────────────────────┘
               │
               ▼
    ┌──────────────────────────────────────┐
    │ StorageProvider saves to:            │
    │ - PlayerPrefs (sync)                 │
    │ - LocalFile (async)                  │
    │ - Firestore (async)                  │
    └──────────┬───────────────────────────┘
               │
               ▼
    ┌──────────────────────────────────────┐
    │ Game Restart                         │
    │ SoundManager.Init()                  │
    └──────────┬───────────────────────────┘
               │
               ▼
    ┌──────────────────────────────────────┐
    │ LoadUserSettings() async             │
    │ UserDataManager.GetAsync(key)        │
    └──────────┬───────────────────────────┘
               │
               ▼
    ┌──────────────────────────────────────┐
    │ Restore saved settings               │
    │ channel.SetVolume(savedVolume)       │
    └──────────────────────────────────────┘
```

---

## Thread Safety & Async Considerations

```
┌────────────────────────────────────────────────┐
│          SoundManager Init Flow                │
├────────────────────────────────────────────────┤
│                                                │
│  Main Thread:                                  │
│  ┌────────────────────────────────────────┐   │
│  │ await Init(location)                   │   │
│  └────────────┬───────────────────────────┘   │
│               │                                │
│               ├─────────► SetupChannels()      │ Sync
│               │                                │
│               ├─────────► CreateAudioSource() │ Sync
│               │                                │
│               └─────────► await              │ Async
│                          PreloadSounds()      │
│                          await               │
│                          LoadUserSettings()  │
│                                               │
│  Async Operations (non-blocking):            │
│  ┌────────────────────────────────────────┐  │
│  │ Preload Phase:                         │  │
│  │ - For each sound with PreloadEnabled   │  │
│  │ - Create temp AudioSource              │  │
│  │ - Play/stop for preload                │  │
│  │ - Destroy temp source                  │  │
│  │ - Small delays between (UniTask.Delay) │  │
│  └────────────────────────────────────────┘  │
│                                               │
│  ┌────────────────────────────────────────┐  │
│  │ UserSettings Phase:                    │  │
│  │ - Query UserDataManager for each type  │  │
│  │ - No blocking (async/await)            │  │
│  │ - Apply loaded settings                │  │
│  └────────────────────────────────────────┘  │
│                                               │
└────────────────────────────────────────────────┘
```

---

## Typical Usage Flow

```
Game Start
    │
    ▼
┌──────────────────────────┐
│ Create SoundManager      │
│ GameObject               │
└──────────┬───────────────┘
           │
           ▼
┌──────────────────────────┐
│ await SoundManager       │
│   .Init()                │
└──────────┬───────────────┘
           │
           ▼
┌──────────────────────────────────────┐
│ Subscribe to Events (optional)       │
│ EventManager.Add<T>(...)             │
└──────────┬───────────────────────────┘
           │
           ▼
┌──────────────────────────────────────┐
│ Game Loop: Use SoundManager          │
│                                      │
│ PlayMusic/PlaySfx/PlayAmbience      │
│ SetChannelVolume/Mute               │
│ StopMusic/StopAmbience              │
└──────────────────────────────────────┘
           │
           ▼ (On user action)
┌──────────────────────────────────────┐
│ Settings Saved Automatically         │
│ (via UserDataManager)                │
└──────────────────────────────────────┘
           │
           ▼
      Game Restart
           │
           ▼ (Settings Loaded)
┌──────────────────────────────────────┐
│ UserSettings restored                │
│ Same volume/mute state               │
└──────────────────────────────────────┘
```

---

## Performance Characteristics

```
Operation Costs:

┌──────────────────────┬──────────┬─────────────┐
│ Operation            │ Time     │ Notes       │
├──────────────────────┼──────────┼─────────────┤
│ Init()               │ Async    │ Non-blocking│
│                      │ ~100-200ms│ w/ preload │
├──────────────────────┼──────────┼─────────────┤
│ PlayMusic()          │ < 1ms    │ Immediate  │
├──────────────────────┼──────────┼─────────────┤
│ PlaySfx()            │ < 1ms    │ Immediate  │
├──────────────────────┼──────────┼─────────────┤
│ PlayAmbience()       │ < 1ms    │ Immediate  │
├──────────────────────┼──────────┼─────────────┤
│ SetChannelVolume()   │ < 1ms    │ Immediate  │
├──────────────────────┼──────────┼─────────────┤
│ ConfigSoundAttribute │ < 5ms    │ Editor only│
│ Dropdown             │          │ ~30 IDs    │
├──────────────────────┼──────────┼─────────────┤
│ UserData Persist     │ Async    │ Non-blocking│
│                      │ ~10-50ms │ varies     │
└──────────────────────┴──────────┴─────────────┘

Memory:
- Base: ~50KB (SoundManager + channels)
- Per AudioSource: ~50-100KB
- Per ConfigSoundData in cache: ~1KB
- UserSettings persistence: ~100 bytes
```

---

**Architecture Version**: 1.0  
**Last Updated**: July 5, 2026

