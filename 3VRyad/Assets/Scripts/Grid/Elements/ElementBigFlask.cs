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
        if (Application.isPlaying)
        {
            //находим аниматор
            transform.GetComponent<AnimatorElement>().playIdleAnimationRandomTime = true;
            //добавляем эфект
            DopPS = ParticleSystemManager.Instance.CreatePS(thisTransform, PSEnum.PSBigFlask);
        }
        
    }
}
