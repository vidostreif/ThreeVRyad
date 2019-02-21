using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEditor;
using System.Xml.Linq;
using System;
using System.Linq;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class Grid : MonoBehaviour

{
    public static Grid Instance; // Синглтон
    public float blockSize = 1;
    public Transform thisTransform;

    public Blocks[] containers;
    public GameObject prefabBlock;
    public GameObject prefabElement;
    public GameObject prefabBlockingWall;
    public List<ElementsPriority> elementsPriority;

    public bool blockedForMove { get; protected set; }//признак что сетка заблокирована для действий игроком
    private List<Element> elementsForMix = new List<Element>();//элементы для замены во время микса
    private List<Blocks> elementsForMove = new List<Blocks>();//элементы для последовательного выполнения ходов
    private bool needFilling = false;
    private System.Random random;

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
        BorderGrid.CircleGrid(this);
    }

    public GameObject GatPrefab(CollectionTypesEnum collectionTypes)
    {
        if (collectionTypes == CollectionTypesEnum.Element)
        {
            return prefabElement;
        }
        else if (collectionTypes == CollectionTypesEnum.BlockingElement)
        {
            return prefabBlockingWall;
        }
        else
        {
            return null;
        }
    }

    //заполнение стандартные блоки элементами из списка
    public void StartFilling(List<ElementsPriority> elementsSAndP = null)
    {
        //проверяем что заполнены все GameObject
        if (prefabElement == null || prefabBlock == null || prefabBlockingWall == null)
        {
            Debug.LogError("В сетке не указан один из префабов!");
            return;
        }
        //проверяем что наш массив элементов инициорван
        if (elementsPriority == null || elementsPriority.Count == 0)
        {
            Debug.LogError("Не указано какие элементы будем создавать на поле!");
            return;
        }
        //если не заданы приоритеты, то берем стандартные
        if (elementsSAndP == null)
        {
            elementsSAndP = elementsPriority;
        }

        //заполнение сетки блоками и элементами [X, Y] [столбец, строка]
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если стандартный блок и в нем нет элемента
                if (ThisStandardBlockWithoutElement(containers[x].block[y]))
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
                                    if (elementsPriority != null && elementsPriority.elementsShape == containers[x - 1].block[y].Element.Shape)
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
                                    if (elementsPriority != null && elementsPriority.elementsShape == containers[x].block[y - 1].Element.Shape)
                                        continue;
                                }
                            }
                        }
                        //если нашли нужный элемент, то заканчиваем цикл
                        //elementShape = elementsShapeAndPriority[random].elementsShape;
                        elementfound = true;

                    } while (!elementfound || iteration == 0);

                    //Проверяем массив для микса, если там есть такой элемент, то отдаем его
                    if (elementsForMix.Count > 0)
                    {
                        Element element = elementsForMix.Find(item => item.Shape == elementsPriority.elementsShape);
                        if (element != null)
                        {
                            containers[x].block[y].Element = element;
                            int index = elementsForMix.IndexOf(element);
                            elementsForMix.RemoveAt(index);
                            continue;
                        }

                    }
                    // иначе создаем новый элемент
                    containers[x].block[y].CreatElement(prefabElement, elementsPriority.elementsShape, elementsPriority.elementsType);

                }
            }
        }

        //очищаем массив для микса от оставшихся элементов
        foreach (Element element in elementsForMix)
        {
            Destroy(element.gameObject);
        }
        elementsForMix.Clear();
    }

    public void Move(Block touchingBlock = null, Block destinationBlock = null)
    {
        //если остались ходы
        if (!Tasks.Instance.endGame)
        {
            Tasks.Instance.SubMoves();//минус ход
            Blocks blocks = new Blocks();
            blocks.block = new Block[2];
            blocks.block[0] = touchingBlock;
            blocks.block[1] = destinationBlock;
            elementsForMove.Add(blocks);

            if (!blockedForMove)
            {
                StartCoroutine(Grid.Instance.MakeMove(touchingBlock, destinationBlock));
            }
        }        
    }

    //выполняем ход
    //принимаем параметры - 1. Блок с которого передвигаем элемент 2. Блок к которому передвигаем элемент 
    private IEnumerator MakeMove(Block touchingBlock = null, Block destinationBlock = null)
    {
        //if (blockedForMove)
        //    yield break;

        blockedForMove = true;
        if (elementsForMove.Count > 0)
        {
            while (elementsForMove.Count > 0)
            {
                Blocks blocks = elementsForMove[0];
                touchingBlock = blocks.block[0];
                destinationBlock = blocks.block[1];

                MainAnimator.Instance.ClearElementsForNextMove();

                //повторяем итерации заполнения и поиска совпадений, пока совпадения не будут найдены
                int iteration = 1;
                List<Block> blockFields;
                bool matchFound;
                do
                {
                    //добавить проверку, что если нет destinationBlock, то не искать совпадающие линии

                    //ищем совпавшие линии 
                    matchFound = false;
                    blockFields = CheckMatchingLine();
                    int CountElementsForMove = elementsForMove.Count;
                    if (blockFields.Count > 0)
                    {
                        matchFound = true;
                        
                        if (iteration != 1)
                        {
                            int countblockFields = 0;                            
                            while (countblockFields < blockFields.Count)
                            {
                                iteration++;
                                yield return StartCoroutine(Filling(blockFields, false, iteration));
                                blockFields = CheckMatchingLine();
                                countblockFields = blockFields.Count;
                                //прерывание в случае вмешательства игрока
                                if (CountElementsForMove < elementsForMove.Count)
                                    break;
                            }

                            if (CountElementsForMove < elementsForMove.Count)
                                break;

                            if (needFilling)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                iteration++;
                                yield return StartCoroutine(Filling(blockFields, false, iteration));
                                }
                                if (CountElementsForMove < elementsForMove.Count)
                                    break;
                                iteration++;
                                StartCoroutine(Filling(blockFields, false, iteration));
                                if (CountElementsForMove < elementsForMove.Count)
                                    break;
                            }
                        }  
                    }
                    // проверяем длинну совпавших линий для бонусов
                    List<List<Block>> findedBlockInLine = CountCollectedLine(blockFields);

                    //ударяем по найденным блокам
                    foreach (Block blockField in blockFields)
                        blockField.Hit();

                    if (iteration == 1)
                    {
                        //активируем активируемые элементы
                        if (touchingBlock != null && touchingBlock.Element != null && touchingBlock.Element.Activated)
                        {
                            touchingBlock.Hit();
                            yield return new WaitForSeconds(0.1f);
                            matchFound = true;
                        }

                        else if (destinationBlock != null && destinationBlock.Element != null && destinationBlock.Element.Activated)
                        {
                            destinationBlock.Hit();
                            yield return new WaitForSeconds(0.1f);
                            matchFound = true;
                        }
                        else if (blockFields.Count == 0 || (!blockFields.Contains(destinationBlock) && !blockFields.Contains(touchingBlock)))
                        {
                            ExchangeElements(touchingBlock, destinationBlock);
                        }
                    }


                    //создаем бонусы
                    if (iteration == 1 && matchFound)
                    {
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
                        if (CountElementsForMove < elementsForMove.Count)
                            break;
                        yield return new WaitForSeconds(0.1f);                        
                    }
                    blockFields.Clear();
                    yield return StartCoroutine(Filling(blockFields, true, iteration));

                    elementsForMove.Remove(blocks);
                    //если есть элементы в очереди на движение
                    if (elementsForMove.Count > 0)
                    {
                        break;
                    }

                    iteration++;
                } while (matchFound || needFilling);

                //действия элементов после хода!!!!               

            }

            //если закончились ходы игрока и ходы всей игры
            if (elementsForMove.Count == 0 && Tasks.Instance.endGame)
            {
                MainSceneScript.Instance.CompleteGame();
                yield break;
            }

            //проверка, что остались доступные ходы
            List<Element> elementsForNextMove;
            int iteration2 = 1;
            do
            {
                elementsForNextMove = CheckElementsForNextMove();
                //Если нет доступных ходов, то перемешиваем поле
                if (elementsForNextMove.Count == 0)
                    MixStandartElements();

                if (iteration2 > 4)
                {
                    Debug.Log("Поле было перемешано " + iteration2 + " раз, но доступные ходы так и небыли найдены");
                    break;
                }
                iteration2++;

            } while (elementsForNextMove.Count == 0);//повторяем проверку

            MainAnimator.Instance.ElementsForNextMove = elementsForNextMove;

        }
        blockedForMove = false;
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
                NeighboringBlocks neighboringBlocks = DeterminingNeighboringBlocks(new Position(x, y));

                //проверяем горизонталь если не первый и не последний стоблец
                if (x != 0 && x != containers.GetLength(0) - 1)
                {
                    //проверяем что соседние блоки существуют
                    if (ThisStandardBlockWithElement(neighboringBlocks.Left) && ThisStandardBlockWithElement(neighboringBlocks.Right) && ThisStandardBlockWithElement(containers[x].block[y]))
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
                    if (ThisStandardBlockWithElement(neighboringBlocks.Up) && ThisStandardBlockWithElement(neighboringBlocks.Down) && ThisStandardBlockWithElement(containers[x].block[y]))
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
                if (ThisStandardBlockWithStandartElementCanMove(blockField))
                {
                    NeighboringBlocks neighboringBlocks = DeterminingNeighboringBlocks(FindPosition(blockField));
                    foreach (Block neighboringBlock in neighboringBlocks.allBlockField)
                    {
                        //если блок находится по соседству и в нем такой же элемент
                        if (listForCheck.Contains(neighboringBlock) && ThisStandardBlockWithStandartElementCanMove(neighboringBlock) && blockField.Element.Shape == neighboringBlock.Element.Shape)
                        {
                            //и если противоположный блок имеет такой же элемент
                            Block OppositeBlock = neighboringBlocks.GetOppositeBlock(neighboringBlock);
                            if (listForCheck.Contains(OppositeBlock) && ThisStandardBlockWithStandartElementCanMove(OppositeBlock) && blockField.Element.Shape == OppositeBlock.Element.Shape)
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


            //do
            //{
            //    findedBlocks.Clear();
            //    //foreach (Block blockField in listForCheck)
            //    //{
            //    //    if (ThisStandardBlockWithStandartElementCanMove(blockField))
            //    //    {
            //            foreach (Block blockField in blocksInLine)
            //            {
            //                //if (blockField != item && blockField.Element.Shape == item.Element.Shape)
            //                //{
            //                    NeighboringBlocks neighboringBlocks = DeterminingNeighboringBlocks(FindPosition(blockField));

            //                    foreach (Block neighboringBlock in neighboringBlocks.allBlockField)
            //                    {                                    
            //                        //если блок находится в основном массиве
            //                        if (listForCheck.Contains(neighboringBlock) && ThisStandardBlockWithStandartElementCanMove(neighboringBlock) && blockField.Element.Shape == neighboringBlock.Element.Shape)
            //                        {
            //                            //Если противоположный блок имеет такой же элемент
            //                            Block OppositeBlock = neighboringBlocks.GetOppositeBlock(neighboringBlock);
            //                            if (listForCheck.Contains(OppositeBlock) && ThisStandardBlockWithStandartElementCanMove(OppositeBlock) && blockField.Element.Shape == OppositeBlock.Element.Shape)
            //                            {
            //                                //проверяем что блок еще не добавили в массив
            //                                if (!blocksInLine.Contains(neighboringBlock) && !findedBlocks.Contains(neighboringBlock))
            //                                    findedBlocks.Add(neighboringBlock);
            //                                //добавляем противоположный блок
            //                                if (!blocksInLine.Contains(OppositeBlock) && !findedBlocks.Contains(OppositeBlock))
            //                                    findedBlocks.Add(OppositeBlock);

            //                            }                                        
            //                        }
            //                    }
            //                //}
            //            }
            //    //    }
            //    //}
            //    //переносим найденные блоки в основной массив
            //    foreach (Block item in findedBlocks)
            //    {
            //        blocksInLine.Add(item);
            //    }
            //} while (findedBlocks.Count > 0);

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
    private List<Element> CheckElementsForNextMove()
    {
        List<Element> elementsList = new List<Element>();

        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если стандартный блок и в нем есть элемент
                if (ThisStandardBlockWithElement(containers[x].block[y]))
                {
                    for (int j = 0; j < 2; j++)
                    {
                        elementsList.Add(containers[x].block[y].Element);
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

                                if (ElementsMatch(curBlock, containers[x].block[y]) && curBlock.Element != elementsList.Last())
                                {
                                    elementsList.Add(curBlock.Element);
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
                                if (ThisBlockWithElementCantMove(curBlock))//если элемент заблокирован для движения
                                    continue;

                                Position position = new Position(posX, posY);
                                NeighboringBlocks neighboringBlocks = DeterminingNeighboringBlocks(position);

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
                                    if (ElementsMatch(block, containers[x].block[y]) && block.Element != elementsList.Last() && !block.Element.LockedForMove)
                                    {
                                        elementsList.Add(block.Element);
                                        elementForMoveFound = true;
                                        elementFound = true;
                                        //Debug.Log("Смещенный" + block);
                                        break;
                                    }
                                }

                            } while (elementFound);
                        }
                        //если количество найденных элементов больше двух то останавливаем поиск
                        if (elementsList.Count > 2)
                            break;
                        else
                            elementsList.Clear();
                    }
                }
                //если количество найденных элементов больше двух то останавливаем поиск
                if (elementsList.Count > 2)
                    break;
            }
            //если количество найденных элементов больше двух то останавливаем поиск
            if (elementsList.Count > 2)
                break;
        }

        return elementsList;
    }

    //перемешать стандартные элементы
    private void MixStandartElements()
    {
        List<ElementsPriority> listPriority = new List<ElementsPriority>();
        elementsForMix.Clear();

        //задать приоритеты
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                if (ThisStandardBlockWithStandartElementCanMove(containers[x].block[y]))
                {
                    ElementsPriority elementsSAndP = listPriority.Find(item => item.elementsShape == containers[x].block[y].Element.Shape);
                    if (elementsSAndP == null)
                    {
                        listPriority.Add(new ElementsPriority(containers[x].block[y].Element.Shape, ElementsTypeEnum.Standard, 1));
                    }
                    else
                    {
                        int index = listPriority.IndexOf(elementsSAndP);
                        listPriority[index].priority++;
                    }
                    elementsForMix.Add(containers[x].block[y].Element);
                    containers[x].block[y].Element = null;
                }
            }
        }

        //проверить весь массив, если нет элементов вхождений которого более 2, то искуственно увеличиваем приоритет у одного из элементов в два раза



        //перезаполняем новыми элементами
        StartFilling(listPriority);
    }

    public bool ThisBlockWithoutElement(Block block)
    {

        if (block != null && (block.Element == null || block.Element.Destroyed))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ThisBlockWithElement(Block block)
    {

        if (block != null && block.Element != null && !block.Element.Destroyed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ThisBlockWithDestroyElement(Block block)
    {

        if (block != null && block.Element != null && block.Element.Destroyed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ThisStandardBlockWithoutElement(Block block)
    {

        if (block != null && block.Type == BlockTypeEnum.Standard && (block.Element == null || block.Element.Destroyed))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ThisStandardBlockWithElement(Block block)
    {

        if (block != null && block.Type == BlockTypeEnum.Standard && block.Element != null && !block.Element.Destroyed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ThisStandardBlockWithStandartElementCanMove(Block block)
    {

        if (block != null && block.Type == BlockTypeEnum.Standard && block.Element != null && !block.Element.Destroyed && block.Element.Type == ElementsTypeEnum.Standard && !block.Element.LockedForMove)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ThisBlockWithElementCanMove(Block block)
    {

        if (block != null && block.Element != null && !block.Element.Destroyed && !block.Element.LockedForMove)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ThisBlockWithElementCantMove(Block block)
    {
        if (block != null && block.Element != null && block.Element.LockedForMove)
        {
            return true;
        }
        else
        {
            return false;
        }
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

    //устанавливаем блок на определенную позицию в сетке
    //возвращает истину если удалось поставить элемент на новую позицию?
    public bool AddBlockToPosition(Block Block, Position newPosition)
    {

        //находим текущую позицию в сетке
        Position oldPosition = this.FindPosition(Block);

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
            if (newPosition.posX != oldPosition.posX || newPosition.posY != oldPosition.posY)
            {
                //и удаляем со старой если такая была
                if (oldPosition.posX != -1 || oldPosition.posY != -1)
                {
                    DelBlock(Block);
                    //Debug.LogAssertion("1 Удалили блок с позиции: " + oldPosition.posX + " " + oldPosition.posY);
                }

                //ставим блок на новую позицию
                containers[newPosition.posX].block[newPosition.posY] = Block;
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
        else if (oldPosition.posX != -1 || oldPosition.posY != -1)//иначе значит элемент вышел за пределы сетки и его нужно удалить из нее
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

    //удаляем все вхождения блока в массив, и если нужно удаляем сам блок
    public void DelBlock(Block block, bool dellGameObject = false)
    {
        //ищем объект
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если нашли наш тоу даляем его из массива
                if (containers[x].block[y] == block)
                {
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

    //функцию поиска позиции оп блоку
    public Position FindPosition(Block block)
    {
        int positionX = -1;
        int positionY = -1;

        //ищем объект
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если нашли наш блок то возвращаем его позицию
                if (containers[x].block[y] == block)
                    return new Position(x, y);
            }
        }

        //если ничего не нашли, то возвращаем -1 -1
        return new Position(positionX, positionY);
    }

    //функцию поиска позиции по элементу
    public Position FindPosition(Element element)
    {
        int positionX = -1;
        int positionY = -1;

        //ищем объект
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если нашли наш блок то возвращаем его позицию
                if (containers[x].block[y] != null)
                {
                    if (containers[x].block[y].Element == element)
                        return new Position(x, y);
                }
            }
        }

        //если ничего не нашли, то возвращаем -1 -1
        return new Position(positionX, positionY);
    }

    //определяем соседние блоки
    public NeighboringBlocks DeterminingNeighboringBlocks(Position position)
    {
        Block upBlock = null;
        Block DownBlock = null;
        Block LeftBlock = null;
        Block RightBlock = null;

        if (position.posX != -1 || position.posY != -1)
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
    public Block[] DeterminingAroundBlocks(Position position)
    {
        Block[] blocks = new Block[8];

        if (position.posX != -1 || position.posY != -1)
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
    public IEnumerator Filling(List<Block> blockFields, bool breakSpeed = true, int iteration = 1)
    {
        //нужна еще итерация
        needFilling = false;
        bool needIteration = true;
        ElementsPriority elementPriority;
        float speed = 0.065f;
        //float dopSpeed = 0.025f;
        //float maxDopSpeed = 0.075f;
        Block dropBlock = null;

        //yield return new WaitForSeconds(0.05f);

        //смещае элементы на один блок вниз за каждую итерацию
        //while (needIteration)
        //{
        needIteration = false;
        //speed += 0.02f;

        //yield return new WaitForSeconds(0.1f);
        for (int y = 0; y < containers[0].block.GetLength(0); y++)
        {
            dropBlock = null;
            yield return new WaitForSeconds(0.016f);
            //начинаем со второй строки
            for (int x = 0; x < containers.GetLength(0); x++)
            {
                Block currentBlock = containers[x].block[y];
                bool createdElement = false;
                if (currentBlock != null && !blockFields.Contains(currentBlock))
                {
                    //если пустой блок и не умеет генерировать элемент, идем дальше
                    if (ThisBlockWithoutElement(currentBlock) && !currentBlock.GeneratorElements)
                    { continue; }

                    //если пустой блок и умеет генерировать элемент, то предварительно создаем случайный элемент
                    else if (ThisBlockWithoutElement(currentBlock) && currentBlock.GeneratorElements)
                    {
                        elementPriority = ProportionalWheelSelection.SelectElement(elementsPriority);
                        currentBlock.CreatElement(prefabElement, elementPriority.elementsShape, elementPriority.elementsType);
                        MainAnimator.Instance.AddElementForSmoothMove(currentBlock.Element.thisTransform, new Vector3(currentBlock.thisTransform.position.x, currentBlock.thisTransform.position.y - 0.2f, currentBlock.thisTransform.position.z), 2, SmoothEnum.InLineWithOneSpeed, smoothTime: speed + iteration * 0.0009f, addToQueue: true);

                        //currentBlock.Element.speed = 1;
                        needIteration = true;
                        createdElement = true;
                        //continue;
                    }

                    //если текущий элемент заблокирован для движения, то переходим к следующему
                    else if (ThisBlockWithElementCantMove(currentBlock))
                    { continue; }

                    if (y > 0)
                    {
                        //ищем место для смещения
                        //если нижний блок не имеет элемента, то смещаем к нему
                        if (ThisStandardBlockWithoutElement(containers[x].block[y - 1]))
                        {
                            Block newBlock = containers[x].block[y - 1];
                            int distance = 1;
                            for (int i = y - 2; i >= 0; i--)
                            {
                                if (ThisStandardBlockWithoutElement(containers[x].block[i]))
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
                            MainAnimator.Instance.AddElementForSmoothMove(newBlock.Element.thisTransform, new Vector3(newBlock.thisTransform.position.x, newBlock.thisTransform.position.y - 0.1f, newBlock.thisTransform.position.z), 2, SmoothEnum.InLineWithAcceleration, smoothTime: speed + distance  * 0.01f, addToQueue: !createdElement);
                            needIteration = true;
                            dropBlock = containers[x].block[y];
                            continue;
                        }

                        //иначе, проверяем правый нижний блок по диагонали, при условии, что справа нет элементов в блоках
                        if ((x < containers.GetLength(0) - 1) && ThisStandardBlockWithoutElement(containers[x + 1].block[y - 1]))
                        {
                            bool moveRight = false;
                            for (int i = y; i < containers[x].block.GetLength(0); i++)
                            {
                                if (dropBlock != null && containers[x + 1].block[i] == dropBlock)
                                {
                                    moveRight = false;
                                    break;
                                }
                                if (ThisBlockWithElementCanMove(containers[x + 1].block[i]))
                                {
                                    moveRight = false;
                                    break;
                                }
                                if (ThisBlockWithElementCantMove(containers[x + 1].block[i]) || containers[x + 1].block[i] == null)
                                {
                                    //if (currentBlock == containers[x].block[i])
                                    //{
                                    //    moveRight = true;
                                    //    break;
                                    //}
                                    //else if (!ThisBlockWithElementCanMove(containers[x].block[i]) && !ThisBlockWithElementCanMove(GetBlock(x + 2, i)))
                                    //{
                                    //    moveRight = true;
                                    //    break;
                                    //}
                                    //else
                                    //{
                                    //    moveRight = false;
                                    //    break;
                                    //}

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
                                //if (currentBlock.Element.speed > 0)
                                //{
                                //    smoothEnum = SmoothEnum.InLineWithOneSpeed;
                                //}
                                //else
                                //{
                                    smoothEnum = SmoothEnum.InLineWithAcceleration;
                                //}

                                ExchangeElements(currentBlock, containers[x + 1].block[y - 1]);
                                MainAnimator.Instance.AddElementForSmoothMove(containers[x + 1].block[y - 1].Element.thisTransform, new Vector3(containers[x + 1].block[y - 1].thisTransform.position.x + 0.1f, containers[x + 1].block[y - 1].thisTransform.position.y - 0.1f, containers[x + 1].block[y - 1].thisTransform.position.z), 2, smoothEnum, smoothTime: speed + 1 * 0.015f, addToQueue: !createdElement);
                                if (y > 1 && (((x < containers.GetLength(0) - 2) && ThisStandardBlockWithoutElement(containers[x + 2].block[y - 2])) || ThisStandardBlockWithoutElement(containers[x + 1].block[y - 2])))
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
                        if ((x > 0) && ThisStandardBlockWithoutElement(containers[x - 1].block[y - 1]))
                        {
                            bool moveLeft = false;
                            for (int i = y; i < containers[x].block.GetLength(0); i++)
                            {
                                if (dropBlock != null && containers[x - 1].block[i] == dropBlock)
                                {
                                    moveLeft = false;
                                    break;
                                }
                                if (ThisBlockWithElementCanMove(containers[x - 1].block[i]))
                                {
                                    moveLeft = false;
                                    break;
                                }
                                if (ThisBlockWithElementCantMove(containers[x - 1].block[i]) || containers[x - 1].block[i] == null)
                                {
                                    //if (currentBlock == containers[x].block[i] && !ThisBlockWithElementCanMove(GetBlock(x - 2, i)))
                                    //{
                                    //    moveLeft = true;
                                    //    break;
                                    //}
                                    //else if (!ThisBlockWithElementCanMove(containers[x].block[i]) && !ThisBlockWithElementCanMove(GetBlock(x - 2, i)))
                                    //{
                                    //    moveLeft = true;
                                    //    break;
                                    //}
                                    //else
                                    //{
                                    //    moveLeft = false;
                                    //    break;
                                    //}
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
                                //if (currentBlock.Element.speed > 0)
                                //{
                                //    smoothEnum = SmoothEnum.InLineWithOneSpeed;
                                //}
                                //else
                                //{
                                    smoothEnum = SmoothEnum.InLineWithAcceleration;
                                //}
                                ExchangeElements(currentBlock, containers[x - 1].block[y - 1]);
                                MainAnimator.Instance.AddElementForSmoothMove(containers[x - 1].block[y - 1].Element.thisTransform, new Vector3(containers[x - 1].block[y - 1].thisTransform.position.x - 0.1f, containers[x - 1].block[y - 1].thisTransform.position.y - 0.1f, containers[x - 1].block[y - 1].thisTransform.position.z), 2, smoothEnum, smoothTime: speed + 1 * 0.015f, addToQueue: !createdElement);
                                if (y > 1 && ((x > 1 && ThisStandardBlockWithoutElement(containers[x - 2].block[y - 2])) || ThisStandardBlockWithoutElement(containers[x - 1].block[y - 2])))
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
                        //}

                    }
                    if (currentBlock.Element.speed > 0)
                    {
                        //Debug.Log(currentBlock+ " " + currentBlock.Element.speed);
                        needIteration = true;
                        //currentBlock.Element.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
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
        //}
    }

    //передаем данные о на стройках в xml формате
    public XElement GetXElement()
    {

        XElement gridXElement = new XElement("grid");

        gridXElement.Add(new XElement("blockSize", blockSize));

        //gridXElement.Add(new XElement("prefabBlock", prefabBlock.name));
        //gridXElement.Add(new XElement("prefabElement", prefabElement.name));
        //gridXElement.Add(new XElement("prefabBlockingWall", prefabBlockingWall.name));

        //записываем размер сетки
        gridXElement.Add(new XElement("XSize", containers.GetLength(0)));
        gridXElement.Add(new XElement("YSize", containers[0].block.GetLength(0)));

        //записываем все внешности стандартных элементов
        //gridXElement.Add(new XElement("shapeSize", elementsShape.Count));
        XElement elementsShapeXElement = new XElement("elementsShape");
        foreach (ElementsPriority shapeAndPriority in elementsPriority)
        {
            XAttribute shape = new XAttribute("shape", shapeAndPriority.elementsShape);
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
                    XAttribute behindElementsType = new XAttribute("behindElementsType", BehindElementsTypeEnum.Empty);
                    XAttribute behindElementsShape = new XAttribute("behindElementsShape", BehindElementsShapeEnum.Empty);
                    XAttribute elementType = new XAttribute("elementType", ElementsTypeEnum.Empty);
                    XAttribute elementShape = new XAttribute("elementShape", ElementsShapeEnum.Empty);
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

                        //если в в элементе есть блокирующий элемент
                        if (containers[x].block[y].Element.BlockingElement != null)
                        {
                            blockingElementType = new XAttribute("blockingElementType", containers[x].block[y].Element.BlockingElement.Type);
                            blockingElementShape = new XAttribute("blockingElementShape", containers[x].block[y].Element.BlockingElement.Shape);
                        }
                    }

                    XElement blockXElement = new XElement("block", posX, posY, blockType, generatorElements, behindElementsType, behindElementsShape, elementType, elementShape, blockingElementType, blockingElementShape);
                    blocksXElement.Add(blockXElement);
                }
            }
        }
        gridXElement.Add(blocksXElement);

        Debug.Log(gridXElement);

        return gridXElement;
    }

    public void RecoverFromXElement(XElement gridXElement)
    {

        this.blockSize = float.Parse(gridXElement.Element("blockSize").Value);
        int XSize = int.Parse(gridXElement.Element("XSize").Value);
        int YSize = int.Parse(gridXElement.Element("YSize").Value);

        //string dir = "Prefabs/";
        //prefabBlock = Resources.Load<GameObject>(dir + gridXElement.Element("prefabBlock").Value);
        //prefabElement = Resources.Load(dir + gridXElement.Element("prefabElement").Value, typeof(GameObject)) as GameObject;
        //prefabBlockingWall = Resources.Load(gridXElement.Element(dir + "prefabBlockingWall").Value, typeof(GameObject)) as GameObject;

        //int shapeSize = int.Parse(gridXElement.Element("shapeSize").Value);
        elementsPriority.Clear();
        //восстанавливаем все блоки и элементы
        foreach (XElement shapeAndPriority in gridXElement.Element("elementsShape").Elements("shapeAndPriority"))
        {
            ElementsShapeEnum shape = (ElementsShapeEnum)Enum.Parse(typeof(ElementsShapeEnum), shapeAndPriority.Attribute("shape").Value);
            ElementsTypeEnum type = (ElementsTypeEnum)Enum.Parse(typeof(ElementsTypeEnum), shapeAndPriority.Attribute("type").Value);
            int priority = int.Parse(shapeAndPriority.Attribute("priority").Value);
            ElementsPriority curShapeAndPriority = new ElementsPriority(shape, type, priority);
            this.elementsPriority.Add(curShapeAndPriority);
        }

        //очищаем сетку
        for (int x = 0; x < containers.GetLength(0); x++)
        {
            for (int y = 0; y < containers[x].block.GetLength(0); y++)
            {
                //если блок существует по данному адресу
                if (containers[x].block[y] != null)
                {
                    //удаляем блок
                    DestroyImmediate(containers[x].block[y].gameObject);
                }
            }
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
            BlockTypeEnum blockType = (BlockTypeEnum)Enum.Parse(typeof(BlockTypeEnum), block.Attribute("blockType").Value);
            BehindElementsTypeEnum behindElementsType = (BehindElementsTypeEnum)Enum.Parse(typeof(BehindElementsTypeEnum), block.Attribute("behindElementsType").Value);
            BehindElementsShapeEnum behindElementsShape = (BehindElementsShapeEnum)Enum.Parse(typeof(BehindElementsShapeEnum), block.Attribute("behindElementsShape").Value);
            ElementsTypeEnum elementType = (ElementsTypeEnum)Enum.Parse(typeof(ElementsTypeEnum), block.Attribute("elementType").Value);
            ElementsShapeEnum elementShape = (ElementsShapeEnum)Enum.Parse(typeof(ElementsShapeEnum), block.Attribute("elementShape").Value);
            BlockingElementsTypeEnum blockingElementType = (BlockingElementsTypeEnum)Enum.Parse(typeof(BlockingElementsTypeEnum), block.Attribute("blockingElementType").Value);
            BlockingElementsShapeEnum blockingElementShape = (BlockingElementsShapeEnum)Enum.Parse(typeof(BlockingElementsShapeEnum), block.Attribute("blockingElementShape").Value);

            if (blockType != BlockTypeEnum.Empty)
            {
                Vector3 position = new Vector3(this.transform.localPosition.x + posX * blockSize, this.transform.localPosition.y + posY * blockSize, this.transform.localPosition.z);

                //создаем блок
                GameObject blockGameObject = Instantiate(prefabBlock, position, Quaternion.identity);
                blockGameObject.name = "Block_" + posX + "_" + posY;
                blockGameObject.transform.parent = this.transform;

                Block blockField = blockGameObject.GetComponent<Block>();
                blockField.GeneratorElements = generatorElements;
                blockField.Type = blockType;
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
                    blockField.CreatElement(prefabElement, elementShape, elementType);
                    //создаем блокирующий элемент
                    if (blockingElementType != BlockingElementsTypeEnum.Empty)
                    {
                        blockField.Element.CreatBlockingElement(prefabBlockingWall, blockingElementShape, blockingElementType);
                    }
                }
            }
        }
    }
}
