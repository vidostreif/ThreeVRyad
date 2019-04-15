﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class HelpToPlayer
{
    private static List<Type> enumTypes = new List<Type>();
    private static List<Hint> hintsList = new List<Hint>();
    private static HintStatus[] hintsStatus = null;
    private static Hint activeHint = null;

    private static bool deletedByClickingOnCanvas = false;
    private static float timeCreateHints;
    private static float delayTime = 0;

    private static void CreateHintStatusList() {
        if (hintsStatus == null)
        {
            //список enum которые используются для подсказок
            enumTypes = new List<Type>();
            enumTypes.Add(typeof(ElementsTypeEnum));
            enumTypes.Add(typeof(HelpEnum));

            //создаем массив статусов подсказок
            int count = 0;
            foreach (Type item in enumTypes)
            {
                count += Enum.GetNames(item).Length;
            }
            hintsStatus = new HintStatus[count];

            //загружаем сохранения
            List <HelpSave> helpSaves = JsonSaveAndLoad.LoadSave().helpSave;
            int i = 0;
            foreach (Type enumType in enumTypes)
            {
                foreach (var curEnum in Enum.GetValues(enumType))
                {
                    HelpSave helpSave = helpSaves.Find(item => item.elementsTypeEnum == curEnum.ToString());
                    //ищем в сохранениях
                    if (helpSave != null)
                    {
                        hintsStatus[i] = new HintStatus(curEnum.ToString(), helpSave.status);
                    }
                    else
                    {
                        hintsStatus[i] = new HintStatus(curEnum.ToString(), false);
                    }

                    i++;
                }
            }
        }
    }

    public static void ClearHintList()
    {
        hintsList.Clear();//очищаем список подсказок
    }

    public static void AddHint(ElementsTypeEnum elementsTypeEnum) {

        AddHint(typeof(ElementsTypeEnum), elementsTypeEnum.ToString(), (int)elementsTypeEnum, false);
    }

    public static void AddHint(HelpEnum helpEnum)
    {
        AddHint(typeof(HelpEnum), helpEnum.ToString(), (int)helpEnum, true);
    }
    
    private static void AddHint(Type enumType, string help, int number, bool toTop) {
        //проверяем показывали ли мы такую подсказку игроку
        CreateHintStatusList();

        //определяем позицию в энумах
        int count = 0;
        foreach (Type item in enumTypes)
        {
            if (item != enumType)
            {
                count+= Enum.GetNames(item).Length;
            }
            else
            {
                break;
            }
        }

        //если уже показывали то не добавляем
        if (hintsStatus[count + number].status == false)
        {
            //если она уже добавлена в массив, то пропускаем
            Hint hint = hintsList.Find(item => item.help == help);
            if (hint == null)
            {
                if (toTop)
                {
                    hintsList.Insert(0, new Hint(help, count + number));
                }
                else
                {
                    hintsList.Add(new Hint(help, count + number));
                }               
            }
        }
    }

    public static bool CreateNextGameHelp()
    {
        bool created = false;
        if (hintsList.Count > 0 && activeHint == null)
        {
            //!!!добавить все блоки в список для запрета обработки
            
            List<Hint> hintForDell = new List<Hint>();
            
            //перебираем подскази до тех пор пока не создадим хоть одну
            foreach (Hint hint in hintsList)
            {
                activeHint = hint;
                activeHint.canvasHelpToPlayer = UnityEngine.Object.Instantiate(PrefabBank.Instance.canvasHelpToPlayer);

                //находим нужную подсказку
                if (activeHint.help == ElementsTypeEnum.Standard.ToString())
                {
                    created = CreateStandardElementHelp();
                }
                else if (activeHint.help == ElementsTypeEnum.CrushableWall.ToString())
                {                    
                    created = CreateCrushableWallHelp((ElementsTypeEnum)Enum.Parse(typeof(ElementsTypeEnum), activeHint.help));
                }
                else if (activeHint.help == ElementsTypeEnum.SmallFlask.ToString() || activeHint.help == ElementsTypeEnum.MediumFlask.ToString() || activeHint.help == ElementsTypeEnum.BigFlask.ToString())
                {
                    created = CreateFlaskHelp((ElementsTypeEnum)Enum.Parse(typeof(ElementsTypeEnum), activeHint.help));
                }
                else if (activeHint.help == ElementsTypeEnum.ImmortalWall.ToString())
                {
                    created = CreateImortalWallHelp();
                }
                else if (activeHint.help == ElementsTypeEnum.Drop.ToString())
                {
                    created = CreateDropElementHelp();
                }
                else if (activeHint.help == HelpEnum.Gnome.ToString())
                {
                    created = InterfaceHelp("Gnome");
                }
                else if (activeHint.help == HelpEnum.Tasks.ToString())
                {
                    created = InterfaceHelp("PanelCollectsElement");
                }
                else if (activeHint.help == HelpEnum.Score.ToString())
                {
                    created = InterfaceHelp("Score");
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
                    Image imageHelpToPlayer = activeHint.canvasHelpToPlayer.GetComponentInChildren<Image>();
                    MainAnimator.Instance.AddElementForSmoothChangeColor(imageHelpToPlayer, new Color(imageHelpToPlayer.color.r, imageHelpToPlayer.color.g, imageHelpToPlayer.color.b, 0.9f), 2);
                    //устанавливаем камеру
                    activeHint.canvasHelpToPlayer.GetComponent<Canvas>().worldCamera = Camera.main;
                    //добавляем действие
                    Transform gOPanel = activeHint.canvasHelpToPlayer.transform.Find("Panel");
                    Button button = gOPanel.GetComponent<Button>();
                    button.onClick.AddListener(delegate { DeletedByClickingOnCanvas(); });

                    //показываем текст
                    CreateTextCloud();

                    //помечаем как показанную
                    hintsStatus[activeHint.numberHelp].status = true;
                    //добавляем для удаления
                    hintForDell.Add(hint);

                    MasterController.Instance.ForcedDropElement();//если игрок перетаскивает элемент, то бросаем его с возвратом на позицию
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
        //собераем все трансформы
        List<Transform> transformsList = new List<Transform>();
        foreach (SpriteRenderSettings item in activeHint.spriteRendersSetingList)
        {
            transformsList.Add(item.spriteRenderer.transform);
        }
        foreach (ParentSettings item in activeHint.ParentSettingsList)
        {
            transformsList.Add(item.gameObjectTransform);
        }

        if (transformsList.Count > 0)
        {
            
            Vector3 newPosition = transformsList[0].position;
            float width = transformsList[0].localScale.x;
            Transform gOPanel = activeHint.canvasHelpToPlayer.transform.Find("Panel");
            Transform textCloud = gOPanel.transform.Find("TextCloud");
            RectTransform rectTransformGOPanel = gOPanel.GetComponent<RectTransform>();
            RectTransform rectTransformTextCloud = textCloud.GetComponent<RectTransform>();

            //находим самый левый верхний объект
            foreach (Transform item in transformsList)
            {
                //обрабатываем все кроме элементов
                if (!item.GetComponent<BaseElement>())
                {
                    if (item.position.x < newPosition.x)
                    {
                        newPosition.x = item.position.x;
                        width = item.localScale.x; // ширина
                    }

                    if (item.position.y > newPosition.y)
                    {
                        newPosition.y = item.position.y;
                        width = item.localScale.x; // ширина
                    }
                }
            }
            textCloud.position = new Vector3(newPosition.x - width, newPosition.y, newPosition.z);
            rectTransformTextCloud.anchoredPosition = new Vector3(rectTransformTextCloud.anchoredPosition.x - rectTransformTextCloud.rect.width * 0.5f, rectTransformTextCloud.anchoredPosition.y);

            //если выходит за пределы экрана слева переносим на право
            if (rectTransformTextCloud.anchoredPosition.x < rectTransformTextCloud.rect.width * 0.5f)
            {
                //находим самый правый верхний объект
                newPosition = transformsList[0].position;
                width = transformsList[0].localScale.x;
                foreach (Transform item in transformsList)
                {
                    //обрабатываем все кроме элементов
                    if (!item.GetComponent<BaseElement>())
                    {
                        if (item.position.x > newPosition.x)
                        {
                            newPosition.x = item.position.x;
                            width = item.localScale.x; // ширина
                        }

                        if (item.position.y > newPosition.y)
                        {
                            newPosition.y = item.position.y;
                            width = item.localScale.x; // ширина
                        }
                    }
                }
                textCloud.position = new Vector3(newPosition.x + width, newPosition.y, newPosition.z);
                rectTransformTextCloud.anchoredPosition = new Vector3(rectTransformTextCloud.anchoredPosition.x + rectTransformTextCloud.rect.width * 0.5f, rectTransformTextCloud.anchoredPosition.y);
            }

            //если слишков высоко, то смещаем вниз до пределов экрана
            if (-rectTransformTextCloud.anchoredPosition.y < rectTransformTextCloud.rect.height * 0.5f)
            {
                rectTransformTextCloud.anchoredPosition = new Vector3(rectTransformTextCloud.anchoredPosition.x, -rectTransformTextCloud.rect.height * 0.5f);
            }

            //если слишков низко, то смещаем вверх до пределов экрана
            if (rectTransformGOPanel.rect.height < -rectTransformTextCloud.anchoredPosition.y + rectTransformTextCloud.rect.height * 0.5f)
            {
                rectTransformTextCloud.anchoredPosition = new Vector3(rectTransformTextCloud.anchoredPosition.x, -rectTransformGOPanel.rect.height + rectTransformTextCloud.rect.height * 0.5f);
            }

            Text text = textCloud.GetComponentInChildren<Text>();

            if (activeHint.help == ElementsTypeEnum.Standard.ToString())
            {
                text.text = "Что бы собрать растения в нашем саду, их нужно собрать в линии из более чем двух растений. Попробуйте передвинуть выделяющееся растение!";
            }
            else if (activeHint.help == ElementsTypeEnum.SmallFlask.ToString())
            {
                text.text = "Это маленький бонус за сбор линии из 4 растений. Нажмите на него дважды и он взорвет всю выделенную область!";
            }
            else if (activeHint.help == ElementsTypeEnum.MediumFlask.ToString())
            {
                text.text = "Это маленький бонус за сбор линии из 5 растений. Нажмите на него дважды и он взорвет всю выделенную область!";
            }
            else if (activeHint.help == ElementsTypeEnum.BigFlask.ToString())
            {
                text.text = "Это маленький бонус за сбор линии из 6 растений. Нажмите на него дважды и он взорвет всю выделенную область!";
            }
            else if (activeHint.help == ElementsTypeEnum.CrushableWall.ToString())
            {
                text.text = "Это стену вы можете взорвать или уничтожить собрав линию рядом! Соберите линию.";
            }
            else if (activeHint.help == ElementsTypeEnum.ImmortalWall.ToString())
            {
                text.text = "Это стену вы не сможете разрушить, она очень крепкая!";
            }
            else if (activeHint.help == ElementsTypeEnum.Drop.ToString())
            {
                text.text = "Этот элемент лишний на поле, и что бы его убрать, его нужно согнать через все поле в самый низ!";
            }
            else if (activeHint.help == ElementsTypeEnum.SeedBarrel.ToString())
            {
                text.text = "Это бочка с зернами, она наполняется когда рядом собираются растения указанные на бочке!";
            }
            else if (activeHint.help == HelpEnum.Gnome.ToString())
            {
                text.text = "Привет! Я твой помошник и буду всячески тебе помогать!";
            }
            else if (activeHint.help == HelpEnum.Tasks.ToString())
            {
                text.text = "На этой панеле показано, сколько элементов нужно собрать и сколько ходов для этого осталось!";
            }
            else if (activeHint.help == HelpEnum.Score.ToString())
            {
                text.text = "Здесь ты видишь количество набранных очков. Чем больше очков ты наберешь, тем больше звезд получишь!";
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

    public static bool DellGameHelp()
    {
        //если есть подсказка для элементов
        if (activeHint != null)
        {
            
            //восстанавливаем значения сортировки спрайтов
            foreach (SpriteRenderSettings item in activeHint.spriteRendersSetingList)
            {
                if (item.spriteRenderer != null)
                {
                    item.spriteRenderer.sortingOrder = item.sortingOrder;
                    item.spriteRenderer.sortingLayerName = item.sortingLayerName;
                }
            }

            //восстанавливаем значения родителя
            foreach (ParentSettings item in activeHint.ParentSettingsList)
            {
                if (item.gameObjectTransform != null && item.parentTransform != null)
                {
                    item.gameObjectTransform.SetParent(item.parentTransform, false);
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

            //удаляем затемнение
            UnityEngine.Object.Destroy(activeHint.canvasHelpToPlayer);

            deletedByClickingOnCanvas = false;
            JsonSaveAndLoad.RecordSave(hintsStatus);
            activeHint = null;
            return true;
        }
        else
        {
            return false;
        }
    }

    //разрешаем удалить подсказку после определенного времени
    private static void CanvasLiveTime(float time)
    {
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
                if (activeHint != null)
                {
                    bool createNextGameHelpByClicking = activeHint.createNextGameHelpByClicking;
                    if (DellGameHelp())
                    {
                        if (createNextGameHelpByClicking)
                        {
                            CreateNextGameHelp();
                        }
                        else
                        {
                            //выполняем ход
                            GridBlocks.Instance.Move();
                        }                        
                    }
                }                
            }
        }
    }

    //подсказки для элементов
    private static bool CreateStandardElementHelp()
    {
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

    private static bool CreateFlaskHelp(ElementsTypeEnum elementsTypeEnum)
    {
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

                //добавляем эффект

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

                                    //добавляем эффект

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

                //добавляем эффект

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

                //добавляем эффект

                //таймаут для удаления подсказки
                CanvasLiveTime(3);
                return true;
            }
        }
        Debug.Log("Не нашли ни одного сбрасываемого элемента!");
        return false;
    }


    //подсказки для интерфейса
    private static bool InterfaceHelp(string goName)
    {
        //находим гнома
        GameObject go = GameObject.Find(goName);

        if (go != null)
        {
            ChangeParent(go, activeHint);
            //таймаут для удаления подсказки
            activeHint.createNextGameHelpByClicking = true;
            CanvasLiveTime(1);
            return true;
        }
        else
        {
            Debug.Log("Не нашли " + goName + " для создания подсказки!");
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

    private static void ChangeParent(GameObject gameObject, Hint hint)
    {
            if (gameObject.transform != null)
            {
                hint.ParentSettingsList.Add(new ParentSettings(gameObject.transform, gameObject.transform.parent));

                gameObject.transform.SetParent(hint.canvasHelpToPlayer.transform.Find("Panel"), false);
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
            if (block != null && !GridBlocks.Instance.BlockInProcessing(block))
            {
                //block.Blocked = true;
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

}

public class Hint {
    public string help;
    public int numberHelp;
    public bool createNextGameHelpByClicking = false;
    public List<BlockControllerSettings> blockControllersSetingList = new List<BlockControllerSettings>();
    public List<SpriteRenderSettings> spriteRendersSetingList = new List<SpriteRenderSettings>();
    public List<ParentSettings> ParentSettingsList = new List<ParentSettings>();
    public GameObject canvasHelpToPlayer;

    public Hint(string help, int numberHelp)
    {
        this.help = help;
        this.numberHelp = numberHelp;
    }
}

public class HintStatus
{
    public string help;
    public bool status = false;

    public HintStatus(string help, bool status)
    {
        this.help = help;
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

public class ParentSettings
{
    public Transform gameObjectTransform;
    public Transform parentTransform;

    public ParentSettings(Transform gameObjectTransform, Transform parentTransform)
    {
        this.gameObjectTransform = gameObjectTransform;
        this.parentTransform = parentTransform;
    }
}