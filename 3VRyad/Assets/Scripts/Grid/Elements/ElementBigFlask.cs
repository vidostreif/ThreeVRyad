using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//элемент большая фласка уничтожает 44 соседних элементов
public class ElementBigFlask : ElementSmallFlask
{
    protected override void DopSettings()
    {
        explosionRadius = 3;
    }
}
