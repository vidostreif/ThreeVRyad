using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//класс кнопки для вещей
public class ThingsButton
{
    private GameObject gameObject;//объект инструмента
    private Image image;
    private Text text;
    private Button button;

    public ThingsButton(GameObject go, InstrumentsEnum type)
    {
        this.GameObject = go;
        button = GameObject.GetComponent(typeof(Button)) as Button;
        image = GameObject.GetComponent(typeof(Image)) as Image;
        image.sprite = SpriteBank.SetShape(type);
        text = image.GetComponentInChildren<Text>();
    }

    public GameObject GameObject
    {
        get
        {
            return gameObject;
        }

        set
        {
            gameObject = value;
        }
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
            //UpdateText();
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

    //обнолвление текста
    public void UpdateText(string str)
    {
        if (text != null)
        {
            text.text = str;
        }
    }

    //добавление действия кнопке
    public void AddAction(Action action)
    {
        //добавляем действие к кнопке
        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(delegate { action(); });
    }
}
