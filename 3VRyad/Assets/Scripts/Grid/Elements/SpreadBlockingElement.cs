using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadBlockingElement : BlockingElement
{

    private GameObject PSNextMove;

    public override void PerformActionAfterMove()
    {
        //если нужно активировать этот элемент в этом ходу
        if (!destroyed && ActivationMove == Tasks.Instance.RealMoves)
        {
            Block block = FoundBlockForSpread();
            BlockingElement newElement = null;
            if (block != null)
            {
                newElement = block.Element.CreatBlockingElement(GridBlocks.Instance.prefabBlockingWall, shape, type, thisTransform);
            }

            //если активируется только один элемент из всех, то только для него ищем следующий ход
            bool deactivate = true;
            if (singleItemActivated)
            {
                //пробуем определить у вновь созданного следующий ход
                bool result = false;
                if (newElement != null)
                {
                    result = newElement.FoundNextActionAfterMove();
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
                ActivationMove = int.MaxValue;
                if (PSNextMove != null)
                {
                    Destroy(PSNextMove);                    
                }
                UpdateSprite();
            }
        }
        //если остался один ход до активации
        else if (!destroyed && ActivationMove - 1 == Tasks.Instance.RealMoves)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Spider_2);
            UpdateSprite(1);
        }
        //если осталось два хода до активации
        else if (!destroyed && ActivationMove - 2 == Tasks.Instance.RealMoves)
        {
            
            UpdateSprite(2);
        }
    }

    public override bool FoundNextActionAfterMove()
    {
        if (nextProcessedMoveForAction <= Tasks.Instance.RealMoves || ActivationMove == Tasks.Instance.RealMoves)
        {
            Block block = FoundBlockForSpread();

            if (block != null)
            {
                //находим все блоки с таким же элементом и указываем у них, что элемент для следующего хода найден
                Block[] blocks = GridBlocks.Instance.GetAllBlocksWithCurBlockingElements(type, shape);

                nextProcessedMoveForAction = Tasks.Instance.RealMoves + 1 + actionDelay;
                ActivationMove = nextProcessedMoveForAction;

                UpdateSprite(actionDelay + 1);
                //звук появления паука
                SoundManager.Instance.PlaySoundInternal(SoundsEnum.Spider_1);

                ////создаем эффект что элемент будет активирован
                //if (ParticleSystemManager.Instance != null && PSNextMove == null)
                //{
                //    PSNextMove = ParticleSystemManager.Instance.CreatePS(thisTransform, PSEnum.PSDirtNextAction);
                //}

                //обрабатываем все блоки, если натыкаемся на блок в котором найденное время активации такое, же или больше, тогда прерываем
                foreach (Block BlockItem in blocks)
                {
                    BlockItem.Element.BlockingElement.NextProcessedMoveForAction = nextProcessedMoveForAction;
                }
                return true;
            }
        }
        return false;
    }

    //поиск блока для распространения
    private Block FoundBlockForSpread()
    {
        //распространение на соседний блок
        NeighboringBlocks neighboringBlocks = GridBlocks.Instance.GetNeighboringBlocks(this.PositionInGrid);
        SupportFunctions.MixArray(neighboringBlocks.allBlockField);//перемешаем соседние блоки

        foreach (Block block in neighboringBlocks.allBlockField)
        {
            //находим не заблокированный элемент без блокирующего элемента
            if (BlockCheck.ThisStandardBlockWithStandartElementCanMove(block))
            {
                if (block.Element.BlockingElement == null || block.Element.BlockingElement.Destroyed)
                {
                    return block;
                }
            }
        }
        return null;
    }

    //в случае уничтожения элемента
    protected override void DestroyElement() {
        //активируется в этот ход или в последующие
        if (ActivationMove <= Tasks.Instance.Moves)
        {
            //звук смерти паука


            //находим все блоки с таким же элементом и указываем у них, что нужно найти новый элемент для действия в этом ходу
            Block[] blocks = GridBlocks.Instance.GetAllBlocksWithCurBlockingElements(type, shape);
            foreach (Block BlockItem in blocks)
            {
                BlockItem.Element.BlockingElement.NextProcessedMoveForAction = Tasks.Instance.Moves;
            }
        }

        base.DestroyElement();
    }
}
