using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//элемент для смещения к другому элементу
public class MoveElement {

    public Transform thisTransform;
    public Vector3 targetPosition;
    //public Vector3 startPosition;    
    public float smoothTime;//скорость перемещения элемента к блоку
    //public float startTime;//начало передвижения
    public int priority;//приоритет
    public SmoothEnum smoothEnum;
    public bool destroyAfterMoving;
    public float yVelocity = 0.0f;

    public MoveElement(Transform objTransform, Vector3 targetPosition, float smoothTime, SmoothEnum smoothEnum, int priority, bool destroyAfterMoving)
    {
        this.thisTransform = objTransform;
        this.targetPosition = targetPosition;
        this.smoothTime = smoothTime;
        this.smoothEnum = smoothEnum;
        this.priority = priority;
        this.destroyAfterMoving = destroyAfterMoving;
        //this.startTime = Time.realtimeSinceStartup;
        //this.startPosition = thisTransform.position;
    }
}
