using SiPVLib.Sound.Configs;

namespace SiPVLib.Sound.Utilities
{
    /// <summary>
    /// Static utility class for common sound operations.
    /// Provides convenient methods for frequently-used sound interactions.
    /// </summary>
    public static class SoundUtility
    {
        /// <summary>Plays a menu navigation sound (SFX).</summary>
        public static void PlayMenuNavigate() => PlaySfx("ui_navigate");

        /// <summary>Plays a menu click sound (SFX).</summary>
        public static void PlayMenuClick() => PlaySfx("ui_click");

        /// <summary>Plays a UI error/deny sound (SFX).</summary>
        public static void PlayError() => PlaySfx("ui_error");

        /// <summary>Plays a success/confirm sound (SFX).</summary>
        public static void PlaySuccess() => PlaySfx("ui_success");

        /// <summary>Plays a generic SFX sound.</summary>
        public static void PlaySfx(string soundId)
        {
            var manager = SoundManager.Instance;
            if (manager != null && manager.IsInitialized)
            {
                manager.PlaySfx(soundId);
            }
        }

        /// <summary>Plays a music track.</summary>
        public static void PlayMusic(string soundId, float fadeDuration = 0f)
        {
            var manager = SoundManager.Instance;
            if (manager != null && manager.IsInitialized)
            {
                manager.PlayMusic(soundId, fadeDuration);
            }
        }

        /// <summary>Stops the current music.</summary>
        public static void StopMusic()
        {
            var manager = SoundManager.Instance;
            if (manager != null && manager.IsInitialized)
            {
                manager.StopMusic();
            }
        }

        /// <summary>Plays an ambience sound.</summary>
        public static void PlayAmbience(string soundId)
        {
            var manager = SoundManager.Instance;
            if (manager != null && manager.IsInitialized)
            {
                manager.PlayAmbience(soundId);
            }
        }

        /// <summary>Stops an ambience sound.</summary>
        public static void StopAmbience(string soundId)
        {
            var manager = SoundManager.Instance;
            if (manager != null && manager.IsInitialized)
            {
                manager.StopAmbience(soundId);
            }
        }

        /// <summary>Sets a channel's volume (0 to 1).</summary>
        public static void SetVolume(SoundType soundType, float volume)
        {
            var manager = SoundManager.Instance;
            if (manager != null && manager.IsInitialized)
            {
                manager.SetChannelVolume(soundType, volume);
            }
        }

        /// <summary>Sets a channel's mute state.</summary>
        public static void SetMute(SoundType soundType, bool muted)
        {
            var manager = SoundManager.Instance;
            if (manager != null && manager.IsInitialized)
            {
                manager.SetChannelMute(soundType, muted);
            }
        }

        /// <summary>Toggles a channel's mute state.</summary>
        public static void ToggleMute(SoundType soundType)
        {
            var manager = SoundManager.Instance;
            if (manager != null && manager.IsInitialized)
            {
                var isMuted = manager.IsChannelMuted(soundType);
                manager.SetChannelMute(soundType, !isMuted);
            }
        }

        /// <summary>Gets a channel's current volume.</summary>
        public static float GetVolume(SoundType soundType)
        {
            var manager = SoundManager.Instance;
            return manager != null && manager.IsInitialized ? manager.GetChannelVolume(soundType) : 1f;
        }

        /// <summary>Gets a channel's mute state.</summary>
        public static bool IsMuted(SoundType soundType)
        {
            var manager = SoundManager.Instance;
            return manager != null && manager.IsInitialized && manager.IsChannelMuted(soundType);
        }
    }
}

