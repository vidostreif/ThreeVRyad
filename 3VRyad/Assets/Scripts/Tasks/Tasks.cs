using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;
using System.Xml.Linq;
using GoogleMobileAds.Api;

#if UNITY_EDITOR
//[InitializeOnLoad]
[ExecuteInEditMode]
#endif
//задания
public class Tasks : MonoBehaviour, IESaveAndLoad
{
    public static Tasks Instance; // Синглтон
    [HideInInspector] public Transform thisTransform;
    public GameObject prefabcollectedElements;
    //public float distanceBetweenTargets;
    public bool collectedAll { get; protected set; }
    public Target[] targets;// список целей

    private Text movesText;
    private GameObject targetsParent;
    private GameObject canvasStartGame;
    [SerializeField] private int moves; //количество ходов
    private int addMoves;
    public bool endGame { get; protected set; } //признак что можно выполнить ход   
    public bool addMovesOnEndGAme { get; protected set; } //признак что добавляли ходы

    public int Moves
    {
        get
        {
            return moves + addMoves;
        }
        private set
        {
            moves = value;
        }
    }

    //добавление ходов в конце игры за просмотр рекламы
    public void AddMovesOnEndGAme(Reward args)
    {
        //Debug.Log("AddMovesOnEndGAme start");
        StartCoroutine(CurAddMovesOnEndGAme(args));
    }

