using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadBlockingElement : BlockingElement
{
    public override void PerformActionAfterMove()
    {
        if (!destroyed)
        {
            if (actionDelay == 0)
            {
                actionDelay = startingActionDelay;
                UpdateSpriteAlfa();

                //распространение на соседний блок
                NeighboringBlocks neighboringBlocks = GridBlocks.Instance.DeterminingNeighboringBlocks(this.PositionInGrid);
                SupportFunctions.MixArray(neighboringBlocks.allBlockField);//перемешаем соседние блоки

                foreach (Block block in neighboringBlocks.allBlockField)
                {
                    //находим не заблокированный элемент
                    if (BlockCheck.ThisStandardBlockWithStandartElementCanMove(block))
                    {
                        if (block.Element.BlockingElement == null || block.Element.BlockingElement.Destroyed)
                        {
                            block.Element.CreatBlockingElement(GridBlocks.Instance.prefabElement, shape, type);
                            break;
                        }
                    }
                }
            }
            else
            {
                actionDelay--;
                UpdateSpriteAlfa();
            }

        }
    }
}
