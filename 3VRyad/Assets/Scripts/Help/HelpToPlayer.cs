using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelpToPlayer
{
    private static List<SpriteRenderSortingLayer> spriteRenderSortingLayers = new List<SpriteRenderSortingLayer>();
    private static GameObject canvasHelpToPlayer;

    public static void CreateGameHelp(ElementsTypeEnum elementsTypeEnum)
    {
        //добавить все блоки в список для запрета обработки

        if (elementsTypeEnum == ElementsTypeEnum.Standard)
        {
            ElementsForNextMove elementsForNextMove = MainAnimator.Instance.ElementsForNextMove;
            List<Block> blocks = new List<Block>();
            //создаем затемнение 
            canvasHelpToPlayer = Object.Instantiate(PrefabBank.Instance.canvasHelpToPlayer);

            //canvasHelpToPlayer.GetComponent<Canvas>().Render

            foreach (Element item in MainAnimator.Instance.ElementsForNextMove.elementsList)
            {
                blocks.Add(GridBlocks.Instance.GetBlock(item.PositionInGrid));
            }

            //записываем данные для блока в который будет смещен элемент
            foreach (SpriteRenderer childrenSpriteRenderer in elementsForNextMove.targetBlock.GetComponentsInChildren<SpriteRenderer>())
            {
                if (childrenSpriteRenderer != null)
                {
                    spriteRenderSortingLayers.Add(new SpriteRenderSortingLayer(childrenSpriteRenderer, childrenSpriteRenderer.sortingLayerName, childrenSpriteRenderer.sortingOrder));

                    childrenSpriteRenderer.sortingOrder = childrenSpriteRenderer.sortingLayerID;
                    childrenSpriteRenderer.sortingLayerName = "Help";
                }
            }
                        
            //записываем разрешенное направление для движения элемента
            BlockController blockController = elementsForNextMove.targetBlock.GetComponent<BlockController>();
            blockController.permittedDirection = MainAnimator.Instance.ElementsForNextMove.oppositeDirectionForMove;

            //перебираем все блоки
            foreach (Block block in blocks)
            {
                if (block != null)
                {

                    foreach (SpriteRenderer childrenSpriteRenderer in block.GetComponentsInChildren<SpriteRenderer>())
                    {
                        if (childrenSpriteRenderer != null)
                        {
                            spriteRenderSortingLayers.Add(new SpriteRenderSortingLayer(childrenSpriteRenderer, childrenSpriteRenderer.sortingLayerName, childrenSpriteRenderer.sortingOrder));
                            
                            childrenSpriteRenderer.sortingOrder = childrenSpriteRenderer.sortingLayerID;
                            childrenSpriteRenderer.sortingLayerName = "Help";
                        }
                    }

                    //деактивируем все элементы управления кроме нужного блока
                    if (block != MainAnimator.Instance.ElementsForNextMove.blockElementForMove)
                    {
                        BoxCollider2D boxCollider2D = block.GetComponent<BoxCollider2D>();
                        boxCollider2D.enabled = false;
                    }
                    else
                    {
                        //записываем разрешенное направление для движения элемента
                        blockController = block.GetComponent<BlockController>();
                        blockController.permittedDirection = MainAnimator.Instance.ElementsForNextMove.directionForMove;
                    }
                }
                else
                {
                    Debug.Log("Не удалось создать подсказку");

                    //восстанавливаем значения
                    DellGameHelp();
                    break;
                }                
            }   
            
        }

        
    }

    public static void DellGameHelp()
    {
        //удаляем затемнение
        Object.Destroy(canvasHelpToPlayer);

        //восстанавливаем значения 
        foreach (SpriteRenderSortingLayer item in spriteRenderSortingLayers)
        {
            if (item != null)
            {
                item.spriteRenderer.sortingOrder = item.sortingOrder;
                item.spriteRenderer.sortingLayerName = item.sortingLayerName;
            }            
        }
    }
}

public class SpriteRenderSortingLayer {
    public SpriteRenderer spriteRenderer;
    public string sortingLayerName;
    public int sortingOrder;

    public SpriteRenderSortingLayer(SpriteRenderer spriteRenderer, string sortingLayerName, int sortingOrder)
    {
        this.spriteRenderer = spriteRenderer;
        this.sortingLayerName = sortingLayerName;
        this.sortingOrder = sortingOrder;
    }
}
