using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementWall : Element {


    public override void Hit(HitTypeEnum hitType = HitTypeEnum.Standart, AllShapeEnum hitElementShape = AllShapeEnum.Empty)
    {
        if (!destroyed)
        {
            //если удар от соседнего элемента или взрыв
            if (hitType == HitTypeEnum.HitFromNearbyElement || hitType == HitTypeEnum.Explosion)
            {
                //если элемент не бессмертный
                if (!Immortal)
                {
                    base.DestroyElement((AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), Shape.ToString()));
                }
            }
        }       
    }

    public override BlockingElement BlockingElement
    {
        get
        {
            return blockingElement;
        }
    }

    public override void CreatBlockingElement(GameObject prefabBlockingElement, AllShapeEnum shape, BlockingElementsTypeEnum typeBlockingElementsEnum)
    {

    }

    protected override void HitNeighboringBlocks(HitTypeEnum hitTypeEnum)
    {

    }
}
