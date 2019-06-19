using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

//подарок на уровне
public class LevelSettings : MonoBehaviour, IESaveAndLoad { 

    public static LevelSettings Instance; // Синглтон
    [SerializeField] private Gift gift;
    [SerializeField] private bool optional;//пометка, что уровень не обязательный


    public Gift Gift { get => gift; }
    public bool Optional { get => optional; }

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
        //gift = new Gift();
    }

    //public void Prepare()
    //{

    //}

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
        foreach (BundleShopV bundleShopV in Gift.Bundel)
        {
            XAttribute type = new XAttribute("type", bundleShopV.type);
            XAttribute count = new XAttribute("count", bundleShopV.count);
            XElement bundleShopVXElement = new XElement("bundleShopV", type, count);
            bundlesXElement.Add(bundleShopVXElement);
        }
        XElement.Add(bundlesXElement);

        //добавляем размер массива, что бы глубоко не искать
        XElement.Add(new XElement("bundelCount", Gift.Bundel.Length));
        //добавляем количество монет
        XElement.Add(new XElement("coins", Gift.Coins));
        XElement.Add(new XElement("timeImmortalLives", Gift.TimeImmortalLives));
        XElement.Add(new XElement("optional", optional));//пометка, что уровень не обязательный

        return XElement;
    }

    public void RecoverFromXElement(XElement XElement)
    {
        int coins = int.Parse(XElement.Element("coins").Value);
        int timeImmortalLives = 0;
        try { timeImmortalLives = int.Parse(XElement.Element("timeImmortalLives").Value); } catch (Exception) { }
        try {optional = bool.Parse(XElement.Element("optional").Value);} catch (Exception){}

        //временны массив
        List<BundleShopV> bundleShopV = new List<BundleShopV>();

        foreach (XElement bundleShopVXElement in XElement.Element("bundles").Elements("bundleShopV"))
        {
            InstrumentsEnum type = (InstrumentsEnum)Enum.Parse(typeof(InstrumentsEnum), bundleShopVXElement.Attribute("type").Value);
            int count = int.Parse(bundleShopVXElement.Attribute("count").Value);

            bundleShopV.Add(new BundleShopV(type, count));
        }

        //восстанавливаем значения
        gift = new Gift(bundleShopV.ToArray(), coins, timeImmortalLives);        
    }
}

[Serializable]
public class Gift {
    [SerializeField] private BundleShopV[] bundel;
    [SerializeField] private int coins;
    [SerializeField] private int timeImmortalLives;//сколько получаем времени бессмертия (минут)

    public BundleShopV[] Bundel { get => bundel; }
    public int Coins { get => coins; set => coins = value; }
    public int TimeImmortalLives { get => timeImmortalLives; }

    public Gift(BundleShopV[] bundel, int coins, int timeImmortalLives)
    {
        this.bundel = bundel;
        this.coins = coins;
        this.timeImmortalLives = timeImmortalLives;
    }

    public Gift()
    {
        this.bundel = new BundleShopV[0];
        this.coins = 0;
        this.timeImmortalLives = 0;
    }
}


