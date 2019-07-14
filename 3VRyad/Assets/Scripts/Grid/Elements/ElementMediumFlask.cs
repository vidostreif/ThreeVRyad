using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//средняя фласка уничтожает 20 соседних элементов
public class ElementMediumFlask : ElementSmallFlask
{
    protected override void DopSettings()
    {               
        //base.DopSettings();
        explosionRadius = 2;
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.StandartHit, HitTypeEnum.Explosion, HitTypeEnum.DoubleClick, HitTypeEnum.Instrument };
        if (Application.isPlaying)
        {
            //находим аниматор
            transform.GetComponent<AnimatorElement>().playIdleAnimationRandomTime = true;
            //добавляем эфект
            DopPS = ParticleSystemManager.Instance.CreatePS(thisTransform, PSEnum.PSMediumFlask);
        }
        
    }
}
