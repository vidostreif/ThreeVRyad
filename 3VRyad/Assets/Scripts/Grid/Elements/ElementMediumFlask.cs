using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//средняя фласка уничтожает 20 соседних элементов
public class ElementMediumFlask : ElementSmallFlask
{
    protected override void DopSettings()
    {
        soundDestroy = SoundsEnum.Boom_big;
        base.DopSettings();
        explosionRadius = 2;
    }
}
