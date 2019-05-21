using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//элемент большая фласка уничтожает 44 соседних элементов
public class ElementBigFlask : ElementSmallFlask
{
    protected override void DopSettings()
    {
        soundDestroy = SoundsEnum.Boom_big;
        base.DopSettings();
        explosionRadius = 3;
    }
}
