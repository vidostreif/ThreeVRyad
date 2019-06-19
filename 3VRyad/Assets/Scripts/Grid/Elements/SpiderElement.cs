using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderElement : Element
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
            if (ActivationMove >= Tasks.Instance.Moves)
            {
                ActivationMove = Tasks.Instance.Moves - 1 - actionDelay;
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
                            block.Element.CreatBlockingElement(GridBlocks.Instance.prefabBlockingWall, AllShapeEnum.Web, BlockingElementsTypeEnum.Liana, thisTransform);
                            break;
                        }
                    }
                }
            }
        }
    }


}
