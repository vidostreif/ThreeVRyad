using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class InstrumentsManager : MonoBehaviour, IESaveAndLoad
{
    public static InstrumentsManager Instance; // Синглтон
    //[HideInInspector] public Transform thisTransform;
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
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров Tasks!");
        }

        Instance = this;
        //thisTransform = transform;
    }

    void Start()
    {
        //CreateInstruments();
    }

    public void ResetParameters()
    {
        //Сбрасываем значения
        DeactivateInstrument();
        CreateInstruments();
    }

    //создание коллекции инструментов
    private void CreateInstruments()
    {
        //удаляем все инструменты
        string instrumentsName = "Instruments";
        Transform instrumentsTransform = transform.Find(instrumentsName);
        if (instrumentsTransform != null)
        {
            DestroyImmediate(instrumentsTransform.gameObject);
        }
        GameObject instrumentsParent = new GameObject();
        instrumentsParent.transform.SetParent(transform, false);
        instrumentsParent.name = instrumentsName;

        //смещение по y
        float startingYPoint = transform.position.y - ((GridBlocks.Instance.blockSize + distanceBetweenInstruments) * (instruments.Length - 1)) * 0.5f;

        for (int i = 0; i < instruments.Length; i++)
        {            
                instruments[i].GameObject = Instantiate(prefabInstrument, new Vector3(transform.position.x, startingYPoint + (i * (GridBlocks.Instance.blockSize + distanceBetweenInstruments)), transform.position.z), Quaternion.identity, instrumentsParent.transform);
                Image image = instruments[i].GameObject.GetComponent(typeof(Image)) as Image;
                image.sprite = SpriteBank.SetShape(instruments[i].Type);
                instruments[i].Image = image;
                instruments[i].Text = image.GetComponentInChildren<Text>();
            //если разрешен на уровне
            if (instruments[i].Allow)
            {
                instruments[i].AddAction(instruments[i].GameObject);
            }
            else
            {
                SupportFunctions.ChangeAlfa(instruments[i].Image, 0.2f);
                
                //!!!сверху повесить замок
            }                 
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
        preparedInstrument.PSSelect = Instantiate(MainParticleSystem.Instance.pSSelect, preparedInstrument.GameObject.transform);
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
            spriteRenderer.sprite = preparedInstrument.Image.sprite;
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
