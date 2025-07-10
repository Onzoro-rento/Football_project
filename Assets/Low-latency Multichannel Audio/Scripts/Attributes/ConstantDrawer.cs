using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace LowLatencyMultichannelAudio
{
    [CustomPropertyDrawer(typeof(ConstantAttribute))]
    public class ConstantDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                label.text += " (Constant)";

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();            
        }
    }
}
#endif