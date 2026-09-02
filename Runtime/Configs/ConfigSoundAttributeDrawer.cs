#if UNITY_EDITOR && ODIN_INSPECTOR

using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace SiPVLib.Sound.Configs.Editor
{
    /// <summary>
    /// Odin Inspector attribute drawer for ConfigSoundAttribute.
    /// Provides a dropdown UI for selecting sound IDs with optional SoundType filtering.
    /// </summary>
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public class ConfigSoundAttributeDrawer : OdinAttributeDrawer<ConfigSoundAttribute, string>
    {
        // DrawPropertyLayout runs on every repaint, so the filtered+sorted id list and its display
        // array are built once per cache generation rather than per frame.
        private string[] _displayOptions;
        private System.Collections.Generic.List<string> _availableSounds;
        private int _cachedVersion = -1;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var soundId = ValueEntry.SmartValue;
            var attribute = Attribute;

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

            // Draw as dropdown or text field
            GUILayout.BeginHorizontal();

            if (label != null)
            {
                GUILayout.Label(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            }

            var selectedIndex = availableSounds.IndexOf(soundId);

            var newIndex = EditorGUILayout.Popup(selectedIndex >= 0 ? selectedIndex : 0, _displayOptions);

            if (newIndex >= 0 && newIndex < availableSounds.Count)
            {
                ValueEntry.SmartValue = availableSounds[newIndex];
            }

            GUILayout.EndHorizontal();
        }
    }
}

#endif


