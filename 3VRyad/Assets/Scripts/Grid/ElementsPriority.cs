using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//вид создаваемого элемента и его приоритет
public class ElementsPriority
{
    public ElementsTypeEnum elementsType;
    public ElementsShapeEnum elementsShape;
    public int priority;

    public ElementsPriority(ElementsShapeEnum elementsShape, ElementsTypeEnum elementsType, int priority)
    {
        this.elementsShape = elementsShape;
        this.elementsType = elementsType;
        this.priority = priority;
    }

    public void Info()
    {
        Debug.LogError("elementsShape:" + elementsShape + "elementsType:" + elementsType + " priority: " + priority);
    }
}
