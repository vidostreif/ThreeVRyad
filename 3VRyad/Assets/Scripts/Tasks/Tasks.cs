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
    public Transform thisTransform;
    public GameObject prefabcollectedElements;
    public float distanceBetweenTargets = 3;
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
    }

    void Start()
    {
        CreateCollectedElements();
    }

    //создание коллекции целей
    private void CreateCollectedElements() {
        //смещение по y
        float startingYPoint = thisTransform.position.y - ((Grid.Instance.blockSize + distanceBetweenTargets) * (targets.Length-1)) * 0.5f;

        //Text MyScore = GetComponentInChildren<Text>();

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

    //public void OnGUI()
    //{

    //    // считаем позицию
    //    Rect rect = new Rect(thisTransform.position.x, thisTransform.position.y, 100f, 20f);

    //    // создаем стиль с выравниванием по центру
    //    GUIStyle label = new GUIStyle(GUI.skin.label);
    //        label.alignment = TextAnchor.MiddleCenter;

    //        // выводим имя объекта с созданным стилем, чтобы имя было выведено по центру
    //        GUI.Label(rect, "12313", label);
    //}

    public bool Collect(Element element) {
        //ищем данный вид элемента в массиве и если нашли, то возвращаем истина
        AllShapeEnum allShape = (AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), element.Shape.ToString());
        foreach (Target target in targets)
        {
            if (target.Collect(allShape))
            {
                //перемещаем элемент к нашему объекту
                MainAnimator.Instance.AddElementForSmoothMove(element.transform, target.Image.transform.position, 10);
                //проверяем, не собрали ли мы коллекцию
                CheckAll();
                return true;
            }
        }
        return false;
    }

    public bool Collect(BlockingElement element)
    {
        //ищем данный вид элемента в массиве и если нашли, то возвращаем истина
        AllShapeEnum allShape = (AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), element.Shape.ToString());
        foreach (Target target in targets)
        {
            if (target.Collect(allShape))
            {
                //перемещаем элемент к нашему объекту
                MainAnimator.Instance.AddElementForSmoothMove(element.transform, target.Image.transform.position, 10);
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
            //заканчиваем игру!!!!
        }
    }
}


//#if UNITY_EDITOR

//[CustomEditor(typeof(Tasks), true)]
//public class TargetEditor : Editor
//{
//    Tasks tasks;

//    void OnEnable()
//    {
//        tasks = (Tasks)target;
//    }

//    public override void OnInspectorGUI()
//    {
//        //base.OnInspectorGUI();
//        int LastSizeOfArray = tasks.sizeOfArray; //сохраняем последний размер массива
//        EditorGUILayout.LabelField("Массив параметров для изменения:", EditorStyles.boldLabel);
//        tasks.sizeOfArray = EditorGUILayout.IntField("Размер массива", tasks.sizeOfArray);

//        if (LastSizeOfArray != tasks.sizeOfArray)//если изменили размер массива, то пересоздаем массив объектов
//        {
//            tasks.targets = new Target[tasks.sizeOfArray];
//        }

//        if (tasks.targets != null)//если инициализирован массив
//        {
//            foreach (Target thisTarget in tasks.targets)
//            {
//                EditorGUILayout.Separator();
//                //если пересоздали массив
//                if (LastSizeOfArray != tasks.sizeOfArray)
//                {
//                    thisTarget.CollectionTypes = CollectionTypesEnum.Element;
//                }

//                CollectionTypesEnum collectionTypes = thisTarget.CollectionTypes;
//                thisTarget.CollectionTypes = (CollectionTypesEnum)EditorGUILayout.EnumPopup("Тип собираемого элемента", thisTarget.CollectionTypes);

//                //если изменили тип собираемого элемента или пересоздали массив
//                if (collectionTypes != thisTarget.CollectionTypes || LastSizeOfArray != tasks.sizeOfArray)
//                {
//                    if (thisTarget.CollectionTypes == CollectionTypesEnum.Element)
//                    {
//                        thisTarget.shape = Enum.ToObject(typeof(ElementsShapeEnum), 0);
//                    }
//                    else if (thisTarget.CollectionTypes == CollectionTypesEnum.BlockingElement)
//                    {
//                        thisTarget.shape = Enum.ToObject(typeof(BlockingElementsShapeEnum), 0);
//                    }
//                }
//                if (thisTarget.CollectionTypes == CollectionTypesEnum.Element)
//                {
//                    thisTarget.shape = (ElementsShapeEnum)EditorGUILayout.EnumPopup("Вид элемента", (ElementsShapeEnum)thisTarget.shape);
//                }
//                else if (thisTarget.CollectionTypes == CollectionTypesEnum.BlockingElement)
//                {
//                    thisTarget.shape = (BlockingElementsShapeEnum)EditorGUILayout.EnumPopup("Вид элемента", (BlockingElementsShapeEnum)thisTarget.shape);
//                }
                
//                thisTarget.goal = EditorGUILayout.IntField("Требуется собрать", thisTarget.goal);

//            }
//        }

//        EditorUtility.SetDirty(tasks);
//    }

//}

//#endif
