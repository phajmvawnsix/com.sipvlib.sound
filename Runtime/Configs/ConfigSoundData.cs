using UnityEngine;

namespace SiPVLib.Sound.Configs
{
    /// <summary>
    /// Configuration data for an individual sound clip.
    /// Defines the audio clip and its properties: volume, pitch, and preload behavior.
    /// </summary>
    [System.Serializable]
    public class ConfigSoundData
    {
        [SerializeField] 
        private AudioClip _audioClip;
        
        [SerializeField] 
        [Range(0f, 1f)]
        private float _volume = 1f;
        
        [SerializeField] 
        [Range(-3f, 3f)]
        private float _pitch = 1f;
        
        [SerializeField] 
        private bool _preloadEnabled = false;

        public AudioClip AudioClip => _audioClip;
        public float Volume => _volume;
        public float Pitch => _pitch;
        public bool PreloadEnabled => _preloadEnabled;
    }
}


