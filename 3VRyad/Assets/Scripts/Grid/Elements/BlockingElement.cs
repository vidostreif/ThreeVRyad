using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlockingElement : BaseElement
{
    //[SerializeField] protected BlockingElementsShapeEnum shape;//форма элемента
    [SerializeField] protected BlockingElementsTypeEnum type;//тип элемента
    public virtual BlockingElementsTypeEnum Type
    {
        get
        {
            return type;
        }
    }

    //установка настроек элементов
    public void InitialSettings(BlockingElementsTypeEnum type, bool immortal, int life, int score)
    {
        this.type = type;
        this.life = life;
        this.immortal = immortal;
        this.score = score;
        
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
            base.DestroyElement();
        }
    }

    void Awake()
    {
        thisTransform = transform;
        destroyed = false;
        soundDestroy = SoundsEnum.LeafRustling;
        spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
    }
}
