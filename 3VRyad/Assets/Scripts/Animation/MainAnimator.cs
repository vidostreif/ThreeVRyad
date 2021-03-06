﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

//!!!Сделать статичным

//[ExecuteInEditMode]
public class MainAnimator : MonoBehaviour {

    public static MainAnimator Instance; // Синглтон

    private ElementsForNextMove elementsForNextMove; //элементы для следующей подсказки игроку
    private List<AnimatorElement> animatorsElementsForNextMove; //аниматоры элементов для следующей подсказки игроку
    private float timeToHint = 2; //время до подсказки
    private float idleHintsTime; //момент отсчета

    private List<MoveElement> moveElements;//элементы для перемещения
    private List<MoveElement> moveElementsForRemove;//элементы которые можно удалить из коллекции

    private List<Explosion> explosions;//данные о взрывах
    private List<Explosion> explosionsForRemove;//элементы которые можно удалить из коллекции

    private List<CompressAndRecover> CompressAndRecoverElements;//элементы для перемещения
    private List<CompressAndRecover> CompressAndRecoverElementsForRemove;//элементы которые можно удалить из коллекции

    private List<ChangeColorElement> changeColorElements;//элементы для смены цвета
    private List<ChangeColorElement> changeColorElementsForRemove;//элементы которые можно удалить из коллекции

    //public GameObject boom;
    public GameObject explosionEffect;//префаб
    //public List<GameObject> explosionEffects;// лист взрывов

    public ElementsForNextMove ElementsForNextMove
    {
        set
        {
            elementsForNextMove = value;
            animatorsElementsForNextMove.Clear();
            foreach (Element item in value.elementsList)
            {
                animatorsElementsForNextMove.Add(item.GetComponent<AnimatorElement>());
            }
            idleHintsTime = Time.time;
        }

        get
        {
            return elementsForNextMove;
        }
    }

    void Awake()
    {
        //// регистрация синглтона
        //if (Instance != null)
        //{
        //    Debug.LogError("Несколько экземпляров MainAnimator!");
        //}

        //Instance = this;

        if (Instance)
        {
            Destroy(this.gameObject); //Delete duplicate
            return;
        }
        else
        {
            Instance = this; //Make this object the only instance            
        }
        //if (Application.isPlaying)
        //{
        //    DontDestroyOnLoad(gameObject); //Set as do not destroy
        //}
        ClearAllMassive();
    }
	
	// Update is called once per frame
	void Update () {
        if (animatorsElementsForNextMove.Count > 0 && (idleHintsTime + timeToHint) < Time.time)
        {
            PlayHintAnimations();
        }
        SmoothMove();
        Explosions();
        CompressAndRecover();
        SmoothChangeColor();
    }

    public void ClearAllMassive() {
        elementsForNextMove = new ElementsForNextMove(); //элементы для следующей подсказки игроку
        animatorsElementsForNextMove = new List<AnimatorElement>(); //аниматоры элементов для следующей подсказки игроку

        //если остались объекты для передвижения которые нужно уничтожить
        if (moveElements != null && moveElements.Count > 0)
        {
            foreach (MoveElement item in moveElements)
            {
                if (item.thisTransform.position != item.targetPosition)
                {
                    item.thisTransform.position = item.targetPosition;
                }

                //выполняем прописанный делегат
                if (item.action != null)
                {
                    if (item.action.Method != null && item.action.Target != null)
                    {
                        item.action();
                    }
                }

                if (item.destroyAfterMoving)
                {
                    Destroy(item.thisTransform.gameObject);
                }
            }
        }
        moveElements = new List<MoveElement>();//элементы для перемещения
        moveElementsForRemove = new List<MoveElement>();//элементы которые можно удалить из коллекции

        explosions = new List<Explosion>();//данные о взрывах
        explosionsForRemove = new List<Explosion>();//элементы которые можно удалить из коллекции

        CompressAndRecoverElements = new List<CompressAndRecover>();//элементы для перемещения
        CompressAndRecoverElementsForRemove = new List<CompressAndRecover>();//элементы которые можно удалить из коллекции

        changeColorElements = new List<ChangeColorElement>();//элементы для смены цвета
        changeColorElementsForRemove = new List<ChangeColorElement>();//элементы которые можно удалить из коллекции
}

