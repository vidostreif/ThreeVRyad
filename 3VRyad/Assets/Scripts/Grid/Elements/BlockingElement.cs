using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public void InitialSettings(BlockingElementsTypeEnum type, bool immortal, int life, int score, bool scoreScale)
    {
        this.type = type;
        this.life = life;
        this.immortal = immortal;
        this.score = score;
        this.scoreScale = scoreScale;
        DopSettings();
    }

    protected virtual void DopSettings()
    {
        //vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.StandartHit, HitTypeEnum.Explosion, HitTypeEnum.Instrument };
    }

    //удар элементу
    public override void Hit(HitTypeEnum hitType = HitTypeEnum.StandartHit, AllShapeEnum hitElementShape = AllShapeEnum.Empty)
    {
        //if (vulnerabilityTypeEnum.Contains(hitType))
        //{
            //если елемент убили
            if (SubLife())
            {
                base.DestroyElement();
            }
        //}
        
    }

    void Awake()
    {
        thisTransform = transform;
        destroyed = false;
        spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        spriteRenderer.sortingLayerName = "Blocking element";
    }
}
