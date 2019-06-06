using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Загрязнение - за элементом, собираются при уничтожении элемента. Загрязнение размножается каждый ход на соседние клетки.
public class DirtBehindElement : BehindElement
{
    public override void PerformActionAfterMove()
    {
        //проверяем что не активировали в этом ходу
        if (!destroyed && lastActivationMove > Tasks.Instance.Moves)
        {
            if (actionDelay == 0)
            {
                actionDelay = startingActionDelay;
                UpdateSpriteAlfa();

                //распространение на соседний блок
                NeighboringBlocks neighboringBlocks = GridBlocks.Instance.GetNeighboringBlocks(this.PositionInGrid);
                SupportFunctions.MixArray(neighboringBlocks.allBlockField);//перемешаем соседние блоки

                foreach (Block block in neighboringBlocks.allBlockField)
                {
                    //находим не заблокированный элемент
                    if (BlockCheck.ThisStandardBlockWithStandartElementCanMove(block))
                    {
                        if (block.BehindElement == null || block.BehindElement.Destroyed)
                        {
                            block.CreatBehindElement(GridBlocks.Instance.prefabElement, shape, type);
                            break;
                        }
                    }
                }

                //!!!находим все блоки с грязью вокруг и указываем в них, что они уже выполнили действие

                lastActivationMove = Tasks.Instance.Moves;
            }
            else
            {   
                actionDelay--;
                UpdateSpriteAlfa();
            }
            
        }
    }
}
