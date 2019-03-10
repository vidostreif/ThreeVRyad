using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseElement : MonoBehaviour
{
    public Transform thisTransform;
    //protected SpriteBank objectManagement;
    protected SpriteRenderer spriteRenderer;
    [SerializeField] protected bool destroyed;//признак что элемент был уничтожен
    [SerializeField] protected int life;
    [SerializeField] protected int score;//количество очков за уничтожение элемента
    [SerializeField] protected bool immortal;//признак бессмертия
    [SerializeField] protected bool actionAfterMove;//признак активируемости по окончанию хода
    [SerializeField] protected int actionDelay;//задержка перед активированием
    [SerializeField] protected int startingActionDelay;//запоминает первичную задержку для расчетов

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

    protected virtual void DestroyElement(AllShapeEnum allShapeEnum)
    {
        destroyed = true;
        Score.Instance.CreatreScoreElement(transform.position, score);
        if (!Tasks.Instance.Collect(allShapeEnum, transform))
        {
            AnimatorElement animatorElement = this.GetComponent<AnimatorElement>();
            animatorElement.PlayDestroyAnimation();
        }
    }

}
