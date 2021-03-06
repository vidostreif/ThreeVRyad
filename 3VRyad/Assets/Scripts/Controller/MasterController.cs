﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MasterController : MonoBehaviour
{
    public static MasterController Instance; // Синглтон

    private Transform transforForLocal = null;
    private Vector3 startPosition = new Vector3(0, 0, 0);
    private Vector3 startPositionLocal;
    private float startDragMoment;//момент взятия элемента
    private float maxDistanceToMove;//максимальная дистанция для перемещения
    private readonly double angle = 0.75;// оптимальное значение для вычисление направления смещения елемента
    private NeighboringBlocks neighboringBlocks; //соседние блоки
    private Element processedNeighboringElement; // соседний элемент с которым мы взаимодействуем
    private Block block;
    private BlockController blockController;
    private bool change = false;//признак для определения того, что мы будем заменять элемент с соседним
    private DirectionEnum offsetDirection;//направление движения

    void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject); //Delete duplicate
            return;
        }
        else
        {
            Instance = this; //Make this object the only instance            
        }
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject); //Set as do not destroy
        }
        //// регистрация синглтона
        //if (Instance != null)
        //{
        //    Debug.LogError("Несколько экземпляров MasterController!");
        //}
        //Instance = this;
    }

    void Update()
    {
        MoveElement();//процедура перемещения взятого объекта
    }

    public void DragElement(BlockController blockController)//записываем данные для последующего перемещения объекта за мышкой или пальцем
    {

        if (transforForLocal == null)
        {
            this.blockController = blockController;
            //проверяем что блока нет в текущих массивах для обработки
            block = blockController.GetComponentInParent<Block>();
            if (GridBlocks.Instance.BlockInProcessing(block))
            {
                return;
            }

            transforForLocal = blockController.thisBlock.Element.thisTransform;
            startPositionLocal = transforForLocal.position;//позиция

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
            
            //gridPosition = block.PositionInGrid;
            //Определяем соседние блоки
            neighboringBlocks = GridBlocks.Instance.GetNeighboringBlocks(block.PositionInGrid);
            //если стартовая позиция мыши нулевая
            if (startPosition == new Vector3(0, 0, 0))
            {
                RecordStartPosition();
            }
        }
    }
       
    public void DropElement()// удаляем данные об объекте который перемещяли мышкой или пальцем и возвращаем его на место
    {
        //если определились, что хотим поменять с соседним объектом
        if (change)
        {
            //перемещаем елементы на новую позицию
            GridBlocks.Instance.ExchangeElements(neighboringBlocks.GetBlock(offsetDirection), block);

            //ищем совпавшие линии             
            GridBlocks.Instance.Move(block, neighboringBlocks.GetBlock(offsetDirection));
        }
        else if (transforForLocal != null)
        {
            //возвращаем на стартовую позицию
            transforForLocal.position = startPosition;
        }

        Reset();//Обнуляем Данные
    }

    public void ForcedDropElement()//принудительно бросаем элемент
    {
        if (blockController != null)
        {
            change = false;
            blockController.EndDrag(blockController.pointerEventData);
            //Debug.Log("Drop");
        }
    }

    private void MoveElement()//процедура перемещения взятого объекта
    {
        if (transforForLocal != null)
        {
            Vector3 touchPosition;
            if (Input.touchCount > 0)
                touchPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            else
                touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //вычисляем позицию пальца
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
            Vector3 newPosition = transforForLocal.position;
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

            //разрешено ли нам двигаться в данном направлении?
            bool canMove;
            if (blockController.permittedDirection == DirectionEnum.All)
            {
                canMove = true;
            }
            else if (blockController.permittedDirection == offsetDirection)
            {
                canMove = true;
            }
            else
            {
                canMove = false;
            }

            //если блок в указанном направлении существует и он стандартный и в этом направлении нам разрешено двигаться
            Block neighboringBlock = neighboringBlocks.GetBlock(offsetDirection);
            if (BlockCheck.ThisStandartBlock(neighboringBlock) && canMove)
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
                    MainAnimator.Instance.AddElementForSmoothMove(transforForLocal, newPosition, 2, SmoothEnum.InLineWithOneSpeed, smoothTime: 0.1f);

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
                    MainAnimator.Instance.AddElementForSmoothMove(transforForLocal, startPosition, 2, SmoothEnum.InLineWithOneSpeed, smoothTime: 0.1f);
                    change = false;
                    if (processedNeighboringElement != null)
                    {
                        processedNeighboringElement.drag = false;
                    }
                }
            }
            else
            {
                //возвращаем элементы на свои позиции
                MainAnimator.Instance.AddElementForSmoothMove(transforForLocal, startPosition, 2, SmoothEnum.InLineWithOneSpeed, smoothTime: 0.1f);
                change = false;
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
        startPosition = startPositionLocal;
    }

    private void Reset()//обнуляем значение стартовой позиции
    {
        transforForLocal = null;
        startPosition = new Vector3(0, 0, 0);
        startPositionLocal = new Vector3(0, 0, 0);
        blockController = null;
        neighboringBlocks = new NeighboringBlocks();
        change = false;

        if (processedNeighboringElement != null)
        {
            processedNeighboringElement.drag = false;
        }
        processedNeighboringElement = null;
    }
}
