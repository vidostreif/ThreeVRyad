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
            if (timerActionDelay == actionDelay)
            {                
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
                            block.CreatBehindElement(GridBlocks.Instance.prefabElement, shape, type, thisTransform);
                            timerActionDelay = 0;
                            UpdateSprite();
                            break;
                        }
                    }
                }

                lastActivationMove = Tasks.Instance.Moves;
            }
            else if (timerActionDelay < actionDelay)
            {                
                timerActionDelay++;
                UpdateSprite();
            }
            
        }
    }

    protected override void UpdateSprite()
    {
        base.UpdateSprite();
        if (ParticleSystemManager.Instance != null)
        {
            //анимация
            ParticleSystemManager.Instance.CreatePSAsync(thisTransform, PSEnum.PSDirt, 3);
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Dirt_swelling);
        }        
    }

}
