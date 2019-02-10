using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//сохраняет данные о взрыве
public class Explosion 
{
    public Vector3 epicenter; //эпицентр взрыва
    public float power;//мощность взрыва
    public float moment;//момент взрыва
    public float radius;// нарастающий радиус взрыва
    public float iteration;// итерации
    public GameObject explosionEffect;//эффект искажения


    public float radiusExplosionEffect;// радиус эффекта
    public float maxRadiusExplosionEffect;// макс радиус эффекта

    public Explosion(Vector3 epicenter, float power, float moment, GameObject explosionEffect)
    {
        this.epicenter = epicenter;
        this.power = power;
        this.moment = moment;
        this.radius = 0;
        this.iteration = 3;
        this.explosionEffect = explosionEffect;
        this.radiusExplosionEffect = 1;
        this.maxRadiusExplosionEffect = power * 10;
    }
}
