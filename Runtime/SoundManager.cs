using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SiPVLib.Config;
using SiPVLib.Debugging;
using SiPVLib.Event;
using SiPVLib.Sound.Configs;
using SiPVLib.UserData;
using SiPVLib.Utilities;
using UnityEngine;
using UnityEngine.Audio;

namespace SiPVLib.Sound
{
    /// <summary>
    /// Central manager for all sound playback in the game.
    /// Handles music, SFX (oneshot), and ambience (looping) sounds with separate AudioSource management,
    /// AudioMixer volume control, and UserDataManager persistence for user settings.
    /// </summary>
    public class SoundManager : MonoSingleton<SoundManager>
    {
        #region Events

        /// <summary>Fired when a sound starts playing. Payload: SoundPlayEvent</summary>
        /// <param name="eventData">The event data containing information about the sound that started. (SoundPlayEvent)</param>
        public const string EventSoundStarted = "Sound.Started";
        
        /// <summary>Fired when a sound stops playing. Payload: SoundStopEvent</summary>
        /// <param name="eventData">The event data containing information about the sound that stopped. (SoundStopEvent)</param>
        public const string EventSoundStopped = "Sound.Stopped";
        
        /// <summary>Fired when a channel's volume changes. Payload: SoundVolumeChangedEvent</summary>
        /// <param name="eventData">The event data containing information about the volume change. (SoundVolumeChangedEvent)</param>
        public const string EventSoundVolumeChanged = "Sound.VolumeChanged";
        
        /// <summary>Fired when a channel's mute state changes. Payload: SoundMuteChangedEvent</summary>
        /// <param name="eventData">The event data containing information about the mute state change. (SoundMuteChangedEvent)</param>
        public const string EventSoundMuteChanged = "Sound.MuteChanged";

        #endregion

        #region Inspector Settings

        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private string _musicMixerGroupName = "Music";
        [SerializeField] private string _sfxMixerGroupName = "SFX";
        [SerializeField] private string _ambienceMixerGroupName = "Ambience";

        [SerializeField] private int _maxAmbienceSounds = 8;

        #endregion

        #region Private Fields

        private AudioSource _musicSfxAudioSource;
        private Dictionary<SoundType, SoundChannel> _channels = new();
        private Dictionary<string, AudioSource> _ambienceSources = new(); // key: soundId
        
        /// <summary>Cache of all ConfigSoundGroup assets loaded during initialization.</summary>
        private ConfigSoundGroup[] _cachedSoundGroups = System.Array.Empty<ConfigSoundGroup>();

        private bool _isInitialized;
        private ConfigLocation _configLocation = ConfigLocation.Local;

        #endregion

        #region Properties

        public bool IsInitialized => _isInitialized;

        #endregion

        #region MonoSingleton Lifecycle

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        protected override void OnSingletonInitialized()
        {
            // Initialization deferred to explicit Init() call
        }

        #endregion

        #region Initialization

