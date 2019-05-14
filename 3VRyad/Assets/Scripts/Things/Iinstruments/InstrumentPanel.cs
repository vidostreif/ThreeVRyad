using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class InstrumentPanel : MonoBehaviour, IESaveAndLoad
{
    public static InstrumentPanel Instance; // Синглтон
    public float distanceBetweenInstruments;
    public InstrumentOnGame[] instrumentsOnGame;// список инструментов 

    private bool instrumentPrepared;
    private InstrumentOnGame preparedInstrument;
    private bool successfulActivation;

    void Awake()
    {
        // регистрация синглтона
        if (Instance)
        {
            Destroy(this.gameObject); //Delete duplicate
            return;
        }
        else
        {
            Instance = this; //Make this object the only instance            
        }

        instrumentPrepared = false;
        successfulActivation = false;
    }

    //создание коллекции инструментов в игре
    public void CreateInstrumentsOnGame()
    {
        FoundInstrumentsOnMenedger();
        //находим панель инструентов в игре
        GameObject panel = GameObject.Find("PanelInstruments");

        if (panel != null)
        {
            //удаляем все инструменты
            string instrumentsName = "Instruments";
            Transform instrumentsTransform = panel.transform.Find(instrumentsName);
            if (instrumentsTransform != null)
            {
                DestroyImmediate(instrumentsTransform.gameObject);
            }
            GameObject instrumentsParent = new GameObject();
            instrumentsParent.transform.SetParent(panel.transform, false);
            instrumentsParent.name = instrumentsName;

            //смещение по y
            float startingYPoint = panel.transform.position.y - ((1 + distanceBetweenInstruments) * (instrumentsOnGame.Length - 1)) * 0.5f;

            for (int i = 0; i < instrumentsOnGame.Length; i++)
            {
                GameObject go = Instantiate(PrefabBank.PrefabButtonThing, new Vector3(panel.transform.position.x, startingYPoint + (i * (1 + distanceBetweenInstruments)), panel.transform.position.z), Quaternion.identity, instrumentsParent.transform);
                instrumentsOnGame[i].CreateGameInstrumentButton(go);
            }
        }
        else
        {
            Debug.Log("Не нашли панель инструментов!");
        }
    }
    //находим наши инструменты в общем менеджере
    public void FoundInstrumentsOnMenedger()
    {
        if (ThingsManager.Instance != null)
        {
            foreach (InstrumentOnGame instrumentOnGame in instrumentsOnGame)
            {
                foreach (Thing instrument in ThingsManager.Instance.instruments)
                {
                    if (instrumentOnGame.Type == instrument.Type)
                    {
                        instrumentOnGame.InstrumentOnManager = instrument;
                    }
                }
            }
        }        
    }
    //обновляем текст инструмента
    public void UpdateTextInstrumentOnGame(InstrumentsEnum type) {
        foreach (InstrumentOnGame item in instrumentsOnGame)
        {
            if (item.Type == type)
            {
                item.UpdateText();
            }
        }
    }

    //взаимодействие игрока с инструментами
    public bool InstrumentPrepared
    {
        get
        {
            return instrumentPrepared;
        }
    }

    public void ResetParameters()
    {
        //Сбрасываем значения
        DeactivateInstrument();
        CreateInstrumentsOnGame();
    }
        
    public void PreparInstrument(InstrumentOnGame instrument)
    {
        //удаляем подсказку если она есть
        HelpToPlayer.DellGameHelp();
        //деактивируем предыдущий инструмент
        if (preparedInstrument != null)
        {
            if (preparedInstrument == instrument)
            {
                DeactivateInstrument();
                return;
            }
            else
            {
                DeactivateInstrument();
            }
        }
        instrumentPrepared = true;
        preparedInstrument = instrument;
        //создаем эффект подсветки
        instrument.PSSelect = Instantiate(MainParticleSystem.Instance.pSSelect, preparedInstrument.GameInstrumentButton.GameObject.transform);
    }

    public void DeactivateInstrument()
    {
        instrumentPrepared = false;
        if (preparedInstrument != null && preparedInstrument.PSSelect != null)
        {
            Destroy(preparedInstrument.PSSelect);
        }
        preparedInstrument = null;
    }

    public void ActivateInstrument(Block block)
    {
        StartCoroutine(Activate(block));
    }

    private IEnumerator Activate(Block block)
    {
        if (instrumentPrepared)
        {
            //в зависимости от типа
            switch (preparedInstrument.Type)
            {
                case InstrumentsEnum.Empty:
                    successfulActivation = false;
                    break;
                case InstrumentsEnum.Shovel:
                    yield return StartCoroutine(ActivateShovel(block));
                    //successfulActivation = ActivateShovel(block);
                    break;
                case InstrumentsEnum.Hoe:
                    yield return StartCoroutine(ActivateHoe(block));
                    //successfulActivation = ActivateHoe(block);
                    break;
                case InstrumentsEnum.Vortex:
                    yield return StartCoroutine(ActivateVortex(block));
                    //successfulActivation = ActivateVortex(block);
                    break;
                case InstrumentsEnum.Repainting:
                    yield return StartCoroutine(ActivateRepainting(block));
                    //successfulActivation = ActivateRepainting(block);
                    break;
                default:
                    Debug.LogError("Не определен тип инструмента" + preparedInstrument.Type);
                    successfulActivation = false;
                    break;
            }
            //instrumentPrepared = false;

            if (successfulActivation)
            {
                Debug.Log("Инструмент активирован!");
                preparedInstrument.InstrumentOnManager.SubQuantity();
                JsonSaveAndLoad.RecordSave(ThingsManager.Instance.instruments);
                GridBlocks.Instance.Move();
                successfulActivation = false;
            }
            DeactivateInstrument();
        }
    }

    //активация лопаты
    //ударяет по одному блоку
    private IEnumerator ActivateShovel(Block block)
    {
        if (BlockCheck.ThisBlockWithMortalElement(block))
        {
            block.Hit(HitTypeEnum.Explosion);
            yield return new WaitForSeconds(0.3f);
            successfulActivation = true;
            yield break;
        }
        successfulActivation = false;
    }

    //активация мотыги
    //ударяет крест на крест по всем блокам от указанного блока 
    private IEnumerator ActivateHoe(Block block)
    {
        Block[] blocks = GridBlocks.Instance.GetAllCrossBlocks(block.PositionInGrid);
        if (blocks.Length > 0)
        {
            GameObject instrumentGO = new GameObject();
            //добавляем эффект
            Instantiate(MainParticleSystem.Instance.pSMagicalTail, instrumentGO.transform);
            SpriteRenderer spriteRenderer = instrumentGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = SpriteBank.SetShape(preparedInstrument.Type);
            spriteRenderer.sortingLayerName = "Magic";

            //находим первый существующий блок, для первичной установки инструмента
            int iteration = 0;
            do
            {
                if (blocks[iteration] != null)
                {
                    instrumentGO.transform.position = blocks[iteration].transform.position;
                    break;
                }
                iteration++;
            } while (iteration < blocks.Length);

            //если сетка выполняет действия, то ожидаем
            do
            {
                yield return new WaitForSeconds(0.01f);
                //если инструмент был деактивирован
                if (instrumentPrepared == false)
                {
                    successfulActivation = false;
                    yield break;
                }
            } while (GridBlocks.Instance.blockedForMove);

            block.Hit(HitTypeEnum.Explosion);
            foreach (Block curBlock in blocks)
            {
                if (curBlock != null)
                {
                    MainAnimator.Instance.AddElementForSmoothMove(instrumentGO.transform, curBlock.transform.position, 1, SmoothEnum.InLineWithOneSpeed, smoothTime: 0.3f, addToQueue: false);
                    //ожидаем передвижения эффекта к текущему блоку
                    do
                    {
                        yield return new WaitForSeconds(0.01f);
                    } while (instrumentGO.transform.position != curBlock.transform.position);
                    curBlock.Hit(HitTypeEnum.Explosion);
                }
            }
            spriteRenderer.sprite = null;
            Destroy(instrumentGO, 3);
            successfulActivation = true;
            yield break;
        }
        successfulActivation = false;
    }

    //активация вихря
    //перемешивает стандартные незаблокированные элементы
    private IEnumerator ActivateVortex(Block block)
    {   //если сетка выполняет действия, то ожидаем
        do
        {
            yield return new WaitForSeconds(0.01f);
            //если инструмент был деактивирован
            if (instrumentPrepared == false)
            {
                successfulActivation = false;
                yield break;
            }
        } while (GridBlocks.Instance.blockedForMove);

        Block[] blocks = GridBlocks.Instance.GetAllBlocksWithStandartElements();
        if (blocks.Length > 1)
        {
            GridBlocks.Instance.MixStandartElements();
            yield return new WaitForSeconds(0.3f);
            successfulActivation = true;
            yield break;
        }
        successfulActivation = false;
    }

    //активация перекраски 
    //перекрашивает 10 стандартных элементов
    private IEnumerator ActivateRepainting(Block block)
    {
        if (BlockCheck.ThisBlockWithStandartElement(block))
        {
            Block[] blocks = GridBlocks.Instance.GetAllBlocksWithStandartElements();
            int quantity = 10;//количество перекрашиваемых элементов
            int repainted = 0;
            if (blocks.Length > 1)
            {
                //если сетка выполняет действия, то ожидаем
                do
                {
                    yield return new WaitForSeconds(0.01f);
                    //если инструмент был деактивирован
                    if (instrumentPrepared == false)
                    {
                        successfulActivation = false;
                        yield break;
                    }
                } while (GridBlocks.Instance.blockedForMove);

                SupportFunctions.MixArray(blocks);
                foreach (Block curBlock in blocks)
                {
                    if (curBlock.Element.Shape != block.Element.Shape)
                    {
                        //создаем новый элемент
                        curBlock.CreatElement(GridBlocks.Instance.prefabElement, block.Element.Shape, block.Element.Type);
                        curBlock.Element.transform.position = block.transform.position;
                        //добавляем эффект
                        GameObject effect = Instantiate(MainParticleSystem.Instance.pSMagicalTail, curBlock.Element.transform);
                        Destroy(effect, 4);

                        repainted++;
                        yield return new WaitForSeconds(0.15f);
                    }

                    if (quantity <= repainted)
                    {
                        break;
                    }
                }
                //если перекрасили хоть один элемент
                if (repainted != 0)
                {
                    successfulActivation = true;
                    yield break;
                }
            }
        }
        successfulActivation = false;
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
        //записываем все внешности и количество
        XElement targetsXElement = new XElement("instruments");
        foreach (InstrumentOnGame instrument in instrumentsOnGame)
        {
            XAttribute type = new XAttribute("type", instrument.Type);
            XAttribute allow = new XAttribute("allow", instrument.Allow);
            XElement shapeAndGoalXElement = new XElement("typeAndAllow", type, allow);
            targetsXElement.Add(shapeAndGoalXElement);
        }
        XElement.Add(targetsXElement);

        return XElement;
    }

    public void RecoverFromXElement(XElement tasksXElement)
    {
        //восстанавливаем значения
        foreach (InstrumentOnGame item in instrumentsOnGame)
        {
            item.Allow = true;
        }

        foreach (XElement shapeAndGoalXElement in tasksXElement.Element("instruments").Elements("typeAndAllow"))
        {
            InstrumentsEnum type = (InstrumentsEnum)Enum.Parse(typeof(InstrumentsEnum), shapeAndGoalXElement.Attribute("type").Value);
            bool allow = bool.Parse(shapeAndGoalXElement.Attribute("allow").Value);

            //ищем наш инструмент
            foreach (InstrumentOnGame item in instrumentsOnGame)
            {
                if (item.Type == type)
                {
                    item.Allow = allow;
                    break;
                }
            }
        }
        ResetParameters();
    }
}

