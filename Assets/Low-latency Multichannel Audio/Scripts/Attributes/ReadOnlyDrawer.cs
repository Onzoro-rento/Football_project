using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace LowLatencyMultichannelAudio
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        private ReadOnlyAttribute ReadOnlyAttribute { get { return attribute as ReadOnlyAttribute; } }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            switch (ReadOnlyAttribute.Type)
            {
                case ReadOnlyType.NeverShow:
                    return 0;

                case ReadOnlyType.ShowOnlyPlaying:
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                        return 0;
                    break;
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            switch (ReadOnlyAttribute.Type)
            {
                case ReadOnlyType.NeverShow:
                    return;

                case ReadOnlyType.ShowOnlyPlaying:
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                        return;
                    break;
            }

            label.text += " (Readonly)";
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
#endif