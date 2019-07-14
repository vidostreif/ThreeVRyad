using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//дикое растение, кажды Х количество ходдов распространяет лиану на блоки вокруг
public class WildPlantElement : Element
{
    private GameObject PSNextMove;

    protected override void DopSettings()
    {
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.HitFromNearbyElement, HitTypeEnum.Explosion, HitTypeEnum.Instrument };

        //создаем текст
        string TextCollectName = "TextCollectElement";
        Transform TextCollectTransform = transform.Find(TextCollectName);
        if (TextCollectTransform != null)
        {
            DestroyImmediate(TextCollectTransform.gameObject);
        }
        GameObject TextCollectObg = Instantiate(PrefabBank.TextLifeElement);
        TextCollectObg.name = TextCollectName;
        TextCollectObg.transform.SetParent(transform, false);
        lifeText = TextCollectObg.GetComponent<TextMesh>();
        lifeText.text = Life.ToString();
    }

    public override void PerformActionAfterMove()
    {
        if (!destroyed)
        {
            if (ActivationMove == -1)
            {
                ActivationMove = Tasks.Instance.RealMoves + actionDelay;
            }

            if (ActivationMove <= Tasks.Instance.RealMoves)
            {                
                //UpdateSprite();

                //распространение на блоки вокруг
                Block[] neighboringBlocks = GridBlocks.Instance.GetAroundBlocks(this.PositionInGrid);
                SupportFunctions.MixArray(neighboringBlocks);//перемешаем соседние блоки

                foreach (Block block in neighboringBlocks)
                {
                    //находим не заблокированный элемент
                    if (BlockCheck.ThisStandardBlockWithStandartElementCanMove(block))
                    {
                        if (block.Element.BlockingElement == null || block.Element.BlockingElement.Destroyed)
                        {
                            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Spread_liana);
                            ActivationMove = Tasks.Instance.RealMoves + 1 + actionDelay;
                            //Destroy(PSNextMove);
                            PoolManager.Instance.ReturnObjectToPool(PSNextMove);
                            block.Element.CreatBlockingElement(GridBlocks.Instance.prefabBlockingWall, AllShapeEnum.Liana, BlockingElementsTypeEnum.Liana, thisTransform);
                            break;
                        }
                    }
                }
            }
            else if(ActivationMove - 1 == Tasks.Instance.RealMoves)
            {
                //создаем эффект что элемент будет активирован
                if (ParticleSystemManager.Instance != null && PSNextMove == null)
                {
                    PSNextMove = ParticleSystemManager.Instance.CreatePS(thisTransform, PSEnum.PSWildPlantNextAction);
                    SoundManager.Instance.PlaySoundInternal(SoundsEnum.Preparation_wildplant);
                }
            }
        }
    }


}
