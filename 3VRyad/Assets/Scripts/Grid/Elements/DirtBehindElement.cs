using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Загрязнение - за элементом, собираются при уничтожении элемента. Загрязнение размножается каждый ход на соседние клетки.
public class DirtBehindElement : BehindElement
{
    public override void PerformActionAfterMove()
    {
        if (!destroyed)
        {
            //распространение на соседний блок
            NeighboringBlocks neighboringBlocks = GridBlocks.Instance.DeterminingNeighboringBlocks(GridBlocks.Instance.FindPosition(this));
            SupportFunctions.MixArray(neighboringBlocks.allBlockField);//перемешаем соседние блоки

            foreach (Block block in neighboringBlocks.allBlockField)
            {
                //находим не заблокированный элемент
                if (GridBlocks.Instance.ThisStandardBlockWithStandartElementCanMove(block))
                {
                    if (block.BehindElement == null || block.BehindElement.Destroyed)
                    {
                        block.CreatBehindElement(GridBlocks.Instance.prefabElement, shape, type);
                        break;
                    }
                }
            }
        }
    }
}
