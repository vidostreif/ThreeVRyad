using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Bonus{

    [SerializeField] private ElementsTypeEnum type;//тип
    [SerializeField] private ElementsShapeEnum shape;//внешний вид
    [SerializeField] private int cost;//стоимость

    public Bonus(ElementsTypeEnum type, ElementsShapeEnum shape, int cost)
    {
        this.type = type;
        this.shape = shape;
        this.cost = cost;
    }

    public ElementsTypeEnum Type
    {
        get
        {
            return type;
        }
    }

    public ElementsShapeEnum Shape
    {
        get
        {
            return shape;
        }
    }

    public int Cost
    {
        get
        {
            return cost;
        }
    }
}
