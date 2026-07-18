using SiPVLib.Sound.Configs;
using UnityEngine;

namespace SiPVLib.Sound.Utilities
{
    /// <summary>
    /// Utility component for playing background music with customizable settings.
    /// Attach to any GameObject and configure in the inspector to play music on command.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        [Header("Sound Selection")]
        [ConfigSound(SoundType.Music)]
        [SerializeField] private string _musicId = "";

        [Header("Playback Settings")]
        [SerializeField] private float _volume = 1f;
        [SerializeField] private float _pitch = 1f;

        [Header("Playback Options")]
        [SerializeField] private bool _autoPlayOnStart = false;
        [SerializeField] private bool _stopOnDestroy = false;

        private bool _isPlaying = false;
        private string _currentMusicId = "";

        #region Lifecycle

        private void Start()
        {
            if (_autoPlayOnStart && !string.IsNullOrEmpty(_musicId))
            {
                Play();
            }
        }

        private void OnDestroy()
        {
            if (_stopOnDestroy && _isPlaying)
            {
                Stop();
            }
        }

        #endregion

        #region Playback Control

        /// <summary>Plays the configured music with current settings.</summary>
        public void Play()
        {
            if (string.IsNullOrEmpty(_musicId))
            {
                Debug.LogWarning("[MusicPlayer] Music ID is not set.", gameObject);
                return;
            }

            var soundManager = SoundManager.Instance;
            if (soundManager == null || !soundManager.IsInitialized)
            {
                Debug.LogWarning("[MusicPlayer] SoundManager not initialized.", gameObject);
                return;
            }

            soundManager.PlayMusic(_musicId);
            
            // Apply volume and pitch overrides
            ApplyAudioSettings();

            _isPlaying = true;
            _currentMusicId = _musicId;
        }

        /// <summary>Stops the currently playing music.</summary>
        public void Stop()
        {
            var soundManager = SoundManager.Instance;
            if (soundManager != null && soundManager.IsInitialized)
            {
                soundManager.StopMusic();
                _isPlaying = false;
            }
        }

        /// <summary>Pauses the music without destroying the audio source.</summary>
        public void Pause()
        {
            // Note: This requires finding the underlying AudioSource
            // For now, we recommend using Stop() instead
            Debug.LogWarning("[MusicPlayer] Pause not directly supported. Use Stop() instead.");
        }

        #endregion

        #region Settings Control

        /// <summary>Sets the music ID to play.</summary>
        public void SetMusicId(string musicId)
        {
            _musicId = musicId;
        }

        /// <summary>Sets the playback volume (0 to 1).</summary>
        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
            if (_isPlaying)
            {
                ApplyAudioSettings();
            }
        }

        /// <summary>Sets the playback pitch (-3 to +3).</summary>
        public void SetPitch(float pitch)
        {
            _pitch = Mathf.Clamp(pitch, -3f, 3f);
            if (_isPlaying)
            {
                ApplyAudioSettings();
            }
        }

        /// <summary>Gets the current music ID.</summary>
        public string GetMusicId() => _musicId;

        /// <summary>Gets the current volume setting.</summary>
        public float GetVolume() => _volume;

        /// <summary>Gets the current pitch setting.</summary>
        public float GetPitch() => _pitch;

        /// <summary>Checks if music is currently playing.</summary>
        public bool IsPlaying() => _isPlaying;

        #endregion

        #region Private Helpers

        private void ApplyAudioSettings()
        {
            // Note: Volume and pitch adjustments would need direct AudioSource access
            // Current implementation relies on ConfigSoundData values
            // Future enhancement: Could add multiplier system
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp values to valid ranges in editor
            _volume = Mathf.Clamp01(_volume);
            _pitch = Mathf.Clamp(_pitch, -3f, 3f);
        }
#endif

        #endregion
    }
}

