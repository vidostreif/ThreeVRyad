using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlockingElement : BaseElement
{
    [SerializeField] protected BlockingElementsShapeEnum shape;//форма элемента
    [SerializeField] protected BlockingElementsTypeEnum type;//тип элемента
    public virtual BlockingElementsTypeEnum Type
    {
        get
        {
            return type;
        }
    }
    public virtual BlockingElementsShapeEnum Shape
    {
        get
        {
            return shape;
        }

        set
        {
            shape = value;
            spriteRenderer.sprite = SpriteBank.SetShape(value);
        }
    }

    //установка настроек элементов
    public void InitialSettings(BlockingElementsTypeEnum type, bool actionAfterMove, bool immortal, int life)
    {
        this.type = type;
        this.life = life;
        this.immortal = immortal;
        this.actionAfterMove = actionAfterMove;
    }

    //удар элементу
    public override void Hit()
    {
        //если не неразрушаемый
        if (!this.immortal)
        {
            life--;
        }
        //если елемент убили, то возвращаем null
        if (life <= 0)
        {
            destroyed = true;
            if (!Tasks.Instance.Collect(this))
            {
                AnimatorElement animatorElement = this.GetComponent<AnimatorElement>();
                animatorElement.PlayDestroyAnimation();
            }
        }
    }

    void Awake()
    {
        thisTransform = transform;
        destroyed = false;
        //objectManagement = GetComponent<SpriteBank>();
        spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
    }
}
