﻿using System.Collections;
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
    List<Transform> transformsInTransitList = new List<Transform>();
    private int itemsInTransit = 0;//элементы в пути
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
        //UpdateText();
    }

    //обнолвление текста
    private void UpdateText()
    {
        int textGoal = goal + itemsInTransit;
        text.text = "" + textGoal;
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
            //если получаем новый объект, то сбрасываем все параметры связанныес с элементами в пути
            transformsInTransitList = new List<Transform>();
            itemsInTransit = 0;
        }
    }

    //если элемент подошел то возвращаем истину
    public bool Collect(AllShapeEnum Shape, Transform transformElement) {
        if (Shape == elementsShape)
        {            
            if (goal > 0)
            {
                goal--;
                transformsInTransitList.Add(transformElement);
                itemsInTransit++;
                UpdateGoal();
                Check();
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    ////проверяем собрали ли коллекцию
    private void Check()
    {
        if (goal <= 0)
        {
            //Debug.Log("Check goal " + goal);
            //Debug.Log("Check itemsInTransit " + itemsInTransit);
            collected = true;
        }
    }

    public void ItemReached(Transform transformElement) {

        bool found = false;
        foreach (Transform item in transformsInTransitList)
        {
            if (item == transformElement)
            {
                found = true;
            }
        }

        if (found)
        {
            transformsInTransitList.Remove(transformElement);
            //goal--;
            itemsInTransit--;
            if (goal + itemsInTransit <= 0)
            {
                if (gameObject != null)
                {
                    //создаем эффект 
                    SoundManager.Instance.PlaySoundInternal(SoundsEnum.CollectElement);
                    ParticleSystemManager.Instance.CreateCollectAllEffect(gameObject.transform, SpriteBank.SetShape(elementsShape, mini:true));
                }
            }
            else
            {
                if (gameObject != null)
                {
                    //создаем эффект 
                    SoundManager.Instance.PlaySoundInternal(SoundsEnum.DestroyElement_1);
                    ParticleSystemManager.Instance.CreateCollectEffect(gameObject.transform, SpriteBank.SetShape(elementsShape, mini: true));
                }
                
            }
            UpdateText();
        }

        //if (standartElement)
        //{
        //    PoolManager.Instance.ReturnObjectToPool(transformElement.gameObject);
        //}
        //else
        //{
        //    GameObject.Destroy(transformElement.gameObject);
        //}
        
    }
}


