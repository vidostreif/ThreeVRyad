using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Instrument
{
    [SerializeField] private InstrumentsEnum type;//какой вид инструмента
    private int quantity;//доступное количество
    private InstrumentButton shopInstrumentButton; //кнопка инструмента в магазине


    public InstrumentButton ShopInstrumentButton
    {
        get
        {
            return shopInstrumentButton;
        }
    }
    public int Quantity
    {
        get
        {
            return quantity;
        }
    }
    public InstrumentsEnum Type
    {
        get
        {
            return type;
        }
    }

    public void CreateShopInstrumentButton(GameObject go)
    {
        if (shopInstrumentButton != null)
        {
            GameObject.Destroy(shopInstrumentButton.GameObject);
        }
        shopInstrumentButton = new InstrumentButton(go, type);
        UpdateText();

        AddAction(shopInstrumentButton, ActionOnShop);
    }

    private void AddAction(InstrumentButton InstrumentButton, Action action) {
        //добавляем действие к кнопке
        InstrumentButton.Button = InstrumentButton.GameObject.GetComponent(typeof(Button)) as Button;
        InstrumentButton.Button.onClick.RemoveAllListeners();
        InstrumentButton.Button.onClick.AddListener(delegate { action(); });
    }

    //действие при нажатии
    public void ActionOnShop()
    {
        
    }

    //обнолвление текста
    private void UpdateText()
    {
        //для отображения в магазине
        if (shopInstrumentButton != null)
        {
            shopInstrumentButton.UpdateText("" + quantity);
        }

        if (InstrumentPanel.Instance != null)
        {
            InstrumentPanel.Instance.UpdateTextInstrumentOnGame(type);
        }
    }

    public void SubQuantity(int count = 1)
    {
        if (count <= quantity)
        {
            quantity -= count;            
        }
        else
        {
            quantity = 0;
        }
        UpdateText();
    }

    public void AddQuantity(int count = 1)
    {
        quantity += count;
        UpdateText();
    }
}

public class InstrumentButton
{
    private GameObject gameObject;//объект инструмента
    private Image image;
    private Text text;
    private Button button;

    public InstrumentButton(GameObject go, InstrumentsEnum type)
    {
        this.GameObject = go;
        Image image = GameObject.GetComponent(typeof(Image)) as Image;
        image.sprite = SpriteBank.SetShape(type);
        Image = image;
        Text = image.GetComponentInChildren<Text>();
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


}
