using SiPVLib.Sound.Configs;
using Alchemy.Inspector;
using UnityEngine;

namespace SiPVLib.Sound.Utilities
{
    /// <summary>
    /// Utility component for playing sound effects with customizable settings.
    /// Attach to any GameObject and configure in the inspector to play SFX on command.
    /// Supports custom volume, pitch, and playback speed adjustments.
    /// </summary>
    public class SoundEffectPlayer : MonoBehaviour
    {
        [Header("Sound Selection")]
        [ConfigSound(SoundType.Sfx)]
        [SerializeField] private string _sfxId = "";

        [Header("Playback Settings")]
        [SerializeField] [Range(0f, 1f)] private float _volumeMultiplier = 1f;
        [SerializeField] [Range(-3f, 3f)] private float _pitchOffset = 0f;
        [SerializeField] [Range(0.1f, 2f)] private float _playbackSpeed = 1f;

        [Header("Advanced")]
        [SerializeField] private bool _randomizePitch = false;
        [ShowIf(nameof(_randomizePitch))]
        [SerializeField] [Range(0f, 1f)] private float _randomPitchAmount = 0.1f;

        private float _currentVolume = 1f;
        private float _currentPitch = 1f;

        #region Playback Control

        /// <summary>Plays the configured SFX with current settings.</summary>
        public void Play()
        {
            if (string.IsNullOrEmpty(_sfxId))
            {
                Debug.LogWarning("[SoundEffectPlayer] SFX ID is not set.", gameObject);
                return;
            }

            var soundManager = SoundManager.Instance;
            if (soundManager == null || !soundManager.IsInitialized)
            {
                Debug.LogWarning("[SoundEffectPlayer] SoundManager not initialized.", gameObject);
                return;
            }

            // Calculate effective pitch with optional randomization
            _currentPitch = _pitchOffset + _playbackSpeed;
            if (_randomizePitch)
            {
                _currentPitch += Random.Range(-_randomPitchAmount, _randomPitchAmount);
            }
            _currentPitch = Mathf.Clamp(_currentPitch, -3f, 3f);

            // Apply settings and play
            ApplyAudioSettings();
            soundManager.PlaySfx(_sfxId);
        }

        /// <summary>Plays an SFX with specified ID and custom volume multiplier.</summary>
        public void PlayWithVolume(float volumeMultiplier)
        {
            _volumeMultiplier = Mathf.Clamp01(volumeMultiplier);
            Play();
        }

        /// <summary>Plays an SFX with specified ID and custom pitch offset.</summary>
        public void PlayWithPitch(float pitchOffset)
        {
            _pitchOffset = Mathf.Clamp(pitchOffset, -3f, 3f);
            Play();
        }

        /// <summary>Plays an SFX with specified ID, volume, and pitch.</summary>
        public void PlayWithSettings(float volumeMultiplier, float pitchOffset)
        {
            _volumeMultiplier = Mathf.Clamp01(volumeMultiplier);
            _pitchOffset = Mathf.Clamp(pitchOffset, -3f, 3f);
            Play();
        }

        #endregion

        #region Settings Control

        /// <summary>Sets the SFX ID to play.</summary>
        public void SetSfxId(string sfxId)
        {
            _sfxId = sfxId;
        }

        /// <summary>Sets the volume multiplier (0 to 1).</summary>
        public void SetVolumeMultiplier(float multiplier)
        {
            _volumeMultiplier = Mathf.Clamp01(multiplier);
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

        /// <summary>Enables or disables pitch randomization.</summary>
        public void SetRandomizePitch(bool enabled)
        {
            _randomizePitch = enabled;
        }

        /// <summary>Sets the random pitch variation amount (0 to 1).</summary>
        public void SetRandomPitchAmount(float amount)
        {
            _randomPitchAmount = Mathf.Clamp01(amount);
        }

        /// <summary>Gets the current SFX ID.</summary>
        public string GetSfxId() => _sfxId;

        /// <summary>Gets the current volume multiplier.</summary>
        public float GetVolumeMultiplier() => _volumeMultiplier;

        /// <summary>Gets the current pitch offset.</summary>
        public float GetPitchOffset() => _pitchOffset;

        /// <summary>Gets the current playback speed.</summary>
        public float GetPlaybackSpeed() => _playbackSpeed;

        #endregion

        #region Private Helpers

        private void ApplyAudioSettings()
        {
            // Note: Direct AudioSource manipulation would be needed for real-time
            // pitch/speed adjustments. Current implementation uses configured values.
            // Future enhancement: Could return to SoundManager for direct access.
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
            _randomPitchAmount = Mathf.Clamp01(_randomPitchAmount);
        }
#endif

        #endregion
    }
}

