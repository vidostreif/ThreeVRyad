using UnityEngine;
using System.Collections;


public enum ElementsTypeEnum
{
    Empty, //пустой блок
    Standard, //стандартный блок в который вкладывается элемент
    SpecElement,
    ImmortalWall, //неразрушаемая стена
    CrushableWall, //разрушаемый блок, который предварительно нужно разбить
    Bomb, //бомба
    Dynamite, //динамит
}//Типы элементов

public enum ElementsShapeEnum
{
    Empty, //пустой элемент
    Carrot, //морковь
    Watermelon, //арбуз
    Apple, //яблоко
    Banana, //банан
    Wall, //стандартная стена
    Bomb, //бомба
    Dynamite, //динамит
    Strawberry, //клубника
    Orange,
    Camomile, //ромашка
    Plum, //слива
}//Внешние виды элементов


public enum BlockingElementsTypeEnum
{
    Empty, //пустой элемент
    Standard, //стандарртный блокирующий элемент   
}//Типы блокирующих элементов

public enum BlockingElementsShapeEnum
{
    Empty, //пустой элемент
    Liana, //лиана   
}//Внешние виды блокирующих элементов


public enum AllShapeEnum
{
    Empty, //пустой элемент
    Carrot, //морковь
    Watermelon, //арбуз
    Apple, //яблоко
    Banana, //банан
    Wall, //стандартная стена
    Bomb, //бомба
    Dynamite, //динамит
    Liana, //лиана
    Strawberry, //клубника
    Orange,
    Camomile, //ромашка
    Plum, //слива
}//Внешние виды элементов

public enum BlockTypeEnum
{
    Empty, //пустой блок
    Standard //стандартный блок
}//Типы блоков


public enum AbilitiesEnum
{
    Empty, //пустой элемент
    WateringCan, //лейка
    Hoe //мотыга
}//

public enum HitTypeEnum
{
    Empty,
    Standart, //прямой удар по элементу
    HitFromNearbyElement, //удар от соседнего элемента
    Explosion, //взрыв
    DoubleClick, //двойной клик по элементу
}//Типы ударов по блоку

public enum CollectionTypesEnum {
    Element,
    BlockingElement

}//Типы коллекций

//public enum BonusesEnum
//{
//    Empty,
//    Bomb,//бомба
//    Wall//стена

//}//Типы бонусов

public enum DirectionEnum
{
    Left,
    Right,
    Up,
    Down
}//направление

public enum BorderEnum
{
    Line,
    InAngle,
    OutAngle
}//типы обвотки