    //проигрование подскази игроку
    private void PlayHintAnimations() {
        foreach (AnimatorElement item in animatorsElementsForNextMove)
        {
            if (item != null)
            {
                item.PlayHintAnimation();
            }                           
        }

        if (elementsForNextMove != null && elementsForNextMove.elementForMove != null)
        {
            SpriteRenderer spriteRenderer = elementsForNextMove.elementForMove.GetComponent<SpriteRenderer>();
            //AddElementForSmoothChangeColor(spriteRenderer, new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.5f), 20, true, 1);
            Vector3 newPosition = elementsForNextMove.elementForMove.transform.position;
            if (elementsForNextMove.directionForMove == DirectionEnum.Down)
            {
                newPosition = new Vector3(newPosition.x, newPosition.y - 0.2f, newPosition.z);
            }
            else if (elementsForNextMove.directionForMove == DirectionEnum.Up)
            {
                newPosition = new Vector3(newPosition.x, newPosition.y + 0.2f, newPosition.z);
            }
            else if (elementsForNextMove.directionForMove == DirectionEnum.Left)
            {
                newPosition = new Vector3(newPosition.x - 0.2f, newPosition.y, newPosition.z);
            }
            else if (elementsForNextMove.directionForMove == DirectionEnum.Right)
            {
                newPosition = new Vector3(newPosition.x + 0.2f, newPosition.y, newPosition.z);
            }
            AddElementForSmoothMove(elementsForNextMove.elementForMove.transform, newPosition, 2, SmoothEnum.InLineWithSlowdown, 0.05f);
            //elementsForNextMove.elementForMove.transform.position = newPosition;
        }
        
        idleHintsTime = Time.time;
    }

    public void ClearElementsForNextMove() {
        animatorsElementsForNextMove.Clear();
    }
    
    //добавление объекта для сглаженного перемещения
    public void AddElementForSmoothMove(Transform objTransform, Vector3 targetPosition, int priority, SmoothEnum smoothEnum, float smoothTime = 0.05f, bool destroyAfterMoving = false, bool addToQueue = false, Action action = null)
    {
        //проверяем есть ли уже такой объект и какой у него приоритет
        bool add = true;
        bool dellOld = false; //признак, что нужно удалить старые записи
        foreach (MoveElement item in moveElements)
        {
            if (item.thisTransform == objTransform)
            {
                if (item.priority > priority)
                {
                    add = false;
                    break;
                }
                else if (item.priority < priority)
                {
                    dellOld = true;
                    //break;
                }           
            }
        }

        //если определились что будем добавлять элемент в массив
        if (add)
        {

            if (dellOld || !addToQueue)
            {
                //предварительный проверяем, что перемещаемого объекта нет в нашем массиве
                //и если есть, то удаляем его
                moveElements.RemoveAll(p => p.thisTransform == objTransform);
            }

            //добавляем
            moveElements.Add(new MoveElement(objTransform, targetPosition, smoothTime, smoothEnum, priority, destroyAfterMoving, action));
        }        
    }

