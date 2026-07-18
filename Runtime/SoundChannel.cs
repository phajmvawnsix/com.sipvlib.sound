using System;
using SiPVLib.Sound.Configs;
using UnityEngine;
using UnityEngine.Audio;

namespace SiPVLib.Sound
{
    /// <summary>
    /// Configuration for a sound channel (Music, SFX, Ambience).
    /// Handles volume, mute state, and mixer group mapping.
    /// </summary>
    public class SoundChannel
    {
        private SoundType _type;
        private float _volume = 1f;
        private bool _isMuted = false;
        private AudioMixerGroup _mixerGroup;
        private Action<float> _onVolumeChanged;
        private Action<bool> _onMuteChanged;

        public SoundType Type => _type;
        public float Volume => _volume;
        public bool IsMuted => _isMuted;
        public AudioMixerGroup MixerGroup => _mixerGroup;

        public SoundChannel(SoundType type, AudioMixerGroup mixerGroup = null)
        {
            _type = type;
            _mixerGroup = mixerGroup;
        }

        public void SetVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (Mathf.Approximately(_volume, volume)) return;
            
            _volume = volume;
            _onVolumeChanged?.Invoke(_volume);
        }

        public void SetMute(bool muted)
        {
            if (_isMuted == muted) return;
            
            _isMuted = muted;
            _onMuteChanged?.Invoke(_isMuted);
        }

        public void OnVolumeChanged(Action<float> callback)
        {
            _onVolumeChanged += callback;
        }

        public void OnMuteChanged(Action<bool> callback)
        {
            _onMuteChanged += callback;
        }

        public void RemoveVolumeListener(Action<float> callback)
        {
            _onVolumeChanged -= callback;
        }

        public void RemoveMuteListener(Action<bool> callback)
        {
            _onMuteChanged -= callback;
        }
    }
}

