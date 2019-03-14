using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.UI;
using UnityEditor;

[System.Serializable]
public class Target
{
    private Image image;
    private GameObject gameObject;
    public AllShapeEnum elementsShape;//какой вид элементов собераем
    [SerializeField] private bool collectEverything;//признак, что нужно собрать все элементы на поле
    [SerializeField] private int goal;//необходимо собрать
    private Text text;
    private bool collected = false; //признак, что коллекция собрана

    public Target(AllShapeEnum elementsShape, int goal, bool collectEverything)
    {
        this.elementsShape = elementsShape;
        this.collectEverything = collectEverything;
        this.goal = goal;
    }
    public int Goal
    {
        get
        {
            return goal;
        }
    }
    public bool CollectEverything
    {
        get
        {
            return collectEverything;
        }
    }
    public bool Collected
    {
        get
        {
            return collected;
        }
    }
    public void UpdateGoal() {
        //если нужно собрать все элементы на поле, то проверяем поле и обновляем счетчик
        if (collectEverything)
        {
            goal = ElementsList.GetAmountOfThisShapeElemets(elementsShape);
            UpdateText();
        }
    }

    //обнолвление текста
    private void UpdateText()
    {
        text.text = "" + goal;
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
            UpdateGoal();
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

    //если элемент подошел то возвращаем истину
    public bool Collect(AllShapeEnum Shape) {
        if (Shape == elementsShape && goal > 0)
        {
            //alreadyCollected++;
            goal--;
            Check();
            UpdateText();
            return true;
        }
        else
        {
            return false;
        }        
    }

    //проверяем собрали ли коллекцию
    private void Check() {
        if (goal <= 0)
        {
            collected = true;
        }
    }
}


