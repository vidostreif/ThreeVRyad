using UnityEngine;
using System.Collections;


public enum ElementsTypeEnum
{
    Empty, //пустой блок
    Standard, //стандартный блок в который вкладывается элемент
    SpecElement,
    ImmortalWall, //неразрушаемая стена
    CrushableWall, //разрушаемый блок, который предварительно нужно разбить
    MediumFlask, //фласка средняя
    SmallFlask, //фласка малая
    BigFlask,//фласка большая
}//Типы элементов

public enum ElementsShapeEnum
{
    Empty, //пустой элемент
    Carrot, //морковь
    Watermelon, //арбуз
    Apple, //яблоко
    Banana, //банан
    Wall, //стандартная стена
    MediumFlask, //фласка средняя
    SmallFlask, //фласка малая
    Strawberry, //клубника
    Orange,
    Camomile, //ромашка
    Plum, //слива
    BigFlask,//фласка большая
}//Внешние виды элементов

public enum BehindElementsTypeEnum
{
    Empty, //пустой 
    Standard, //стандартный элемент
    Dirt, //грязь
}//Типы элементов позади

public enum BehindElementsShapeEnum
{
    Empty, //пустой элемент
    Grass, //трава
    Dirt, //грязь
}//Внешние виды элементов позади

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
    MediumFlask, //бомба
    SmallFlask, //динамит
    Liana, //лиана
    Strawberry, //клубника
    Orange,
    Camomile, //ромашка
    Plum, //слива
    BigFlask,//фласка большая
    Grass, //трава
    Dirt, //грязь
}//Внешние виды элементов

public enum BlockTypeEnum
{
    Empty, //пустой блок
    Standard //стандартный блок
}//Типы блоков


public enum InstrumentsEnum
{
    Empty, //пустой элемент
    Shovel, //лопата
    Hoe, //мотыга
    Vortex, //вихрь
    Repainting, //перекраска

}//инсструменты

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

public enum SmoothEnum
{
    InArc,
    InLine,
    InLineWithOneSpeed,
    InLineWithAcceleration
}//тип сглаженного перемещения