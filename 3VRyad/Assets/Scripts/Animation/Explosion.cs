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

    public Explosion(Vector3 epicenter, float power, float moment)
    {
        this.epicenter = epicenter;
        this.power = power;
        this.moment = moment;
        this.radius = 0;
    }
}
