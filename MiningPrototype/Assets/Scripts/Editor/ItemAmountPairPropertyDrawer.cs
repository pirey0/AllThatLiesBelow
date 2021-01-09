using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(ItemAmountPair))]
public class ItemAmountPairPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var amountRect = new Rect(position.x, position.y, 150, position.height);
        var unitRect = new Rect(position.x + 155, position.y, 50, position.height);
        var buttonRect = new Rect(position.x + 210, position.y, 60, position.height);
        var infoRect = new Rect(position.x + 275, position.y, 100, position.height);

        EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("type"), GUIContent.none);
        EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("amount"), GUIContent.none);

        ItemType type = (ItemType)property.FindPropertyRelative("type").enumValueIndex;
        if (type == ItemType.LetterNote)
        {
            EditorGUI.LabelField(unitRect, new GUIContent("", LettersHolder.LETTERS_ID_DESC));
            int id = property.FindPropertyRelative("amount").intValue;
            var letter = LettersHolder.Instance.GetLetterWithID(id);

            if (letter != null)
            {
                if (GUI.Button(buttonRect, "Select"))
                {
                    EditorGUIUtility.PingObject(letter);
                }
                EditorGUI.LabelField(infoRect, letter.name);
            }
            else
            {
                if (GUI.Button(buttonRect, "Create"))
                {
                   var newLetter = ScriptableObject.CreateInstance<Letter>();
                    newLetter.ID = id;
                    newLetter.name = "Note_" +id + "_";
                    UnityEditor.AssetDatabase.CreateAsset(newLetter, "Assets/Resources/Letters/" + newLetter.name + ".asset");
                    AssetDatabase.Refresh();
                    LettersHolder.Instance.Refresh();
                    EditorGUIUtility.PingObject(newLetter);
                }
                EditorGUI.LabelField(infoRect, "not defined");
            }
        }

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
