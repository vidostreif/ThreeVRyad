using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//дикое растение, кажды Х количество ходдов распространяет лиану на блоки вокруг
public class WildPlantElement : Element
{    
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
        GameObject TextCollectObg = Instantiate(PrefabBank.TextCollectElement);
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
                            block.Element.CreatBlockingElement(GridBlocks.Instance.prefabBlockingWall, AllShapeEnum.Liana, BlockingElementsTypeEnum.Liana, thisTransform);
                            break;
                        }
                    }
                }
            }
        }
    }


}
