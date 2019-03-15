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

    public void DeactivateInstrument()
    {
        instrumentPrepared = false;
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
            instrumentPrepared = false;

            if (successfulActivation)
            {
                Debug.Log("Инструмент активирован!");
                preparedInstrument.SubQuantity();
                GridBlocks.Instance.Move();
                successfulActivation = false;
            }            
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
        Block[] blocks = GridBlocks.Instance.DeterminingAllCrossBlocks(GridBlocks.Instance.FindPosition(block));
        if (blocks.Length > 0)
        {            
            GameObject instrumentGO = new GameObject();
            //добавляем эффект
            Instantiate(MainParticleSystem.Instance.prefabMagicalTail, instrumentGO.transform);
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
    {
        Block[] blocks = GridBlocks.Instance.ReturnAllBlocksWithStandartElements();
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
            Block[] blocks = GridBlocks.Instance.ReturnAllBlocksWithStandartElements();
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
                        GameObject effect = Instantiate(MainParticleSystem.Instance.prefabMagicalTail, curBlock.Element.transform);
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
}
