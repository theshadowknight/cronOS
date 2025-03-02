using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ThreadSafeList<>))]
public class ThreadSafeListPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("items"));
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property.FindPropertyRelative("items"), label, true);
        //  EditorGUI.LabelField(position, "s");
    }
}