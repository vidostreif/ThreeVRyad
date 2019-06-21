using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//элемент большая фласка уничтожает 44 соседних элементов
public class ElementBigFlask : ElementSmallFlask
{
    protected override void DopSettings()
    {        
        //base.DopSettings();
        explosionRadius = 3;

        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.StandartHit, HitTypeEnum.Explosion, HitTypeEnum.DoubleClick, HitTypeEnum.Instrument };
        //находим аниматор
        transform.GetComponent<AnimatorElement>().playIdleAnimationRandomTime = true;
        //добавляем эфект
        GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/PSBigFlask") as GameObject, thisTransform);
    }
}
