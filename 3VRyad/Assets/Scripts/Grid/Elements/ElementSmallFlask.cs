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
        //находим аниматор
        transform.GetComponent<AnimatorElement>().playIdleAnimationRandomTime = true;
        //добавляем эфект
        GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/PSSmallFlask") as GameObject, thisTransform);
    }

    //действие после удара
    protected override void ActionAfterHitting(HitTypeEnum hitType)
    {
        Position position = new Position(PositionInGrid.posX, PositionInGrid.posY);
        base.DestroyElement();
        MainAnimator.Instance.AddExplosionEffect(thisTransform.position, ExplosionRadius);
        HitNeighboringBlocks(thisHitTypeEnum, position);
    }

    //ударяем по соседним блокам
    protected override void HitNeighboringBlocks(HitTypeEnum hitTypeEnum, Position position)
    {
        ////Находим позицию блока в сетке
        //Position gridPosition = this.PositionInGrid;
        //Определяем блоки вокруг
        Block[] aroundBlocks = GridBlocks.Instance.GetBlocksForHit(position, ExplosionRadius);
        GameObject blockBacklightGo = Instantiate(PrefabBank.BlockBacklight, thisTransform.position, Quaternion.identity, thisTransform);
        Destroy(blockBacklightGo, 1f);

        for (int i = 0; i < aroundBlocks.Length; i++)
        {
            if (aroundBlocks[i] != null) {
                blockBacklightGo = Instantiate(PrefabBank.BlockBacklight, aroundBlocks[i].thisTransform.position, Quaternion.identity, aroundBlocks[i].thisTransform);
                Destroy(blockBacklightGo, 1f);
                aroundBlocks[i].Hit(hitTypeEnum, this.shape);
            }
                
        }
    }    

}
