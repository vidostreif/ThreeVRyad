using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class HelpToPlayer
{
    private static List<Hint> hintsList = new List<Hint>();
    private static HintStatus[] hintsStatus = null;
    private static Hint activeHint = null;

    private static bool deletedByClickingOnCanvas = false;
    private static float timeCreateHints;
    private static float delayTime = 0;

    private static void CreateHintStatusList() {
        if (hintsStatus == null)
        {
            hintsStatus = new HintStatus[Enum.GetNames(typeof(ElementsTypeEnum)).Length];

            int i = 0;
            foreach (ElementsTypeEnum elementsTypeEnum in Enum.GetValues(typeof(ElementsTypeEnum)))
            {
                hintsStatus[i] = new HintStatus(elementsTypeEnum, false);
                i++;
            }
        }
    }

    public static void ClearHintList()
    {
        hintsList.Clear();//очищаем список подсказок
    }

    public static void AddHint(ElementsTypeEnum elementsTypeEnum) {

        //проверяем показывали ли мы такую подсказку игроку
        CreateHintStatusList();
        //если уже показывали то не добавляем
        if (hintsStatus[(int)elementsTypeEnum].status == false)
        {
            //если она уже добавлена в массив, то пропускаем
            Hint hint = hintsList.Find(item => item.elementsTypeEnum == elementsTypeEnum);
            if (hint == null)
            {
                hintsList.Add(new Hint(elementsTypeEnum));
            }            
        }        
    }

    public static bool CreateNextGameHelp()
    {
        bool created = false;
        if (hintsList.Count > 0)
        {
            //!!!добавить все блоки в список для запрета обработки
            
            List<Hint> hintForDell = new List<Hint>();
            //перебираем подскази до тех пор пока не создадим хоть одну
            foreach (Hint hint in hintsList)
            {
                activeHint = hint;                                

                //находим нужную подсказку
                if (activeHint.elementsTypeEnum == ElementsTypeEnum.Standard)
                {
                    created = CreateStandardElementHelp();
                }
                else if (activeHint.elementsTypeEnum == ElementsTypeEnum.CrushableWall)
                {                    
                    created = CreateCrushableWallHelp(activeHint.elementsTypeEnum);
                }
                else if (activeHint.elementsTypeEnum == ElementsTypeEnum.SmallFlask || activeHint.elementsTypeEnum == ElementsTypeEnum.MediumFlask || activeHint.elementsTypeEnum == ElementsTypeEnum.BigFlask)
                {
                    created = CreateFlaskHelp(activeHint.elementsTypeEnum);
                }
                else if (activeHint.elementsTypeEnum == ElementsTypeEnum.ImmortalWall)
                {
                    created = CreateImortalWallHelp();
                }
                else if (activeHint.elementsTypeEnum == ElementsTypeEnum.Drop)
                {
                    created = CreateDropElementHelp();
                }
                else
                {
                    //неудалось определить подсказку
                    Debug.Log("Неудалось определить подсказку!");
                }

                //если удалось создать подсказку, выходим из цикла
                if (created)
                {
                    //создаем затемнение 
                    activeHint.canvasHelpToPlayer = UnityEngine.Object.Instantiate(PrefabBank.Instance.canvasHelpToPlayer);
                    Image imageHelpToPlayer = activeHint.canvasHelpToPlayer.GetComponentInChildren<Image>();
                    MainAnimator.Instance.AddElementForSmoothChangeColor(imageHelpToPlayer, new Color(imageHelpToPlayer.color.r, imageHelpToPlayer.color.g, imageHelpToPlayer.color.b, 0.7f), 2);
                    //устанавливаем камеру
                    activeHint.canvasHelpToPlayer.GetComponent<Canvas>().worldCamera = Camera.main;
                    //добавляем действие
                    Transform gOPanel = activeHint.canvasHelpToPlayer.transform.Find("Panel");
                    Button button = gOPanel.GetComponent<Button>();
                    button.onClick.AddListener(delegate { DeletedByClickingOnCanvas(); });

                    //показываем текст
                    CreateTextCloud();

                    //помечаем как показанную
                    hintsStatus[(int)activeHint.elementsTypeEnum].status = true;
                    //добавляем для удаления
                    hintForDell.Add(hint);
                    break;
                }
                else
                {
                    //неудалось создать подсказку
                    DellGameHelp();
                }
            }

            //удаляем пройденные подсказки
            foreach (Hint item in hintForDell)
            {
                hintsList.Remove(item);
            }
        }

        return created;
    }

    private static void CreateTextCloud()
    {
        if (activeHint.spriteRendersSetingList.Count > 0)
        {
            //находим самый левый верхний объект
            Vector3 newPosition = activeHint.spriteRendersSetingList[0].spriteRenderer.transform.position;
            foreach (SpriteRenderSettings item in activeHint.spriteRendersSetingList)
            {
                //обрабатываем только блоки
                if (item.spriteRenderer.GetComponent<Block>())
                {
                    if (item.spriteRenderer.transform.position.x < newPosition.x)
                    {
                        newPosition.x = item.spriteRenderer.transform.position.x;
                    }

                    if (item.spriteRenderer.transform.position.y > newPosition.y)
                    {
                        newPosition.y = item.spriteRenderer.transform.position.y;
                    }
                }                
            }

            Transform gOPanel = activeHint.canvasHelpToPlayer.transform.Find("Panel");
            Transform textCloud = gOPanel.transform.Find("TextCloud");
            textCloud.position = new Vector3(newPosition.x - 3, newPosition.y, newPosition.z);

            //если слишков высоко, то смещаем вниз до пределов экрана
            RectTransform rectTransformGOPanel = gOPanel.GetComponent<RectTransform>();
            RectTransform rectTransformTextCloud = textCloud.GetComponent<RectTransform>();
            if (-rectTransformTextCloud.anchoredPosition.y < rectTransformTextCloud.rect.height * 0.5f)
            {
                rectTransformTextCloud.anchoredPosition = new Vector3(rectTransformTextCloud.anchoredPosition.x, - rectTransformTextCloud.rect.height * 0.5f);
            }

            //если слишков низко, то смещаем вверх до пределов экрана
            if (rectTransformGOPanel.rect.height < -rectTransformTextCloud.anchoredPosition.y + rectTransformTextCloud.rect.height * 0.5f)
            {
                rectTransformTextCloud.anchoredPosition = new Vector3(rectTransformTextCloud.anchoredPosition.x, -rectTransformGOPanel.rect.height + rectTransformTextCloud.rect.height * 0.5f);
            }

            //!!!сделать перенос на правую сторону
            
            Text text = textCloud.GetComponentInChildren<Text>();

            if (activeHint.elementsTypeEnum == ElementsTypeEnum.Standard)
            {
                text.text = "Что бы собрать растения в нашем саду, их нужно собрать в линии из более чем двух растений. Попробуйте передвинуть выделяющееся растение!";
            }
            else if (activeHint.elementsTypeEnum == ElementsTypeEnum.SmallFlask)
            {
                text.text = "Это маленький бонус за сбор линии из 4 растений. Нажмите на него дважды и он взорвет всю выделенную область!";
            }
            else if (activeHint.elementsTypeEnum == ElementsTypeEnum.MediumFlask)
            {
                text.text = "Это маленький бонус за сбор линии из 5 растений. Нажмите на него дважды и он взорвет всю выделенную область!";
            }
            else if (activeHint.elementsTypeEnum == ElementsTypeEnum.BigFlask)
            {
                text.text = "Это маленький бонус за сбор линии из 6 растений. Нажмите на него дважды и он взорвет всю выделенную область!";
            }
            else if (activeHint.elementsTypeEnum == ElementsTypeEnum.CrushableWall)
            {
                text.text = "Это стену вы можете взорвать или уничтожить собрав линию рядом! Соберите линию.";
            }
            else if (activeHint.elementsTypeEnum == ElementsTypeEnum.ImmortalWall)
            {
                text.text = "Это стену вы не сможете разрушить, она очень крепкая!";
            }
            else if (activeHint.elementsTypeEnum == ElementsTypeEnum.Drop)
            {
                text.text = "Этот элемент лишний на поле, и что бы его убрать, его нужно согнать через все поле в самый низ!";
            }
            else if (activeHint.elementsTypeEnum == ElementsTypeEnum.SeedBarrel)
            {
                text.text = "Это бочка с зернами, она наполняется когда рядом собираются растения указанные на бочке!";
            }
            else
            {
                text.text = "Често говоря, я и сам не понимаю, что происходит :)";
            }
        }
        else
        {
            Debug.Log("Нет ни одного SpriteRenderSettings для обработки");
            return;
        }
    }

    private static bool CreateStandardElementHelp() {
        List<ElementsForNextMove> elementsForNextMoveList = GridBlocks.Instance.CheckElementsForNextMove();
        //Если нет доступных ходов, то выходим
        if (elementsForNextMoveList.Count == 0)
        {
            return false;
        }

        ElementsForNextMove elementsForNextMove = elementsForNextMoveList[0];

        if (HighlightSpecifiedMove(elementsForNextMove))
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    private static bool CreateFlaskHelp(ElementsTypeEnum elementsTypeEnum) {
        //берем любую маленькую фласку
        //делаем подсветку фласки и соседних блококв
        //наоходим все объекты с нужным элементом
        ElementSmallFlask[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(ElementSmallFlask)) as ElementSmallFlask[];

        //если нашли хоть один элемент
        foreach (ElementSmallFlask item in findeObjects)
        {
            //берем блок с нашей флаской
            Block curBlock = GridBlocks.Instance.GetBlock(item.PositionInGrid);            
            if (BlockCheck.ThisBlockWithElementCanMove(curBlock) && item.Type == elementsTypeEnum)
            {
                    //высвечиваем блок
                    ChangeSorting(curBlock.gameObject, activeHint);

                    //отключаем перетаскивание у фласки
                    BlockController blockController = curBlock.GetComponent<BlockController>();
                    activeHint.blockControllersSetingList.Add(new BlockControllerSettings(blockController, blockController.handleСlick, blockController.handleDragging, blockController.permittedDirection));
                    blockController.handleDragging = false;

                    //получаем соседние блоки
                    Block[] blocks = GridBlocks.Instance.GetBlocksForHit(curBlock.PositionInGrid, item.GetComponent<ElementSmallFlask>().ExplosionRadius);

                    //перебираем все блоки
                    foreach (Block block in blocks)
                    {
                        if (block != null)
                        {
                            ChangeSorting(block.gameObject, activeHint);
                            blockController = block.GetComponent<BlockController>();
                            //сохраняем настройки
                            activeHint.blockControllersSetingList.Add(new BlockControllerSettings(blockController, blockController.handleСlick, blockController.handleDragging, blockController.permittedDirection));
                            //деактивируем
                            blockController.handleDragging = false;
                            blockController.handleСlick = false;
                        }
                    }
                return true;
            }
        }
        Debug.Log("Не нашли не одной маленькой фласки для создания подсказки!");
        return false;
    }

    private static bool CreateCrushableWallHelp(ElementsTypeEnum elementsTypeEnum)
    {
        //получаем возможные ходы
        List<ElementsForNextMove> elementsForNextMoveList = GridBlocks.Instance.CheckElementsForNextMove();
        //Если нет доступных ходов, то выходим
        if (elementsForNextMoveList.Count == 0)
        {
            return false;
        }

        //получаем все стены
        ElementWall[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(ElementWall)) as ElementWall[];

        //если нашли хоть один элемент
        foreach (ElementWall item in findeObjects)
        {
            //берем блок с нашим элементом
            Block curBlock = GridBlocks.Instance.GetBlock(item.PositionInGrid);

            if (item.Type == elementsTypeEnum && BlockCheck.ThisBlockWithElementWithoutBlockingElement(curBlock))
            {
                //получаем соседние блоки
                NeighboringBlocks blocks = GridBlocks.Instance.GetNeighboringBlocks(curBlock.PositionInGrid);
                //пытаемся найти ход где есть соседний блок вследующем ходе
                foreach (ElementsForNextMove curElementsForNextMove in elementsForNextMoveList)
                {
                    
                    foreach (Element element in curElementsForNextMove.elementsList)
                    {
                        //если элемент не для передвижения
                        if (element != curElementsForNextMove.elementForMove)
                        {
                            foreach (Block NeighboringBlock in blocks.allBlockField)
                            {
                                if (NeighboringBlock == GridBlocks.Instance.GetBlock(element.PositionInGrid))
                                {
                                    //высвечиваем блок
                                    ChangeSorting(curBlock.gameObject, activeHint);

                                    //высвечиваем нужный ход
                                    return HighlightSpecifiedMove(curElementsForNextMove);
                                }
                            }
                        }                        
                    }
                }
                //если не нашли соседний блок
                return false;
            }
        }
        Debug.Log("Не нашли ни одной разрушаемой стены!");
        return false;
    }

    private static bool CreateImortalWallHelp()
    {
        //получаем все стены
        ElementWall[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(ElementWall)) as ElementWall[];

        //если нашли хоть один элемент
        foreach (ElementWall item in findeObjects)
        {
            //берем блок с нашим элементом
            Block curBlock = GridBlocks.Instance.GetBlock(item.PositionInGrid);

            if (item.Type == ElementsTypeEnum.ImmortalWall && curBlock != null)
            {
                //высвечиваем блок
                ChangeSorting(curBlock.gameObject, activeHint);

                //таймаут для удаления подсказки
                CanvasLiveTime(3);
                return true;
            }
        }
        Debug.Log("Не нашли ни одной бесмертной стены!");
        return false;
    }

    private static bool CreateDropElementHelp()
    {
        //получаем все элементы
        Element[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(Element)) as Element[];

        //если нашли хоть один элемент
        foreach (Element item in findeObjects)
        {
            //берем блок с нашим элементом
            Block curBlock = GridBlocks.Instance.GetBlock(item.PositionInGrid);

            if (item.Type == ElementsTypeEnum.Drop && curBlock != null)
            {
                //высвечиваем блок
                ChangeSorting(curBlock.gameObject, activeHint);

                //таймаут для удаления подсказки
                CanvasLiveTime(3);
                return true;
            }
        }
        Debug.Log("Не нашли ни одного сбрасываемого элемента!");
        return false;
    }

    public static bool DellGameHelp()
    {
        if (activeHint != null)
        {
            //удаляем затемнение
            UnityEngine.Object.Destroy(activeHint.canvasHelpToPlayer);

            //восстанавливаем значения сортировки спрайтов
            foreach (SpriteRenderSettings item in activeHint.spriteRendersSetingList)
            {
                if (item.spriteRenderer != null)
                {
                    item.spriteRenderer.sortingOrder = item.sortingOrder;
                    item.spriteRenderer.sortingLayerName = item.sortingLayerName;
                }
            }

            //восстанавливаем значения блок контроллеров
            foreach (BlockControllerSettings item in activeHint.blockControllersSetingList)
            {
                if (item.blockController != null)
                {
                    item.blockController.handleDragging = item.handleDragging;
                    item.blockController.handleСlick = item.handleСlick;
                    item.blockController.permittedDirection = item.permittedDirection;
                }
            }

            //hintsList.Remove(activeHint);
            activeHint = null;
            deletedByClickingOnCanvas = false;
            return true;
        }
        else
        {
            return false;
        }       
    }


    //вспомогательные
    private static void ChangeSorting(GameObject gameObject, Hint hint) {
        foreach (SpriteRenderer childrenSpriteRenderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if (childrenSpriteRenderer != null)
            {
                hint.spriteRendersSetingList.Add(new SpriteRenderSettings(childrenSpriteRenderer, childrenSpriteRenderer.sortingLayerName, childrenSpriteRenderer.sortingOrder));

                childrenSpriteRenderer.sortingOrder = childrenSpriteRenderer.sortingLayerID;
                childrenSpriteRenderer.sortingLayerName = "Help";
            }
        }
    }

    private static bool HighlightSpecifiedMove(ElementsForNextMove elementsForNextMove)
    {
        MainAnimator.Instance.ElementsForNextMove = elementsForNextMove;
        List<Block> blocks = new List<Block>();

        //получаем все блоки
        foreach (Element item in elementsForNextMove.elementsList)
        {
            blocks.Add(GridBlocks.Instance.GetBlock(item.PositionInGrid));
        }

        //записываем данные для блока в который будет смещен элемент
        ChangeSorting(elementsForNextMove.targetBlock.gameObject, activeHint);

        //записываем разрешенное направление для движения элемента
        BlockController blockController = elementsForNextMove.targetBlock.GetComponent<BlockController>();
        activeHint.blockControllersSetingList.Add(new BlockControllerSettings(blockController, blockController.handleСlick, blockController.handleDragging, blockController.permittedDirection));
        blockController.permittedDirection = elementsForNextMove.oppositeDirectionForMove;

        //перебираем все блоки
        foreach (Block block in blocks)
        {
            if (block != null)
            {
                ChangeSorting(block.gameObject, activeHint);
                blockController = block.GetComponent<BlockController>();
                //сохраняем настройки
                activeHint.blockControllersSetingList.Add(new BlockControllerSettings(blockController, blockController.handleСlick, blockController.handleDragging, blockController.permittedDirection));
                //деактивируем все элементы управления кроме нужного блока
                if (block != elementsForNextMove.blockElementForMove)
                {
                    blockController.handleDragging = false;
                    blockController.handleСlick = false;
                }
                else
                {
                    //записываем разрешенное направление для движения элемента                        
                    blockController.permittedDirection = elementsForNextMove.directionForMove;
                }
            }
            else
            {
                Debug.Log("Не удалось высветить следующий ход");
                return false;
            }
        }
        return true;
    }

    //разрешаем удалить подсказку после определенного времени
    private static void CanvasLiveTime(float time) {
        deletedByClickingOnCanvas = true;
        timeCreateHints = Time.time;
        delayTime = time;
    }

    //попытка удалить по клику
    public static void DeletedByClickingOnCanvas()
    {
        if (deletedByClickingOnCanvas)
        {
            //если прошло больше времени чем указано
            if ((Time.time - delayTime) > timeCreateHints)
            {
                if (DellGameHelp())
                {
                    //выполняем ход
                    GridBlocks.Instance.Move();
                }
            }
        }
    }
}

public class Hint {
    public ElementsTypeEnum elementsTypeEnum;
    public List<BlockControllerSettings> blockControllersSetingList = new List<BlockControllerSettings>();
    public List<SpriteRenderSettings> spriteRendersSetingList = new List<SpriteRenderSettings>();
    public GameObject canvasHelpToPlayer;

    public Hint(ElementsTypeEnum elementsTypeEnum)
    {
        this.elementsTypeEnum = elementsTypeEnum;
    }
}

public class HintStatus
{
    public ElementsTypeEnum elementsTypeEnum;
    public bool status = false;

    public HintStatus(ElementsTypeEnum elementsTypeEnum, bool status)
    {
        this.elementsTypeEnum = elementsTypeEnum;
        this.status = status;
    }
}

public class SpriteRenderSettings {
    public SpriteRenderer spriteRenderer;
    public string sortingLayerName;
    public int sortingOrder;

    public SpriteRenderSettings(SpriteRenderer spriteRenderer, string sortingLayerName, int sortingOrder)
    {
        this.spriteRenderer = spriteRenderer;
        this.sortingLayerName = sortingLayerName;
        this.sortingOrder = sortingOrder;
    }
}

public class BlockControllerSettings
{
    public BlockController blockController;
    public bool handleСlick;
    public bool handleDragging;
    public DirectionEnum permittedDirection;

    public BlockControllerSettings(BlockController blockController, bool handleСlick, bool handleDragging, DirectionEnum permittedDirection)
    {
        this.blockController = blockController;
        this.handleСlick = handleСlick;
        this.handleDragging = handleDragging;
        this.permittedDirection = permittedDirection;
    }
}
