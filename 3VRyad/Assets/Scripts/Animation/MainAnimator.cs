﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MainAnimator : MonoBehaviour {

    public static MainAnimator Instance; // Синглтон

    private List<AnimatorElement> elementsForNextMove = new List<AnimatorElement>(); //элементы для следующей подсказки игроку
    private float timeToHint = 5; //время до подсказки
    private float idleHintsTime; //момент отсчета

    private List<MoveElement> moveElements = new List<MoveElement>();//элементы для перемещения
    private List<MoveElement> moveElementsForRemove = new List<MoveElement>();//элементы которые можно удалить из коллекции

    private List<Explosion> explosions = new List<Explosion>();//данные о взрывах
    private List<Explosion> explosionsForRemove = new List<Explosion>();//элементы которые можно удалить из коллекции

    private List<CompressAndRecover> CompressAndRecoverElements = new List<CompressAndRecover>();//элементы для перемещения
    private List<CompressAndRecover> CompressAndRecoverElementsForRemove = new List<CompressAndRecover>();//элементы которые можно удалить из коллекции

    public GameObject explosionEffect;//префаб
    //public List<GameObject> explosionEffects;// лист взрывов

    public List<Element> ElementsForNextMove
    {
        set
        {
            elementsForNextMove.Clear();
            foreach (Element item in value)
            {
                elementsForNextMove.Add(item.GetComponent<AnimatorElement>());
            }
            idleHintsTime = Time.time;
        }
    }

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров MainAnimator!");
        }

        Instance = this;
    }
	
	// Update is called once per frame
	void Update () {
        if (elementsForNextMove.Count > 0 && (idleHintsTime + timeToHint) < Time.time)
        {
            PlayHintAnimations();
        }
        SmoothMove();
        Explosions();
        CompressAndRecover();
    }

    //проигрование подскази игроку
    private void PlayHintAnimations() {
        foreach (AnimatorElement item in elementsForNextMove)
        {
            if (item != null)
            {
                item.PlayHintAnimation();
            }                           
        }
        idleHintsTime = Time.time;
    }

    public void ClearElementsForNextMove() {
        elementsForNextMove.Clear();
    }
    
    //добавление объекта для сглаженного перемещения
    public void AddElementForSmoothMove(Transform objTransform, Vector3 targetPosition, int priority, float smoothTime = 0.05f)
    {
        //проверяем есть ли уже такой объект и какой у него приоритет
        bool add = true;
        foreach (MoveElement item in moveElements)
        {
            if (item.thisTransform == objTransform)
            {
                if (item.priority >= priority)
                {
                    add = false;
                    break;
                }             
            }
        }

        //если определились что будем добавлять элемент в массив
        if (add)
        {
            //предварительный проверяем, что перемещаемого объекта нет в нашем массиве
            //и если есть, то удаляем его
            moveElements.RemoveAll(p => p.thisTransform == objTransform);
            //добавляем
            moveElements.Add(new MoveElement(objTransform, targetPosition, smoothTime));
        }
        
    }

    //процедура сглаженного перемещения объектов к указанной позиции
    private void SmoothMove() {
        //перемещаем все элементы которые попали в коллекцию
        foreach (MoveElement item in moveElements)
        {
            if (item.thisTransform != null)
            {
                float newPositionX = Mathf.SmoothDamp(item.thisTransform.position.x, item.targetPosition.x, ref item.yVelocity, item.smoothTime);
                float newPositionY = Mathf.SmoothDamp(item.thisTransform.position.y, item.targetPosition.y, ref item.yVelocity, item.smoothTime);
                item.thisTransform.position = new Vector3(newPositionX, newPositionY, item.targetPosition.z);
                //item.thisTransform.position = Vector3.Lerp(item.thisTransform.position, item.targetTransform.position, Time.deltaTime * item.smoothTime);

                if (item.thisTransform.position == item.targetPosition)
                    moveElementsForRemove.Add(item);
            }
            else
            {
                moveElementsForRemove.Add(item);
            }
        }

        foreach (MoveElement item in moveElementsForRemove)
        {
            moveElements.Remove(item);
        }
        moveElementsForRemove.Clear();
    }

    public void AddExplosionEffect(Vector3 epicenter, float power) {

        GameObject explosionEf = Instantiate(explosionEffect, epicenter, Quaternion.identity);
        //Destroy(explosionEf, power * 0.33f);//устанавливаем время жизни
        explosions.Add(new Explosion(epicenter, power, Time.time, explosionEf));
    }
    
    //эффекты взрыва
    private void Explosions() {
        //создаем эффекты взрыва
        foreach (Explosion item in explosions)
        {
            //item.radiusExplosionEffect *= 70 * Time.deltaTime;
            ////если эффект все еще есть
            //if (item.radiusExplosionEffect < item.maxRadiusExplosionEffect)
            //{
            //    //увеличиваем эффект искажения
            //    item.explosionEffect.transform.localScale = new Vector3(item.radiusExplosionEffect, item.radiusExplosionEffect, 1);
                
            //}
            //else
            //{
            //    Debug.Log(item.explosionEffect.transform.localScale + "  " + item.maxRadiusExplosionEffect + "  " + item.radiusExplosionEffect);
            //}

            if (item.moment < Time.time)
            {
                item.explosionEffect.transform.localScale = new Vector3(item.radiusExplosionEffect, item.radiusExplosionEffect, 1);
                item.radiusExplosionEffect += 1;
                item.moment = Time.time + 0.025f;
                item.radius += Grid.Instance.blockSize * 0.45f;
                GameObject[] objectsToMove = FindObjectsInRadiusWithComponent(item.epicenter, item.radius, item.radius + Grid.Instance.blockSize, typeof(Element));

                foreach (GameObject objectToMove in objectsToMove)
                {
                    if (objectToMove.transform != null)
                    {
                        //расчитываем вектор смещения
                        Vector3 translation = objectToMove.transform.position - item.epicenter;
                        //вычисляем расстояние до объекта
                        float offsetDistance = translation.magnitude;
                        //нормализируем вектор для упрощения вычисления направления
                        Vector3 direction = translation / offsetDistance;
                        AddElementForSmoothMove(objectToMove.transform, objectToMove.transform.position + direction * item.power, 5, 0.01f);
                        AddElementForCompressAndRecover(objectToMove.transform, direction, item.power);
                        AnimatorElement animatorElement = objectToMove.GetComponent<AnimatorElement>();
                        animatorElement.PlayIdleAnimation();
                    }
                }                
                item.power -= 0.1f;
            }

            if (item.power <= 0)
                explosionsForRemove.Add(item);
        }

        //удаляем из массива
        foreach (Explosion item in explosionsForRemove)
        {
            Destroy(item.explosionEffect);
            explosions.Remove(item);
        }
        explosionsForRemove.Clear();
    }


    public void AddElementForCompressAndRecover(Transform objTransform, Vector3 direction, float power) {
        CompressAndRecoverElements.Add(new CompressAndRecover(objTransform, direction, power));
    }

    //процедура сглаженного сжимания и разжимания
    private void CompressAndRecover()
    {
        //сжимаем и разжимаем все элементы которые попали в коллекцию
        foreach (CompressAndRecover item in CompressAndRecoverElements)
        {
            if (item.thisTransform != null)
            {
                //if (item.compress)
                //{
                    item.thisTransform.localRotation = Quaternion.Euler(-item.direction.y * (item.power * 100f), item.direction.x * (item.power * 100f), item.thisTransform.localRotation.z);
                    //new Quaternion(item.thisTransform.localRotation.x + (item.direction.y * item.power), item.thisTransform.localRotation.y + (-item.direction.x * item.power), item.thisTransform.localRotation.z, item.thisTransform.localRotation.w);
                    item.power -= 0.05f;
                    //item.iteration++;
                //}
                

                if (item.power < 0)
                    CompressAndRecoverElementsForRemove.Add(item);
            }
            else
            {
                CompressAndRecoverElementsForRemove.Add(item);
            }
        }

        foreach (CompressAndRecover item in CompressAndRecoverElementsForRemove)
        {
            CompressAndRecoverElements.Remove(item);
        }
        CompressAndRecoverElementsForRemove.Clear();
    }

    //вспомогательные

    public static GameObject[] FindObjectsInRadiusWithComponent(Vector2 position, float RadiusIn, float RadiusOut, Type component)//поиск объектов по тегу в радиусе
    {
        List<GameObject> objectsToInteract = new List<GameObject>();//список найденных объектов

        Component[] findeObjects = FindObjectsOfType(component) as Component[]; //находим всех объекты с компонентом и создаём массив из них

        foreach (var currentObject in findeObjects) //для каждого объекта в массиве
        {
            float distance = Vector2.Distance(currentObject.transform.position, position);//дистанция до объекта
            if (distance <= RadiusOut && distance >= RadiusIn)//если объект находиться в радиусе действия
            {
                //добавляем в список
                objectsToInteract.Add(currentObject.gameObject);
            }
        }
        return objectsToInteract.ToArray();
    }

}
