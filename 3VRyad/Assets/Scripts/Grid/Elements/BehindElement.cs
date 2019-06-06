using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BehindElement : BaseElement
{
    [SerializeField] protected BehindElementsTypeEnum type;//тип элемента
    public BehindElementsTypeEnum Type
    {
        get
        {
            return type;
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
    public void InitialSettings(BehindElementsTypeEnum type, bool immortal, int life, int score)
    {
        this.type = type;
        this.immortal = immortal;
        this.life = life;
        this.score = score;
        //UpdateSpriteAlfa();
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
            base.DestroyElement();
        }
    }

}
