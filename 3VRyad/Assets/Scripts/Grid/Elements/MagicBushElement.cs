using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//куст распространяющий ягоды если собрать комбинацию рядом
public class MagicBushElement : Element
{
    int wasLife;
    protected override void DopSettings()
    {
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.HitFromNearbyElement, HitTypeEnum.Explosion, HitTypeEnum.Instrument };
        wasLife = life;
    }

    //действие после не смертельного удара
    protected override void ActionAfterHitting(HitTypeEnum hitType)
    {
        if (ActivationMove <= Tasks.Instance.RealMoves)
        {
            //UpdateSprite(2);
        }
    }

    public override IEnumerator PerformActionAfterMove()
    {
        if (!destroyed)
        {
            //if (ActivationMove == -1)
            //{
            //    UpdateSprite(1);
            //}

            if (ActivationMove - 1 <= Tasks.Instance.RealMoves)
            {
                //SoundManager.Instance.PlaySoundInternal(SoundsEnum.);
                UpdateSprite();
            }

            //если на кусте есть ягоды
            if (life < wasLife && ActivationMove <= Tasks.Instance.RealMoves)
            {
                ActivationMove = Tasks.Instance.RealMoves + 1 + actionDelay;
                UpdateSprite(1);
                Block[] neighboringBlocks = GridBlocks.Instance.GetBlocksForHit(this.positionInGrid, 4);
                SupportFunctions.MixArray(neighboringBlocks);//перемешаем блоки
                int numberOfCopies = 3;
                foreach (Block block in neighboringBlocks)
                {
                    //находим не заблокированный элемент который сейчас ни где не обрабатывается
                    if ((BlockCheck.ThisStandardBlockWithStandartElementCanMove(block) || BlockCheck.ThisStandardBlockWithoutElement(block)) && !GridBlocks.Instance.BlockInProcessing(block))
                    {
                        //if (block.Element.BlockingElement == null || block.Element.BlockingElement.Destroyed)
                        //{
                        SoundManager.Instance.PlaySoundInternal(SoundsEnum.Repainting);

                        block.CreatElement(GridBlocks.Instance.prefabBlockingWall, this.collectShape, ElementsTypeEnum.StandardElement);
                        block.Element.transform.position = thisTransform.position;
                        //block.Element.AnimatElement.PlayIncreaseAnimation();
                        ParticleSystemManager.Instance.CreatePS(block.Element.transform, PSEnum.PSMagicalTail, 4);

                        numberOfCopies--;
                        //}
                    }

                    if (numberOfCopies == 0)
                    {
                        break;
                    }
                }
                yield return new WaitForSeconds(0.3f);
            }

            wasLife = life;
        }
    }
 }
