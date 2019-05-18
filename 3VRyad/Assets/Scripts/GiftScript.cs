using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

//подарок на уровне
public class GiftScript : MonoBehaviour, IESaveAndLoad
{
    public static GiftScript Instance; // Синглтон
    public Gift gift;

    void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject); //Delete duplicate
            return;
        }
        else
        {
            Instance = this; //Make this object the only instance            
        }
    }

    //сохранение и заргрузка
    public Type GetClassName()
    {
        return this.GetType();
    }
    //передаем данные о на стройках в xml формате
    public XElement GetXElement()
    {
        XElement XElement = new XElement(this.GetType().ToString());
        //записываем все бандлы
        XElement bundlesXElement = new XElement("bundles");
        foreach (BundleShopV bundleShopV in gift.bundel)
        {
            XAttribute type = new XAttribute("type", bundleShopV.type);
            XAttribute count = new XAttribute("count", bundleShopV.count);
            XElement bundleShopVXElement = new XElement("bundleShopV", type, count);
            bundlesXElement.Add(bundleShopVXElement);
        }
        XElement.Add(bundlesXElement);

        //добавляем размер массива, что бы глубоко не искать
        XElement.Add(new XElement("bundelCount", gift.bundel.Length));
        //добавляем количество монет
        XElement.Add(new XElement("coins", gift.coins));

        return XElement;
    }

    public void RecoverFromXElement(XElement XElement)
    {
        //восстанавливаем значения
        gift = new Gift();

        gift.coins = int.Parse(XElement.Element("coins").Value);

        //временны массив
        List<BundleShopV> bundleShopV = new List<BundleShopV>();

        foreach (XElement bundleShopVXElement in XElement.Element("bundles").Elements("bundleShopV"))
        {
            InstrumentsEnum type = (InstrumentsEnum)Enum.Parse(typeof(InstrumentsEnum), bundleShopVXElement.Attribute("type").Value);
            int count = int.Parse(bundleShopVXElement.Attribute("count").Value);

            bundleShopV.Add(new BundleShopV(type, count));
        }

        //переносим данные
        //bundel = new BundleShopV[bundleShopV.Count];
        gift.bundel = bundleShopV.ToArray();
    }
}

[Serializable]
public class Gift {
    public BundleShopV[] bundel;
    public int coins;

    public Gift()
    {
        bundel = new BundleShopV[0];
        coins = 0;
    }
}