    private IEnumerator CurAddMovesOnEndGAme(Reward args)
    {
        //если не собрали коллекцию, конец игры, и нет ходов
        if (!addMovesOnEndGAme && !collectedAll && endGame && Moves == 0)
        {
        //Debug.Log("CurAddMovesOnEndGAme start");
        addMovesOnEndGAme = true;
        //отменяем конец игры
        endGame = false;
        //уничтожаем канвас окончания игры
        
        MainGameSceneScript.Instance.DestroyCanvasMenu();
        addMoves = (int)args.Amount;
            Debug.Log("addMoves: " + addMoves);
            if (movesText == null)
            {
                movesText = GetComponentInChildren<Text>();
            }

            Transform moveImageTransform = movesText.transform.Find("MoveImage");
            Image moveImage = moveImageTransform.GetComponent<Image>();

            while (addMoves > 0)
            {
                //добавляем ходов
                //звук добавления
                SoundManager.Instance.PlaySoundInternal(SoundsEnum.AddMove);
                //эффект                
                ParticleSystemManager.Instance.CreateCollectAllEffect(moveImageTransform, moveImage);
                moves++;
                addMoves--;
                UpdateMovesText();
                yield return new WaitForSeconds(0.3f);
            }
        }
        //Debug.Log("CurAddMovesOnEndGAme finish");
    }

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров Tasks!");
        }
        Instance = this;
        addMoves = 0;
        thisTransform = transform;
        movesText = GetComponentInChildren<Text>();
        endGame = false;
        collectedAll = false;
        addMovesOnEndGAme = false;
    }

    public void ResetParameters()
    {
        //Сбрасываем значения
        endGame = false;
        collectedAll = false;
        addMovesOnEndGAme = false;
        addMoves = 0;
        UpdateMovesText();
        CreateCollectedElements();
    }

    //создание коллекции целей
    public void CreateCollectedElements()
    {
        //Debug.Log("CreateCollectedElements");
        //удаляем все задания
        string targetsName = "Targets";
        Transform targetsTransform = transform.Find(targetsName);
        if (targetsTransform != null)
        {
            DestroyImmediate(targetsTransform.gameObject);
        }
        CreateTargetsParent();

        if (targets.Length > 0)
        {
            //создаем задания
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].GameObject = Instantiate(prefabcollectedElements);
                Image image = targets[i].GameObject.GetComponent(typeof(Image)) as Image;
                image.sprite = SpriteBank.SetShape(targets[i].elementsShape);
                targets[i].Image = image;
                targets[i].Text = image.GetComponentInChildren<Text>();
            }

            //создаем первичную анимацию если в игре
            if (Application.isPlaying)
            {
                StartCoroutine(StartGameAnimation());
            }
            else
            {
                //перемещаем на основную панель   
                MovingTasksToMainPanel();
            }
        }        
    }

    public IEnumerator StartGameAnimation()
    {
        //создаем первичную анимацию если в игре
        Destroy(canvasStartGame);
        canvasStartGame = Instantiate(PrefabBank.CanvasStartGame);
        canvasStartGame.GetComponent<Canvas>().worldCamera = Camera.main;
        Transform canvasStartGamePanel = canvasStartGame.transform.Find("Panel");
        //смещение по x
        RectTransform rectTransformTarget = targets[0].GameObject.transform.GetComponent<RectTransform>();
        float scale = rectTransformTarget.rect.width / rectTransformTarget.rect.size.x * 2.5f * 1.5f;
        float startingXPoint = canvasStartGamePanel.transform.position.x - ((scale) * (targets.Length - 1)) * 0.5f;
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].GameObject.transform.SetParent(canvasStartGamePanel, false);
            targets[i].GameObject.transform.position = new Vector3(startingXPoint + (i * (scale)), canvasStartGamePanel.transform.position.y, canvasStartGamePanel.transform.position.z);
            targets[i].GameObject.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        }
        yield return new WaitForSeconds(1.0f);

        //если другая куротина не уничтожила канвас
        if (canvasStartGamePanel != null)
        {
            //делаем исчезновение
            Image imageCanvasStartGamePanel = canvasStartGamePanel.GetComponent<Image>();
            MainAnimator.Instance.AddElementForSmoothChangeColor(imageCanvasStartGamePanel, new Color(imageCanvasStartGamePanel.color.r, imageCanvasStartGamePanel.color.g, imageCanvasStartGamePanel.color.b, 0), 4f);
            Destroy(canvasStartGame, 3f);
            //перемещаем на основную панель
            MovingTasksToMainPanel();
        }
        
    }

    public void CreateTargetsParent()
    {
        string targetsName = "Targets";
        targetsParent = new GameObject();
        targetsParent.transform.SetParent(transform, false);
        targetsParent.name = targetsName;
        targetsParent.transform.localPosition = new Vector3(-32, -32, targetsParent.transform.localPosition.z);
    }

    public void MovingTasksToMainPanel()
    {
        if (targets.Length > 0)
        {
            if (targetsParent == null)
            {
                CreateTargetsParent();
            }
            //перемещаем на основную панель
            //смещение по y
            RectTransform rectTransformTarget = targets[0].GameObject.transform.GetComponent<RectTransform>();
            float scale = rectTransformTarget.rect.height / rectTransformTarget.rect.size.y * 1.5f;
            float startingYPoint = targetsParent.transform.position.y - ((scale) * (targets.Length - 1)) * 0.5f;

            for (int i = 0; i < targets.Length; i++)
            {
                if (Application.isPlaying)
                {
                    Transform targetTransform = targets[i].GameObject.transform;
                    MainAnimator.Instance.AddElementForSmoothMove(targets[i].GameObject.transform, new Vector3(targetsParent.transform.position.x, startingYPoint + (i * (scale)), targetsParent.transform.position.z), 
                        1, SmoothEnum.InLineWithOneSpeed, 0.3f, action: () => SetParentTargets(targetTransform));
                }
                else
                {
                    targets[i].GameObject.transform.SetParent(targetsParent.transform, false);
                    targets[i].GameObject.transform.position = new Vector3(targetsParent.transform.position.x, startingYPoint + (i * (scale)), targetsParent.transform.position.z);
                }
                targets[i].GameObject.transform.localScale = new Vector3(1, 1, 1);

            }
        }

    }

    public void SetParentTargets(Transform targetTransform)
    {
        if (targetTransform != null && targetsParent != null)
        {
            targetTransform.SetParent(targetsParent.transform, true);
        }        
    }


    public bool Collect(AllShapeEnum allShape, Transform transformElement)
    {
        foreach (Target target in targets)
        {
            if (target.Collect(allShape, transformElement))
            {                
                //GameObject go = new GameObject();
                //go.transform.position = transformElement.position;
                //go.transform.parent = this.thisTransform;
                //transformElement.parent = this.thisTransform;
                transformElement.SetParent(this.thisTransform, true);
                transformElement.GetComponent<SpriteRenderer>().sortingLayerName = "Magic";
                //transformElement.GetComponent<AnimatorElement>().PlayIdleAnimation();
                //перемещаем элемент к нашему объекту
                MainAnimator.Instance.AddElementForSmoothMove(transformElement, target.Image.transform.position, 7, SmoothEnum.InLineWithOneSpeed, 1.30f, true, false, delegate { target.ItemReached(transformElement); } );
                //проверяем, не собрали ли мы коллекцию
                CheckAll();
                return true;
            }
        }
        return false;
    }

    //проверяем, не собрали ли мы коллекции
    private void CheckAll()
    {
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

            //!!! сделать мега взрыв показывающий что все элементы собраны
        }
    }

    public void UpdateAllGoal()
    {
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
    public void UpdateMovesText()
    {
        if (movesText == null)
        {
            Debug.Log("потеряли movesText");
            movesText = GetComponentInChildren<Text>();
        }
        movesText.text = "" + moves;
    }

    //ход
    //возвращяет удалось ли уменьшить количество ходов
    public bool SubMoves()
    {
        if (Moves > 0)
        {
            Moves--;
            UpdateMovesText();
            if (Moves == 0)
            {
                endGame = true;
            }
            return true;
        }
        else
        {
            return false;
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

        return tasksXElement;
    }

    public void RecoverFromXElement(XElement tasksXElement)
    {
        //восстанавливаем значения
        this.Moves = int.Parse(tasksXElement.Element("moves").Value);
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
        
        ResetParameters();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Tasks), true)]
public class TasksEditor : Editor
{
    Tasks thisTasks;

    void OnEnable()
    {
        thisTasks = (Tasks)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Настройка заданий:", EditorStyles.boldLabel);
        base.OnInspectorGUI();
        if (GUILayout.Button("Обновить"))
        {
            thisTasks.ResetParameters();
        }
    }
}
#endif