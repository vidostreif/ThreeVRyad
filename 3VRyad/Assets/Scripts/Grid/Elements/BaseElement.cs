﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseElement : MonoBehaviour
{
    public Transform thisTransform;
    //protected SpriteBank objectManagement;
    protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Position positionInGrid;//позиция в сетке
    [SerializeField] protected AllShapeEnum shape;//форма элемента
    [SerializeField] protected HitTypeEnum[] vulnerabilityTypeEnum;//уязвимость к типам удара
    [SerializeField] protected bool destroyed = false;//признак что элемент был уничтожен
    [SerializeField] protected int life;
    [SerializeField] protected int score;//количество очков за уничтожение элемента
    [SerializeField] protected bool immortal;//признак бессмертия

    [SerializeField] protected bool actionAfterMove = false;//признак активируемости по окончанию хода
    [SerializeField] protected int actionDelay;//задержка перед активированием
    [SerializeField] protected int startingActionDelay;//запоминает первичную задержку для расчетов

    [SerializeField] protected bool collector = false;//признак что элемент коллекционирует другие элементы
    [SerializeField] protected AllShapeEnum collectShape;//форма коллекционированого элемента
    [SerializeField] protected int numberOfElementCollected;//количество коллекционируемых элементов

    [SerializeField] protected bool drop = false;//сбрасываемый элемент

    public virtual Position PositionInGrid
    {
        get
        {
            return positionInGrid;
        }

        set
        {
            positionInGrid = value;
        }
    }
    public bool Destroyed
    {
        get
        {
            return destroyed;
        }
    }
    public bool Immortal
    {
        get
        {
            return immortal;
        }
    }//признак бессмертия
    public int Life
    {
        get
        {
            return life;
        }
    }//признак бессмертия
    public bool ActionAfterMove
    {
        get
        {
            return actionAfterMove;
        }
    }
    public AllShapeEnum Shape
    {
        get
        {
            return shape;
        }

        set
        {
            if (shape != AllShapeEnum.Empty)
            {
                ElementsList.DellElement(shape);
            }
            shape = value;
            ElementsList.AddElement(shape);
            spriteRenderer.sprite = SpriteBank.SetShape(value);
        }
    }
    public bool Collector
    {
        get
        {
            return collector;
        }
    }
    public AllShapeEnum CollectShape
    {
        get
        {
            return collectShape;
        }
        set
        {
            MakeCollector(value, this.numberOfElementCollected);
        }
    }
    public int NumberOfElementCollected
    {
        get
        {
            return numberOfElementCollected;
        }
    }
    public bool Drop
    {
        get
        {
            return drop;
        }
    }

    //делаем элемент активным после хода
    public virtual void MakeActionAfterMove(int actionDelay)
    {
        this.actionAfterMove = true;
        this.actionDelay = actionDelay;
        this.startingActionDelay = actionDelay;
        UpdateSpriteAlfa();
    }

    //делаем элемент коллекционером
    public virtual void MakeCollector(AllShapeEnum collectShape, int numberOfElementCollected) {
        this.collector = true;
        this.collectShape = collectShape;
        this.numberOfElementCollected = numberOfElementCollected;
    }

    //делаем элемент сбрасываемым
    public virtual void MakeDrop()
    {
        this.drop = true;
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.Drop };
    }

    //добавляем в коллекцию элемент
    public virtual bool AddToCollection(AllShapeEnum elementShape, Transform elementTransform)
    {
        if (!this.destroyed && this.collector && elementShape == this.collectShape && this.numberOfElementCollected > 0)
        {
            //перемещаем элемент к себе
            MainAnimator.Instance.AddElementForSmoothMove(elementTransform, this.transform.position, 10, SmoothEnum.InLineWithAcceleration, 0.1f, true);

            numberOfElementCollected--;
            //если собрали колецию
            if (numberOfElementCollected == 0)
            {
                DestroyElement();
            }
            return true;
        }
        return false;
    }

    //удар элементу
    public virtual void Hit()
    {
    }

    public virtual void Hit(HitTypeEnum hitType, AllShapeEnum hitElementShape)
    { }

    //действие после хода
    public virtual void PerformActionAfterMove()
    {
    }

    protected virtual void DestroyElement()
    {
        ////удаляем из блока
        //Block block = GridBlocks.Instance.GetBlock(this.positionInGrid);
        //if (block != null)
        //{
        //    if (block.BehindElement == this)
        //    {
        //        block.BehindElement = null;
        //    }
        //    if (block.Element == this)
        //    {
        //        block.Element = null;
        //    }
        //}

        destroyed = true;
        ElementsList.DellElement(shape);
        Score.Instance.CreateScoreElement(transform.position, score);
        SuperBonus.Instance.CreatePowerSuperBonus(transform.position, score);
        
        //определяем есть ли вокруг элементы коллекционирующие наш вид элемента
        Block[] blocksAround = GridBlocks.Instance.GetAroundBlocks(this.PositionInGrid);
        foreach (Block item in blocksAround)
        {
            if (BlockCheck.ThisBlockWithCollectorElementAndNoBlockingElement(item))
            {
                //если добавили элемент в коллекцию то выходим
                if (item.Element.AddToCollection(this.shape, this.transform))
                {
                    return;
                }  
            }
        }
        //проверяем по заданиям
        if (!Tasks.Instance.Collect(shape, transform))
        {
            AnimatorElement animatorElement = this.GetComponent<AnimatorElement>();
            animatorElement.PlayDestroyAnimation();
        }
        this.PositionInGrid = null;
    }

    protected virtual void UpdateSpriteAlfa()
    {
        if (startingActionDelay != 0)
        {
            //изменяем альфу спрайта
            float dActionDelay = actionDelay;
            float dStartingActionDelay = startingActionDelay;
            SupportFunctions.ChangeAlfa(spriteRenderer, 1 - (dActionDelay / (dStartingActionDelay + 1)));
        }
    }

}
