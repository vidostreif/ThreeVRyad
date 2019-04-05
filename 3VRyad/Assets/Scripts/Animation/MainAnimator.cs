using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

//!!!Сделать статичным

[ExecuteInEditMode]
public class MainAnimator : MonoBehaviour {

    public static MainAnimator Instance; // Синглтон

    private ElementsForNextMove elementsForNextMove = new ElementsForNextMove(); //элементы для следующей подсказки игроку
    private List<AnimatorElement> animatorsElementsForNextMove = new List<AnimatorElement>(); //аниматоры элементов для следующей подсказки игроку
    private float timeToHint = 3; //время до подсказки
    private float idleHintsTime; //момент отсчета

    private List<MoveElement> moveElements = new List<MoveElement>();//элементы для перемещения
    private List<MoveElement> moveElementsForRemove = new List<MoveElement>();//элементы которые можно удалить из коллекции

    private List<Explosion> explosions = new List<Explosion>();//данные о взрывах
    private List<Explosion> explosionsForRemove = new List<Explosion>();//элементы которые можно удалить из коллекции

    private List<CompressAndRecover> CompressAndRecoverElements = new List<CompressAndRecover>();//элементы для перемещения
    private List<CompressAndRecover> CompressAndRecoverElementsForRemove = new List<CompressAndRecover>();//элементы которые можно удалить из коллекции

    private List<ChangeColorElement> changeColorElements = new List<ChangeColorElement>();//элементы для смены цвета
    private List<ChangeColorElement> changeColorElementsForRemove = new List<ChangeColorElement>();//элементы которые можно удалить из коллекции

    public GameObject boom;
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
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров MainAnimator!");
        }

        Instance = this;
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

    //проигрование подскази игроку
    private void PlayHintAnimations() {
        foreach (AnimatorElement item in animatorsElementsForNextMove)
        {
            if (item != null)
            {
                item.PlayHintAnimation();
            }                           
        }
        idleHintsTime = Time.time;
    }

    public void ClearElementsForNextMove() {
        animatorsElementsForNextMove.Clear();
    }
    
    //добавление объекта для сглаженного перемещения
    public void AddElementForSmoothMove(Transform objTransform, Vector3 targetPosition, int priority, SmoothEnum smoothEnum, float smoothTime = 0.05f, bool destroyAfterMoving = false, bool addToQueue = false)
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
                    break;
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
            moveElements.Add(new MoveElement(objTransform, targetPosition, smoothTime, smoothEnum, priority, destroyAfterMoving));
        }
        
    }

    //процедура сглаженного перемещения объектов к указанной позиции
    private void SmoothMove()
    {

        //список элементов которые уже двигали, что бы не двигать повторно
        List<Transform> moveIitems = new List<Transform>();

        //перемещаем все элементы которые попали в коллекцию
        foreach (MoveElement item in moveElements)
        {
            //если еще не двигали
            if (item.thisTransform != null && !moveIitems.Contains(item.thisTransform))
            {
                moveIitems.Add(item.thisTransform);

                if (item.smoothEnum == SmoothEnum.InArc)
                {
                    float newPositionX = Mathf.SmoothDamp(item.thisTransform.position.x, item.targetPosition.x, ref item.yVelocity, Time.deltaTime * 100 * item.smoothTime);
                    float newPositionY = Mathf.SmoothDamp(item.thisTransform.position.y, item.targetPosition.y, ref item.yVelocity, Time.deltaTime * 100 * item.smoothTime);
                    item.thisTransform.position = new Vector3(newPositionX, newPositionY, item.targetPosition.z);
                }
                else if(item.smoothEnum == SmoothEnum.InArcWithAcceleration)
                {
                    if (item.yVelocity < 1.0f)
                    {
                        item.yVelocity += item.smoothTime * Time.deltaTime;

                        Vector3 m1 = Vector3.Lerp(item.startPosition, item.intermediatePosition, item.yVelocity);
                        Vector3 m2 = Vector3.Lerp(item.intermediatePosition, item.targetPosition, item.yVelocity);
                        item.thisTransform.position = Vector3.Lerp(m1, m2, item.yVelocity);
                    }
                }
                else if (item.smoothEnum == SmoothEnum.InLine)
                {
                    item.thisTransform.position = Vector3.Lerp(item.thisTransform.position, item.targetPosition, Time.deltaTime * 100 * item.smoothTime);
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
                    //перемещаем с ускорением по прямой
                    //расчитываем вектор смещения
                    Vector3 translation = item.targetPosition - item.thisTransform.position;
                    //вычисляем расстояние на которое смещаем объект
                    float offsetDistance = translation.magnitude;
                    //нормализируем вектор для упрощения вычисления направления
                    Vector3 direction = translation / offsetDistance;
                    item.yVelocity = 1f;
                    if (offsetDistance > (Time.deltaTime * 100 * item.smoothTime * item.yVelocity))
                    {
                        item.thisTransform.Translate(direction * Time.deltaTime * 100 * item.smoothTime * item.yVelocity);
                    }
                    else
                    {
                        item.thisTransform.position = item.targetPosition;
                    }

                }

                if (item.thisTransform.position == item.targetPosition)
                {
                    //if (item.smoothEnum == SmoothEnum.InLineWithOneSpeed)
                    //{
                    //    AnimatorElement animatorElement = item.thisTransform.GetComponent<AnimatorElement>();
                    //    animatorElement.PlayFallAnimation();
                    //}

                    if (item.destroyAfterMoving)
                        Destroy(item.thisTransform.gameObject, 0.5f);
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
            moveElements.Remove(item);
        }
        moveElementsForRemove.Clear();
    }

    public void AddExplosionEffect(Vector3 epicenter, float power) {

        GameObject explosionEf = Instantiate(explosionEffect, epicenter, Quaternion.identity);
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
                            //расчитываем вектор смещения
                            Vector3 translation = objectToMove.transform.position - item.epicenter;
                            //вычисляем расстояние до объекта
                            float offsetDistance = translation.magnitude;
                            //нормализируем вектор для упрощения вычисления направления
                            Vector3 direction = translation / offsetDistance;
                            AddElementForSmoothMove(objectToMove.transform, objectToMove.transform.position + direction * item.power * 0.2f, 5, SmoothEnum.InArc, 0.01f);
                            //AddElementForCompressAndRecover(objectToMove.transform, direction, item.power);
                            AnimatorElement animatorElement = objectToMove.GetComponent<AnimatorElement>();
                            animatorElement.PlayIdleAnimation();
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
            Destroy(item.explosionEffect, 2);
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

    public void AddElementForSmoothChangeColor(Image image, Color newColor, float speed)
    {
        changeColorElements.Add(new ChangeColorElement(image, newColor, speed));
    }

    //изменение цвета
    private void SmoothChangeColor()
    {        
        foreach (ChangeColorElement item in changeColorElements)
        {
            if (item.image != null)
            {
                item.image.color = Color.Lerp(item.image.color, item.newColor, Time.deltaTime * item.speed);
                if (item.image.color == item.newColor)
                    changeColorElementsForRemove.Add(item);
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
