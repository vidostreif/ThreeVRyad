using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseElement : MonoBehaviour
{
    public Transform thisTransform;
    //protected SpriteBank objectManagement;
    protected SpriteRenderer spriteRenderer;
    [SerializeField] protected bool destroyed;//признак что элемент был уничтожен
    [SerializeField] protected int life = 1;
    [SerializeField] protected bool immortal;//признак бессмертия
    [SerializeField] protected bool actionAfterMove;//признак активируемости по окончанию хода

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

    //удар элементу
    public virtual void Hit()
    {
    }

    public virtual void Hit(HitTypeEnum hitType, ElementsShapeEnum hitElementShape)
    { }

    public virtual void PerformActionAfterMove()
    {
    }
}