    //процедура сглаженного перемещения объектов к указанной позиции
    private void SmoothMove()
    {
        //список элементов которые уже двигали, что бы не двигать повторно
        List<Transform> moveIitems = new List<Transform>();

        if (moveElements == null)
        {
            ClearAllMassive();
        }

        //перемещаем все элементы которые попали в коллекцию
        foreach (MoveElement item in moveElements)
        {
            //если еще не двигали
            if (item.thisTransform != null && !moveIitems.Contains(item.thisTransform) && item.thisTransform.gameObject.activeInHierarchy)
            {
                moveIitems.Add(item.thisTransform);

                if (item.smoothEnum == SmoothEnum.InArcWithSlowdown)
                {
                    float newPositionX = Mathf.SmoothDamp(item.thisTransform.position.x, item.targetPosition.x, ref item.yVelocity, Time.deltaTime * 100 * item.smoothTime);
                    float newPositionY = Mathf.SmoothDamp(item.thisTransform.position.y, item.targetPosition.y, ref item.yVelocity, Time.deltaTime * 100 * item.smoothTime);
                    item.thisTransform.position = new Vector3(newPositionX, newPositionY, item.targetPosition.z);
                }
                else if(item.smoothEnum == SmoothEnum.InArc)
                {
                    //if (item.yVelocity < 1.0f)
                    //{
                        item.yVelocity += 0.1f * Time.deltaTime * item.smoothTime * 100;

                        Vector3 m1 = Vector3.Lerp(item.startPosition, item.intermediatePosition, item.yVelocity);
                        Vector3 m2 = Vector3.Lerp(item.intermediatePosition, item.targetPosition, item.yVelocity);
                        item.thisTransform.position = Vector3.Lerp(m1, m2, item.yVelocity);
                    //}
                }
                else if (item.smoothEnum == SmoothEnum.InLineWithSlowdown)
                {                    
                    //расчитываем вектор смещения
                    Vector3 translation = item.targetPosition - item.thisTransform.position;
                    //вычисляем расстояние на которое смещаем объект
                    float offsetDistance = translation.magnitude;
                    //если растояние меньше чем указанно, то моментально перемещаем объект
                    if (offsetDistance < 0.005f)
                    {
                        item.thisTransform.position = item.targetPosition;
                    }
                    else
                    {
                        //item.thisTransform.position = Vector3.SmoothDamp(item.thisTransform.position, item.targetPosition, ref item.vectorVelocity, Time.deltaTime * 100 * 1/item.smoothTime);
                        item.thisTransform.position = Vector3.Lerp(item.thisTransform.position, item.targetPosition, Time.deltaTime * 100 * item.smoothTime);
                    }
                    
                }
                else if (item.smoothEnum == SmoothEnum.InLineWithAcceleration)
                {
                    //перемещаем с ускорением по прямой
                    //расчитываем вектор смещения
                    Vector3 translation = item.targetPosition - item.thisTransform.position;
                    //вычисляем расстояние на которое смещаем объект
                    float offsetDistance = translation.magnitude;
                    //нормализируем вектор для упрощения вычисления направления
                    Vector3 direction = translation / offsetDistance;
                    item.yVelocity += 0.1f;
                    if (offsetDistance > (Time.deltaTime * 100 * item.smoothTime * item.yVelocity))
                    {
                        item.thisTransform.Translate(direction * Time.deltaTime * 100 * item.smoothTime * item.yVelocity);
                    }
                    else
                    {
                        item.thisTransform.position = item.targetPosition;
                    }
                }
                else if (item.smoothEnum == SmoothEnum.InLineWithOneSpeed)
                {
                    //перемещаем с одной скоростью по прямой
                    //расчитываем вектор смещения
                    Vector3 translation = item.targetPosition - item.thisTransform.position;
                    //вычисляем расстояние на которое нужно сместить объект
                    float requiredOffsetDistance = translation.magnitude;
                    //нормализируем вектор для упрощения вычисления направления
                    Vector3 direction = translation / requiredOffsetDistance;
                    //расстояние
                    float factor = Time.deltaTime * 100 * item.smoothTime;                                 

                    if (factor < requiredOffsetDistance)
                    {
                        //вектор для смещения
                        Vector3 offset = direction * factor;
                        Vector3 newPosition = item.thisTransform.position + offset;
                        item.thisTransform.position = newPosition;
                    }
                    else
                    {
                        item.thisTransform.position = item.targetPosition;
                    }
                }

                if (item.thisTransform.position == item.targetPosition)
                {              
                    moveElementsForRemove.Add(item);
                }
            }
            else if (item.thisTransform == null)
            {
                moveElementsForRemove.Add(item);
            }
        }

        foreach (MoveElement item in moveElementsForRemove)
        {
            //выполняем прописанный делегат
            if (item.action != null)
            {
                if (item.action.Method != null && item.action.Target != null)
                {
                    item.action();
                }
            }
            //удаляем если требуется
            if (item.destroyAfterMoving && item.thisTransform != null)
                Destroy(item.thisTransform.gameObject, 0.1f);
            moveElements.Remove(item);
        }
        moveElementsForRemove.Clear();
    }

