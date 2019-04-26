using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class InstrumentsManager : MonoBehaviour, IESaveAndLoad
{
    public static InstrumentsManager Instance; // Синглтон
    public float distanceBetweenInstruments;
    public GameObject prefabInstrument;
    public Instrument[] instruments;// список инструментов 

    private bool instrumentPrepared = false;
    private Instrument preparedInstrument;
    private bool successfulActivation = false;
    
    public bool InstrumentPrepared
    {
        get
        {
            return instrumentPrepared;
        }
    }

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

        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject); //Set as do not destroy
        }

        //загружаем сохранение
        Save save = JsonSaveAndLoad.LoadSave();
        foreach (InstrumentsSave instrumentsSave in save.instrumentsSave)
        {
            foreach (Instrument instrument in instruments)
            {
                if (instrumentsSave.instrumenTypeEnum == instrument.Type.ToString())
                {
                    instrument.AddQuantity(instrumentsSave.count);
                    break;
                }
            }
        }
        
    }

    void Start()
    {
        CreateInstrumentsOnGame();
    }
    
    //добавление количества инструментов из бандла
    public bool addinstruments(BundleShopV[] bundleShopV) {

        bool res = true;
        //добавляем бандл к инструментам
        foreach (BundleShopV item in bundleShopV)
        {
            bool found = false;
            foreach (Instrument instrument in instruments)
            {
                if (instrument.Type == item.type)
                {
                    instrument.AddQuantity(item.count);
                    Debug.Log("Добавили к инструменту " + instrument.Type +  ", " + item.count + " шт.");
                    found = true;
                    break;
                }
            }

            //если не нашли нужный инструмент
            if (!found)
            {
                res = false;
            }
        }

        //отнимаем обратно если не нашли все инструменты
        if (!res)
        {
            foreach (BundleShopV item in bundleShopV)
            {
                foreach (Instrument instrument in instruments)
                {
                    if (instrument.Type == item.type)
                    {
                        instrument.SubQuantity(item.count);
                        Debug.Log("Отняли у инструмента " + instrument.Type + ", " + item.count + " шт.");
                        break;
                    }
                }
            }
        }
        else
        {
            JsonSaveAndLoad.RecordSave(instruments);
        }

        return res;
    }
    
    public void ResetParameters()
    {
        //Сбрасываем значения
        DeactivateInstrument();
        CreateInstrumentsOnGame();
    }

    //создание коллекции инструментов в игре
    public void CreateInstrumentsOnGame()
    {
        //если на сцене игры
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
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
                float startingYPoint = panel.transform.position.y - ((1 + distanceBetweenInstruments) * (instruments.Length - 1)) * 0.5f;

                for (int i = 0; i < instruments.Length; i++)
                {
                    
                    GameObject go = Instantiate(prefabInstrument, new Vector3(panel.transform.position.x, startingYPoint + (i * (1 + distanceBetweenInstruments)), panel.transform.position.z), Quaternion.identity, instrumentsParent.transform);
                    instruments[i].CreateGameInstrumentButton(go);
                }
            }
            else
            {
                Debug.Log("Не нашли панель инструментов!");
            }
        }        
    }

    //создание коллекции инструментов в магазине
    public void CreateInstrumentsOnShop(Transform panelTransform)
    {        
            if (panelTransform != null)
            {
                //смещение по x
                float startingXPoint = panelTransform.position.x - ((1 + distanceBetweenInstruments) * (instruments.Length - 1)) * 0.5f;

                for (int i = 0; i < instruments.Length; i++)
                {
                    GameObject go = Instantiate(prefabInstrument, new Vector3(startingXPoint + (i * (1 + distanceBetweenInstruments)), panelTransform.position.y, panelTransform.position.z), Quaternion.identity, panelTransform);
                    instruments[i].CreateShopInstrumentButton(go);
                }
            }
            else
            {
                Debug.Log("Не нашли панель в магазине для отображения инструментов!");
            }
    }

    public void PreparInstrument(Instrument instrument)
    {
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
        preparedInstrument.PSSelect = Instantiate(MainParticleSystem.Instance.pSSelect, preparedInstrument.GameInstrumentButton.GameObject.transform);
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

    public void ActivateInstrument(Block block) {
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
                preparedInstrument.SubQuantity();
                JsonSaveAndLoad.RecordSave(instruments);
                GridBlocks.Instance.Move();
                successfulActivation = false;
            }
            DeactivateInstrument();
        }
    }

    //активация лопаты
    //ударяет по одному блоку
    private IEnumerator ActivateShovel(Block block) {
        if (BlockCheck.ThisBlockWithMortalElement(block))
        {
            block.Hit();
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

            block.Hit();
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
                    curBlock.Hit();
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
        foreach (Instrument instrument in instruments)
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
        foreach (Instrument item in instruments)
        {
            item.Allow = true;
        }

        foreach (XElement shapeAndGoalXElement in tasksXElement.Element("instruments").Elements("typeAndAllow"))
        {
            InstrumentsEnum type = (InstrumentsEnum)Enum.Parse(typeof(InstrumentsEnum), shapeAndGoalXElement.Attribute("type").Value);
            bool allow = bool.Parse(shapeAndGoalXElement.Attribute("allow").Value);

            //ищем наш инструмент
            foreach (Instrument item in instruments)
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
