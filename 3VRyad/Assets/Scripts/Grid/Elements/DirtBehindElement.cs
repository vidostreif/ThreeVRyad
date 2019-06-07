using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Загрязнение - за элементом, собираются при уничтожении элемента. Загрязнение размножается каждый ход на соседние клетки.
public class DirtBehindElement : BehindElement
{
    private GameObject PSNextMove;

    public override void PerformActionAfterMove()
    {
        //если нужно активировать этот элемент в этом ходу
        if (!destroyed && ActivationMove == Tasks.Instance.Moves)
        {  
            Block block = FoundBlockForSpread();

            if (block != null)
            {
                block.CreatBehindElement(GridBlocks.Instance.prefabElement, shape, type, thisTransform);
            }

            //если активируется только один элемент из всех, то только для него ищем следующий ход
            if (singleItemActivated)
            {
                //ищем следующий элемент для распространения грязи
                if (!FoundNextActionAfterMove())
                {
                    ActivationMove = int.MaxValue;
                    if (PSNextMove != null)
                    {
                        Destroy(PSNextMove);
                        //Debug.Log("Уничтожение PSNextMove");
                        UpdateSprite();
                    }
                }
            }
            else
            {
                ActivationMove = int.MaxValue;
                if (PSNextMove != null)
                {
                    Destroy(PSNextMove);
                    //Debug.Log("Уничтожение PSNextMove");
                    UpdateSprite();
                }
            }            
        }
        //else
        //{
        //    //если активируется только один элемент из всех, то только для него ищем следующий ход
        //    if (singleItemActivated)
        //    {
        //        //ищем следующий элемент для распространения грязи
        //        FoundNextActionAfterMove();
        //    }
        //}             
    }

    public override bool FoundNextActionAfterMove()
    {
        if (nextProcessedMoveForAction >= Tasks.Instance.Moves || ActivationMove == Tasks.Instance.Moves)
        {
            Block block = FoundBlockForSpread();

            if (block != null)
            {                
                //находим все блоки с таким же элементом на заднем плане и указываем у них, что элемент для следующего хода найден
                Block[] blocks = GridBlocks.Instance.GetAllBlocksWithCurBehindElements(type, shape);

                ////обрабатываем все блоки, если натыкаемся на блок в котором найденное время активации такое, же или больше, тогда прерываем
                //foreach (Block BlockItem in blocks)
                //{
                //    if (Tasks.Instance.Moves - 1 - actionDelay >= BlockItem.BehindElement.NextProcessedMoveForAction)
                //    {
                //        nextProcessedMoveForAction = BlockItem.BehindElement.NextProcessedMoveForAction;
                //        return false;
                //    }
                //}
                nextProcessedMoveForAction = Tasks.Instance.Moves - 1 - actionDelay;
                ActivationMove = nextProcessedMoveForAction;

                UpdateSprite(1);

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
            //находим не заблокированный элемент
            if (BlockCheck.ThisStandardBlockWithStandartElementCanMove(block))
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
