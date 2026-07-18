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
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var soundId = ValueEntry.SmartValue;
            var attribute = Attribute;

            // Optional SoundType filtering
            SoundType? filterType = attribute?.SoundTypeFilter;

            // Get available sound IDs from SoundRefsEditor
            SoundRefsEditor.CacheSoundGroups();
            var availableSounds = SoundRefsEditor.GetAvailableSoundIds(filterType);

            // Draw as dropdown or text field
            GUILayout.BeginHorizontal();

            if (label != null)
            {
                GUILayout.Label(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            }

            var selectedIndex = availableSounds.IndexOf(soundId);
            var displayOptions = availableSounds.ToArray();

            var newIndex = EditorGUILayout.Popup(selectedIndex >= 0 ? selectedIndex : 0, displayOptions);

            if (newIndex >= 0 && newIndex < availableSounds.Count)
            {
                ValueEntry.SmartValue = availableSounds[newIndex];
            }

            GUILayout.EndHorizontal();
        }
    }
}

#endif


