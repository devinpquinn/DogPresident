using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Response))]
public class ResponseDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Adjust height dynamically based on whether the headline is shown
        float height = EditorGUIUtility.singleLineHeight * 2.5f; // responseText + approvalEffect
        SerializedProperty approvalProp = property.FindPropertyRelative("approvalEffect");

        if ((ApprovalRatingEffect)approvalProp.enumValueIndex != ApprovalRatingEffect.NA)
        {
            height += EditorGUIUtility.singleLineHeight + 6; // Add space for headline
        }

        SerializedProperty responseTextProp = property.FindPropertyRelative("responseText");
        height += EditorGUI.GetPropertyHeight(responseTextProp); // Expand for TextArea

        return height + 10;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(rect, label);

        // Draw responseText
        SerializedProperty responseTextProp = property.FindPropertyRelative("responseText");
        rect.y += EditorGUIUtility.singleLineHeight + 2;
        float responseHeight = EditorGUI.GetPropertyHeight(responseTextProp);
        rect.height = responseHeight;
        EditorGUI.PropertyField(rect, responseTextProp);

        // Draw approvalEffect
        SerializedProperty approvalProp = property.FindPropertyRelative("approvalEffect");
        rect.y += responseHeight + 4;
        rect.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(rect, approvalProp);

        // Conditionally draw headline
        if ((ApprovalRatingEffect)approvalProp.enumValueIndex != ApprovalRatingEffect.NA)
        {
            SerializedProperty headlineProp = property.FindPropertyRelative("headline");
            rect.y += EditorGUIUtility.singleLineHeight + 4;
            EditorGUI.PropertyField(rect, headlineProp);
        }

        EditorGUI.EndProperty();
    }
}
