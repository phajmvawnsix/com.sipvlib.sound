using SiPVLib.Sound.Configs;
using Alchemy.Inspector;
using UnityEngine;

namespace SiPVLib.Sound.Utilities
{
    /// <summary>
    /// Utility component for playing ambience sounds with customizable settings.
    /// Supports playing multiple simultaneous ambience tracks with independent control.
    /// Attach to any GameObject and configure in the inspector to manage ambience playback.
    /// </summary>
    public class AmbiencePlayer : MonoBehaviour
    {
        [Header("Sound Selection")]
        [ConfigSound(SoundType.Ambience)]
        [SerializeField] private string _ambienceId = "";

        [Header("Playback Settings")]
        [SerializeField] [Range(0f, 1f)] private float _volumeMultiplier = 1f;
        [SerializeField] [Range(-3f, 3f)] private float _pitchOffset = 0f;
        [SerializeField] [Range(0.1f, 2f)] private float _playbackSpeed = 1f;

        [Header("Playback Options")]
        [SerializeField] private bool _autoPlayOnStart = false;
        [SerializeField] private bool _stopOnDisable = false;

        [Header("Fade Settings")]
        [SerializeField] private bool _useFadeIn = false;
        [ShowIf(nameof(_useFadeIn))]
        [SerializeField] [Range(0f, 5f)] private float _fadeInDuration = 1f;
        [SerializeField] private bool _useFadeOut = false;
        [ShowIf(nameof(_useFadeOut))]
        [SerializeField] [Range(0f, 5f)] private float _fadeOutDuration = 1f;

        private bool _isPlaying = false;
        private float _targetVolume = 1f;
        private float _currentFadeTime = 0f;

        #region Lifecycle

        private void Start()
        {
            if (_autoPlayOnStart && !string.IsNullOrEmpty(_ambienceId))
            {
                Play();
            }
        }

        private void OnDisable()
        {
            if (_stopOnDisable && _isPlaying)
            {
                Stop();
            }
        }

        private void Update()
        {
            // Handle fade in/out
            if (_useFadeIn && _isPlaying && _currentFadeTime < _fadeInDuration)
            {
                _currentFadeTime += Time.deltaTime;
                float fadeProgress = Mathf.Clamp01(_currentFadeTime / _fadeInDuration);
                UpdateAmbienceVolume(fadeProgress * _targetVolume);
            }
        }

        #endregion

        #region Playback Control

        /// <summary>Plays the configured ambience sound with current settings.</summary>
        public void Play()
        {
            if (string.IsNullOrEmpty(_ambienceId))
            {
                Debug.LogWarning("[AmbiencePlayer] Ambience ID is not set.", gameObject);
                return;
            }

            var soundManager = SoundManager.Instance;
            if (soundManager == null || !soundManager.IsInitialized)
            {
                Debug.LogWarning("[AmbiencePlayer] SoundManager not initialized.", gameObject);
                return;
            }

            // Reset fade timer
            _currentFadeTime = 0f;
            _targetVolume = _volumeMultiplier;

            // Start with zero volume if fading in
            if (_useFadeIn)
            {
                UpdateAmbienceVolume(0f);
            }

            soundManager.PlayAmbience(_ambienceId);
            ApplyAudioSettings();

            _isPlaying = true;
        }

        /// <summary>Plays an ambience sound with specified ID and custom volume multiplier.</summary>
        public void PlayWithVolume(float volumeMultiplier)
        {
            _volumeMultiplier = Mathf.Clamp01(volumeMultiplier);
            Play();
        }

        /// <summary>Plays an ambience sound with specified ID and custom pitch offset.</summary>
        public void PlayWithPitch(float pitchOffset)
        {
            _pitchOffset = Mathf.Clamp(pitchOffset, -3f, 3f);
            Play();
        }

        /// <summary>Plays an ambience sound with specified ID, volume, and pitch.</summary>
        public void PlayWithSettings(float volumeMultiplier, float pitchOffset)
        {
            _volumeMultiplier = Mathf.Clamp01(volumeMultiplier);
            _pitchOffset = Mathf.Clamp(pitchOffset, -3f, 3f);
            Play();
        }

