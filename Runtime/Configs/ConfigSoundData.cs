using Cysharp.Threading.Tasks;
using PipaPlanet.PipaPlanet.Scripts.Utilities;
using SiPVLib.Config;
using UnityEngine;

namespace SiPVLib.Sound.Configs
{
    /// <summary>
    /// Configuration data for an individual sound clip.
    /// Holds only the Id of an AudioClipConfig (com.sipvlib.config) plus playback tuning; the actual
    /// AudioClip asset lives in that AudioClipConfig and is loaded via ConfigManager per its own
    /// ConfigLocation (Local/Resources/Addressable), never embedded directly here.
    /// </summary>
    [System.Serializable]
    public class ConfigSoundData
    {
        [SerializeField]
        [ConfigRef(typeof(AudioClipConfig))]
        private string _audioClipConfigId;

        [SerializeField]
        [Range(0f, 1f)]
        private float _volume = 1f;

        [SerializeField]
        [Range(-3f, 3f)]
        private float _pitch = 1f;

        public string AudioClipConfigId => _audioClipConfigId;
        public float Volume => _volume;
        public float Pitch => _pitch;

        /// <summary>Resolves the AudioClipConfig referenced by Id, searching all ConfigLocations.</summary>
        public AudioClipConfig GetAudioClipConfig()
        {
            return string.IsNullOrEmpty(_audioClipConfigId)
                ? null
                : ConfigManager.Get<AudioClipConfig>(_audioClipConfigId, findAllIfNotFound: true);
        }

        public AudioClip GetAudioClip() => GetAudioClipConfig()?.GetAsset();

        public async UniTask<AudioClip> GetAudioClipAsync()
        {
            var config = GetAudioClipConfig();
            return config == null ? null : await config.GetAssetAsync();
        }
    }
}
