using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ElementsList 
{
    private static QuantityElement[] elementsOnField;

    private static void CreateElementsOnField() {
        //Debug.Log("Создание elementsOnField");
        elementsOnField = new QuantityElement[Enum.GetNames(typeof(AllShapeEnum)).Length];

        int i = 0;
        foreach (AllShapeEnum ShapeEnum in Enum.GetValues(typeof(AllShapeEnum)))
        {
            elementsOnField[i] = new QuantityElement(ShapeEnum, 0);
            i++;
        }

        //наоходим все объекты с базовым элементом и подсчитываем количество элементов каждого вида
        BaseElement[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(BaseElement)) as BaseElement[]; //находим всех объекты с компонентом и создаём массив из них

        foreach (BaseElement item in findeObjects)
        {
            AddElement(item.Shape);
        }
    }

    public static void ClearElementsOnField()
    {
        //Debug.Log("Уничтожение elementsOnField");
        elementsOnField = null;
    }

    //добавляем количество элементов на поле
    public static void AddElement(AllShapeEnum shape, int quantity = 1) {
        if (elementsOnField == null)
        {
            CreateElementsOnField();
        }
        else
        {
            elementsOnField[(int)shape].quantity += quantity;
        }          
    }

    //удаление элементов с поля
    public static void DellElement(AllShapeEnum shape, int quantity = 1)
    {
        if (elementsOnField == null)
        {
            CreateElementsOnField();
        }

        elementsOnField[(int)shape].quantity -= quantity;
    }

    public static int GetAmountOfThisShapeElemets(AllShapeEnum shape) {

        if (elementsOnField == null)
        {
            CreateElementsOnField();
        }

        return elementsOnField[(int)shape].quantity;
    }
}

public class QuantityElement {
    public AllShapeEnum shape;
    public int quantity;

    public QuantityElement(AllShapeEnum shape, int quantity)
    {
        this.shape = shape;
        this.quantity = quantity;
    }
}