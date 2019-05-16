﻿using System;
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
    private bool open = false;//открыт
    private bool passed = false;//пройден
    private int stars = 0;//количество звезд
    private int score = 0;//количество очков

    private Image image;
    private Text text;
    private Button button;

    public Level()
    {
        if (LevelMenu.Instance != null)
        {
            LevelMenu.Instance.GetXmlDocument(this);
        }        
    }

    public Level(UnityEngine.Object xmlDocument)
    {
        this.xmlDocument = xmlDocument;
    }

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
    }

    public int Stars
    {
        get
        {
            return stars;
        }
    }

    public int Score
    {
        get
        {
            return score;
        }
    }

    public bool Passed
    {
        get
        {
            return passed;
        }
    }

    public bool Open
    {
        get
        {
            return open;
        }
    }

    public void LoadSave(bool open, bool passed, int stars, int score) {
        this.open = open;
        this.passed = passed;
        this.stars = stars;
        this.score = score;
    }

    public void SetLevelOpend()
    {
        open = true;
    }

    public void SetLevelPassed(int stars, int score)
    {
        open = true;
        passed = true;

            if (this.stars < stars)
            {
                if (stars > 3)
                {
                stars = 3;
                }
            Shop.Instance.ExchangeStarsForCoins(this, stars);
                this.stars = stars;
            }

        if (this.score < score)
        {
            this.score = score;
        }                   
    }

    public void GetButtonFrom(GameObject elementGameObject)
    {
        //добавляем действие к кнопке
        button = elementGameObject.GetComponent(typeof(Button)) as Button;
        if (open)
        {
            Button.onClick.AddListener(delegate { Action(); });
        }
        else
        {
            button.interactable = false;
        }
        
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

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Level))]
public class LevelDrawer : PropertyDrawer
{
    LevelMenu levelMenu = null;
    Level level = null;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        GetValues(property);

        if (levelMenu.ItLastLevelOnRegion(level))
        {
            return 36;
        }
        return 17;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        GetValues(property);
        bool lastLevelOnRegion = levelMenu.ItLastLevelOnRegion(level);
        float height;
        if (lastLevelOnRegion)
            height = position.height / 2 - 2;
        else
            height = position.height;

        Rect xmlDocumentRect = new Rect(position.x, position.y, 110, height);
        Rect buttonRect = new Rect(position.x + 120, position.y, 100, height);

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(xmlDocumentRect, property.FindPropertyRelative("xmlDocument"), GUIContent.none);
        //if (EditorGUI.EndChangeCheck())
        //{
        //    RenderSettings.skybox.SetColor("_highlight", property.FindPropertyRelative("id").colorValue);
        //}

        var xmlDocument = property.FindPropertyRelative("xmlDocument").objectReferenceValue;

        UnityEngine.Object LastLoadxmlDocument = null;
        if (levelMenu.LastLoadLevel != null)
        {
            LastLoadxmlDocument = levelMenu.LastLoadLevel.xmlDocument;
        }

        if (xmlDocument != null)
        {
            if (LastLoadxmlDocument != xmlDocument)
            {
                if (GUI.Button(buttonRect, "Загрузить"))
                {                    
                    if (Application.isPlaying)
                    {
                        levelMenu.LoadLevel(level);
                    }
                    else
                    {
                        levelMenu.LoadXml(level);
                    }
                }
            }
            else
            {
                if (GUI.Button(buttonRect, "Сохранить"))
                {
                    levelMenu.SaveXml(level);
                }
            }
        }
        else
        {
            if (level != null)
            {
                levelMenu.GetXmlDocument(level);
                if (level.xmlDocument == null)
                {
                    if (GUI.Button(buttonRect, "Создать"))
                    {
                        levelMenu.CreateXml(level);
                    }
                }
            }
        }

        if (lastLevelOnRegion)
        {
            Rect butDel = new Rect(position.x, position.y + height + 2, 110, height);
            Rect butAdd = new Rect(position.x + 120, position.y + height + 2, 100, height);

            if (GUI.Button(butDel, "Удалить"))
            {
                levelMenu.DellLevelOnRegion(level);
            }

            if (GUI.Button(butAdd, "Добавить"))
            {
                levelMenu.AddLevelOnRegion(level);
            }
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    private void GetValues(SerializedProperty property) {
        //if (levelMenu == null || myDataClass == null)
        //{
            levelMenu = property.serializedObject.targetObject as LevelMenu;
            level = PropertyDrawerUtility.GetActualObjectForSerializedProperty<Level>(fieldInfo, property);
        //}        
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

            int index;//234
            int index2;//234

            if (int.TryParse(@int, out index) && int.TryParse(@int2, out index2))
            {
                if (obj.regionsList.Count > index && obj.regionsList[index].levelList.Count > index2)
                {
                    actualObject = obj.regionsList[index].levelList[index2];
                }
            }
        }
        return actualObject;
    }
}
#endif
