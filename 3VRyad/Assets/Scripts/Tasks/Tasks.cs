﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;
using System.Xml.Linq;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
//задания
public class Tasks : MonoBehaviour, IESaveAndLoad
{
        
    public static Tasks Instance; // Синглтон
    [HideInInspector] public Transform thisTransform;
    public GameObject prefabcollectedElements;
    public float distanceBetweenTargets;
    public bool collectedAll { get; protected set; }
    public Target[] targets;// список целей

    private Text movesText;
    [SerializeField] private int moves; //количество ходов
    public bool endGame { get; protected set; } //признак что можно выполнить ход    

    public int Moves
    {
        get
        {
            return moves;
        }
    }

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров Tasks!");
        }

        Instance = this;
        thisTransform = transform;
        movesText = GetComponentInChildren<Text>();
        endGame = false;
        collectedAll = false;
    }

    void Start()
    {        
        //UpdateMovesText();
        //CreateCollectedElements();
    }

    //создание коллекции целей
    public void CreateCollectedElements() {
        //смещение по y
        float startingYPoint = transform.position.y - ((1 + distanceBetweenTargets) * (targets.Length-1)) * 0.5f;
        //удаляем все задания
        string targetsName = "Targets";
        Transform targetsTransform = transform.Find(targetsName);
        if (targetsTransform != null)
        {
            DestroyImmediate(targetsTransform.gameObject);
        }
        GameObject targetsParent;
        targetsParent = new GameObject();
        targetsParent.name = targetsName;
        targetsParent.transform.SetParent(transform, false);

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].GameObject = Instantiate(prefabcollectedElements, new Vector3(transform.position.x, startingYPoint + (i * (1 + distanceBetweenTargets)), transform.position.z), Quaternion.identity, targetsParent.transform);
            Image image = targets[i].GameObject.GetComponent(typeof(Image)) as Image;
            image.sprite = SpriteBank.SetShape(targets[i].elementsShape);
            targets[i].Image = image;
            targets[i].Text = image.GetComponentInChildren<Text>();
        }
    }

    public bool Collect(Element element) {
        //ищем данный вид элемента в массиве и если нашли, то возвращаем истина
        return Collect((AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), element.Shape.ToString()), element.transform);
    }

    public bool Collect(BlockingElement element)
    {
        //ищем данный вид элемента в массиве и если нашли, то возвращаем истина
        return Collect((AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), element.Shape.ToString()), element.transform);
    }

    public bool Collect(BehindElement element)
    {
        //ищем данный вид элемента в массиве и если нашли, то возвращаем истина
        return Collect((AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), element.Shape.ToString()), element.transform);
    }

    private bool Collect(AllShapeEnum allShape, Transform transformElement)
    {
        foreach (Target target in targets)
        {
            if (target.Collect(allShape))
            {
                transformElement.parent = this.thisTransform;
                //перемещаем элемент к нашему объекту
                MainAnimator.Instance.AddElementForSmoothMove(transformElement, target.Image.transform.position, 10, SmoothEnum.InArc, 0.05f, true);
                //проверяем, не собрали ли мы коллекцию
                CheckAll();
                return true;
            }
        }
        return false;
    }

    //проверяем, не собрали ли мы коллекции
    private void CheckAll() {
        bool CollectedAll = true;
        foreach (Target item in targets)
        {
            //если собрали коллекцию
            if (item.Collected)
            {
                //анимация подтверждающая, что все собрали?
                //item.objectManagement.animatorElement.PlayIdleAnimation();
            }
            else
            {
                CollectedAll = false;
            }
        }

        //если собрали все коллекции заканчиваем игру
        if (CollectedAll)
        {
            endGame = true;
            collectedAll = true;
        }
    }

    public void UpdateAllGoal() {
        //обновляем данные по коллекциям
        foreach (Target item in targets)
        {
            //если не собрали коллекцию
            if (!item.Collected)
            {
                item.UpdateGoal();
            }
        }
    }
    //обнолвление текста количества ходов
    public void UpdateMovesText() {
        movesText.text = "Ходы:" + Moves;
    }

    //ход
    public void SubMoves()
    {
        if (moves > 0)
        {
            moves--;
            UpdateMovesText();
            if (moves == 0)
            {
                endGame = true;
            }
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
        XElement tasksXElement = new XElement(this.GetType().ToString());

        tasksXElement.Add(new XElement("moves", Moves));//количество ходов
        tasksXElement.Add(new XElement("sizeTargets", targets.GetLength(0)));//записываем количество заданий

        //записываем все внешности и количество
        XElement targetsXElement = new XElement("targets");
        foreach (Target target in targets)
        {
            XAttribute shape = new XAttribute("shape", target.elementsShape);
            XAttribute goal = new XAttribute("goal", target.Goal);
            XAttribute collectEverything = new XAttribute("collectEverything", target.CollectEverything);
            XElement shapeAndGoalXElement = new XElement("shapeAndGoal", shape, goal, collectEverything);
            targetsXElement.Add(shapeAndGoalXElement);
        }
        tasksXElement.Add(targetsXElement);

        Debug.Log(tasksXElement);

        return tasksXElement;
    }

    public void RecoverFromXElement(XElement tasksXElement)
    {
        //Сбрасываем значения
        endGame = false;
        collectedAll = false;
        //удаляем GameObject
        foreach (Target target in targets)
        {
            DestroyImmediate(target.GameObject);
        }
        //восстанавливаем значения
        this.moves = int.Parse(tasksXElement.Element("moves").Value);
        int sizeTargets = int.Parse(tasksXElement.Element("sizeTargets").Value);
        targets = new Target[sizeTargets];

        int iteration = 0;
        foreach (XElement shapeAndGoalXElement in tasksXElement.Element("targets").Elements("shapeAndGoal"))
        {
            AllShapeEnum shape = (AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), shapeAndGoalXElement.Attribute("shape").Value);
            int goal = int.Parse(shapeAndGoalXElement.Attribute("goal").Value);
            bool collectEverything = bool.Parse(shapeAndGoalXElement.Attribute("collectEverything").Value);
            Target target = new Target(shape, goal, collectEverything);
            this.targets[iteration] = target;
            iteration++;
        }
        CreateCollectedElements();
    }
}
