using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElementWall : Element {   

    protected override void DopSettings()
    {
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.HitFromNearbyElement, HitTypeEnum.Explosion, HitTypeEnum.Instrument };
    }

    //удар по элементу
    public override void Hit(HitTypeEnum hitType = HitTypeEnum.StandartHit, AllShapeEnum hitElementShape = AllShapeEnum.Empty)
    {
        if (!destroyed)
        {
            //если прямой удар или взрыв
            if (vulnerabilityTypeEnum.Contains(hitType))
            {
                //если стоит блокировка на элементе, то пытаемся ее снять
                if (BlockingElementExists())
                {
                    blockingElement.Hit();
                }
                //если элемент не заблокирован, то уничтожаем элемент        
                else
                {
                    //если элемент не бессмертный
                    if (!Immortal)
                    {
                        base.DestroyElement();
                    }
                }
            }
        }
    }
}
