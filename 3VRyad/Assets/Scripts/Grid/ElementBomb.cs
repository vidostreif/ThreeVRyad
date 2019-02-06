using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementBomb : ElementDynamite
{
    protected override void DopSettings()
    {
        explosionRadius = 0.8f;
    }

    //ударяем по соседним блокам
    protected override void HitNeighboringBlocks(HitTypeEnum hitTypeEnum)
    {
        //Находим позицию блока в сетке
        Position gridPosition = Grid.Instance.FindPosition(this);
        //Определяем блоки вокруг
        Block[] aroundBlocks = Grid.Instance.DeterminingAroundBlocks(gridPosition);

        for (int i = 0; i < aroundBlocks.Length; i++)
        {
            if (aroundBlocks[i] != null)
                aroundBlocks[i].Hit(hitTypeEnum, this.shape);
        }
    }
}
