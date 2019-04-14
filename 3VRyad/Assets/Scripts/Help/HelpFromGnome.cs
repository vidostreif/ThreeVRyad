using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

//подсказка от помошника в начале уровня
public class HelpFromGnome : MonoBehaviour, IESaveAndLoad
{
    public HelpEnum helpEnum;
    public static HelpFromGnome Instance;
    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров HelpFromGnome!");
        }
        Instance = this;
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
        XElement.Add(new XElement("helpEnum", helpEnum));//подсказка
        return XElement;
    }

    public void RecoverFromXElement(XElement XElement)
    {
        //восстанавливаем значения
        this.helpEnum =  (HelpEnum)Enum.Parse(typeof(HelpEnum), XElement.Element("helpEnum").Value);

        if (Application.isPlaying && this.helpEnum != HelpEnum.Empty)
        {
            HelpToPlayer.AddHint(HelpFromGnome.Instance.helpEnum);//подсказка
        }
    }
}
