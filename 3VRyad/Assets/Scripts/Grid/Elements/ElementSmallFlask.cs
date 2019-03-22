using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//маленькая фласка уничтожает 4 соседних элемента
public class ElementSmallFlask : Element
{
    [SerializeField] protected int explosionRadius;

    public int ExplosionRadius
    {
        get
        {
            return explosionRadius;
        }
    }

    protected override void DopSettings()
    {
        explosionRadius = 1;
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.Standart, HitTypeEnum.Explosion, HitTypeEnum.DoubleClick };
    }

    public override void Hit(HitTypeEnum hitType = HitTypeEnum.Standart, AllShapeEnum hitElementShape = AllShapeEnum.Empty)
    {
        if (!destroyed)
        {
            //если праввильный тип удара
            if (vulnerabilityTypeEnum.Contains(hitType))
            {
                //если стоит блокировка на элементе, то пытаемся ее снять
                if (BlockingElementExists())
                {
                    blockingElement.Hit();

                    //если уничтожили блокирующий элемент
                    if (blockingElement.Destroyed)
                    {
                        lockedForMove = false;
                    }
                }
                //если элемент не заблокирован, то уничтожаем элемент        
                else
                {
                    //если элемент не бессмертный
                    if (!Immortal)
                    {
                        base.DestroyElement();
                        MainAnimator.Instance.AddExplosionEffect(thisTransform.position, ExplosionRadius);
                        HitNeighboringBlocks(thisHitTypeEnum);
                    }
                }
            }
        }
    }

    //действие после удара
    protected override void ActionAfterHitting(HitTypeEnum hitType)
    {
        base.DestroyElement();
        MainAnimator.Instance.AddExplosionEffect(thisTransform.position, ExplosionRadius);
        HitNeighboringBlocks(thisHitTypeEnum);
    }

    //ударяем по соседним блокам
    protected override void HitNeighboringBlocks(HitTypeEnum hitTypeEnum)
    {
        //Находим позицию блока в сетке
        Position gridPosition = this.PositionInGrid;
        //Определяем блоки вокруг
        Block[] aroundBlocks = GridBlocks.Instance.GetBlocksForHit(gridPosition, ExplosionRadius);

        for (int i = 0; i < aroundBlocks.Length; i++)
        {
            if (aroundBlocks[i] != null)
                aroundBlocks[i].Hit(hitTypeEnum, this.shape);
        }
    }    

}