[System.Serializable]
public class InstrumentOnGame {
    [SerializeField] private InstrumentsEnum type;//какой вид инструмента
    [SerializeField] private bool allow;//разрешен на уровне
    private Thing instrumentOnManager; //ссылка на инструмент в менеджере
    private GameObject pSSelect;//наложенный эффект выделения
    private ThingsButton gameInstrumentButton; //кнопка инструмента в игре

    public ThingsButton GameInstrumentButton
    {
        get
        {
            return gameInstrumentButton;
        }
    }
    public GameObject PSSelect
    {
        get
        {
            return pSSelect;
        }

        set
        {
            pSSelect = value;
        }
    }
    public bool Allow
    {
        get
        {
            return allow;
        }

        set
        {
            allow = value;
        }
    }

    public InstrumentsEnum Type { get => type;}
    public Thing InstrumentOnManager { get => instrumentOnManager; set => instrumentOnManager = value; }

    public void ActionOnGame()
    {
        if (InstrumentOnManager.Quantity > 0)
        {
            InstrumentPanel.Instance.PreparInstrument(this);
        }
    }

    private void AddAction(ThingsButton InstrumentButton, Action action)
    {
        //добавляем действие к кнопке
        InstrumentButton.Button = InstrumentButton.GameObject.GetComponent(typeof(Button)) as Button;
        InstrumentButton.Button.onClick.RemoveAllListeners();
        InstrumentButton.Button.onClick.AddListener(delegate { action(); });
    }

    public void CreateGameInstrumentButton(GameObject go)
    {
        go.name = "Instrument" + type.ToString();
        if (gameInstrumentButton != null)
        {
            GameObject.DestroyImmediate(gameInstrumentButton.GameObject);
        }
        gameInstrumentButton = new ThingsButton(go, Type);
        UpdateText();

        //если разрешен на уровне
        if (Allow)
        {
            AddAction(gameInstrumentButton, ActionOnGame);
        }
        else
        {
            SupportFunctions.ChangeAlfa(gameInstrumentButton.Image, 0.2f);

            //!!!сверху повесить замок и действие открытия магазина
        }
    }

    public void UpdateText()
    {
        //для игрового отображения
        if (gameInstrumentButton != null && InstrumentOnManager != null)
        {
            gameInstrumentButton.UpdateText("" + InstrumentOnManager.Quantity);
        }
        else if (gameInstrumentButton != null)
        {
            gameInstrumentButton.UpdateText("");
        }
    }
}