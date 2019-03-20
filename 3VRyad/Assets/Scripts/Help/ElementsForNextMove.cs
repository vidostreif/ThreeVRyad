using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementsForNextMove
{
    public List<Element> elementsList = new List<Element>();//список элементов для подсказки
    public Element elementForMove;//элемент для смещения
    public Block blockElementForMove;//блок в котором элемент для смещения
    public Block targetBlock;//блок в который будет перемещен элемент
    public DirectionEnum directionForMove;//направление смещения
    public DirectionEnum oppositeDirectionForMove;//противоположное направление
}
