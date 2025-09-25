using UnityEditor;
using UnityEngine;

namespace UltimateFramework.Utils
{
    [CustomPropertyDrawer(typeof(ConditionalField))]
    public class ConditionalFieldPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalField condHAtt = (ConditionalField)attribute;
            SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.conditionalSourceField);

            if (sourcePropertyValue != null && sourcePropertyValue.propertyType == SerializedPropertyType.Enum)
            {
                if (sourcePropertyValue.enumNames[sourcePropertyValue.enumValueIndex].Equals(condHAtt.expectedValue.ToString()))
                {
                    return EditorGUI.GetPropertyHeight(property);
                }
            }
            else if (sourcePropertyValue != null && sourcePropertyValue.boolValue.Equals(condHAtt.expectedValue))
            {
                return EditorGUI.GetPropertyHeight(property);
            }
            else if (sourcePropertyValue.propertyType == SerializedPropertyType.String)
            {
                if (!string.IsNullOrEmpty(sourcePropertyValue.stringValue))
                {
                    return EditorGUI.GetPropertyHeight(property);
                }
            }
            else if (sourcePropertyValue.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (sourcePropertyValue.objectReferenceValue != null)
                {
                    return EditorGUI.GetPropertyHeight(property);
                }
            }

            return 0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalField condHAtt = (ConditionalField)attribute;
            SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.conditionalSourceField);

            if (sourcePropertyValue != null && sourcePropertyValue.propertyType == SerializedPropertyType.Enum)
            {
                if (sourcePropertyValue.enumNames[sourcePropertyValue.enumValueIndex].Equals(condHAtt.expectedValue.ToString()))
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
            else if (sourcePropertyValue != null && sourcePropertyValue.boolValue.Equals(condHAtt.expectedValue))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            else if (sourcePropertyValue.propertyType == SerializedPropertyType.String)
            {
                if (!string.IsNullOrEmpty(sourcePropertyValue.stringValue))
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
            else if (sourcePropertyValue.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (sourcePropertyValue.objectReferenceValue != null)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
        }
    }
}
