using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterController : MonoBehaviour
{
    public static MasterController Instance; // Синглтон

    private Transform transforForLocalDAndD = null;
    private Vector3 startPosition = new Vector3(0, 0, 0);
    private Vector3 startPositionLocalDAndD;
    //private Vector3 cursorDeviation; //отклонение курсора
    private float startDragMoment;//момент взятия элемента
    private float maxDistanceToMove;//максимальная дистанция для перемещения
    private readonly double angle = 0.75;// оптимальное значение для вычисление направления смещения елемента
    private Position gridPositionDAndD; //позиция блока в сетке
    private NeighboringBlocks neighboringBlocksDAndD; //соседние блоки
    private Element processedNeighboringElement; // соседний элемент с которым мы взаимодействуем
    private Block blockFieldDAndD;
    private bool change = false;//признак для определения того, что мы будем заменять элемент с соседним
    private DirectionEnum offsetDirection;//направление движения
    //bool matchFound;

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров MasterController!");
        }
        Instance = this;
    }

    void Update()
    {
        MoveDragObject();//процедура перемещения взятого объекта
    }

    public void DragLocalObject(Transform gameObjectTransform)//записываем данные для последующего перемещения объекта за мышкой или пальцем
    {

        if (transforForLocalDAndD == null)
        {
            //проверяем что блока нет в текущих массивах для обработки
            blockFieldDAndD = gameObjectTransform.GetComponentInParent<Block>();
            if (GridBlocks.Instance.BlockInProcessing(blockFieldDAndD))
            {
                return;
            }

            transforForLocalDAndD = gameObjectTransform;
            startPositionLocalDAndD = gameObjectTransform.position;//позиция

            //вычисляем позицию пальца для записи отклонения курсора
            Vector3 touchPosition;
            if (Input.touchCount > 0)
                touchPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            else
                touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            touchPosition.z = startPosition.z;
            //cursorDeviation = touchPosition - startPositionLocalDAndD;

            //записываем момент взятия элемента
            startDragMoment = Time.time;
            maxDistanceToMove = GridBlocks.Instance.blockSize;

            //Находим позицию блока в сетке
            
            gridPositionDAndD = blockFieldDAndD.PositionInGrid;
            //Определяем соседние блоки
            neighboringBlocksDAndD = GridBlocks.Instance.DeterminingNeighboringBlocks(gridPositionDAndD);
            //если стартовая позиция мыши нулевая
            if (startPosition == new Vector3(0, 0, 0))
            {
                RecordStartPosition();
            }
        }
    }



    public void DropLocalObject()// удаляем данные об объекте который перемещяли мышкой или пальцем и возвращаем его на место
    {
        //если определились, что хотим поменять с соседним объектом
        if (change)
        {
            //перемещаем елементы на новую позицию
            GridBlocks.Instance.ExchangeElements(neighboringBlocksDAndD.GetBlock(offsetDirection), blockFieldDAndD);

            //ищем совпавшие линии             
            GridBlocks.Instance.Move(blockFieldDAndD, neighboringBlocksDAndD.GetBlock(offsetDirection));

            ////если совпадение не нашли, то возвращаем элементы обратно
            //if (!matchFound)
            //{
            //    Grid.Instance.ExchangeElements(neighboringBlocksDAndD.GetBlock(offsetDirection), blockFieldDAndD);
            //}

        }
        else if (transforForLocalDAndD != null)
        {
            //возвращаем на стартовую позицию
            transforForLocalDAndD.position = startPosition;
        }

        Reset();//Обнуляем Данные
    }

    private void MoveDragObject()//процедура перемещения взятого объекта
    {
        if (transforForLocalDAndD != null)
        {
            Vector3 touchPosition;
            if (Input.touchCount > 0)
                touchPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            else
                touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //вычисляем позицию пальца
            //touchPosition = touchPosition - cursorDeviation;
            touchPosition.z = startPosition.z;
            //расчитываем вектор смещения
            Vector3 translation = touchPosition - startPosition;
            //вычисляем расстояние на которое смещаем объект
            float offsetDistance = translation.magnitude;
            //нормализируем вектор для упрощения вычисления направления
            Vector3 direction = translation / offsetDistance;

            // Если растояние меньше допустимого то смещаем объект за пальцем
            //уменьшить вектор до нужного размера, что бы смещать элемент на нужное растояние
            if (offsetDistance > maxDistanceToMove)
            {
                translation = direction * maxDistanceToMove;
            }

            //передаем координаты взятому объекту
            // направляем строго влево или вправо или вверх или вниз
            Vector3 newPosition = transforForLocalDAndD.position;
            //DirectionEnum oldDirection = offsetDirection; // записываем старое направление
            if (direction.x > angle || direction.x < -angle)
            {
                newPosition = new Vector3(startPosition.x + translation.x, startPosition.y, startPosition.z);
                //записываем направление
                if (direction.x > 0)
                    offsetDirection = DirectionEnum.Right;
                else
                    offsetDirection = DirectionEnum.Left;
            }
            else if (direction.y > angle || direction.y < -angle)
            {
                newPosition = new Vector3(startPosition.x, startPosition.y + translation.y, startPosition.z);
                //записываем направление
                if (direction.y > 0)
                    offsetDirection = DirectionEnum.Up;
                else
                    offsetDirection = DirectionEnum.Down;
            }

            //узнаем заблокирован ли элемент в укзанном направлении
            Block neighboringBlock = neighboringBlocksDAndD.GetBlock(offsetDirection);
            if (neighboringBlock != null)
            {
                //проверяем что блока нет в текущих массивах для обработки
                if (GridBlocks.Instance.BlockInProcessing(neighboringBlock))
                {
                    return;
                }
                //Если в соседнем блоке есть элемент и он не заблокирован, то смещаем его к нашему блоку на тоже растояние
                //или если нет элемента
                if ((neighboringBlock.Element != null && !neighboringBlock.Element.LockedForMove) || neighboringBlock.Element == null)
                {
                    //записываем достаточно ли мы сместили объект, что бы поменять его с соседним
                    //или если прошло очень мало времени, то меняем элементы
                    if (offsetDistance > maxDistanceToMove * 0.4f || Time.time - startDragMoment < 0.3f)
                        change = true;
                    else
                        change = false;

                    //тащим элемент за пальцем
                    //transforForLocalDAndD.position = newPosition;
                    MainAnimator.Instance.AddElementForSmoothMove(transforForLocalDAndD, newPosition, 2, SmoothEnum.InLineWithOneSpeed, smoothTime: 0.1f);

                    //Если в соседнем блоке есть элемент, то смещаем его к нашему блоку на тоже растояние
                    if (neighboringBlock.Element != null && offsetDistance > maxDistanceToMove * 0.3f)
                    {
                        //расчитываем вектор смещения для соседнего элемента
                        Vector3 neighboringTranslation = newPosition - startPosition;
                        Vector3 newNeighboringPosition = neighboringBlock.transform.position - neighboringTranslation;
                        MainAnimator.Instance.AddElementForSmoothMove(neighboringBlock.Element.transform, newNeighboringPosition, 2, SmoothEnum.InLineWithOneSpeed, smoothTime: 0.1f);
                        neighboringBlock.Element.drag = true;

                        //если изменился соседний блок к которому движемся, то у предыдущего блока сбрасываем что он перетаскивается
                        if (processedNeighboringElement != neighboringBlock.Element)
                        {
                            if (processedNeighboringElement != null)
                            {
                                processedNeighboringElement.drag = false;
                            }
                        }
                        processedNeighboringElement = neighboringBlock.Element;
                    }
                    else if (processedNeighboringElement != null)
                    {
                        processedNeighboringElement.drag = false;
                    }
                }
                else
                {
                    //возвращаем элементы на свои позиции
                    //transforForLocalDAndD.position = startPosition;
                    MainAnimator.Instance.AddElementForSmoothMove(transforForLocalDAndD, startPosition, 2, SmoothEnum.InLineWithOneSpeed, smoothTime: 0.1f);
                    if (processedNeighboringElement != null)
                    {
                        processedNeighboringElement.drag = false;
                    }
                }
            }
            else
            {
                //возвращаем элементы на свои позиции
                //transforForLocalDAndD.position = startPosition;
                MainAnimator.Instance.AddElementForSmoothMove(transforForLocalDAndD, startPosition, 2, SmoothEnum.InLineWithOneSpeed, smoothTime: 0.1f);
                if (processedNeighboringElement != null)
                {
                    processedNeighboringElement.drag = false;
                }
            }
        }
    }

    private void RecordStartPosition()//записываем стартовую позицию мыши или пальца
    {
        //записываем стартовое положение объекта (потом привезать к родителю)
        startPosition = startPositionLocalDAndD;
    }

    private void Reset()//обнуляем значение стартовой позиции
    {
        transforForLocalDAndD = null;
        startPosition = new Vector3(0, 0, 0);
        startPositionLocalDAndD = new Vector3(0, 0, 0);
        //cursorDeviation = new Vector3(0, 0, 0);
        gridPositionDAndD = null;
        neighboringBlocksDAndD = new NeighboringBlocks();
        change = false;

        if (processedNeighboringElement != null)
        {
            processedNeighboringElement.drag = false;
        }
        processedNeighboringElement = null;
    }
}
