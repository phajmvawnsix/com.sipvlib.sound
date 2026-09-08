#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SiPVLib.Sound.Configs.Editor
{
    /// <summary>
    /// Attribute drawer for ConfigSoundAttribute. Provides a dropdown UI for selecting sound IDs
    /// with optional SoundType filtering.
    /// </summary>
    [CustomPropertyDrawer(typeof(ConfigSoundAttribute))]
    public class ConfigSoundAttributeDrawer : PropertyDrawer
    {
        // OnGUI runs on every repaint, so the filtered+sorted id list and its display array are
        // built once per cache generation rather than per frame.
        private string[] _displayOptions;
        private List<string> _availableSounds;
        private int _cachedVersion = -1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var soundId = property.stringValue;
            var attribute = (ConfigSoundAttribute) base.attribute;

            // Optional SoundType filtering
            SoundType? filterType = attribute?.SoundTypeFilter;

            // Get available sound IDs from SoundRefsEditor
            SoundRefsEditor.CacheSoundGroups();
            if (_cachedVersion != SoundRefsEditor.CacheVersion || _availableSounds == null)
            {
                _availableSounds = SoundRefsEditor.GetAvailableSoundIds(filterType);
                _displayOptions = _availableSounds.ToArray();
                _cachedVersion = SoundRefsEditor.CacheVersion;
            }

            var availableSounds = _availableSounds;

            EditorGUI.BeginProperty(position, label, property);

            var fieldRect = EditorGUI.PrefixLabel(position, label);

            var selectedIndex = availableSounds.IndexOf(soundId);
            var newIndex = EditorGUI.Popup(fieldRect, selectedIndex >= 0 ? selectedIndex : 0, _displayOptions);

            if (newIndex >= 0 && newIndex < availableSounds.Count)
            {
                property.stringValue = availableSounds[newIndex];
            }

            EditorGUI.EndProperty();
        }
    }
}

#endif
