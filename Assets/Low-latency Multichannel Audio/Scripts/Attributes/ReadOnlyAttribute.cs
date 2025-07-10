using UnityEngine;

namespace LowLatencyMultichannelAudio
{
    public enum ReadOnlyType
    {
        None,
        ShowOnlyPlaying,
        NeverShow
    }

    public class ReadOnlyAttribute : PropertyAttribute
    {
        public ReadOnlyType Type = ReadOnlyType.None;

        public ReadOnlyAttribute() : this(ReadOnlyType.None) { }

        public ReadOnlyAttribute(ReadOnlyType type)
        {
            Type = type;
        }
    }
}