using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

//задания
public class Tasks : MonoBehaviour {
        
    //public int sizeOfArray;
    public static Tasks Instance; // Синглтон
    [HideInInspector] public Transform thisTransform;
    public GameObject prefabcollectedElements;
    public float distanceBetweenTargets;
    public bool collected { get; protected set; }

    private Text movesText;
    [SerializeField] private int moves; //остаток ходов
    public bool endGame { get; protected set; } //признак что можно выполнить ход

    public Target[] targets;// список целей

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров Tasks!");
        }

        Instance = this;
        thisTransform = transform;
        endGame = false;
        collected = false;
    }

    void Start()
    {
        movesText = GetComponentInChildren<Text>();
        UpdateMovesText();
        CreateCollectedElements();
    }

    //создание коллекции целей
    private void CreateCollectedElements() {
        //смещение по y
        float startingYPoint = thisTransform.position.y - ((Grid.Instance.blockSize + distanceBetweenTargets) * (targets.Length-1)) * 0.5f;

        

        for (int i = 0; i < targets.Length; i++)
        {
            GameObject elementGameObject = Instantiate(prefabcollectedElements, new Vector3(thisTransform.position.x, startingYPoint + (i * (Grid.Instance.blockSize + distanceBetweenTargets)), thisTransform.position.z), Quaternion.identity, this.thisTransform);
            //SpriteBank objectManagement = elementGameObject.GetComponent<SpriteBank>();
            Image image = elementGameObject.GetComponent(typeof(Image)) as Image;
            image.sprite = SpriteBank.SetShape(targets[i].elementsShape);
            //objectManagement.SetShape(targets[i].elementsShape);
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
            collected = true;
        }
    }
    
    //обнолвление текста количества ходов
    private void UpdateMovesText() {
        movesText.text = "Ходы:" + moves;
    }

    //ход
    public void SubMoves()
    {
        if (moves > 0)
        {
            moves--;
            UpdateMovesText();
        }
        else
        {
            endGame = true;
        }
    }
}
