using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementDynamite : Element
{
    [SerializeField] protected float explosionRadius;

    protected override void DopSettings()
    {
        explosionRadius = 0.6f;
    }

    public override void Hit(HitTypeEnum hitType = HitTypeEnum.Standart, ElementsShapeEnum hitElementShape = ElementsShapeEnum.Empty)
    {
        if (!destroyed)
        {
            //если прямой удар или взрыв
            if (hitType == HitTypeEnum.Standart || hitType == HitTypeEnum.Explosion || hitType == HitTypeEnum.DoubleClick)
            {
                //Debug.LogWarning("Удар по элементу " + this.transform.parent.name);
                //если стоит блокировка на элементе, то пытаемся ее снять
                if (BlockingElementExists())
                {
                    blockingElement = blockingElement.Hit();

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
                        //воздействие на соседние блоки
                        destroyed = true;
                        MainAnimator.Instance.AddExplosionEffect(thisTransform.position, explosionRadius);
                        HitNeighboringBlocks(thisHitTypeEnum);
                        if (!Tasks.Instance.Collect(this))
                        {
                            AnimatorElement animatorElement = this.GetComponent<AnimatorElement>();
                            animatorElement.PlayDestroyAnimation();
                        }
                    }
                }
            }
        }  
    }

    public override void Activate()
    {
        Hit();
    }


}
