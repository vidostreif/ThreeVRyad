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
    public int regionNumber;//номер региона
    public int levelNumber;//номер уровня
    private bool open = false;//открыт
    private bool passed = false;//пройден
    //public bool haveGift = false;//имеет подарок
    private bool giftIssued = false;//подарок выдан
    private int stars = 0;//количество звезд
    private int score = 0;//количество очков

    private Image image;
    private Text text;
    private Button button;

    public Level(int regionNumber, int levelNumber)
    {
        this.regionNumber = regionNumber;
        this.levelNumber = levelNumber;
        if (LevelMenu.Instance != null)
        {
            LevelMenu.Instance.GetXmlDocument(this);
        }        
    }

    public Level(UnityEngine.Object xmlDocument, int regionNumber, int levelNumber)
    {
        this.xmlDocument = xmlDocument;
        this.regionNumber = regionNumber;
        this.levelNumber = levelNumber;
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

    public bool GiftIssued
    {
        get
        {
            return giftIssued;
        }
    }

    public void LoadSave(bool open, bool passed, bool giftIssued, int stars, int score) {
        this.open = open;
        this.passed = passed;
        this.giftIssued = giftIssued;
        this.stars = stars;
        this.score = score;
    }

    public void SetLevelOpend()
    {
        open = true;
    }

    public LevelPassedResult SetLevelPassed(int stars, int score)
    {
        open = true;
        passed = true;

        int plusStars = 0;
        int plusCoins = 0;
        if (this.stars < stars)
        {
            if (stars > 3)
            {
                stars = 3;
            }
            plusStars = stars - this.stars;
            plusCoins = Shop.Instance.ExchangeStarsForCoins(this, stars);
            this.stars = stars;
        }

        //есть получили три звезды, есть подарок и он не выдан
        //Gift gift = new Gift();
        bool displayBox = false;
        bool boxOpen = false;
        if (!giftIssued && this.stars == 3)
        {
            //выдаем подарок
            bool result = true;
            ////подарок существует
            //bool giftExists = false;

            //если есть бандл инструментов
            if (LevelSettings.Instance.Gift.Bundel.Length > 0)
            {
                result = ThingsManager.Instance.addinstruments(LevelSettings.Instance.Gift.Bundel);
                displayBox = true;
            }

            //если небыло никаких ошибок и есть монеты - добавляем монеты
            if (result && LevelSettings.Instance.Gift.Coins > 0)
            {
                result = Shop.Instance.AddGiftCoins(this, LevelSettings.Instance.Gift.Coins);
                displayBox = true;
            }

            //если небыло никаких ошибок и есть жизни - добавляем время бессмертия
            if (result && LevelSettings.Instance.Gift.TimeImmortalLives > 0)
            {
                result = LifeManager.Instance.addTimeImmortal(LevelSettings.Instance.Gift.TimeImmortalLives);
                displayBox = true;
            }

            //если небыло никаких ошибок и подарок существует помечаем подарок как выданный
            if (result && displayBox)
            {
                giftIssued = true;
                //displayBox = true;
                boxOpen = true;
            }
        }
        else if (!giftIssued)
        {
            //если подарок не выдан, но есть что выдать
            if (LevelSettings.Instance.Gift.Bundel.Length > 0 || LevelSettings.Instance.Gift.Coins > 0 || LevelSettings.Instance.Gift.TimeImmortalLives > 0)
            {
                displayBox = true;
            }
        }        

        int plusScore = 0;
        if (this.score < score)
        {
            plusScore = score - this.score;
            this.score = score;
        }

        return new LevelPassedResult(score, plusScore, stars, plusStars, plusCoins, new GiftOptions(LevelSettings.Instance.Gift, displayBox, boxOpen));
    }

    //есть невыданный подарок
    public bool HasNotIssuedGift() {

        if (!giftIssued)
        {
            //если есть бандл инструментов
            if (LevelSettings.Instance.Gift.Bundel.Length > 0)
            {
                return true;
            }
            else if (LevelSettings.Instance.Gift.Coins > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public void GetButtonFrom(GameObject elementGameObject)
    {
        //добавляем действие к кнопке
        button = elementGameObject.GetComponent(typeof(Button)) as Button;
        if (open)
        {
            Button.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
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
                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.normal.textColor = Color.red;
                style.fontStyle = FontStyle.Bold;

                if (GUI.Button(buttonRect, "Сохранить", style))
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
                    GUIStyle style = new GUIStyle(GUI.skin.button);
                    style.fontStyle = FontStyle.Bold;

                    if (GUI.Button(buttonRect, "Создать", style))
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
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fontStyle = FontStyle.Bold;

            if (GUI.Button(butDel, "Удалить", style))
            {
                levelMenu.DellLevelOnRegion(level);
            }

            if (GUI.Button(butAdd, "Добавить", style))
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

public class LevelPassedResult
{
    public int score;
    public int plusScore;
    public int stars;
    public int plusStars;
    public int plusCoins;
    public GiftOptions giftOptions;

    public LevelPassedResult()
    {
        score = 0;
        plusScore = 0;
        stars = 0;
        plusStars = 0;
        plusCoins = 0;
        giftOptions = new GiftOptions(new Gift(), false, false);
    }

    public LevelPassedResult(int score, int plusScore, int stars, int plusStars, int plusCoins, GiftOptions giftOptions)
    {
        this.score = score;
        this.plusScore = plusScore;
        this.stars = stars;
        this.plusStars = plusStars;
        this.plusCoins = plusCoins;
        this.giftOptions = giftOptions;
    }
}

//параметры выдачи бокса
public class GiftOptions
{
    public Gift gift;
    public bool displayBox;
    public bool BoxOpen;

    public GiftOptions(Gift gift, bool displayBox, bool boxOpen)
    {
        this.gift = gift;
        this.displayBox = displayBox;
        BoxOpen = boxOpen;
    }
}

