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
                hintForDell.Add(hint);
                //создаем затемнение 
                activeHint.canvasHelpToPlayer = UnityEngine.Object.Instantiate(PrefabBank.Instance.canvasHelpToPlayer);
                Image imageHelpToPlayer = activeHint.canvasHelpToPlayer.GetComponentInChildren<Image>();
                MainAnimator.Instance.AddElementForSmoothChangeColor(imageHelpToPlayer, new Color(imageHelpToPlayer.color.r, imageHelpToPlayer.color.g, imageHelpToPlayer.color.b, 0.7f), 2);
                //устанавливаем камеру
                activeHint.canvasHelpToPlayer.GetComponent<Canvas>().worldCamera = Camera.main;

                //находим нужную подсказку
                if (activeHint.elementsTypeEnum == ElementsTypeEnum.Standard)
                {
                    created = CreateElementsTypeStandardHelp();
                }
                else if (activeHint.elementsTypeEnum == ElementsTypeEnum.CrushableWall)
                {
                    //перебираем все наши стены
                    //получить все возможные линии и если есть линия рядом с нашим блоком, то создаем подсказку
                    //где подсвечиваем блок и возможную линию
                }
                else if (activeHint.elementsTypeEnum == ElementsTypeEnum.SmallFlask || activeHint.elementsTypeEnum == ElementsTypeEnum.MediumFlask || activeHint.elementsTypeEnum == ElementsTypeEnum.BigFlask)
                {
                    created = CreateElementsTypeFlaskHelp(activeHint.elementsTypeEnum);
                }
                else
                {
                    //неудалось определить подсказку
                }

                //если удалось создать подсказку, выходим из цикла
                if (created)
                {
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

    public static bool CreateElementsTypeStandardHelp() {
        ElementsForNextMove elementsForNextMove = GridBlocks.Instance.CheckElementsForNextMove();
        //Если нет доступных ходов, то выходим
        if (elementsForNextMove.elementsList.Count == 0)
        {
            DellGameHelp();
            return false;
        }

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
                Debug.Log("Не удалось создать подсказку");
                //восстанавливаем значения
                DellGameHelp();
                return false;
            }
        }
        //помечаем как показанную
        hintsStatus[(int)activeHint.elementsTypeEnum].status = true;
        return true;
    }

    public static bool CreateElementsTypeFlaskHelp(ElementsTypeEnum elementsTypeEnum) {
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
                //помечаем как показанную
                hintsStatus[(int)activeHint.elementsTypeEnum].status = true;
                return true;
            }
        }
        Debug.Log("Не нашли не одной маленькой фласки для создания подсказки!");
        //восстанавливаем значения
        DellGameHelp();
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
