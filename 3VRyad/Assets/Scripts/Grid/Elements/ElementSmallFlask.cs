using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//маленькая фласка уничтожает 4 соседних элемента
public class ElementSmallFlask : Element
{
    [SerializeField] protected int explosionRadius;

    protected override void DopSettings()
    {
        explosionRadius = 1;
    }

    public override void Hit(HitTypeEnum hitType = HitTypeEnum.Standart, AllShapeEnum hitElementShape = AllShapeEnum.Empty)
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
                    blockingElement.Hit();

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
                        base.DestroyElement();
                        MainAnimator.Instance.AddExplosionEffect(thisTransform.position, explosionRadius);
                        HitNeighboringBlocks(thisHitTypeEnum);
                    }
                }
            }
        }  
    }

    //ударяем по соседним блокам
    protected override void HitNeighboringBlocks(HitTypeEnum hitTypeEnum)
    {
        //Находим позицию блока в сетке
        Position gridPosition = GridBlocks.Instance.FindPosition(this);
        //Определяем блоки вокруг
        Block[] aroundBlocks = DeterminingBlocksForHit(gridPosition, explosionRadius);

        for (int i = 0; i < aroundBlocks.Length; i++)
        {
            if (aroundBlocks[i] != null)
                aroundBlocks[i].Hit(hitTypeEnum, this.shape);
        }
    }

    //определяем блоки для удара
    //берем все блоки в указанном радиусе, за минусом центрального блока и блоков на углах
    protected Block[] DeterminingBlocksForHit(Position position, int radius)
    {
        int diameter = radius * 2;
        Blocks[] containers = GridBlocks.Instance.containers;
        Block[] blocks = new Block[(diameter + 1) * (diameter + 1) - 5];

        if (position.posX != -1 || position.posY != -1)
        {
            int iteration = 0;
            int posX;
            int posY;
            for (int x = 0; x < diameter + 1; x++)
            {
                for (int y = 0; y < diameter + 1; y++)
                {
                    //пропускаем все не нужные блоки
                    if ((x == 0 && y == 0) || (x == 0 && y == diameter) || (x == diameter && y == diameter) || (x == diameter && y == 0) || (x == radius && y == radius))
                    {
                        continue;
                    }

                    posX = position.posX - radius + x;
                    posY = position.posY - radius + y;
                    if (posX >= 0 && posX < containers.GetLength(0) &&
                        posY >= 0 && posY < containers[posX].block.GetLength(0))
                    {
                        blocks[iteration] = containers[posX].block[posY];
                        iteration++;
                    }
                }
            }
        }
        return blocks;
    }


}
