using System;

namespace SiPVLib.Sound.Configs
{
    /// <summary>
    /// Runtime-safe attribute to reference a sound by storing only its Id in a string field.
    /// The accompanying editor drawer (in an Editor folder) provides rich UI, sound selection validation,
    /// and optional filtering by SoundType when the Unity Editor is present (Odin Inspector required).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ConfigSoundAttribute : Attribute
    {
        /// <summary>
        /// Optional sound type filter. When supplied, only sounds of this type are offered in the editor.
        /// </summary>
        public SoundType? SoundTypeFilter { get; }

        /// <summary>
        /// When true, only shows the preview object field, hiding the label and ID text field. Default: false.
        /// </summary>
        public bool PreviewOnly { get; }

        public ConfigSoundAttribute()
        {
        }

        public ConfigSoundAttribute(SoundType soundTypeFilter, bool previewOnly = false)
        {
            SoundTypeFilter = soundTypeFilter;
            PreviewOnly = previewOnly;
        }

        public ConfigSoundAttribute(bool previewOnly = false)
        {
            PreviewOnly = previewOnly;
        }
    }
}

