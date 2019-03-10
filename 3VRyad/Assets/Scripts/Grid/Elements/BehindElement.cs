using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BehindElement : BaseElement
{
    [SerializeField] protected BehindElementsTypeEnum type;//тип элемента
    [SerializeField] protected BehindElementsShapeEnum shape;//форма элемента

    public BehindElementsTypeEnum Type
    {
        get
        {
            return type;
        }
    }
    public BehindElementsShapeEnum Shape
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

    public virtual void Awake()
    {
        thisTransform = transform;
        destroyed = false;
        spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        spriteRenderer.sortingLayerName = "BackgroundElements";
    }

    //установка настроек элементов
    public void InitialSettings(BehindElementsTypeEnum type, bool immortal, int life, bool actionAfterMove, int actionDelay, int score)
    {
        this.type = type;
        this.actionAfterMove = actionAfterMove;
        this.actionDelay = actionDelay;
        this.startingActionDelay = actionDelay;
        this.immortal = immortal;
        this.life = life;
        this.score = score;
        UpdateSpriteAlfa();
    }

    public override void Hit()
    {
        //если не неразрушаемый
        if (!destroyed && !this.immortal)
        {
            life--;
        }
        if (life <= 0)
        {
            base.DestroyElement((AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), Shape.ToString()));
        }
    }

    protected void UpdateSpriteAlfa()
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
