using System.Collections.Generic;
using SiPVLib.Config.Configs;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace SiPVLib.Sound.Configs
{
    public class ConfigSoundGroup : GameConfig
    {
        [SerializeField]
#if ODIN_INSPECTOR
        [DictionaryDrawerSettings(KeyLabel = "Id", ValueLabel = "Sound Data", DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
#endif
        private Dictionary<string, ConfigSoundData> _sfxClips, _musicClips, _ambienceClips;
        
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