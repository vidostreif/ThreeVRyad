﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Загрязнение - за элементом, собираются при уничтожении элемента. Загрязнение размножается каждый ход на соседние клетки.
public class DirtBehindElement : BehindElement
{
    private GameObject PSNextMove;

    public override IEnumerator PerformActionAfterMove()
    {
        //если нужно активировать этот элемент в этом ходу
        if (!destroyed && ActivationMove == Tasks.Instance.RealMoves)
        {  
            Block block = FoundBlockForSpread();
            BehindElement newBehindElement = null;
            if (block != null)
            {
                newBehindElement = block.CreatBehindElement(GridBlocks.Instance.prefabElement, shape, type, thisTransform);
                yield return new WaitForSeconds(0.1f);
            }

            //если активируется только один элемент из всех, то только для него ищем следующий ход
            bool deactivate = true;
            if (singleItemActivated)
            {
                //пробуем определить у вновь созданного следующий ход
                bool result = false;
                if (newBehindElement != null)
                {
                    result = newBehindElement.FoundNextActionAfterMove();
                }

                if (!result)
                {
                    //пытаемся определить этот элемент для распространения грязи
                    if (FoundNextActionAfterMove())
                    {
                        deactivate = false;
                    }
                }
            }

            if (deactivate)
            {
                ActivationMove = -1;
                if (PSNextMove != null)
                {
                    //Destroy(PSNextMove);
                    PoolManager.Instance.ReturnObjectToPool(PSNextMove);
                    PSNextMove = null;
                    UpdateSprite();
                }
            }
        }
            
    }

    public override bool FoundNextActionAfterMove()
    {
        if (nextProcessedMoveForAction <= Tasks.Instance.RealMoves || ActivationMove == Tasks.Instance.RealMoves)
        {
            Block block = FoundBlockForSpread();

            if (block != null)
            {                
                //находим все блоки с таким же элементом на заднем плане и указываем у них, что элемент для следующего хода найден
                Block[] blocks = GridBlocks.Instance.GetAllBlocksWithCurBehindElements(type, shape);

                nextProcessedMoveForAction = Tasks.Instance.RealMoves + 1 + actionDelay;
                ActivationMove = nextProcessedMoveForAction;

                UpdateSprite(1);
                SoundManager.Instance.PlaySoundInternal(SoundsEnum.Dirt_swelling);

                //создаем эффект что элемент будет активирован
                if (ParticleSystemManager.Instance != null && PSNextMove == null)
                {
                    PSNextMove = ParticleSystemManager.Instance.CreatePS(thisTransform, PSEnum.PSDirtNextAction);
                }

                //обрабатываем все блоки, если натыкаемся на блок в котором найденное время активации такое, же или больше, тогда прерываем
                foreach (Block BlockItem in blocks)
                {
                    BlockItem.BehindElement.NextProcessedMoveForAction = nextProcessedMoveForAction;
                }
                return true;
            }            
        }
        return false;
    }

    //поиск блока для распространения грязи
    private Block FoundBlockForSpread() {

        //распространение на соседний блок
        NeighboringBlocks neighboringBlocks = GridBlocks.Instance.GetNeighboringBlocks(this.PositionInGrid);
        SupportFunctions.MixArray(neighboringBlocks.allBlockField);//перемешаем соседние блоки

        foreach (Block block in neighboringBlocks.allBlockField)
        {
            //находим стандартный элемент
            if (BlockCheck.ThisStandardBlockWithStandartElement(block))
            {
                if (block.BehindElement == null || block.BehindElement.Destroyed)
                {
                    return block;
                }
            }
        }
        return null;
    }
}
