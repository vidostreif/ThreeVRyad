using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//куст распространяющий ягоды если собрать комбинацию рядом
public class MagicBushElement : Element
{
    bool wasHit;
    protected override void DopSettings()
    {
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.Explosion, HitTypeEnum.Instrument };
        wasHit = false;
    }

    //действие после не смертельного удара
    protected override void ActionAfterHitting(HitTypeEnum hitType)
    {
        if (ActivationMove - 1 <= Tasks.Instance.RealMoves)
        {
            UpdateSprite(2);
            wasHit = true;            
        }

        if (hitType == HitTypeEnum.Instrument)
        {
            Spread();
            wasHit = false;
        }
    }

    public override IEnumerator PerformActionAfterMove()
    {
        if (!destroyed)
        {
            //if (ActivationMove == -1)
            //{
            //    ActivationMove = 1;
            //}

            if (ActivationMove - 1 == Tasks.Instance.RealMoves)
            {
                //SoundManager.Instance.PlaySoundInternal(SoundsEnum.);
                UpdateSprite();
            }

            //если на кусте есть ягоды
            if (wasHit && ActivationMove <= Tasks.Instance.RealMoves)
            {
                Spread();
                yield return new WaitForSeconds(0.4f);
            }

            wasHit = false;
        }
    }

    public void Spread()
    {
        ActivationMove = Tasks.Instance.RealMoves + 1 + actionDelay;
        UpdateSprite(1);
        Block[] neighboringBlocks = GridBlocks.Instance.GetBlocksForHit(this.positionInGrid, 5);
        SupportFunctions.MixArray(neighboringBlocks);//перемешаем блоки
        int numberOfCopies = 5;
        foreach (Block block in neighboringBlocks)
        {
            //находим не заблокированный элемент который сейчас ни где не обрабатывается
            if (((BlockCheck.ThisStandardBlockWithStandartElementCanMove(block) && block.Element.Shape != this.collectShape)
                || BlockCheck.ThisStandardBlockWithoutElement(block)) && !GridBlocks.Instance.BlockInProcessing(block))
            {
                SoundManager.Instance.PlaySoundInternal(SoundsEnum.Repainting);

                block.CreatElement(GridBlocks.Instance.prefabBlockingWall, this.collectShape, ElementsTypeEnum.StandardElement);
                block.Element.transform.position = thisTransform.position;
                //block.Element.AnimatElement.PlayIncreaseAnimation();
                ParticleSystemManager.Instance.CreatePS(block.Element.transform, PSEnum.PSMagicalTail, 4);

                numberOfCopies--;
            }

            if (numberOfCopies == 0)
            {
                break;
            }
        }
    }
 }
