using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEditor;
using System.Xml.Linq;
using System;
using System.Linq;

//#if UNITY_EDITOR
//[InitializeOnLoad]
//#endif
[ExecuteInEditMode]
public class GridBlocks : MonoBehaviour, IESaveAndLoad
{
    public static GridBlocks Instance; // Синглтон
    public float blockSize = 1;
    public Transform thisTransform;

    public Blocks[] containers;
    public GameObject prefabBlock;
    public GameObject prefabElement;
    public GameObject prefabBlockingWall;
    public List<ElementsPriority> elementsPriorityList;

    public bool blockedForMove { get; protected set; }//признак что сетка заблокирована для действий игроком
    private List<Element> elementsForMixList = new List<Element>();//элементы для замены во время микса
    private List<Blocks> elementsForMoveList = new List<Blocks>();//элементы для последовательного выполнения ходов
    private List<Block> blockFieldsList = new List<Block>();// найденные блоки для удара
    private List<Block> droppingBlockList = new List<Block>();// список сбрасывающих блоков
    private bool needFilling = false;

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров GridBlocks!");
        }

        Instance = this;
        thisTransform = transform;
        blockedForMove = false;

        //заполнение у блоков позиции
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                if (containers[x].block[y] != null)
                {
                    containers[x].block[y].PositionInGrid = new Position(x, y);
                }                
            }
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        //в режиме редактора
        if (!Application.isPlaying)
        {
            SettingBlocksOnPosition();
        }
