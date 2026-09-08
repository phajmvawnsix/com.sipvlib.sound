using System;
using System.Collections.Generic;
using SiPVLib.Config.Configs;
using SiPVLib.Utilities.Serialization;
using UnityEngine;

namespace SiPVLib.Sound.Configs
{
    public class ConfigSoundGroup : GameConfig
    {
        /// <summary>
        /// Unity only serializes concrete, closed generic types, so the dictionary of sound entries
        /// needs a named subclass rather than <c>SerializableDictionary&lt;string, ConfigSoundData&gt;</c>
        /// used directly as a field type.
        /// </summary>
        [Serializable]
        public class SoundDataDictionary : SerializableDictionary<string, ConfigSoundData>
        {
        }

        [SerializeField, SerializableDictionary]
        private SoundDataDictionary _sfxClips = new();

        [SerializeField, SerializableDictionary]
        private SoundDataDictionary _musicClips = new();

        [SerializeField, SerializableDictionary]
        private SoundDataDictionary _ambienceClips = new();

        public Dictionary<string, ConfigSoundData> SfxClips => _sfxClips;
        public Dictionary<string, ConfigSoundData> MusicClips => _musicClips;
        public Dictionary<string, ConfigSoundData> AmbienceClips => _ambienceClips;

        public ConfigSoundData GetSfxData(string id)
        {
            if (_sfxClips.TryGetValue(id, out var data))
            {
                return data;
            }

            Debug.LogWarning($"SFX Clip with id '{id}' not found in ConfigSoundGroup '{name}'");
            return null;
        }

        public ConfigSoundData GetMusicData(string id)
        {
            if (_musicClips.TryGetValue(id, out var data))
            {
                return data;
            }

            Debug.LogWarning($"Music Clip with id '{id}' not found in ConfigSoundGroup '{name}'");
            return null;
        }

        public ConfigSoundData GetAmbienceData(string id)
        {
            if (_ambienceClips.TryGetValue(id, out var data))
            {
                return data;
            }

            Debug.LogWarning($"Ambience Clip with id '{id}' not found in ConfigSoundGroup '{name}'");
            return null;
        }
    }
}
