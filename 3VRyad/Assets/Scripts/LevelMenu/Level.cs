using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Level
{
    [SerializeField] public UnityEngine.Object xmlDocument = null;
    private Image image;
    private Text text;
    private Button button;

    public Text Text
    {
        get
        {
            return text;
        }

        set
        {
            text = value;
            UpdateText();
        }
    }
    public Image Image
    {
        get
        {
            return image;
        }

        set
        {
            image = value;
        }
    }
    public Button Button
    {
        get
        {
            return button;
        }

        set
        {
            button = value;
        }
    }

    public void AddAction(GameObject elementGameObject)
    {
        //добавляем действие к кнопке
        Button = elementGameObject.GetComponent(typeof(Button)) as Button;
        Button.onClick.AddListener(delegate { Action(); });
    }

    //действие при нажатии
    public void Action()
    {
            Debug.Log(text);
            LevelMenu.Instance.LoadLevel(this);
    }

    //обнолвление текста
    private void UpdateText()
    {
        text.text = "" + text;
    }
}


[CustomPropertyDrawer(typeof(Level))]
public class LevelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        Rect xmlDocumentRect = new Rect(position.x, position.y, 110, position.height);
        Rect buttonRect = new Rect(position.x + 120, position.y, 100, position.height);

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(xmlDocumentRect, property.FindPropertyRelative("xmlDocument"), GUIContent.none);
        //if (EditorGUI.EndChangeCheck())
        //{
        //    RenderSettings.skybox.SetColor("_highlight", property.FindPropertyRelative("id").colorValue);
        //}

        var xmlDocument = property.FindPropertyRelative("xmlDocument").objectReferenceValue;

        if (xmlDocument != null)
        {
            if (SaveAndLoadScene.Instance.xmlDocument != xmlDocument)
            {
                if (GUI.Button(buttonRect, "Загрузить"))
                {
                    LevelMenu levelMenu = property.serializedObject.targetObject as LevelMenu;
                    Level myDataClass = PropertyDrawerUtility.GetActualObjectForSerializedProperty<Level>(fieldInfo, property);

                    levelMenu.LoadXml(myDataClass);
                }
            }
            else
            {
                if (GUI.Button(buttonRect, "Сохранить"))
                {
                    LevelMenu levelMenu = property.serializedObject.targetObject as LevelMenu;
                    Level myDataClass = PropertyDrawerUtility.GetActualObjectForSerializedProperty<Level>(fieldInfo, property);

                    levelMenu.SaveXml(myDataClass);
                }
            }
        }
        else
        {
            LevelMenu levelMenu = property.serializedObject.targetObject as LevelMenu;
            Level myDataClass = PropertyDrawerUtility.GetActualObjectForSerializedProperty<Level>(fieldInfo, property);

            if (myDataClass != null)
            {
                levelMenu.GetXmlDocument(myDataClass);
                if (myDataClass.xmlDocument == null)
                {
                    if (GUI.Button(buttonRect, "Создать"))
                    {
                        levelMenu.CreateXml(myDataClass);
                    }
                }
            }
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}

public class PropertyDrawerUtility
{
    public static Level GetActualObjectForSerializedProperty<T>(FieldInfo fieldInfo, SerializedProperty property) where T : class
    {
        LevelMenu obj = property.serializedObject.targetObject as LevelMenu;
        if (obj == null) { return null; }
        Level actualObject = null;

        if (obj.regionsList.Count > 0)
        {
            string @int = string.Empty;
            string @int2 = string.Empty;
            bool entry = false;
            bool entry2 = false;
            //первое вхождение
            foreach (char ch in property.propertyPath.ToCharArray())
                if (char.IsDigit(ch) && !entry2)
                {
                    @int += ch.ToString();
                    entry = true;
                }
                else if (entry)
                {
                    entry2 = true;
                    if (char.IsDigit(ch))
                    {
                        @int2 += ch.ToString();
                    }
                }

            int index = int.Parse(@int);//234
            int index2 = int.Parse(@int2);//234

            //Debug.Log(index);
            //Debug.Log(index2);

            if (obj.regionsList.Count > index && obj.regionsList[index].levelList.Count > index2)
            {
                actualObject = obj.regionsList[index].levelList[index2];
            }
            
        }
        return actualObject;
    }
}
