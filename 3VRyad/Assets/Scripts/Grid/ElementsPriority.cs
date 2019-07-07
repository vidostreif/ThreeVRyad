using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//вид создаваемого элемента и его приоритет
public class ElementsPriority
{
    public ElementsTypeEnum elementsType;
    [SerializeField] private ElementsShapeEnum elementsShape;
    private int priority;//приоритет при создании новых элементов
    [SerializeField] private int startPriority;//стартовый приоритет
    public int maxAmountOnField = 999;//максимальное одновременное количество на поле
    public int limitOnAmountCreated = 9999;//ограничение на создаваемое количество

    public AllShapeEnum ElementsShape
    {
        get
        {
            return (AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), elementsShape.ToString());
        }

        set
        {
            elementsShape = (ElementsShapeEnum)Enum.Parse(typeof(ElementsShapeEnum), value.ToString());
        }
    }

    public int StartPriority { get => startPriority; }
    public int Priority { get => priority; set => priority = value; }

    public ElementsPriority(ElementsShapeEnum elementsShape, ElementsTypeEnum elementsType, int priority, int maxAmountOnField, int limitOnAmountCreated)
    {
        this.elementsShape = elementsShape;
        this.elementsType = elementsType;
        this.priority = priority;
        this.startPriority = priority;
        this.maxAmountOnField = maxAmountOnField;
        this.limitOnAmountCreated = limitOnAmountCreated;
    }

    public ElementsPriority(AllShapeEnum elementsShape, ElementsTypeEnum elementsType, int priority, int limitOnAmountCreated = 9999)
    {
        this.elementsShape = (ElementsShapeEnum)Enum.Parse(typeof(ElementsShapeEnum), elementsShape.ToString());
        this.elementsType = elementsType;
        this.priority = priority;
        this.startPriority = priority;
        this.limitOnAmountCreated = limitOnAmountCreated;
    }

    public void ResetPriority() {
        this.priority = startPriority;
    }

    public void Info()
    {
        Debug.LogError("elementsShape:" + elementsShape + "elementsType:" + elementsType + " priority: " + Priority);
    }
}
