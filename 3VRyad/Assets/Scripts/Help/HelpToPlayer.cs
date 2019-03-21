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

    public static void CreateNextGameHelp()
    {
        //!!!добавить все блоки в список для запрета обработки

        Hint hint = null;
        if (hintsList.Count > 0)
        {
            hint = hintsList[0];
            activeHint = hint;
        }
        else
        {
            return;
        }        

        if (hint.elementsTypeEnum == ElementsTypeEnum.Standard)
        {
            ElementsForNextMove elementsForNextMove = MainAnimator.Instance.ElementsForNextMove;
            List<Block> blocks = new List<Block>();

            //создаем затемнение 
            hint.canvasHelpToPlayer = UnityEngine.Object.Instantiate(PrefabBank.Instance.canvasHelpToPlayer);
            Image imageHelpToPlayer = hint.canvasHelpToPlayer.GetComponentInChildren<Image>();
            MainAnimator.Instance.AddElementForSmoothChangeColor(imageHelpToPlayer, new Color(imageHelpToPlayer.color.r, imageHelpToPlayer.color.g, imageHelpToPlayer.color.b, 0.7f), 2);
            //устанавливаем камеру
            hint.canvasHelpToPlayer.GetComponent<Canvas>().worldCamera = Camera.main;
            //получаем все блоки
            foreach (Element item in elementsForNextMove.elementsList)
            {
                blocks.Add(GridBlocks.Instance.GetBlock(item.PositionInGrid));
            }

            //записываем данные для блока в который будет смещен элемент
            foreach (SpriteRenderer childrenSpriteRenderer in elementsForNextMove.targetBlock.GetComponentsInChildren<SpriteRenderer>())
            {
                if (childrenSpriteRenderer != null)
                {
                    hint.spriteRendersSetingList.Add(new SpriteRenderSettings(childrenSpriteRenderer, childrenSpriteRenderer.sortingLayerName, childrenSpriteRenderer.sortingOrder));

                    childrenSpriteRenderer.sortingOrder = childrenSpriteRenderer.sortingLayerID;
                    childrenSpriteRenderer.sortingLayerName = "Help";
                }
            }
                        
            //записываем разрешенное направление для движения элемента
            BlockController blockController = elementsForNextMove.targetBlock.GetComponent<BlockController>();
            blockController.permittedDirection = elementsForNextMove.oppositeDirectionForMove;

            //перебираем все блоки
            foreach (Block block in blocks)
            {
                if (block != null)
                {

                    foreach (SpriteRenderer childrenSpriteRenderer in block.GetComponentsInChildren<SpriteRenderer>())
                    {
                        if (childrenSpriteRenderer != null)
                        {
                            hint.spriteRendersSetingList.Add(new SpriteRenderSettings(childrenSpriteRenderer, childrenSpriteRenderer.sortingLayerName, childrenSpriteRenderer.sortingOrder));
                            
                            childrenSpriteRenderer.sortingOrder = childrenSpriteRenderer.sortingLayerID;
                            childrenSpriteRenderer.sortingLayerName = "Help";
                        }
                    }

                    
                    blockController = block.GetComponent<BlockController>();
                    //сохраняем настройки
                    hint.blockControllersSetingList.Add(new BlockControllerSettings(blockController, blockController.handleСlick, blockController.handleDragging, blockController.permittedDirection));
                    //деактивируем все элементы управления кроме нужного блока
                    if (block != elementsForNextMove.blockElementForMove)
                    {
                        //BoxCollider2D boxCollider2D = block.GetComponent<BoxCollider2D>();
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
                    return;
                }                
            }
            //помечаем как показанную
            hintsStatus[(int)hint.elementsTypeEnum].status = true;
        }        
    }

    public static void DellGameHelp()
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

            hintsList.Remove(activeHint);
            activeHint = null;
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