    public void AddExplosionEffect(Vector3 epicenter, float power)
    {
        //GameObject explosionEf = Instantiate(explosionEffect, epicenter, Quaternion.identity);
        GameObject explosionEf = ParticleSystemManager.Instance.CreatePS(this.transform, PSEnum.PSBoom);
        explosionEf.transform.position = epicenter;
        explosionEf.transform.localScale = explosionEf.transform.localScale * power;
        explosions.Add(new Explosion(epicenter, power, Time.time, explosionEf));
    }
    
    //эффекты взрыва
    private void Explosions() {
        //создаем эффекты взрыва
        foreach (Explosion item in explosions)
        {
            if (item.moment < Time.time)
            {
                //item.explosionEffect.transform.localScale = new Vector3(item.radiusExplosionEffect, item.radiusExplosionEffect, 1);
                //item.radiusExplosionEffect += 2.2f;
                item.moment = Time.time + 0.05f;
                item.radius += GridBlocks.Instance.blockSize * 0.95f * item.power;

                    GameObject[] objectsToMove = FindObjectsInRadiusWithComponent(item.epicenter, item.radius, item.radius + GridBlocks.Instance.blockSize, typeof(Element));

                    foreach (GameObject objectToMove in objectsToMove)
                    {
                        if (objectToMove.transform != null)
                        {
                            Element element = objectToMove.transform.GetComponent<Element>();
                            if (!element.Destroyed)
                            {
                                //расчитываем вектор смещения
                                Vector3 translation = objectToMove.transform.position - item.epicenter;
                                //вычисляем расстояние до объекта
                                float offsetDistance = translation.magnitude;
                                //нормализируем вектор для упрощения вычисления направления
                                Vector3 direction = translation / offsetDistance;
                                AddElementForSmoothMove(objectToMove.transform, objectToMove.transform.position + direction * item.power * 0.2f, 5, SmoothEnum.InArcWithSlowdown, 0.01f);
                            //AddElementForCompressAndRecover(objectToMove.transform, direction, item.power);                            
                                //AnimatorElement animatorElement = objectToMove.GetComponent<AnimatorElement>();
                            element.AnimatElement.PlayIdleAnimation();
                            }                            
                        }
                    }
                             
                item.power *= 0.5f;
                item.iteration--;
            }

            if (item.iteration == 0)
                explosionsForRemove.Add(item);
        }

        //удаляем из массива
        foreach (Explosion item in explosionsForRemove)
        {
            //Destroy(item.explosionEffect, 2);
            item.explosionEffect.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            PoolManager.Instance.ReturnObjectToPool(item.explosionEffect, 2);
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
            if (item.thisTransform != null && item.thisTransform.gameObject.activeInHierarchy)
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

    //добавление каритнки для плавного изменения цвета
    public void AddElementForSmoothChangeColor(Image image, Color newColor, float speed)
    {
        changeColorElements.Add(new ChangeColorElement(image, newColor, speed));
    }

    public void AddElementForSmoothChangeColor(Image image, Color newColor, float speed, bool inCycle, int numberOfCycles = 1000)
    {
        changeColorElements.Add(new ChangeColorElement(image, newColor, speed, inCycle, numberOfCycles * 2));
    }

    //удаляем из массива сглаженной смены цвета
    public void DellElementForSmoothChangeColor(Image image)
    {
        ChangeColorElement curItem = null;
        foreach (ChangeColorElement item in changeColorElements)
        {
            if (item.image == image)
            {
                curItem = item;
                //возвращаем стандартный цвет
                item.image.color = item.standartColor;
                break;
            }
        }
        if (curItem != null)
        {
            changeColorElements.Remove(curItem);
        }
    }

    //добавление каритнки для плавного изменения цвета
    public void AddElementForSmoothChangeColor(SpriteRenderer spriteRenderer, Color newColor, float speed)
    {
        changeColorElements.Add(new ChangeColorElement(spriteRenderer, newColor, speed));
    }

    public void AddElementForSmoothChangeColor(SpriteRenderer spriteRenderer, Color newColor, float speed, bool inCycle, int numberOfCycles = 1000)
    {
        changeColorElements.Add(new ChangeColorElement(spriteRenderer, newColor, speed, inCycle, numberOfCycles * 2));
    }

    //удаляем из массива сглаженной смены цвета
    public void DellElementForSmoothChangeColor(SpriteRenderer spriteRenderer)
    {
        ChangeColorElement curItem = null;
        foreach (ChangeColorElement item in changeColorElements)
        {
            if (item.spriteRenderer != null && item.spriteRenderer == spriteRenderer)
            {
                curItem = item;
                //возвращаем стандартный цвет
                item.spriteRenderer.color = item.standartColor;
                break;
            }
        }
        if (curItem != null)
        {
            changeColorElements.Remove(curItem);
        }
    }

    //изменение цвета
    private void SmoothChangeColor()
    {        
        foreach (ChangeColorElement item in changeColorElements)
        {
            if (item.image != null && item.image.gameObject.activeInHierarchy)
            {
                item.image.color = Color.Lerp(item.image.color, item.newColor, Time.deltaTime * item.speed);
                if (item.image.color == item.newColor)
                {
                    if (item.inCycle && item.numberOfCycles > 0)
                    {   
                        //обмениваем стартовый и конечный цвет
                        Color startColor = item.startColor;
                        item.startColor = item.newColor;
                        item.newColor = startColor;
                        item.numberOfCycles--;
                    }
                    else if (item.inCycle)
                    {
                        //возвращаем стандартный цвет
                        item.image.color = item.standartColor;
                        changeColorElementsForRemove.Add(item);
                    }
                    else
                    {
                        changeColorElementsForRemove.Add(item);
                    }                    
                }                    
            }
            else if (item.spriteRenderer != null && item.spriteRenderer.gameObject.activeInHierarchy)
            {
                item.spriteRenderer.color = Color.Lerp(item.spriteRenderer.color, item.newColor, Time.deltaTime * item.speed);
                if (item.spriteRenderer.color == item.newColor)
                {
                    if (item.inCycle && item.numberOfCycles > 0)
                    {
                        //обмениваем стартовый и конечный цвет
                        Color startColor = item.startColor;
                        item.startColor = item.newColor;
                        item.newColor = startColor;
                        item.numberOfCycles--;
                    }
                    else if (item.inCycle)
                    {
                        //возвращаем стандартный цвет
                        item.spriteRenderer.color = item.standartColor;
                        changeColorElementsForRemove.Add(item);
                    }
                    else
                    {
                        changeColorElementsForRemove.Add(item);
                    }
                }
            }
            else
            {
                changeColorElementsForRemove.Add(item);
            }
        }

        //удаление из массива
        foreach (ChangeColorElement item in changeColorElementsForRemove)
        {
            changeColorElements.Remove(item);
        }
        changeColorElementsForRemove.Clear();
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