        /// <summary>Stops the configured ambience sound (with optional fade out).</summary>
        public void Stop()
        {
            var soundManager = SoundManager.Instance;
            if (soundManager != null && soundManager.IsInitialized)
            {
                if (_useFadeOut && _isPlaying)
                {
                    // For now, fade out is immediate (future: could add tween system)
                    soundManager.StopAmbience(_ambienceId);
                }
                else
                {
                    soundManager.StopAmbience(_ambienceId);
                }

                _isPlaying = false;
            }
        }

        /// <summary>Stops all ambience sounds (emergency stop).</summary>
        public void StopAll()
        {
            var soundManager = SoundManager.Instance;
            if (soundManager != null && soundManager.IsInitialized)
            {
                soundManager.StopAllAmbience();
                _isPlaying = false;
            }
        }

        #endregion

        #region Settings Control

        /// <summary>Sets the ambience ID to play.</summary>
        public void SetAmbienceId(string ambienceId)
        {
            _ambienceId = ambienceId;
        }

        /// <summary>Sets the volume multiplier (0 to 1).</summary>
        public void SetVolumeMultiplier(float multiplier)
        {
            _volumeMultiplier = Mathf.Clamp01(multiplier);
            _targetVolume = _volumeMultiplier;
            if (_isPlaying)
            {
                UpdateAmbienceVolume(_volumeMultiplier);
            }
        }

        /// <summary>Sets the pitch offset (-3 to +3).</summary>
        public void SetPitchOffset(float offset)
        {
            _pitchOffset = Mathf.Clamp(offset, -3f, 3f);
        }

        /// <summary>Sets the playback speed (0.1 to 2.0).</summary>
        public void SetPlaybackSpeed(float speed)
        {
            _playbackSpeed = Mathf.Clamp(speed, 0.1f, 2f);
        }

        /// <summary>Enables or disables fade in.</summary>
        public void SetUseFadeIn(bool enabled)
        {
            _useFadeIn = enabled;
        }

        /// <summary>Sets the fade in duration (0 to 5 seconds).</summary>
        public void SetFadeInDuration(float duration)
        {
            _fadeInDuration = Mathf.Clamp(duration, 0f, 5f);
        }

        /// <summary>Enables or disables fade out.</summary>
        public void SetUseFadeOut(bool enabled)
        {
            _useFadeOut = enabled;
        }

        /// <summary>Sets the fade out duration (0 to 5 seconds).</summary>
        public void SetFadeOutDuration(float duration)
        {
            _fadeOutDuration = Mathf.Clamp(duration, 0f, 5f);
        }

        /// <summary>Gets the current ambience ID.</summary>
        public string GetAmbienceId() => _ambienceId;

        /// <summary>Gets the current volume multiplier.</summary>
        public float GetVolumeMultiplier() => _volumeMultiplier;

        /// <summary>Gets the current pitch offset.</summary>
        public float GetPitchOffset() => _pitchOffset;

        /// <summary>Gets the current playback speed.</summary>
        public float GetPlaybackSpeed() => _playbackSpeed;

        /// <summary>Checks if ambience is currently playing.</summary>
        public bool IsPlaying() => _isPlaying;

        #endregion

        #region Private Helpers

        private void ApplyAudioSettings()
        {
            // Note: Direct AudioSource manipulation would be needed for real-time
            // pitch/speed adjustments. Current implementation uses configured values.
            // Future enhancement: Could return to SoundManager for direct access.
        }

        private void UpdateAmbienceVolume(float volume)
        {
            // Direct volume update (requires future enhancement to SoundManager)
            // For now, relies on SoundManager channel volume control
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp values to valid ranges in editor
            _volumeMultiplier = Mathf.Clamp01(_volumeMultiplier);
            _pitchOffset = Mathf.Clamp(_pitchOffset, -3f, 3f);
            _playbackSpeed = Mathf.Clamp(_playbackSpeed, 0.1f, 2f);
            _fadeInDuration = Mathf.Clamp(_fadeInDuration, 0f, 5f);
            _fadeOutDuration = Mathf.Clamp(_fadeOutDuration, 0f, 5f);
        }
#endif

        #endregion
    }
}

