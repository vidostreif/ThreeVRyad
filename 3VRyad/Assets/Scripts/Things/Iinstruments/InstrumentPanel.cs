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
    private bool activatedAtTheMoment;//активируется в данны момент
    private bool stopActivation;//остановить активацию
    private GameObject[] psGOs; //массив эффектов подсвечивания
    private bool createPsGOsMassive;//создается массив эффектов

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

        //устанавливаем спрайты под разрешение экрана
        GetComponent<Image>().sprite = SpriteBank.SetShape(SpritesEnum.Tasks_panel);
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
                Thing thing = ThingsManager.Instance.GetThing(instrumentOnGame.Type);
                if (thing != null)
                {
                    instrumentOnGame.InstrumentOnManager = thing;
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

    public ThingsButton GetThingButton(InstrumentsEnum type)
    {
        if (instrumentsOnGame != null)
        {
            foreach (InstrumentOnGame item in instrumentsOnGame)
            {
                if (item.Type == type)
                {
                    return item.GameInstrumentButton;
                }
            }
        }
        return null;
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
        activatedAtTheMoment = false;
        createPsGOsMassive = false;
        StopAllCoroutines();
        //Сбрасываем значения
        DeactivateInstrument();
        DestroyHighlightBlocks();
        CreateInstrumentsOnGame();        
    }
        
    public void PreparInstrument(InstrumentOnGame instrument)
    {
        SceneSettings.Instance.HideSetings();
        if (!activatedAtTheMoment)
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
            //instrument.PSSelect = Instantiate(MainParticleSystem.Instance.pSSelect, preparedInstrument.GameInstrumentButton.GameObject.transform);
            instrument.PSSelect = ParticleSystemManager.Instance.CreatePS(preparedInstrument.GameInstrumentButton.GameObject.transform, PSEnum.PSSelect);
        }        
    }

    public void DeactivateInstrument()
    {
        instrumentPrepared = false;
        //если не активируется в текущий момент
        if (!activatedAtTheMoment)
        {
            if (preparedInstrument != null && preparedInstrument.PSSelect != null)
            {
                //Destroy(preparedInstrument.PSSelect);
                PoolManager.Instance.ReturnObjectToPool(preparedInstrument.PSSelect);
            }   

            preparedInstrument = null;
            stopActivation = false;
        }
        else
        {
            stopActivation = true;
        }
    }

    public void ActivateInstrument(Block block)
    {
        if (instrumentPrepared)
        {
            SceneSettings.Instance.HideSetings();
            instrumentPrepared = false;
            StartCoroutine(Activate(block));            
        }
        
    }

    private IEnumerator Activate(Block block)
    {
        //if (instrumentPrepared)
        //{
            activatedAtTheMoment = true;
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

            if (successfulActivation)
            {
                Debug.Log("Инструмент активирован!");
                preparedInstrument.InstrumentOnManager.SubQuantity();
                ThingsManager.Instance.RecordSave();
                GridBlocks.Instance.Move();
                successfulActivation = false;
            }
            activatedAtTheMoment = false;
            DeactivateInstrument();
        //}
    }

    //активация лопаты
    //ударяет по одному блоку
    private IEnumerator ActivateShovel(Block block)
    {
        if (BlockCheck.ThisBlockWithMortalElement(block))
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Shovel);
            //высвечиваем блок
            Block[] blocks = new Block[1];
            blocks[0] = block;
            StartCoroutine(HighlightBlocks(blocks));
            yield return new WaitForSeconds(0.1f);
            if (block != null)
            {
                block.Hit(HitTypeEnum.Instrument);
                yield return new WaitForSeconds(0.5f);
                DestroyHighlightBlocks();
                successfulActivation = true;
                yield break;
            }
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
            //Instantiate(MainParticleSystem.Instance.pSMagicalTail, instrumentGO.transform);
            GameObject PSMagicalTail = ParticleSystemManager.Instance.CreatePS(instrumentGO.transform, PSEnum.PSMagicalTail);
            SpriteRenderer spriteRenderer = instrumentGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = SpriteBank.SetShape(preparedInstrument.Type);
            spriteRenderer.sortingLayerName = "Magic";
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.LawnMowerStart);

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

            //подсвечиваем все блоки
            Block[] highlight = new Block[blocks.Length + 1];
            highlight[0] = block;
            for (int i = 0; i < blocks.Length; i++)
            {
                highlight[i+1] = blocks[i];
            }
            yield return StartCoroutine(HighlightBlocks(highlight));

            //если сетка выполняет действия, то ожидаем
            yield return new WaitForSeconds(0.3f);
            do
            {
                yield return new WaitForSeconds(0.01f);
                //если инструмент был деактивирован
                if (stopActivation)
                {
                    successfulActivation = false;
                    DestroyHighlightBlocks();                    
                    PoolManager.Instance.ReturnObjectToPool(PSMagicalTail);
                    Destroy(instrumentGO);
                    yield break;
                }
            } while (GridBlocks.Instance.blockedForMove);

            SoundManager.Instance.PlaySoundInternal(SoundsEnum.LawnMowerWork);
            block.Hit(HitTypeEnum.Instrument);
            DestroyHighlightBlock(block);
            foreach (Block curBlock in blocks)
            {
                if (curBlock != null)
                {
                    MainAnimator.Instance.AddElementForSmoothMove(instrumentGO.transform, curBlock.transform.position, 1, SmoothEnum.InLineWithOneSpeed, smoothTime: 0.25f, addToQueue: false);
                    //ожидаем передвижения эффекта к текущему блоку
                    do
                    {
                        if (curBlock != null)
                        {
                            yield return new WaitForSeconds(0.01f);
                        }
                        else
                        {
                            break;
                        }
                        
                    } while (instrumentGO.transform.position != curBlock.transform.position);

                    DestroyHighlightBlock(curBlock);
                    curBlock.Hit(HitTypeEnum.Instrument);
                }
            }
            DestroyHighlightBlocks();
            spriteRenderer.sprite = null;
            PoolManager.Instance.ReturnObjectToPool(PSMagicalTail);
            Destroy(instrumentGO, 3);
            successfulActivation = true;
            yield break;
        }
        successfulActivation = false;
    }

    //активация вихря
    //перемешивает стандартные незаблокированные элементы
    private IEnumerator ActivateVortex(Block block)
    {   
        SoundManager.Instance.PlaySoundInternal(SoundsEnum.Wind);
        Block[] blocks = GridBlocks.Instance.GetAllBlocksWithStandartElements();
        yield return StartCoroutine(HighlightBlocks(blocks));

        //если сетка выполняет действия, то ожидаем
        do
        {
            yield return new WaitForSeconds(0.01f);
            //если инструмент был деактивирован
            if (stopActivation)
            {
                successfulActivation = false;
                DestroyHighlightBlocks();
                yield break;
            }
        } while (GridBlocks.Instance.blockedForMove);
        
        if (blocks.Length > 1)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Wind_active);
            GridBlocks.Instance.MixStandartElements();
            yield return new WaitForSeconds(0.3f);
            DestroyHighlightBlocks();
            successfulActivation = true;
            yield break;
        }
        DestroyHighlightBlocks();
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
                SoundManager.Instance.PlaySoundInternal(SoundsEnum.Repainting);
                //перемешиваем
                SupportFunctions.MixArray(blocks);
                Block[] blocksForWork = new Block[quantity + 1];
                blocksForWork[0] = block;
                //подбераем блоки для обработки
                foreach (Block curBlock in blocks)
                {
                    if (curBlock.Element.Shape != block.Element.Shape && (curBlock.Element.BlockingElement == null || (curBlock.Element.BlockingElement != null && curBlock.Element.BlockingElement.Destroyed)) && !curBlock.Blocked)
                    {
                        repainted++;
                        blocksForWork[repainted] = curBlock;
                        curBlock.Blocked = true;//блокируем                        
                    }

                    if (quantity <= repainted)
                    {
                        break;
                    }
                }

                //если нашли хоть один блок для обработки
                if (repainted != 0)
                {                
                    yield return StartCoroutine(HighlightBlocks(blocksForWork));
                    yield return new WaitForSeconds(0.2f);

                    //если сетка выполняет действия, то ожидаем
                    do
                    {
                        yield return new WaitForSeconds(0.01f);
                        //если инструмент был деактивирован
                        if (stopActivation)
                        {
                            foreach (Block curBlock in blocksForWork)
                            {
                                if (curBlock != null)
                                {
                                    curBlock.Blocked = false;//разблокируем
                                }
                            }
                            successfulActivation = false;
                            DestroyHighlightBlocks();
                            yield break;
                        }
                    } while (GridBlocks.Instance.blockedForMove);

                    //создаем и перемещаем новые элементы
                    foreach (Block curBlock in blocksForWork)
                    {
                        if (curBlock != null)
                        {
                            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Repainting_ring);
                            //создаем новый элемент
                            curBlock.CreatElement(GridBlocks.Instance.prefabElement, block.Element.Shape, block.Element.Type);
                            curBlock.Element.transform.position = block.transform.position;
                            //добавляем эффект для перемещяемого эллемента
                            //GameObject effect = Instantiate(MainParticleSystem.Instance.pSMagicalTail, curBlock.Element.transform);
                            ParticleSystemManager.Instance.CreatePS(curBlock.Element.transform, PSEnum.PSMagicalTail, 4);
                            //Destroy(effect, 4);

                            yield return new WaitForSeconds(0.25f);
                            DestroyHighlightBlock(curBlock);
                            curBlock.Blocked = false;//разблокируем
                        }                            
                    }
                    successfulActivation = true;
                    yield break;
                }
            }
        }
        successfulActivation = false;
    }

    //дополнительные функции
    //подсветить блоки
    private IEnumerator HighlightBlocks(Block[] blocks) {
        //если массив существует, то предварительно удаляем все эффекты в нем
        DestroyHighlightBlocks();
        createPsGOsMassive = true;
        //создаем новый массив
        psGOs = new GameObject[blocks.Length];
        //создаем эффекты
        for (int i = 0; i < blocks.Length; i++)
        {
            if (stopActivation)
            {
                createPsGOsMassive = false;
                DestroyHighlightBlocks();
            }

            if (blocks[i] != null && blocks[i].thisTransform != null)
            {
                psGOs[i] = CreateHighlightEffect(blocks[i].thisTransform);
                yield return new WaitForSeconds(0.03f);
            }            
        }
        createPsGOsMassive = false;
    }

    private GameObject CreateHighlightEffect(Transform parentBlock, float lifeTime = 0) {        
        GameObject psGO = ParticleSystemManager.Instance.CreatePS(parentBlock, PSEnum.PSSelectTargetBlockBlue, lifeTime);
        //if (lifeTime != 0)
        //{
        //    Destroy(psGO, lifeTime);
        //}
        return psGO;
    }

    private void DestroyHighlightBlocks()
    {
        if (!createPsGOsMassive)
        {
            //если массив существует, то предварительно удаляем все эффекты в нем
            if (psGOs != null)
            {
                for (int i = 0; i < psGOs.Length; i++)
                {
                    PoolManager.Instance.ReturnObjectToPool(psGOs[i]);
                    //Destroy(psGOs[i]);
                }
            }
            psGOs = null;
        }
        
    }

    private void DestroyHighlightBlock(Block block)
    {
        //удаляем указанный спецэффект
        if (psGOs != null)
        {
            for (int i = 0; i < psGOs.Length; i++)
            {
                if (psGOs[i] != null && psGOs[i].transform.parent == block.transform)
                {
                    PoolManager.Instance.ReturnObjectToPool(psGOs[i]);
                    //Destroy(psGOs[i]);
                }                
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
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.ClickButton);
            InstrumentPanel.Instance.PreparInstrument(this);
        }
    }

    private void AddAction(ThingsButton InstrumentButton, Action action)
    {
        //добавляем действие к кнопке
        InstrumentButton.Button = InstrumentButton.GameObject.GetComponent(typeof(Button)) as Button;
        InstrumentButton.Button.onClick.RemoveAllListeners();
        InstrumentButton.Button.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
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