#endif
    }

    //стартовое заполнение сетки и параметров
    public void StartFilling(List<ElementsPriority> elementsSAndP = null)
    {
        ElementsList.ClearElementsOnField();
        //проверяем что заполнены все GameObject
        if (prefabElement == null || prefabBlock == null || prefabBlockingWall == null)
        {
            Debug.LogError("В сетке не указан один из префабов!");
            return;
        }
        //проверяем что наш массив элементов инициорван
        if (elementsPriorityList == null || elementsPriorityList.Count == 0)
        {
            Debug.LogError("Не указано какие элементы будем создавать на поле!");
            return;
        }
        //если не заданы приоритеты, то берем стандартные
        if (elementsSAndP == null)
        {
            elementsSAndP = elementsPriorityList;
        }

        //заполнение сетки блоками и элементами [X, Y] [столбец, строка]
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //заполняем список сбрасываемых блоков
                if (BlockCheck.ThisBlockDroping(containers[x].block[y]))
                {
                    droppingBlockList.Add(containers[x].block[y]);
                }
                //если стандартный блок и в нем нет элемента
                if (BlockCheck.ThisStandardBlockWithoutElement(containers[x].block[y]))
                {
                    bool elementfound = false;
                    ElementsPriority elementsPriority = null;
                    //повторяем цикл пока не найдем нужный элемент но не более 10 раз
                    int iteration = 10;
                    do
                    {
                        iteration--;
                        //выбираем случайное число для выбора типа элемента из списка
                        //int random = UnityEngine.Random.Range(0, elementsShapeAndPriority.Count);
                        elementsPriority = ProportionalWheelSelection.SelectElement(elementsSAndP);

                        if (elementsPriority == null)
                        {
                            Debug.Log("Не нашли элемент для заполнения!");
                            return;
                        }
                        //проверяем элементы слева и снизу, что бы небыло 3 одинаковых элемента подряд
                        //по горизонтали проверяем со второго элемента
                        if (x > 0)
                        {
                            //проверяем что блоки существуют
                            if (containers[x - 1].block[y] != null)
                            {
                                //проверяем что в блоках есть элементы
                                if (containers[x - 1].block[y].Element != null)
                                {
                                    //если текущий элемент совпадает с елементом слева повторяем цикл
                                    if (elementsPriority != null && elementsPriority.ElementsShape == containers[x - 1].block[y].Element.Shape)
                                        continue;
                                }
                            }
                        }
                        //по вертикали проверяем со второго элемента
                        if (y > 0)
                        {
                            //проверяем что блоки существуют
                            if (containers[x].block[y - 1] != null)
                            {
                                //проверяем что в блоках есть элементы
                                if (containers[x].block[y - 1].Element != null)
                                {
                                    //если текущий элемент совпадает с елементом слева повторяем цикл
                                    if (elementsPriority != null && elementsPriority.ElementsShape == containers[x].block[y - 1].Element.Shape)
                                        continue;
                                }
                            }
                        }
                        //если нашли нужный элемент, то заканчиваем цикл
                        //elementShape = elementsShapeAndPriority[random].elementsShape;
                        elementfound = true;

                    } while (!elementfound && iteration > 0);

                    //Проверяем массив для микса, если там есть такой элемент, то отдаем его
                    if (elementsForMixList.Count > 0)
                    {
                        Element element = elementsForMixList.Find(item => item.Shape == elementsPriority.ElementsShape);
                        if (element != null)
                        {
                            containers[x].block[y].Element = element;
                            ElementsList.AddElement(element.Shape);
                            int index = elementsForMixList.IndexOf(element);
                            elementsForMixList.RemoveAt(index);
                            elementsPriority.limitOnAmountCreated--;
                            continue;
                        }

                    }
                    // иначе создаем новый элемент
                    containers[x].block[y].CreatElement(prefabElement, elementsPriority.ElementsShape, elementsPriority.elementsType);
                    elementsPriority.limitOnAmountCreated--;
                }
            }
        }

        //очищаем массив для микса от оставшихся элементов
        foreach (Element element in elementsForMixList)
        {
            Destroy(element.gameObject);
        }
        elementsForMixList.Clear();
    }

    public void Move(Block touchingBlock = null, Block destinationBlock = null)
    {
        //если остались ходы и нет подготовленных для действия инструментов
        if ((!Tasks.Instance.endGame && !InstrumentsManager.Instance.InstrumentPrepared && (Tasks.Instance.Moves - elementsForMoveList.Count) > 0) ||
            (touchingBlock == null && destinationBlock == null))
        {            
            Blocks blocks = new Blocks();
            blocks.block = new Block[2];
            blocks.block[0] = touchingBlock;
            blocks.block[1] = destinationBlock;
            elementsForMoveList.Add(blocks);

            if (!blockedForMove)
            {
                StartCoroutine(GridBlocks.Instance.MakeMove(touchingBlock, destinationBlock));
            }
        }
        else if ((touchingBlock != null && destinationBlock != null))
        {
            ExchangeElements(touchingBlock, destinationBlock);
        }
    }

    //выполняем ход
    //принимаем параметры - 1. Блок с которого передвигаем элемент 2. Блок к которому передвигаем элемент 
    private IEnumerator MakeMove(Block touchingBlock = null, Block destinationBlock = null)
    {
        blockedForMove = true;
        if (elementsForMoveList.Count > 0)
        {
            while (elementsForMoveList.Count > 0)
            {
                //удаляем подсказку
                bool gameHelpWasDell = HelpToPlayer.DellGameHelp();
                Debug.Log("gameHelpWasDell: " + gameHelpWasDell);

                Blocks blocks = elementsForMoveList[0];
                touchingBlock = blocks.block[0];
                destinationBlock = blocks.block[1];

                MainAnimator.Instance.ClearElementsForNextMove();

                //повторяем итерации заполнения и поиска совпадений, пока совпадения не будут найдены
                int iteration = 1;
                bool matchFound;
                bool makeActionElementsAfterMove = false;
                do
                {
                    //добавить проверку, что если нет destinationBlock, то не искать совпадающие линии

                    //ищем совпавшие линии 
                    matchFound = false;
                    blockFieldsList = CheckMatchingLine();
                    int CountElementsForMove = elementsForMoveList.Count;
                    if (blockFieldsList.Count > 0)
                    {
                        matchFound = true;
                        if (iteration != 1)
                        {
                            int countblockFields = 0;
                            while (countblockFields < blockFieldsList.Count)
                            {
                                iteration++;
                                yield return StartCoroutine(Filling(false, iteration));

                                blockFieldsList = CheckMatchingLine();
                                countblockFields = blockFieldsList.Count;
                                //прерывание в случае вмешательства игрока
                                if (CountElementsForMove < elementsForMoveList.Count)
                                    break;
                            }

                            if (CountElementsForMove < elementsForMoveList.Count)
                                break;

                            if (needFilling)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    iteration++;
                                    yield return StartCoroutine(Filling(false, iteration));
                                }
                                if (CountElementsForMove < elementsForMoveList.Count)
                                    break;
                                iteration++;
                                StartCoroutine(Filling(false, iteration));
                                if (CountElementsForMove < elementsForMoveList.Count)
                                    break;
                            }
                        }
                        else
                        {
                            makeActionElementsAfterMove = true;
                        }
                    }

                    //если в этом ходу не удаляли предыдущую подсказку
                    //создаем подсказку
                    if (!gameHelpWasDell && HelpToPlayer.CreateNextGameHelp())
                    {
                        Debug.Log("Создаем подсказку!");
                        //если создали, то прерываем процесс
                        blockFieldsList.Clear();
                        elementsForMoveList.Clear();
                        blockedForMove = false;
                        yield break;
                    }

                    // проверяем длинну совпавших линий для бонусов
                    List<List<Block>> findedBlockInLine = CountCollectedLine(blockFieldsList);

                    //ударяем по найденным блокам
                    foreach (Block blockField in blockFieldsList) { 
                        //сбрасывающий блок со сбрасываемым элементом
                        if (BlockCheck.ThisBlockDropingWithDropElement(blockField))
                        {
                            blockField.Hit(HitTypeEnum.Drop);
                        }
                        else
                        {
                            blockField.Hit();
                        }                        
                    }

                    if (iteration == 1)
                    {
                        //активируем активируемые элементы
                        if (touchingBlock != null && touchingBlock.Element != null && touchingBlock.Element.Activated)
                        {
                            touchingBlock.Hit();
                            yield return new WaitForSeconds(0.1f);
                            matchFound = true;
                            makeActionElementsAfterMove = true;
                        }

                        else if (destinationBlock != null && destinationBlock.Element != null && destinationBlock.Element.Activated)
                        {
                            destinationBlock.Hit();
                            yield return new WaitForSeconds(0.1f);
                            matchFound = true;
                            makeActionElementsAfterMove = true;
                        }
                        else if (blockFieldsList.Count == 0 || (!blockFieldsList.Contains(destinationBlock) && !blockFieldsList.Contains(touchingBlock)))
                        {
                            ExchangeElements(touchingBlock, destinationBlock);
                        }
                    }
                    
                    //создаем бонусы
                    if (iteration == 1 && matchFound && (touchingBlock != null || destinationBlock != null))
                    {
                        //минус ход
                        Tasks.Instance.SubMoves();

                        foreach (List<Block> item in findedBlockInLine)
                            Bonuses.Instance.CheckBonuses(item, touchingBlock, destinationBlock);
                        yield return new WaitForSeconds(0.07f);
                    }
                    else if (matchFound)
                    {
                        foreach (List<Block> item in findedBlockInLine)
                            Bonuses.Instance.CheckBonuses(item, null, null);
                        yield return new WaitForSeconds(0.07f);
                    }
                    
                    if (matchFound && iteration != 1)
                    {
                        if (CountElementsForMove < elementsForMoveList.Count)
                            break;
                        yield return new WaitForSeconds(0.1f);                        
                    }
                    blockFieldsList.Clear();
                    elementsForMoveList.Remove(blocks);
                    yield return StartCoroutine(Filling(true, iteration));
                    //создаем подсказку
                    if (HelpToPlayer.CreateNextGameHelp())
                    {
                        Debug.Log("Создаем подсказку!");
                        //если создали, то прерываем процесс
                        blockFieldsList.Clear();
                        elementsForMoveList.Clear();
                        blockedForMove = false;
                        yield break;
                    }

                    //если есть элементы в очереди на движение
                    if (elementsForMoveList.Count > 0)
                        break;

                    iteration++;
                } while (matchFound || needFilling);

                if (makeActionElementsAfterMove)
                {
                    //действия элементов после хода             
                    PerformActionElementsAfterMove();
                }

                blockFieldsList.Clear();
                //обновляем данные по коллекциям
                Tasks.Instance.UpdateAllGoal();
            }

            //если закончились ходы игрока и ходы всей игры
            if (elementsForMoveList.Count == 0 && Tasks.Instance.endGame)
            {
                MainSceneScript.Instance.CompleteGame();
                yield break;
            }

            //проверка, что остались доступные ходы
            List<ElementsForNextMove> elementsForNextMoveList;
            int iteration2 = 1;
            do
            {
                elementsForNextMoveList = CheckElementsForNextMove();
                //Если нет доступных ходов, то перемешиваем поле
                if (elementsForNextMoveList.Count == 0)
                    MixStandartElements();

                if (iteration2 > 4)
                {
                    Debug.Log("Поле было перемешано " + iteration2 + " раз, но доступные ходы так и небыли найдены");
                    break;
                }
                iteration2++;

            } while (elementsForNextMoveList.Count == 0);//повторяем проверку

            //пока первый
            //!!!! Переделать, находить лучший ход
            MainAnimator.Instance.ElementsForNextMove = elementsForNextMoveList[0];            
        }
        blockedForMove = false;
    }

    ////обработка сбрасывающих блоков
    //private void ProcessingDroppingBlock() {
    //    foreach (Block item in droppingBlockList)
    //    {
    //        //сбрасывающий блок со сбрасываемым элементом
    //        if (BlockCheck.ThisBlockDropingWithDropElement(item))
    //        {
    //            item.Hit(HitTypeEnum.Drop);
    //        }
    //    }                
    //}

    //действия элементов после хода
    private void PerformActionElementsAfterMove() {

        //предварительно собераем все элементы которые выполняют действие
        BaseElement[] findeObjects = FindObjectsOfType(typeof(BaseElement)) as BaseElement[]; //находим всех объекты с компонентом и создаём массив из них
        List<BaseElement> elementsForAction = new List<BaseElement>();

        foreach (BaseElement item in findeObjects)
        {
            if ( item.ActionAfterMove)
            {
                elementsForAction.Add(item);
            }
        }
        //выполняем действия
        foreach (BaseElement item in elementsForAction)
        {
            item.PerformActionAfterMove();
        }
    }

    //проверяет нахождение блока в массивах для обработки
    public bool BlockInProcessing(Block inBlock) {
        if (blockFieldsList.Contains(inBlock))
        {
            return true;
        }
        foreach (Blocks blocks in elementsForMoveList)
        {
            foreach (Block block in blocks.block)
            {
                if (block == inBlock)
                {

                }
            }
        }
        return false;
    }

    //проверка совпадений по всей сетке, возвращает массив найденых блоков
    private List<Block> CheckMatchingLine()
    {
        //matchFound = false;
        List<Block> blockFieldsToRemoveElement = new List<Block>();
        //List<Block> blockedBlockElement = new List<Block>();//блокирующий список

        //ищем
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                NeighboringBlocks neighboringBlocks = GetNeighboringBlocks(new Position(x, y));

                //проверяем горизонталь если не первый и не последний стоблец
                if (x != 0 && x != containers.GetLength(0) - 1)
                {
                    //проверяем что соседние блоки существуют
                    if (BlockCheck.ThisStandardBlockWithElement(neighboringBlocks.Left) && BlockCheck.ThisStandardBlockWithElement(neighboringBlocks.Right) && BlockCheck.ThisStandardBlockWithElement(containers[x].block[y]))
                    {
                        ////проверяем что элементы существуют в блоках
                        if (neighboringBlocks.Left.Element.speed == 0 && neighboringBlocks.Right.Element.speed == 0 && containers[x].block[y].Element.speed == 0)
                        {
                            //Если все три блока в линии создают линию
                            if (neighboringBlocks.Left.Element.CreateLine && neighboringBlocks.Right.Element.CreateLine && containers[x].block[y].Element.CreateLine)
                            {
                                //если все три елементы в линии равны
                                if (neighboringBlocks.Left.Element.Shape == neighboringBlocks.Right.Element.Shape
                                && neighboringBlocks.Right.Element.Shape == containers[x].block[y].Element.Shape)
                                {
                                
                                    //предварительно проверяем, что блоков нет в списке
                                    if (!blockFieldsToRemoveElement.Contains(neighboringBlocks.Left))
                                        blockFieldsToRemoveElement.Add(neighboringBlocks.Left);
                                    if (!blockFieldsToRemoveElement.Contains(neighboringBlocks.Right))
                                        blockFieldsToRemoveElement.Add(neighboringBlocks.Right);
                                    if (!blockFieldsToRemoveElement.Contains(containers[x].block[y]))
                                        blockFieldsToRemoveElement.Add(containers[x].block[y]);
                                }
                            }
                        }
                    }
                }

                //проверяем вертикаль если не первая и не последняя строка
                if (y != 0 && y != containers[x].block.GetLength(0) - 1)
                {
                    //Проверяем что соседние блоки существуют
                    if (BlockCheck.ThisStandardBlockWithElement(neighboringBlocks.Up) && BlockCheck.ThisStandardBlockWithElement(neighboringBlocks.Down) && BlockCheck.ThisStandardBlockWithElement(containers[x].block[y]))
                    {
                        if (neighboringBlocks.Up.Element.speed == 0 && neighboringBlocks.Down.Element.speed == 0 && containers[x].block[y].Element.speed == 0)
                        {
                            //Если все три блока в линии создают линию
                            if (neighboringBlocks.Up.Element.CreateLine && neighboringBlocks.Down.Element.CreateLine && containers[x].block[y].Element.CreateLine)
                            {
                                //Если все три елементы в линии равны
                                if (neighboringBlocks.Up.Element.Shape == neighboringBlocks.Down.Element.Shape
                                && neighboringBlocks.Down.Element.Shape == containers[x].block[y].Element.Shape)
                                {
                                    //предварительно проверяем, что блоков нет в списке
                                    if (!blockFieldsToRemoveElement.Contains(neighboringBlocks.Up))
                                        blockFieldsToRemoveElement.Add(neighboringBlocks.Up);
                                    if (!blockFieldsToRemoveElement.Contains(neighboringBlocks.Down))
                                        blockFieldsToRemoveElement.Add(neighboringBlocks.Down);
                                    if (!blockFieldsToRemoveElement.Contains(containers[x].block[y]))
                                        blockFieldsToRemoveElement.Add(containers[x].block[y]);
                                }
                            }
                        }
                    }
                }

                //сбрасывающий блок со сбрасываемым элементом
                if (BlockCheck.ThisBlockDropingWithDropElement(containers[x].block[y]))
                {
                    if (!blockFieldsToRemoveElement.Contains(containers[x].block[y]))
                        blockFieldsToRemoveElement.Add(containers[x].block[y]);
                }
            }
        }
        return blockFieldsToRemoveElement;
    }

    //поиск линий совпавших элементов более трех
    private List<List<Block>> CountCollectedLine(List<Block> blockFields)
    {
        //проверяем длинну совпавших линий для бонусов
        //копируем массив
        List<Block> listForCheck = new List<Block>();
        foreach (Block item in blockFields)
            listForCheck.Add(item);

        List<List<Block>> listBlocksInLine = new List<List<Block>>();//возвращаемый массив
        List<Block> blocksInLine;//одна линия
        //List<Block> findedBlocks = new List<Block>();//промежуточный массив

        //bool repit;
        do
        {
            blocksInLine = new List<Block>();
            foreach (Block blockField in listForCheck)
            {
                //добавляем первые элементы                
                if (BlockCheck.ThisStandardBlockWithStandartElementCanMove(blockField))
                {
                    NeighboringBlocks neighboringBlocks = GetNeighboringBlocks(blockField.PositionInGrid);
                    foreach (Block neighboringBlock in neighboringBlocks.allBlockField)
                    {
                        //если блок находится по соседству и в нем такой же элемент
                        if (listForCheck.Contains(neighboringBlock) && BlockCheck.ThisStandardBlockWithStandartElementCanMove(neighboringBlock) && blockField.Element.Shape == neighboringBlock.Element.Shape)
                        {
                            //и если противоположный блок имеет такой же элемент
                            Block OppositeBlock = neighboringBlocks.GetOppositeBlock(neighboringBlock);
                            if (listForCheck.Contains(OppositeBlock) && BlockCheck.ThisStandardBlockWithStandartElementCanMove(OppositeBlock) && blockField.Element.Shape == OppositeBlock.Element.Shape)
                            {
                                if (blocksInLine.Count == 0)
                                {
                                    //проверяем что блок еще не добавили в массив
                                    //добавляем все три блока и прерываем
                                    //if (!blocksInLine.Contains(blockField))
                                    blocksInLine.Add(blockField);
                                    //проверяем что блок еще не добавили в массив
                                    //if (!blocksInLine.Contains(neighboringBlock))
                                    blocksInLine.Add(neighboringBlock);
                                    //и сразу добавляем противоположный блок
                                    //if (!blocksInLine.Contains(OppositeBlock))
                                    blocksInLine.Add(OppositeBlock);
                                }
                                else
                                {
                                    bool blockFieldInLine = blocksInLine.Contains(blockField);
                                    bool neighboringBlockInLine = blocksInLine.Contains(neighboringBlock);
                                    bool OppositeBlockInLine = blocksInLine.Contains(OppositeBlock);

                                    if (blockFieldInLine || neighboringBlockInLine || OppositeBlockInLine)
                                    {
                                        //проверяем что блок еще не добавили в массив
                                        //добавляем все три блока и прерываем
                                        if (!blockFieldInLine)
                                            blocksInLine.Add(blockField);
                                        //проверяем что блок еще не добавили в массив
                                        if (!neighboringBlockInLine)
                                            blocksInLine.Add(neighboringBlock);
                                        //и сразу добавляем противоположный блок
                                        if (!OppositeBlockInLine)
                                            blocksInLine.Add(OppositeBlock);
                                    }
                                }


                                //break;
                            }
                        }
                    }
                }
                ////если добавили блоки
                //if (blocksInLine.Count > 0)
                //    break;
            }

            if (blocksInLine.Count > 3)
            {
                Debug.Log("Найдено блоков в линии:" + blocksInLine.Count);

                listBlocksInLine.Add(blocksInLine);
                //repit = true;
            }

            //Удаляем из основного массива
            foreach (Block item in blocksInLine)
            {
                listForCheck.Remove(item);
            }

        } while (blocksInLine.Count > 0);

        //возвращаем блоки которые составили линию больше 3
        return listBlocksInLine;
    }

    //возвращает массив элементов которые могут составить линию в следующем ходу
    //можно использовать как подсказку игроку
    public List<ElementsForNextMove> CheckElementsForNextMove()
    {
        List<ElementsForNextMove> ElementsForNextMoveList = new List<ElementsForNextMove>();
        ElementsForNextMove elementsForNextMove = new ElementsForNextMove();

        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если стандартный блок и в нем есть элемент
                if (BlockCheck.ThisStandardBlockWithElement(containers[x].block[y]))
                {
                    for (int j = 0; j < 2; j++)
                    {
                        elementsForNextMove.elementsList.Add(containers[x].block[y].Element);
                        bool elementForMoveFound = false;

                        for (int i = -1; i < 2; i = i + 2)
                        {
                            int iterationX = 0;
                            int iterationY = 0;
                            bool elementFound;
                            do
                            {
                                elementFound = false;
                                if (j == 0)
                                    iterationX = iterationX + i;
                                else if (j == 1)
                                    iterationY = iterationY + i;

                                int posX = x + iterationX;
                                int posY = y + iterationY;
                                Block curBlock = GetBlock(posX, posY);

                                if (ElementsMatch(curBlock, containers[x].block[y]) && curBlock.Element != elementsForNextMove.elementsList.Last())
                                {
                                    elementsForNextMove.elementsList.Add(curBlock.Element);
                                    elementFound = true;
                                    //Debug.Log("В линии" + curBlock);
                                    continue;
                                }

                                if (elementForMoveFound)
                                    continue;
                                if (curBlock == null)//если блок не существует
                                    continue;
                                if (curBlock != null && curBlock.Type != BlockTypeEnum.Standard)//если тип не стандартный
                                    continue;
                                if (BlockCheck.ThisBlockWithElementCantMove(curBlock))//если элемент заблокирован для движения
                                    continue;

                                Position position = new Position(posX, posY);
                                NeighboringBlocks neighboringBlocks = GetNeighboringBlocks(position);

                                //пересобираем массив для правильной последовательности проверки элементов
                                Block[] blocks;
                                if (j == 1)
                                {
                                    blocks = new Block[4] { neighboringBlocks.Left, neighboringBlocks.Right, neighboringBlocks.Up, neighboringBlocks.Down };
                                }
                                else
                                {
                                    blocks = new Block[4] { neighboringBlocks.Up, neighboringBlocks.Down, neighboringBlocks.Left, neighboringBlocks.Right };
                                }

                                foreach (Block block in blocks)
                                {
                                    //ищем элемент для смещения
                                    if (ElementsMatch(block, containers[x].block[y]) && block.Element != elementsForNextMove.elementsList.Last() && !block.Element.LockedForMove)
                                    {
                                        elementsForNextMove.elementsList.Add(block.Element);
                                        elementsForNextMove.elementForMove = block.Element;
                                        elementsForNextMove.blockElementForMove = block;
                                        elementsForNextMove.directionForMove = neighboringBlocks.GetOppositeDirection(block);
                                        elementsForNextMove.oppositeDirectionForMove = neighboringBlocks.GetDirection(block);
                                        elementsForNextMove.targetBlock = curBlock;
                                        elementForMoveFound = true;
                                        elementFound = true;
                                        //Debug.Log("Смещенный" + block);
                                        break;
                                    }
                                }

                            } while (elementFound);
                        }
                        //если количество найденных элементов больше двух то ищем следующую линию
                        if (elementsForNextMove.elementsList.Count > 2)
                        {
                            ElementsForNextMoveList.Add(elementsForNextMove);
                            elementsForNextMove = new ElementsForNextMove();
                            break;
                        }
                        else
                            elementsForNextMove.elementsList.Clear();
                    }
                }
                ////если количество найденных элементов больше двух то останавливаем поиск
                //if (elementsForNextMove.elementsList.Count > 2)
                //    break;
            }
            ////если количество найденных элементов больше двух то останавливаем поиск
            //if (elementsForNextMove.elementsList.Count > 2)
            //    break;
        }

        return ElementsForNextMoveList;
    }

    //перемешать стандартные элементы
    public void MixStandartElements()
    {
        List<ElementsPriority> listPriority = new List<ElementsPriority>();
        elementsForMixList.Clear();

        //задать приоритеты
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                if (BlockCheck.ThisStandardBlockWithStandartElementCanMove(containers[x].block[y]))
                {
                    ElementsPriority elementsSAndP = listPriority.Find(item => item.ElementsShape == containers[x].block[y].Element.Shape);
                    if (elementsSAndP == null)
                    {
                        listPriority.Add(new ElementsPriority(containers[x].block[y].Element.Shape, ElementsTypeEnum.Standard, 1, 1));
                    }
                    else
                    {
                        int index = listPriority.IndexOf(elementsSAndP);
                        listPriority[index].priority++;
                        listPriority[index].limitOnAmountCreated++;
                    }
                    elementsForMixList.Add(containers[x].block[y].Element);
                    containers[x].block[y].Element = null;
                }
            }
        }

        //проверить весь массив, если нет элементов вхождений которого более 2, то искуственно увеличиваем приоритет у одного из элементов в два раза
        bool increasePriority = true;
        foreach (ElementsPriority item in listPriority)
        {
            if (item.priority > 2)
            {
                increasePriority = false;
                break;
            }
        }

        if (increasePriority)
        {
            //мешаем массив
            SupportFunctions.MixArray(listPriority);
            for (int i = 0; i < listPriority.Count; i++)
            {
                //ищем нет ли такого элемента в заданиях
                bool found = false;
                foreach (Target item in Tasks.Instance.targets)
                {
                    if (item.elementsShape == listPriority[i].ElementsShape)
                    {
                        found = true;
                        break;
                    }
                }
                //если не нашли
                if (!found)
                {
                    listPriority[i].priority++;
                    listPriority[i].limitOnAmountCreated++;
                    break;
                }                
            }            
        }

        //перезаполняем новыми элементами
        StartFilling(listPriority);
    }

    //элементы совпадают
    private bool ElementsMatch(Block block1, Block block2)
    {

        if (block1 != null && block2 != null)
        {
            if (block1 != block2)
            {
                if (block1.Type == block2.Type)
                {
                    if (block1.Element != null && block2.Element != null)
                    {
                        if (block1.Element.Shape == block2.Element.Shape)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    
    //возвращает блок на указанной позиции
    public Block GetBlock(Position position)
    {
        if (position.posX >= 0 && position.posX < containers.GetLength(0) &&
            position.posY >= 0 && position.posY < containers[position.posX].block.GetLength(0))
        {
            return containers[position.posX].block[position.posY];
        }

        return null;
    }

    //возвращает блок на указанной позиции
    public Block GetBlock(int posX, int posY)
    {
        if (posX >= 0 && posX < containers.GetLength(0) &&
            posY >= 0 && posY < containers[posX].block.GetLength(0))
        {
            return containers[posX].block[posY];
        }

        return null;
    }

    //возвращает блок с указанным элементом
    public Block GetBlock(Element element)
    {
        //ищем объект
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если нашли наш блок то возвращаем его позицию
                if (containers[x].block[y] != null && containers[x].block[y].Element == element)
                    return containers[x].block[y];
            }
        }
        return null;
    }

    //возвращает блок с указанным элементом на заднем плане
    public Block GetBlock(BehindElement behindElement)
    {
        //ищем объект
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если нашли наш блок то возвращаем его позицию
                if (containers[x].block[y] != null && containers[x].block[y].BehindElement == behindElement)
                    return containers[x].block[y];
            }
        }
        return null;
    }
    
    //определяем соседние блоки
    public NeighboringBlocks GetNeighboringBlocks(Position position)
    {
        Block upBlock = null;
        Block DownBlock = null;
        Block LeftBlock = null;
        Block RightBlock = null;

        if (position != null)
        {
            //определяем блоки
            if (position.posX - 1 >= 0)
                LeftBlock = containers[position.posX - 1].block[position.posY];
            if (position.posY - 1 >= 0)
                DownBlock = containers[position.posX].block[position.posY - 1];
            if (position.posX + 1 < containers.GetLength(0))
                RightBlock = containers[position.posX + 1].block[position.posY];
            if (position.posY + 1 < containers[position.posX].block.GetLength(0))
                upBlock = containers[position.posX].block[position.posY + 1];
        }

        return new NeighboringBlocks(upBlock, DownBlock, LeftBlock, RightBlock);
    }

    //определяем блоки вокруг
    public Block[] GetAroundBlocks(Position position)
    {
        Block[] blocks = new Block[8];

        if (position != null)
        {
            int iteration = 0;
            int posX;
            int posY;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    posX = position.posX - 1 + x;
                    posY = position.posY - 1 + y;
                    if (posX >= 0 && posX < containers.GetLength(0) &&
                        posY >= 0 && posY < containers[posX].block.GetLength(0))
                    {
                        if (position.posX == posX && position.posY == posY)
                        {
                            continue;
                        }
                        blocks[iteration] = containers[posX].block[posY];
                        iteration++;
                    }
                }
            }
        }
        return blocks;
    }

    //определяем блоки крест на крест кроме центрального
    public Block[] GetAllCrossBlocks(Position position)
    {
        Block[] blocks = new Block[0];

        if (position != null)
        {
            blocks = new Block[containers.GetLength(0) + containers[position.posX].block.GetLength(0) - 2];
            int iteration = 0;

            for (int x = 0; x < containers.GetLength(0); x++)
            {
                if (position.posX != x)
                {
                    blocks[iteration] = containers[x].block[position.posY];
                    iteration++;
                }
            }

            for (int y = 0; y < containers[position.posX].block.GetLength(0); y++)
            {
                if (position.posY != y)
                {
                    blocks[iteration] = containers[position.posX].block[y];
                    iteration++;
                }
            }
        }
        return blocks;
    }

    //возвращает все блоки со стандартными элементами в сетке
    public Block[] GetAllBlocksWithStandartElements()
    {
        List<Block> blocks = new List<Block>();

        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если блок существует по данному адресу и в нем есть стандартный элемент
                if (BlockCheck.ThisBlockWithStandartElement(containers[x].block[y]))
                {
                    blocks.Add(containers[x].block[y]);
                }
            }
        }
        return blocks.ToArray();
    }

    //берем все блоки в указанном радиусе, за минусом центрального блока и блоков на углах
    public Block[] GetBlocksForHit(Position position, int radius)
    {
        int diameter = radius * 2;
        //Blocks[] containers = GridBlocks.Instance.containers;
        Block[] blocks = new Block[(diameter + 1) * (diameter + 1) - 5];

        if (position.posX != -1 || position.posY != -1)
        {
            int iteration = 0;
            int posX;
            int posY;
            for (int x = 0; x < diameter + 1; x++)
            {
                for (int y = 0; y < diameter + 1; y++)
                {
                    //пропускаем все не нужные блоки
                    if ((x == 0 && y == 0) || (x == 0 && y == diameter) || (x == diameter && y == diameter) || (x == diameter && y == 0) || (x == radius && y == radius))
                    {
                        continue;
                    }

                    posX = position.posX - radius + x;
                    posY = position.posY - radius + y;
                    if (posX >= 0 && posX < containers.GetLength(0) &&
                        posY >= 0 && posY < containers[posX].block.GetLength(0))
                    {
                        blocks[iteration] = containers[posX].block[posY];
                        iteration++;
                    }
                }
            }
        }
        return blocks;
    }

    //процедура замены элементов между блоками
    public void ExchangeElements(Block Block1, Block Block2)
    {
        if (Block1 != null && Block2 != null)
        {
            Element element1 = Block1.Element;
            Block1.Element = Block2.Element;
            Block2.Element = element1;
        }

    }

    //заполнение сетки элементами
    public IEnumerator Filling(bool breakSpeed = true, int iteration = 1)
    {
        //нужна еще итерация
        needFilling = false;
        bool needIteration = true;
        ElementsPriority elementPriorit;
        float speed = 0.08f;
        Block dropBlock = null;

        needIteration = false;
        for (int y = 0; y < containers[0].block.GetLength(0); y++)
        {
            dropBlock = null;
            yield return new WaitForSeconds(0.0165f);
            //начинаем со второй строки
            for (int x = 0; x < containers.GetLength(0); x++)
            {
                Block currentBlock = containers[x].block[y];
                bool createdElement = false;
                if (currentBlock != null && !BlockInProcessing(currentBlock))
                {
                    //если пустой блок и не умеет генерировать элемент, идем дальше
                    if (BlockCheck.ThisBlockWithoutElement(currentBlock) && !currentBlock.GeneratorElements)
                    { continue; }

                    //если пустой блок и умеет генерировать элемент, то предварительно создаем случайный элемент
                    else if (BlockCheck.ThisBlockWithoutElement(currentBlock) && currentBlock.GeneratorElements)
                    {
                        elementPriorit = ProportionalWheelSelection.SelectElement(elementsPriorityList);
                        if (elementPriorit == null)
                        {
                            Debug.Log("Не нашли элемент для заполнения!");
                            needIteration = false;
                            yield break;
                        }
                        currentBlock.CreatElement(prefabElement, elementPriorit.ElementsShape, elementPriorit.elementsType);
                        elementPriorit.limitOnAmountCreated--;
                        MainAnimator.Instance.AddElementForSmoothMove(currentBlock.Element.thisTransform, new Vector3(currentBlock.thisTransform.position.x, currentBlock.thisTransform.position.y - 0.2f, currentBlock.thisTransform.position.z), 2, SmoothEnum.InLineWithOneSpeed, smoothTime: speed + iteration * 0.0009f, addToQueue: true);

                        needIteration = true;
                        createdElement = true;
                    }

                    //если текущий элемент заблокирован для движения, то переходим к следующему
                    else if (BlockCheck.ThisBlockWithElementCantMove(currentBlock))
                    { continue; }

                    if (y > 0)
                    {
                        //ищем место для смещения
                        //если нижний блок не имеет элемента, то смещаем к нему
                        if (BlockCheck.ThisStandardBlockWithoutElement(containers[x].block[y - 1]))
                        {
                            Block newBlock = containers[x].block[y - 1];
                            int distance = 1;
                            for (int i = y - 2; i >= 0; i--)
                            {
                                if (BlockCheck.ThisStandardBlockWithoutElement(containers[x].block[i]))
                                {
                                    newBlock = containers[x].block[i];
                                    distance++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            ExchangeElements(currentBlock, newBlock);
                            MainAnimator.Instance.AddElementForSmoothMove(newBlock.Element.thisTransform, new Vector3(newBlock.thisTransform.position.x, newBlock.thisTransform.position.y - 0.1f, newBlock.thisTransform.position.z), 2, SmoothEnum.InLineWithAcceleration, smoothTime: speed + distance  * 0.009f, addToQueue: !createdElement);
                            needIteration = true;
                            dropBlock = containers[x].block[y];
                            continue;
                        }
                        //иначе, проверяем правый нижний блок по диагонали, при условии, что справа нет элементов в блоках
                        if ((x < containers.GetLength(0) - 1) && BlockCheck.ThisStandardBlockWithoutElement(containers[x + 1].block[y - 1]))
                        {
                            bool moveRight = false;
                            for (int i = y; i < containers[x].block.GetLength(0); i++)
                            {
                                if (dropBlock != null && containers[x + 1].block[i] == dropBlock)
                                {
                                    moveRight = false;
                                    break;
                                }
                                if (BlockCheck.ThisBlockWithElementCanMove(containers[x + 1].block[i]))
                                {
                                    moveRight = false;
                                    break;
                                }
                                if (BlockCheck.ThisBlockWithElementCantMove(containers[x + 1].block[i]) || containers[x + 1].block[i] == null)
                                {
                                    moveRight = true;
                                    break;
                                }
                                if (containers[x + 1].block[i] != null && containers[x + 1].block[i].GeneratorElements == true)
                                {
                                    moveRight = false;
                                    break;
                                }
                                else
                                    moveRight = true;
                            }

                            if (moveRight)
                            {
                                SmoothEnum smoothEnum;
                                if (currentBlock.Element.speed > 0)
                                {
                                    smoothEnum = SmoothEnum.InLineWithOneSpeed;
                                }
                                else
                                {
                                    smoothEnum = SmoothEnum.InLineWithAcceleration;
                                }

                                ExchangeElements(currentBlock, containers[x + 1].block[y - 1]);
                                MainAnimator.Instance.AddElementForSmoothMove(containers[x + 1].block[y - 1].Element.thisTransform, new Vector3(containers[x + 1].block[y - 1].thisTransform.position.x + 0.1f, containers[x + 1].block[y - 1].thisTransform.position.y - 0.1f, containers[x + 1].block[y - 1].thisTransform.position.z), 2, smoothEnum, smoothTime: speed + 1 * 0.015f, addToQueue: !createdElement);
                                if (y > 1 && (((x < containers.GetLength(0) - 2) && BlockCheck.ThisStandardBlockWithoutElement(containers[x + 2].block[y - 2])) || BlockCheck.ThisStandardBlockWithoutElement(containers[x + 1].block[y - 2])))
                                {
                                    containers[x + 1].block[y - 1].Element.speed = 1;
                                }
                                else
                                {
                                    containers[x + 1].block[y - 1].Element.speed = 0;
                                }                                
                                needIteration = true;
                                continue;
                            }
                        }
                        //иначе, проверяем левый нижний блок по диагонали , при условии, что слева нет элементов в блоках
                        if ((x > 0) && BlockCheck.ThisStandardBlockWithoutElement(containers[x - 1].block[y - 1]))
                        {
                            bool moveLeft = false;
                            for (int i = y; i < containers[x].block.GetLength(0); i++)
                            {
                                if (dropBlock != null && containers[x - 1].block[i] == dropBlock)
                                {
                                    moveLeft = false;
                                    break;
                                }
                                if (BlockCheck.ThisBlockWithElementCanMove(containers[x - 1].block[i]))
                                {
                                    moveLeft = false;
                                    break;
                                }
                                if (BlockCheck.ThisBlockWithElementCantMove(containers[x - 1].block[i]) || containers[x - 1].block[i] == null)
                                {
                                    moveLeft = true;
                                    break;
                                }
                                if (containers[x - 1].block[i] != null && containers[x - 1].block[i].GeneratorElements == true)
                                {
                                    moveLeft = false;
                                    break;
                                }
                                else
                                    moveLeft = true;
                            }

                            if (moveLeft)
                            {
                                SmoothEnum smoothEnum;
                                if (currentBlock.Element.speed > 0)
                                {
                                    smoothEnum = SmoothEnum.InLineWithOneSpeed;
                                }
                                else
                                {
                                    smoothEnum = SmoothEnum.InLineWithAcceleration;
                                }
                                ExchangeElements(currentBlock, containers[x - 1].block[y - 1]);
                                MainAnimator.Instance.AddElementForSmoothMove(containers[x - 1].block[y - 1].Element.thisTransform, new Vector3(containers[x - 1].block[y - 1].thisTransform.position.x - 0.1f, containers[x - 1].block[y - 1].thisTransform.position.y - 0.1f, containers[x - 1].block[y - 1].thisTransform.position.z), 2, smoothEnum, smoothTime: speed + 1 * 0.015f, addToQueue: !createdElement);
                                if (y > 1 && ((x > 1 && BlockCheck.ThisStandardBlockWithoutElement(containers[x - 2].block[y - 2])) || BlockCheck.ThisStandardBlockWithoutElement(containers[x - 1].block[y - 2])))
                                {
                                    containers[x - 1].block[y - 1].Element.speed = 1;
                                }
                                else
                                {
                                    containers[x - 1].block[y - 1].Element.speed = 0;
                                }
                                needIteration = true;
                                continue;
                            }
                        }
                    }
                    if (currentBlock.Element.speed > 0)
                    {
                        needIteration = true;
                        currentBlock.Element.speed = 0;
                    }
                }
            }
        }
        if (needIteration)
        {
            //Debug.Log(Time.deltaTime);
            //yield return new WaitForSeconds(0.15f - Time.deltaTime);
            needFilling = true;
            //yield break;
        }
    }
    
    //передаем данные о на стройках в xml формате
    public Type GetClassName()
    {
        return this.GetType();
    }

    public XElement GetXElement()
    {
        XElement gridXElement = new XElement(this.GetType().ToString());

        gridXElement.Add(new XElement("blockSize", blockSize));

        //записываем размер сетки
        gridXElement.Add(new XElement("XSize", containers.GetLength(0)));
        gridXElement.Add(new XElement("YSize", containers[0].block.GetLength(0)));

        //записываем все внешности стандартных элементов
        //gridXElement.Add(new XElement("shapeSize", elementsShape.Count));
        XElement elementsShapeXElement = new XElement("elementsShape");
        foreach (ElementsPriority shapeAndPriority in elementsPriorityList)
        {
            XAttribute shape = new XAttribute("shape", shapeAndPriority.ElementsShape);
            XAttribute type = new XAttribute("type", shapeAndPriority.elementsType);
            XAttribute priority = new XAttribute("priority", shapeAndPriority.priority);
            XElement shapeXElement = new XElement("shapeAndPriority", shape, type, priority);
            elementsShapeXElement.Add(shapeXElement);
        }
        gridXElement.Add(elementsShapeXElement);

        //записываем все блоки
        XElement blocksXElement = new XElement("blocks");
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если блок существует по данному адресу
                if (containers[x].block[y] != null)
                {
                    XAttribute posX = new XAttribute("posX", x);
                    XAttribute posY = new XAttribute("posY", y);
                    XAttribute blockType = new XAttribute("blockType", containers[x].block[y].Type);
                    XAttribute generatorElements = new XAttribute("generatorElements", containers[x].block[y].GeneratorElements);
                    XAttribute dropping = new XAttribute("dropping", containers[x].block[y].Dropping);
                    XAttribute behindElementsType = new XAttribute("behindElementsType", BehindElementsTypeEnum.Empty);
                    XAttribute behindElementsShape = new XAttribute("behindElementsShape", BehindElementsShapeEnum.Empty);
                    XAttribute elementType = new XAttribute("elementType", ElementsTypeEnum.Empty);
                    XAttribute elementShape = new XAttribute("elementShape", ElementsShapeEnum.Empty);
                    XAttribute dopShape = new XAttribute("dopShape", ElementsShapeEnum.Empty);
                    XAttribute blockingElementType = new XAttribute("blockingElementType", BlockingElementsTypeEnum.Empty);
                    XAttribute blockingElementShape = new XAttribute("blockingElementShape", BlockingElementsShapeEnum.Empty);

                    //если в блоке есть элемент на заднем фоне
                    if (containers[x].block[y].BehindElement != null)
                    {
                        behindElementsType = new XAttribute("behindElementsType", containers[x].block[y].BehindElement.Type);
                        behindElementsShape = new XAttribute("behindElementsShape", containers[x].block[y].BehindElement.Shape);
                    }

                    //если в блоке есть элемент
                    if (containers[x].block[y].Element != null)
                    {
                        elementType = new XAttribute("elementType", containers[x].block[y].Element.Type);
                        elementShape = new XAttribute("elementShape", containers[x].block[y].Element.Shape);
                        dopShape = new XAttribute("dopShape", containers[x].block[y].Element.CollectShape);

                        //если в в элементе есть блокирующий элемент
                        if (containers[x].block[y].Element.BlockingElement != null)
                        {
                            blockingElementType = new XAttribute("blockingElementType", containers[x].block[y].Element.BlockingElement.Type);
                            blockingElementShape = new XAttribute("blockingElementShape", containers[x].block[y].Element.BlockingElement.Shape);
                        }
                    }

                    XElement blockXElement = new XElement("block", posX, posY, blockType, generatorElements, dropping, behindElementsType, behindElementsShape, elementType, elementShape, dopShape, blockingElementType, blockingElementShape);
                    blocksXElement.Add(blockXElement);
                }
            }
        }
        gridXElement.Add(blocksXElement);

        return gridXElement;
    }

    public void RecoverFromXElement(XElement gridXElement)
    {
        //Сбрасываем значения
        blockedForMove = false;
        elementsForMixList = new List<Element>();
        elementsForMoveList = new List<Blocks>();
        needFilling = false;
        elementsPriorityList = new List<ElementsPriority>();

        //удаляем все блоки
        string blocksName = "Blocks";        
        Transform blocksTransform = transform.Find(blocksName);
        if (blocksTransform != null)
        {
            DestroyImmediate(blocksTransform.gameObject);
        }
        ElementsList.ClearElementsOnField();
        GameObject blocks;
        blocks = new GameObject();
        blocks.name = blocksName;
        blocks.transform.parent = transform;        

        this.blockSize = float.Parse(gridXElement.Element("blockSize").Value);
        int XSize = int.Parse(gridXElement.Element("XSize").Value);
        int YSize = int.Parse(gridXElement.Element("YSize").Value);
        
        //восстанавливаем приоритеты
        foreach (XElement shapeAndPriority in gridXElement.Element("elementsShape").Elements("shapeAndPriority"))
        {
            ElementsShapeEnum shape = (ElementsShapeEnum)Enum.Parse(typeof(ElementsShapeEnum), shapeAndPriority.Attribute("shape").Value);
            ElementsTypeEnum type = (ElementsTypeEnum)Enum.Parse(typeof(ElementsTypeEnum), shapeAndPriority.Attribute("type").Value);
            int priority = int.Parse(shapeAndPriority.Attribute("priority").Value);
            ElementsPriority curShapeAndPriority = new ElementsPriority(shape, type, priority);
            this.elementsPriorityList.Add(curShapeAndPriority);
        }

        //создаем массив массивов блоков
        this.containers = new Blocks[XSize];
        for (int i = 0; i < containers.Length; i++)
        {
            containers[i] = new Blocks() { block = new Block[YSize] };
        }

        //восстанавливаем все блоки и элементы
        foreach (XElement block in gridXElement.Element("blocks").Elements("block"))
        {
            int posX = int.Parse(block.Attribute("posX").Value);
            int posY = int.Parse(block.Attribute("posY").Value);
            bool generatorElements = bool.Parse(block.Attribute("generatorElements").Value);
            bool dropping = false;
            try{dropping = bool.Parse(block.Attribute("dropping").Value);}catch (Exception){}                        
            BlockTypeEnum blockType = (BlockTypeEnum)Enum.Parse(typeof(BlockTypeEnum), block.Attribute("blockType").Value);
            BehindElementsTypeEnum behindElementsType = (BehindElementsTypeEnum)Enum.Parse(typeof(BehindElementsTypeEnum), block.Attribute("behindElementsType").Value);
            AllShapeEnum behindElementsShape = (AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), block.Attribute("behindElementsShape").Value);
            ElementsTypeEnum elementType = (ElementsTypeEnum)Enum.Parse(typeof(ElementsTypeEnum), block.Attribute("elementType").Value);
            AllShapeEnum elementShape = (AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), block.Attribute("elementShape").Value);

            AllShapeEnum dopShape = AllShapeEnum.Empty;
            try
            {
                dopShape = (AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), block.Attribute("dopShape").Value);
            }
            catch (Exception)
            {
            }
                        
            BlockingElementsTypeEnum blockingElementType = (BlockingElementsTypeEnum)Enum.Parse(typeof(BlockingElementsTypeEnum), block.Attribute("blockingElementType").Value);
            AllShapeEnum blockingElementShape = (AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), block.Attribute("blockingElementShape").Value);

            if (blockType != BlockTypeEnum.Empty)
            {
                Vector3 position = new Vector3(this.transform.localPosition.x + posX * blockSize, this.transform.localPosition.y + posY * blockSize, this.transform.localPosition.z);

                //создаем блок
                GameObject blockGameObject = Instantiate(prefabBlock, position, Quaternion.identity);
                blockGameObject.name = "Block_" + posX + "_" + posY;
                blockGameObject.transform.parent = blocks.transform;

                Block blockField = blockGameObject.GetComponent<Block>();
                blockField.GeneratorElements = generatorElements;
                blockField.Dropping = dropping;
                blockField.Type = blockType;
                blockField.PositionInGrid = new Position(posX, posY);
                //добавляем блок в массив блоков
                containers[posX].block[posY] = blockField;

                //создаем элемент на заднем фоне
                if (behindElementsType != BehindElementsTypeEnum.Empty)
                {
                    blockField.CreatBehindElement(prefabElement, behindElementsShape, behindElementsType);
                }
                //создаем элемент
                if (elementType != ElementsTypeEnum.Empty)
                {
                    blockField.CreatElement(prefabElement, elementShape, elementType, dopShape);
                    //создаем блокирующий элемент
                    if (blockingElementType != BlockingElementsTypeEnum.Empty)
                    {
                        blockField.Element.CreatBlockingElement(prefabBlockingWall, blockingElementShape, blockingElementType);
                    }
                }
            }
        }
    }    

