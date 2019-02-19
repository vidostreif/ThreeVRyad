using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.UI;

[System.Serializable]
public class Target
{
    private Image image;
    public AllShapeEnum elementsShape;//какой вид элементов собераем
    public int goal;//необходимо собрать
    public int alreadyCollected = 0;//собрано
    private Text text;
    private bool collected = false; //признак, что коллекция собрана
       
    public bool Collected
    {
        get
        {
            return collected;
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
            text.text = "" + goal;
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

    //если элемент подошел то возвращаем истину
    public bool Collect(AllShapeEnum Shape) {
        if (Shape == elementsShape && alreadyCollected < goal)
        {
            alreadyCollected++;
            Check();
            Text.text = ""+(goal - alreadyCollected);
            return true;
        }
        else
        {
            return false;
        }        
    }

    //проверяем собрали ли коллекцию
    private void Check() {
        if (alreadyCollected >= goal)
        {
            collected = true;
        }
    }
}

