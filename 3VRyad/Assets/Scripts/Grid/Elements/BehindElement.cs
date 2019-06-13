﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        DopSettings();
    }

    protected virtual void DopSettings()
    {
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.StandartHit, HitTypeEnum.Explosion, HitTypeEnum.Instrument };
    }

    public override void Hit(HitTypeEnum hitType = HitTypeEnum.StandartHit, AllShapeEnum hitElementShape = AllShapeEnum.Empty)
    {
        if (vulnerabilityTypeEnum.Contains(hitType))
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
    }

}