#if UNITY_EDITOR
    //устанавливаем блок на определенную позицию в сетке
    //возвращает истину если удалось поставить элемент на новую позицию?
    public bool AddBlockToPosition(Block Block, Position newPosition)
    {

        //находим текущая позиция в сетке
        Position oldPosition = Block.PositionInGrid;

        //если позиция в рамках размера нашей сетки
        if (newPosition.posX >= 0 && newPosition.posX < containers.GetLength(0) &&
            newPosition.posY >= 0 && newPosition.posY < containers[newPosition.posX].block.GetLength(0))
        {
            //если позиция занята, то выходим
            if (GetBlock(newPosition) != null)
            {
                return false;
            }

            //если не тажа позиция на которой блок уже стоит
            if (oldPosition == null || (oldPosition != null && (newPosition.posX != oldPosition.posX || newPosition.posY != oldPosition.posY)))
            {
                //и удаляем со старой если такая была
                if (oldPosition != null)
                {
                    DelBlock(Block);
                    //Debug.LogAssertion("1 Удалили блок с позиции: " + oldPosition.posX + " " + oldPosition.posY);
                }

                //ставим блок на новую позицию
                containers[newPosition.posX].block[newPosition.posY] = Block;
                Block.PositionInGrid = newPosition;
                //Debug.LogAssertion("Поставили блок на позицию: " + newPosition.posX + " " + newPosition.posY);

                //переместить блок на новую позицию в соответствии с позицией в сетке
                //Block.transform.parent = this.transform;
                Block.transform.position = new Vector3(this.transform.localPosition.x + newPosition.posX * blockSize, this.transform.localPosition.y + newPosition.posY * blockSize, this.transform.localPosition.z);
                Block.name = "Block_" + newPosition.posX + "_" + newPosition.posY;
                //Block.transform.parent = this.transform;
                //Block.transform.SetParent(this.transform, false);

                return true;
            }
        }
        else if (oldPosition != null)//иначе значит элемент вышел за пределы сетки и его нужно удалить из нее
        {
            DelBlock(Block);
            //Debug.LogAssertion("2 Удалили блок с позиции: " + oldPosition.posX + " " + oldPosition.posY);
            Block.name = "Block_Free";
            //Vector3 savePosition =this.transform.position + Block.transform.localPosition;
            //Block.transform.parent = null;
            //Block.transform.position = savePosition;
            //Block.transform.SetParent(null, false);
        }

        return false;
    }

    private void SettingBlocksOnPosition()
    {
        //работаем только с объектом у которого есть BlockField
        foreach (var obj in Selection.GetFiltered(typeof(Block), SelectionMode.Assets))
        {
            //записать данные о позиции объекта
            Vector3 pos = (obj as Block).transform.position;
            Vector3 posGrid = transform.position;
            pos.x = Mathf.CeilToInt(pos.x / blockSize) * blockSize + (float)(posGrid.x - Math.Truncate(posGrid.x));
            pos.y = Mathf.CeilToInt(pos.y / blockSize) * blockSize + (float)(posGrid.y - Math.Truncate(posGrid.y));
            pos.z = posGrid.z;


            //выполняем если изменилась позиция
            if (pos != (obj as Block).transform.position)
            {
                //Определяем на какой позиции он должен быть 
                Position newPosition = new Position((int)(pos.x - posGrid.x), (int)(pos.y - posGrid.y));
                //Vector3 posAfterAdaptation = (obj as BlockField).transform.position;
                //newPosition.posX = (int)(pos.x - posGrid.x);
                //newPosition.posY = (int)(pos.y - posGrid.y);

                //если позиция не занята в сетке другим блоком
                if (GetBlock(newPosition) == null || GetBlock(newPosition) == (obj as Block))
                {
                    //добавляем блок в сетку
                    AddBlockToPosition(obj as Block, newPosition);
                    //если блок есть в сетке, то двигаем его по ней
                    if (SearchBlock(obj as Block))
                    {
                        (obj as Block).transform.position = pos;
                    }

                }


            }
        }

        EditorUtility.SetDirty(this);
    }

    //удаляем все вхождения блока в массив, и если нужно удаляем сам блок
    public void DelBlock(Block block, bool dellGameObject = false)
    {
        //ищем объект
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если нашли наш то даляем его из массива
                if (containers[x].block[y] == block)
                {
                    containers[x].block[y].PositionInGrid = null;
                    containers[x].block[y] = null;
                    //если указано, то удаляем объект
                    if (dellGameObject)
                    {
                        Destroy(block.gameObject);
                    }
                }
            }
        }
    }

    //поиск блока в массиве
    public bool SearchBlock(Block block)
    {
        //ищем объект
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если нашли наш то
                if (containers[x].block[y] == block)
                {
                    return true;

                }
            }
        }
        return false;
    }
#endif
}
