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
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.StandartHit, HitTypeEnum.Explosion, HitTypeEnum.DoubleClick, HitTypeEnum.Instrument };
        if (Application.isPlaying)
        {
            //находим аниматор
            transform.GetComponent<AnimatorElement>().playIdleAnimationRandomTime = true;
            //добавляем эфект
            DopPS = ParticleSystemManager.Instance.CreatePS(thisTransform, PSEnum.PSSmallFlask);
        }
        
    }

    //действие после удара
    protected override void ActionAfterDestroyHitting(HitTypeEnum hitType)
    {
        Position position = new Position(PositionInGrid.posX, PositionInGrid.posY);
        base.DestroyElement();
        MainAnimator.Instance.AddExplosionEffect(thisTransform.position, ExplosionRadius);
        HitNeighboringBlocks(thisHitTypeEnum, position);
    }

    //ударяем по соседним блокам
    protected override void HitNeighboringBlocks(HitTypeEnum hitTypeEnum, Position position)
    {
        //Определяем блоки вокруг
        Block[] aroundBlocks = GridBlocks.Instance.GetBlocksForHit(position, ExplosionRadius);

        Block thisBlock = GridBlocks.Instance.GetBlock(position);
        PoolManager.Instance.GetObjectToRent("BlockBacklight", thisBlock.thisTransform.position, thisBlock.thisTransform, 1f);

        for (int i = 0; i < aroundBlocks.Length; i++)
        {
            if (aroundBlocks[i] != null)
            {
                PoolManager.Instance.GetObjectToRent("BlockBacklight", aroundBlocks[i].thisTransform.position, aroundBlocks[i].thisTransform, 1f);
                aroundBlocks[i].Hit(hitTypeEnum, this.shape);
            }                
        }
    }    


}
