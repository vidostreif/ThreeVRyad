using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ElementsList 
{
    private static QuantityElement[] elementsOnField;

    private static void CreateElementsOnField() {

        elementsOnField = new QuantityElement[Enum.GetNames(typeof(AllShapeEnum)).Length];

        int i = 0;
        foreach (AllShapeEnum ShapeEnum in Enum.GetValues(typeof(AllShapeEnum)))
        {
            elementsOnField[i].shape = ShapeEnum;
            elementsOnField[i].quantity = 0;
            i++;
        }
    }

    //добавляем количество элементов на поле
    public static void AddElement(AllShapeEnum shape, int quantity = 1) {
        if (elementsOnField == null)
        {
            CreateElementsOnField();
        }

        QuantityElement quantityElement = elementsOnField.Find(item => item.shape == shape);
        if (quantityElement != null)
        {
            quantityElement.quantity += quantity;
        }
        else
        {
            elementsOnField.Add(new QuantityElement(shape, quantity));
        }        
    }

    //удаление элементов с поля
    public static void DellElement(AllShapeEnum shape, int quantity = 1)
    {
        if (elementsOnField == null)
        {
            CreateElementsOnField();
        }

        QuantityElement quantityElement = elementsOnField.Find(item => item.shape == shape);
        if (quantityElement != null)
        {
            quantityElement.quantity -= quantity;
        }
        else
        {
            Debug.Log("Удаляем не существующий в ElementsList элемент!");
        }
    }

    public static int GetAmountOfThisShapeElemets(AllShapeEnum shape) {

        if (elementsOnField == null)
        {
            CreateElementsOnField();
        }
        QuantityElement quantityElement = elementsOnField.Find(item => item.shape == shape);
            
            if (quantityElement != null)
            {
                return quantityElement.quantity;
            }
            else
            {
                Debug.Log("запрошено количество не существующего элемента!");
                return 0;
            }
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