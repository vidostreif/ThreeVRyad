using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Thing
{
    [SerializeField] private InstrumentsEnum type;//какой вид инструмента
    private int quantity;//доступное количество
    private ThingsButton shopThingButton; //кнопка вещи в магазине
    
    public ThingsButton ShopThingButton
    {
        get
        {
            return shopThingButton;
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

    public void CreateShopThingButton(GameObject go)
    {
        if (shopThingButton != null)
        {
            GameObject.Destroy(shopThingButton.GameObject);
        }
        shopThingButton = new ThingsButton(go, type);
        UpdateText();

        shopThingButton.AddAction(ActionOnShop);
    }
       
    //действие при нажатии
    public void ActionOnShop()
    {
        
    }

    //обнолвление текста
    private void UpdateText()
    {
        //для отображения в магазине
        if (shopThingButton != null)
        {
            shopThingButton.UpdateText("" + quantity);
        }

        if (InstrumentPanel.Instance != null)
        {
            InstrumentPanel.Instance.UpdateTextInstrumentOnGame(type);
        }
    }

    //отнимаем количество
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
    //добавляем количество
    public void AddQuantity(int count = 1)
    {
        quantity += count;
        UpdateText();
    }
}