        /// <summary>Asynchronously initializes the SoundManager with channels, audio sources, and user settings.</summary>
        public async UniTask<bool> Init(ConfigLocation configLocation = ConfigLocation.Local)
        {
            if (_isInitialized)
            {
                CustomLog.LogWarning("[SoundManager] Already initialized.");
                return true;
            }

            _configLocation = configLocation;

            try
            {
                // Wait for ConfigManager to initialize
                var configManager = ConfigManager.Instance;
                if (configManager == null)
                {
                    CustomLog.LogError("[SoundManager] ConfigManager not available.");
                    return false;
                }

                // Wait for ConfigManager initialization if needed
                int waitTime = 0;
                int maxWaitTime = 30000; // 30 second timeout
                while (!configManager.IsFullInitialized && waitTime < maxWaitTime)
                {
                    await UniTask.Delay(100);
                    waitTime += 100;
                }

                if (!configManager.IsFullInitialized)
                {
                    CustomLog.LogError("[SoundManager] ConfigManager initialization timeout.");
                    return false;
                }

                // Cache all ConfigSoundGroup assets
                if (!await CacheAllSoundGroups())
                {
                    CustomLog.LogWarning("[SoundManager] No ConfigSoundGroup assets found, but continuing with empty cache.");
                }

                // Setup audio channels
                SetupChannels();

                // Create audio source for Music/SFX
                _musicSfxAudioSource = CreateAudioSource("MusicSfxAudioSource");

                // Load user settings from UserDataManager
                await LoadUserSettings();

                // Preload sounds marked with preloadEnabled
                await PreloadSounds();

                _isInitialized = true;
                CustomLog.Log($"[SoundManager] Initialized successfully. Cached {_cachedSoundGroups.Length} ConfigSoundGroup(s).");
                return true;
            }
            catch (Exception ex)
            {
                CustomLog.LogError($"[SoundManager] Initialization failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>Caches all ConfigSoundGroup assets from ConfigManager.</summary>
        private async UniTask<bool> CacheAllSoundGroups()
        {
            var soundGroups = ConfigManager.GetAll<ConfigSoundGroup>(_configLocation);
            if (soundGroups == null || soundGroups.Length == 0)
            {
                _cachedSoundGroups = System.Array.Empty<ConfigSoundGroup>();
                return false;
            }

            _cachedSoundGroups = soundGroups;
            return true;
        }

        private void SetupChannels()
        {
            var musicGroup = GetMixerGroup(_musicMixerGroupName);
            var sfxGroup = GetMixerGroup(_sfxMixerGroupName);
            var ambienceGroup = GetMixerGroup(_ambienceMixerGroupName);

            _channels[SoundType.Music] = new SoundChannel(SoundType.Music, musicGroup);
            _channels[SoundType.Sfx] = new SoundChannel(SoundType.Sfx, sfxGroup);
            _channels[SoundType.Ambience] = new SoundChannel(SoundType.Ambience, ambienceGroup);

            // Subscribe to channel changes for persistence
            foreach (var channel in _channels.Values)
            {
                channel.OnVolumeChanged(vol => SaveChannelSettings(channel.Type));
                channel.OnMuteChanged(muted => SaveChannelSettings(channel.Type));
            }
        }

        private AudioMixerGroup GetMixerGroup(string groupName)
        {
            if (_audioMixer == null)
            {
                CustomLog.LogWarning($"[SoundManager] AudioMixer not assigned. Channel '{groupName}' will not route through mixer.");
                return null;
            }

            var groups = _audioMixer.FindMatchingGroups(groupName);
            if (groups.Length > 0)
            {
                return groups[0];
            }

            CustomLog.LogWarning($"[SoundManager] AudioMixer group '{groupName}' not found.");
            return null;
        }

        private AudioSource CreateAudioSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var audioSource = go.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            return audioSource;
        }

        private async UniTask PreloadSounds()
        {
            // Use cached sound groups instead of querying ConfigManager
            foreach (var group in _cachedSoundGroups)
            {
                if (group == null) continue;

                await PreloadSoundDictionary(group.SfxClips);
                await PreloadSoundDictionary(group.MusicClips);
                await PreloadSoundDictionary(group.AmbienceClips);
            }
        }

        private async UniTask PreloadSoundDictionary(Dictionary<string, ConfigSoundData> soundDict)
        {
            if (soundDict == null) return;

            foreach (var kvp in soundDict)
            {
                var data = kvp.Value;
                if (data != null && data.PreloadEnabled && data.AudioClip != null)
                {
                    // Preload by playing silently and stopping
                    var tempSource = CreateAudioSource($"PreloadSource_{kvp.Key}");
                    tempSource.clip = data.AudioClip;
                    tempSource.volume = 0f;
                    tempSource.Play();
                    await UniTask.Delay(10); // Allow a frame for preload
                    tempSource.Stop();
                    Destroy(tempSource.gameObject);
                }
            }
        }

        private async UniTask LoadUserSettings()
        {
            var userDataManager = UserDataManager.Instance;
            if (userDataManager == null) return;

            foreach (var soundType in new[] { SoundType.Music, SoundType.Sfx, SoundType.Ambience })
            {
                var volumeKey = GetVolumeKey(soundType);
                var muteKey = GetMuteKey(soundType);

                var savedVolume = await userDataManager.GetAsync<float>(volumeKey);
                var savedMute = await userDataManager.GetAsync<bool>(muteKey);

                var channel = GetChannel(soundType);
                if (channel != null)
                {
                    channel.SetVolume(savedVolume > 0 ? savedVolume : 1f);
                    channel.SetMute(savedMute);
                }
            }
        }

        #endregion

        #region Playback Control

        /// <summary>Plays a music track, stopping any currently playing music.</summary>
        public void PlayMusic(string soundId, float fadeDuration = 0f)
        {
            if (!_isInitialized)
            {
                CustomLog.LogWarning("[SoundManager] Not initialized.");
                return;
            }

            if (string.IsNullOrEmpty(soundId))
            {
                CustomLog.LogWarning("[SoundManager] Sound ID is null or empty.");
                return;
            }

            var soundData = GetSoundData(soundId, SoundType.Music);
            if (soundData?.AudioClip == null)
            {
                CustomLog.LogWarning($"[SoundManager] Music '{soundId}' not found or has no clip.");
                return;
            }

            // Stop current music
            if (_musicSfxAudioSource.isPlaying)
            {
                _musicSfxAudioSource.Stop();
            }

            // Setup and play
            _musicSfxAudioSource.clip = soundData.AudioClip;
            _musicSfxAudioSource.volume = GetEffectiveVolume(SoundType.Music) * soundData.Volume;
            _musicSfxAudioSource.pitch = soundData.Pitch;
            _musicSfxAudioSource.loop = true;
            _musicSfxAudioSource.outputAudioMixerGroup = _channels[SoundType.Music].MixerGroup;
            _musicSfxAudioSource.Play();

            EventManager.Invoke(EventSoundStarted, new SoundPlayEvent
            {
                soundId = soundId,
                soundType = SoundType.Music
            });
        }

        /// <summary>Plays a one-shot SFX sound.</summary>
        public void PlaySfx(string soundId)
        {
            if (!_isInitialized)
            {
                CustomLog.LogWarning("[SoundManager] Not initialized.");
                return;
            }

            if (string.IsNullOrEmpty(soundId))
            {
                CustomLog.LogWarning("[SoundManager] Sound ID is null or empty.");
                return;
            }

            var soundData = GetSoundData(soundId, SoundType.Sfx);
            if (soundData?.AudioClip == null)
            {
                CustomLog.LogWarning($"[SoundManager] SFX '{soundId}' not found or has no clip.");
                return;
            }

            _musicSfxAudioSource.clip = soundData.AudioClip;
            _musicSfxAudioSource.volume = GetEffectiveVolume(SoundType.Sfx) * soundData.Volume;
            _musicSfxAudioSource.pitch = soundData.Pitch;
            _musicSfxAudioSource.loop = false;
            _musicSfxAudioSource.outputAudioMixerGroup = _channels[SoundType.Sfx].MixerGroup;
            _musicSfxAudioSource.PlayOneShot(soundData.AudioClip, _musicSfxAudioSource.volume);

            EventManager.Invoke(EventSoundStarted, new SoundPlayEvent
            {
                soundId = soundId,
                soundType = SoundType.Sfx
            });
        }

        /// <summary>Plays an ambience sound (loop, can have multiple playing).</summary>
        public void PlayAmbience(string soundId)
        {
            if (!_isInitialized)
            {
                CustomLog.LogWarning("[SoundManager] Not initialized.");
                return;
            }

            if (string.IsNullOrEmpty(soundId))
            {
                CustomLog.LogWarning("[SoundManager] Sound ID is null or empty.");
                return;
            }

            // Don't play duplicate ambience
            if (_ambienceSources.ContainsKey(soundId))
            {
                CustomLog.LogWarning($"[SoundManager] Ambience '{soundId}' is already playing.");
                return;
            }

            // Check max ambience limit
            if (_ambienceSources.Count >= _maxAmbienceSounds)
            {
                CustomLog.LogWarning($"[SoundManager] Max ambience sounds ({_maxAmbienceSounds}) reached.");
                return;
            }

            var soundData = GetSoundData(soundId, SoundType.Ambience);
            if (soundData?.AudioClip == null)
            {
                CustomLog.LogWarning($"[SoundManager] Ambience '{soundId}' not found or has no clip.");
                return;
            }

            var ambienceSource = CreateAudioSource($"Ambience_{soundId}");
            ambienceSource.clip = soundData.AudioClip;
            ambienceSource.volume = GetEffectiveVolume(SoundType.Ambience) * soundData.Volume;
            ambienceSource.pitch = soundData.Pitch;
            ambienceSource.loop = true;
            ambienceSource.outputAudioMixerGroup = _channels[SoundType.Ambience].MixerGroup;
            ambienceSource.Play();

            _ambienceSources[soundId] = ambienceSource;

            EventManager.Invoke(EventSoundStarted, new SoundPlayEvent
            {
                soundId = soundId,
                soundType = SoundType.Ambience
            });
        }

        /// <summary>Stops playing music.</summary>
        public void StopMusic()
        {
            if (_musicSfxAudioSource != null && _musicSfxAudioSource.isPlaying && _musicSfxAudioSource.loop)
            {
                var currentClipName = _musicSfxAudioSource.clip?.name ?? "Unknown";
                _musicSfxAudioSource.Stop();
                
                EventManager.Invoke(EventSoundStopped, new SoundStopEvent
                {
                    soundType = SoundType.Music
                });
            }
        }

        /// <summary>Stops playing a specific ambience sound.</summary>
        public void StopAmbience(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;

            if (_ambienceSources.TryGetValue(soundId, out var ambienceSource))
            {
                ambienceSource.Stop();
                Destroy(ambienceSource.gameObject);
                _ambienceSources.Remove(soundId);

                EventManager.Invoke(EventSoundStopped, new SoundStopEvent
                {
                    soundType = SoundType.Ambience
                });
            }
        }

        /// <summary>Stops all ambience sounds.</summary>
        public void StopAllAmbience()
        {
            var soundIds = new List<string>(_ambienceSources.Keys);
            foreach (var soundId in soundIds)
            {
                StopAmbience(soundId);
            }
        }

        #endregion

        #region Volume & Mute Control

        /// <summary>Sets the volume for a sound channel (0 to 1).</summary>
        public void SetChannelVolume(SoundType soundType, float volume)
        {
            var channel = GetChannel(soundType);
            if (channel == null) return;

            channel.SetVolume(volume);
            UpdateAudioSourceVolume(soundType);

            EventManager.Invoke(EventSoundVolumeChanged, new SoundVolumeChangedEvent
            {
                soundType = soundType,
                volume = volume
            });
        }

        /// <summary>Sets the mute state for a sound channel.</summary>
        public void SetChannelMute(SoundType soundType, bool muted)
        {
            var channel = GetChannel(soundType);
            if (channel == null) return;

            channel.SetMute(muted);
            UpdateAudioSourceVolume(soundType);

            EventManager.Invoke(EventSoundMuteChanged, new SoundMuteChangedEvent
            {
                soundType = soundType,
                isMuted = muted
            });
        }

        /// <summary>Gets the volume for a sound channel.</summary>
        public float GetChannelVolume(SoundType soundType)
        {
            var channel = GetChannel(soundType);
            return channel?.Volume ?? 1f;
        }

        /// <summary>Gets the mute state for a sound channel.</summary>
        public bool IsChannelMuted(SoundType soundType)
        {
            var channel = GetChannel(soundType);
            return channel?.IsMuted ?? false;
        }

        #endregion

        #region Private Helpers

        private ConfigSoundData GetSoundData(string soundId, SoundType expectedType)
        {
            var soundGroup = FindSoundGroupContainingSoundId(soundId, expectedType);
            if (soundGroup == null) return null;

            return expectedType switch
            {
                SoundType.Music => soundGroup.GetMusicData(soundId),
                SoundType.Sfx => soundGroup.GetSfxData(soundId),
                SoundType.Ambience => soundGroup.GetAmbienceData(soundId),
                _ => null
            };
        }

        private ConfigSoundGroup FindSoundGroupContainingSoundId(string soundId, SoundType soundType)
        {
            // Use cached sound groups instead of querying ConfigManager at runtime
            foreach (var group in _cachedSoundGroups)
            {
                if (group == null) continue;

                var dict = soundType switch
                {
                    SoundType.Music => group.MusicClips,
                    SoundType.Sfx => group.SfxClips,
                    SoundType.Ambience => group.AmbienceClips,
                    _ => null
                };

                if (dict != null && dict.ContainsKey(soundId))
                {
                    return group;
                }
            }

            return null;
        }

        private float GetEffectiveVolume(SoundType soundType)
        {
            var channel = GetChannel(soundType);
            if (channel == null) return 1f;
            return channel.IsMuted ? 0f : channel.Volume;
        }

        private void UpdateAudioSourceVolume(SoundType soundType)
        {
            var effectiveVolume = GetEffectiveVolume(soundType);

            if (soundType == SoundType.Music || soundType == SoundType.Sfx)
            {
                if (_musicSfxAudioSource != null)
                {
                    _musicSfxAudioSource.volume = effectiveVolume;
                }
            }
            else if (soundType == SoundType.Ambience)
            {
                foreach (var source in _ambienceSources.Values)
                {
                    if (source != null)
                    {
                        source.volume = effectiveVolume;
                    }
                }
            }
        }

        private SoundChannel GetChannel(SoundType soundType)
        {
            return _channels.TryGetValue(soundType, out var channel) ? channel : null;
        }

        private async void SaveChannelSettings(SoundType soundType)
        {
            var channel = GetChannel(soundType);
            if (channel == null) return;

            var userDataManager = UserDataManager.Instance;
            if (userDataManager == null) return;

            var volumeKey = GetVolumeKey(soundType);
            var muteKey = GetMuteKey(soundType);

            await userDataManager.SetAsync(volumeKey, channel.Volume);
            await userDataManager.SetAsync(muteKey, channel.IsMuted);
        }

        private static string GetVolumeKey(SoundType soundType) => $"Sound_Volume_{soundType}";
        private static string GetMuteKey(SoundType soundType) => $"Sound_Mute_{soundType}";

        #endregion

        #region Debug

        public void LogStatus()
        {
            var status = "[SoundManager] Status:\n";
            status += $"  Initialized: {_isInitialized}\n";
            foreach (var kvp in _channels)
            {
                status += $"  {kvp.Key}: Volume={kvp.Value.Volume:F2}, Muted={kvp.Value.IsMuted}\n";
            }
            status += $"  Active Ambience Sounds: {_ambienceSources.Count}/{_maxAmbienceSounds}\n";
            CustomLog.Log(status);
        }

        #endregion
    }

    #region Event Payloads

    [System.Serializable]
    public struct SoundPlayEvent
    {
        public string soundId;
        public SoundType soundType;
    }

    [System.Serializable]
    public struct SoundStopEvent
    {
        public SoundType soundType;
    }

    [System.Serializable]
    public struct SoundVolumeChangedEvent
    {
        public SoundType soundType;
        public float volume;
    }

    [System.Serializable]
    public struct SoundMuteChangedEvent
    {
        public SoundType soundType;
        public bool isMuted;
    }

    #endregion
}

