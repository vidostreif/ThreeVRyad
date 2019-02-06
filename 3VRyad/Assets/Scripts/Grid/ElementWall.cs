using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementWall : Element {


    public override void Hit(HitTypeEnum hitType = HitTypeEnum.Standart, ElementsShapeEnum hitElementShape = ElementsShapeEnum.Empty)
    {
        if (!destroyed)
        {
            //если удар от соседнего элемента или взрыв
            if (hitType == HitTypeEnum.HitFromNearbyElement || hitType == HitTypeEnum.Explosion)
            {
                //если элемент не бессмертный
                if (!Immortal)
                {
                    destroyed = true;
                    Tasks.Instance.Collect(this);
                    AnimatorElement animatorElement = this.GetComponent<AnimatorElement>();
                    animatorElement.PlayDestroyAnimation();                    
                    //return null;
                }
            }
            //return this;
        }
        //else
        //{
        //    return null;
        //}
        
    }

    public override BlockingElement BlockingElement
    {
        get
        {
            return blockingElement;
        }
    }

    public override void CreatBlockingElement(GameObject prefabBlockingElement, BlockingElementsShapeEnum shape, BlockingElementsTypeEnum typeBlockingElementsEnum)
    {

    }

    protected override void HitNeighboringBlocks(HitTypeEnum hitTypeEnum)
    {

    }
}
