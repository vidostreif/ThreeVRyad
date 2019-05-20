using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Thing
{
    [SerializeField] private InstrumentsEnum type;//какой вид инструмента
    private int addQuantity = 0;//количество которое добавляем
    private int quantity = 0;//доступное количество
    private GameObject go;
    private ThingsButton shopThingButton; //кнопка вещи в магазине

    public Thing(InstrumentsEnum type, int quantity)
    {
        this.type = type;
        this.quantity = quantity;
        UpdateText();
    }

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
            return quantity + addQuantity;
        }
    }
    public InstrumentsEnum Type
    {
        get
        {
            return type;
        }
    }

    public GameObject Go { get => go; }

    public void CreateShopThingButton(GameObject go)
    {
        if (shopThingButton != null)
        {
            GameObject.Destroy(shopThingButton.GameObject);
        }
        this.go = go;
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
   
    //добавляем количество и ожидаем прилета вещей
    public void AddQuantity(int count = 1)
    {
        addQuantity += count;        
    }

    //вещ прилетела - добавляем количество и отображаем
    public void ThingFlew(int addCount)
    {
        if (addQuantity < addCount)
        {
            addCount = addQuantity;
        }
        quantity += addCount;
        addQuantity -= addCount;
        UpdateText();
    }

    public void CountNumber() {
        quantity += addQuantity;
        addQuantity = 0;
        UpdateText();
    }
}

