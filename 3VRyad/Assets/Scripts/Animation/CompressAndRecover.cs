using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompressAndRecover
{
    public Transform thisTransform;
    public Vector3 direction;//направление    
    public float power;//скорость перемещения элемента к блоку
    //public int iteration;//количество проведенных итераций
    //public bool compress;//переключатель направления 

    public CompressAndRecover(Transform thisTransform, Vector3 direction, float power)
    {
        this.thisTransform = thisTransform;
        this.direction = direction;        
        this.power = power;
        //this.iteration = 0;
        //this.compress = true;
    }
    //public float yVelocity = 0.0f;
}
