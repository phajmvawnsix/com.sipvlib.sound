using System.Collections.Generic;
using System.Linq;
using SiPVLib.Config;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SiPVLib.Sound.Configs
{
    /// <summary>
    /// Editor-only utility for accessing and caching sound data from ConfigSoundGroup assets.
    /// Automatically detects changes and invalidates cache when ConfigSoundGroup assets are modified.
    /// </summary>
    public static class SoundRefsEditor
    {
        private static Dictionary<string, ConfigSoundGroup> _soundGroupCache = new();
        private static Dictionary<string, ConfigSoundData> _soundDataCache = new();
        private static Dictionary<string, SoundType> _soundIdToTypeCache = new();
        
        /// <summary>Tracks modification times of ConfigSoundGroup assets to detect changes.</summary>
        private static Dictionary<string, long> _configSoundGroupModTimes = new();
        
        /// <summary>Last check time to avoid excessive checks.</summary>
        private static double _lastCheckTime = 0;
        
        /// <summary>Check interval in seconds for detecting changes.</summary>
        private const double CHECK_INTERVAL = 0.5;
        
        public static AudioClip GetSoundClip(string soundId)
        {
            if (_soundDataCache.TryGetValue(soundId, out var data) && data != null)
            {
                return data.AudioClip;
            }
            
            CacheSoundGroups();
            if (_soundDataCache.TryGetValue(soundId, out data) && data != null)
            {
                return data.AudioClip;
            }
            return null;
        }

        public static ConfigSoundData GetSoundData(string soundId)
        {
            if (_soundDataCache.TryGetValue(soundId, out var data))
            {
                return data;
            }
            
            CacheSoundGroups();
            if (_soundDataCache.TryGetValue(soundId, out data))
            {
                return data;
            }
            return null;
        }

        public static SoundType? GetSoundType(string soundId)
        {
            if (_soundIdToTypeCache.TryGetValue(soundId, out var type))
            {
                return type;
            }
            
            CacheSoundGroups();
            if (_soundIdToTypeCache.TryGetValue(soundId, out type))
            {
                return type;
            }
            return null;
        }

        public static List<string> GetAvailableSoundIds(SoundType? filterType = null)
        {
            CacheSoundGroups();
            
            var soundIds = filterType.HasValue
                ? _soundIdToTypeCache.Where(kvp => kvp.Value == filterType.Value).Select(kvp => kvp.Key).ToList()
                : _soundDataCache.Keys.ToList();

            soundIds.Sort();
            return soundIds;
        }
        
        public static void CacheSoundGroups()
        {
            if (_soundGroupCache.Count > 0) return;
            
            _soundDataCache.Clear();
            _soundGroupCache.Clear();
            _soundIdToTypeCache.Clear();
            
            ConfigRootRefsEditor.UpdateCache();
            var soundGroups = ConfigRootRefsEditor.GetConfigs<ConfigSoundGroup>();
            foreach (var soundGroup in soundGroups)
            {
                _soundGroupCache[soundGroup.Id] = soundGroup;
                
                foreach (var sfx in soundGroup.SfxClips)
                {
                    _soundDataCache[sfx.Key] = sfx.Value;
                    _soundIdToTypeCache[sfx.Key] = SoundType.Sfx;
                }
                foreach (var music in soundGroup.MusicClips)
                {
                    _soundDataCache[music.Key] = music.Value;
                    _soundIdToTypeCache[music.Key] = SoundType.Music;
                }
                foreach (var ambience in soundGroup.AmbienceClips)
                {
                    _soundDataCache[ambience.Key] = ambience.Value;
                    _soundIdToTypeCache[ambience.Key] = SoundType.Ambience;
                }
            }
        }

        public static void ClearCache()
        {
            _soundDataCache.Clear();
            _soundGroupCache.Clear();
            _soundIdToTypeCache.Clear();
            _configSoundGroupModTimes.Clear();
        }

        /// <summary>
        /// Checks if ConfigSoundGroup assets have been modified and invalidates cache if so.
        /// Called automatically by editor initialization.
        /// </summary>
        private static void CheckForConfigChanges()
        {
            // Rate limit checks to avoid excessive scanning
            if (EditorApplication.timeSinceStartup - _lastCheckTime < CHECK_INTERVAL)
                return;

            _lastCheckTime = EditorApplication.timeSinceStartup;

            // Find all ConfigSoundGroup assets
            var configGuids = AssetDatabase.FindAssets("t:ConfigSoundGroup");
            bool anyChanged = false;

            // Check if any ConfigSoundGroup has been modified
            foreach (var guid in configGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;

                // Get the modification time
                var asset = AssetDatabase.LoadAssetAtPath<ConfigSoundGroup>(path);
                if (asset == null) continue;

                long currentModTime = asset.GetHashCode(); // Simplified change detection

                if (_configSoundGroupModTimes.TryGetValue(path, out var lastModTime))
                {
                    // Check if the asset has changed
                    if (currentModTime != lastModTime)
                    {
                        anyChanged = true;
                        _configSoundGroupModTimes[path] = currentModTime;
                    }
                }
                else
                {
                    // New asset found
                    _configSoundGroupModTimes[path] = currentModTime;
                    anyChanged = true;
                }
            }

            // Remove tracking for deleted assets
            var deletedPaths = _configSoundGroupModTimes.Keys
                .Where(path => !AssetDatabase.FindAssets($"{System.IO.Path.GetFileNameWithoutExtension(path)} t:ConfigSoundGroup").Any())
                .ToList();

            foreach (var path in deletedPaths)
            {
                _configSoundGroupModTimes.Remove(path);
                anyChanged = true;
            }

            // Invalidate cache if changes detected
            if (anyChanged && _soundGroupCache.Count > 0)
            {
                ClearCache();
            }
        }

        /// <summary>
        /// Initializes the editor listener for automatic cache invalidation.
        /// Registered via InitializeOnLoad to run on editor startup.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void InitializeEditorListener()
        {
            EditorApplication.update -= CheckForConfigChanges;
            EditorApplication.update += CheckForConfigChanges;
        }
    }

    /// <summary>
    /// Asset processor for detecting ConfigSoundGroup changes immediately upon save.
    /// </summary>
    public class SoundRefsAssetProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool soundGroupChanged = false;

            // Check imported assets
            foreach (var asset in importedAssets)
            {
                if (asset.EndsWith(".asset"))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<ConfigSoundGroup>(asset);
                    if (obj != null)
                    {
                        soundGroupChanged = true;
                        break;
                    }
                }
            }

            // Check deleted assets
            if (!soundGroupChanged)
            {
                foreach (var asset in deletedAssets)
                {
                    if (asset.Contains("ConfigSoundGroup") || asset.EndsWith(".asset"))
                    {
                        soundGroupChanged = true;
                        break;
                    }
                }
            }

            // Check moved assets
            if (!soundGroupChanged)
            {
                foreach (var asset in movedAssets)
                {
                    if (asset.EndsWith(".asset"))
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<ConfigSoundGroup>(asset);
                        if (obj != null)
                        {
                            soundGroupChanged = true;
                            break;
                        }
                    }
                }
            }

            // Clear cache if ConfigSoundGroup was changed
            if (soundGroupChanged)
            {
                SoundRefsEditor.ClearCache();
            }
        }
    }
}

#endif

