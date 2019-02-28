using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstrumentsManager : MonoBehaviour
{
    public static InstrumentsManager Instance; // Синглтон
    [HideInInspector] public Transform thisTransform;
    public float distanceBetweenInstruments;
    public GameObject prefabInstrument;

    public Instrument[] instruments;// список инструментов 
    private bool instrumentPrepared = false;
    private Instrument preparedInstrument;


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
        thisTransform = transform;
    }

    void Start()
    {
        CreateInstruments();
    }

    //создание коллекции инструментов
    private void CreateInstruments()
    {
        //смещение по y
        float startingYPoint = thisTransform.position.y - ((GridBlocks.Instance.blockSize + distanceBetweenInstruments) * (instruments.Length - 1)) * 0.5f;

        for (int i = 0; i < instruments.Length; i++)
        {
            GameObject elementGameObject = Instantiate(prefabInstrument, new Vector3(thisTransform.position.x, startingYPoint + (i * (GridBlocks.Instance.blockSize + distanceBetweenInstruments)), thisTransform.position.z), Quaternion.identity, this.thisTransform);
            Image image = elementGameObject.GetComponent(typeof(Image)) as Image;
            image.sprite = SpriteBank.SetShape(instruments[i].Type);
            instruments[i].Image = image;
            instruments[i].Text = image.GetComponentInChildren<Text>();
            instruments[i].AddAction(elementGameObject);            
        }
    }

    public void PreparInstrument(Instrument instrument)
    {
        //добавить проверку что нет актиного инструмента
        instrumentPrepared = true;
        preparedInstrument = instrument;
    }

    public void ActivateInstrument(Block block)
    {
        if (instrumentPrepared)
        {
            bool successfulActivation;
            //в зависимости от типа
            switch (preparedInstrument.Type)
            {
                case InstrumentsEnum.Empty:
                    successfulActivation = false;
                    break;
                case InstrumentsEnum.Shovel:
                    successfulActivation = ActivateShovel(block);
                    break;
                case InstrumentsEnum.Hoe:
                    successfulActivation = ActivateHoe(block);
                    break;
                case InstrumentsEnum.Vortex:
                    successfulActivation = ActivateVortex(block);
                    break;
                case InstrumentsEnum.Repainting:
                    successfulActivation = ActivateRepainting(block);
                    break;
                default:
                    Debug.LogError("Не определен тип инструмента" + preparedInstrument.Type);
                    successfulActivation = false;
                    break;
            }

            if (successfulActivation)
            {
                preparedInstrument.SubQuantity();
                GridBlocks.Instance.Move();
            }
            instrumentPrepared = false;
        }
    }

    //активация лопаты
    //ударяет по одному блоку
    private bool ActivateShovel(Block block) {
        if (GridBlocks.Instance.ThisBlockWithMortalElement(block))
        {
            block.Hit();
            return true;
        }
        return false;
    }

    //активация мотыги
    //ударяет крест на крест по всем блокам от указанного блока 
    private bool ActivateHoe(Block block)
    {
        Block[] blocks = GridBlocks.Instance.DeterminingAllCrossBlocks(GridBlocks.Instance.FindPosition(block));
        if (blocks.Length > 0)
        {
            block.Hit();
            foreach (Block curBlock in blocks)
            {
                if (curBlock != null)
                {
                    curBlock.Hit();
                }                
            }
            return true;
        }
        return false;
    }

    //активация вихря
    //перемешивает стандартные незаблокированные элементы
    private bool ActivateVortex(Block block)
    {
        Block[] blocks = GridBlocks.Instance.ReturnAllBlocksWithStandartElements();
        if (blocks.Length > 1)
        {
            GridBlocks.Instance.MixStandartElements();
            return true;
        }
        return false;
    }

    //активация перекраски 
    //перекрашивает 10 стандартных элементов
    private bool ActivateRepainting(Block block)
    {
        if (GridBlocks.Instance.ThisBlockWithStandartElement(block))
        {
            Block[] blocks = GridBlocks.Instance.ReturnAllBlocksWithStandartElements();
            int quantity = 10;//количество перекрашиваемых элементов
            int repainted = 0;
            if (blocks.Length > 1)
            {
                SupportFunctions.MixArray(blocks);
                foreach (Block curBlock in blocks)
                {
                    if (curBlock.Element.Shape != block.Element.Shape)
                    {
                        //создаем новый элемент
                        curBlock.CreatElement(GridBlocks.Instance.prefabElement, block.Element.Shape, block.Element.Type);
                        repainted++;
                    }

                    if (quantity <= repainted)
                    {
                        break;
                    }
                }
                //если перекрасили хоть один элемент
                if (repainted != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }                
            }
            return false;
        }
        return false;
    }
}
