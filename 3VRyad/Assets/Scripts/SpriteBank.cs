using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class SpriteBank 
{
    public static Sprite SetShape(ElementsShapeEnum shape, int option = 0)
    {
        ////в зависимости от типа
        if (shape == ElementsShapeEnum.Empty)
        {
            return Resources.Load<Sprite>("Sprites/plus") as Sprite;
        }
        else
        {
            string dopString = "";
            if (option != 0)
            {
                dopString = "_" + option;
            }

            return Resources.Load<Sprite>("Sprites/Elements/" + shape.ToString() + dopString) as Sprite;
        }               

    }

    public static Sprite SetShape(BehindElementsShapeEnum shape, int option = 0)
    {
        ////в зависимости от типа
        if (shape == BehindElementsShapeEnum.Empty)
        {
            return Resources.Load<Sprite>("Sprites/plus") as Sprite;
        }
        else
        {
            string dopString = "";
            if (option != 0)
            {
                dopString = "_" + option;
            }

            return Resources.Load<Sprite>("Sprites/BehindElements/" + shape.ToString() + dopString) as Sprite;
        }        
    }

    public static Sprite SetShape(BlockTypeEnum shape, int option = 0)
    {
        //в зависимости от типа блока 
        switch (shape)
        {
            case BlockTypeEnum.Empty:
                return null;
            case BlockTypeEnum.StandardBlock:
                return Resources.Load<Sprite>("Sprites/Blocks/ground") as Sprite;
            case BlockTypeEnum.Sliding:
                return Resources.Load<Sprite>("Sprites/Blocks/Sliding") as Sprite;
            default:
                Debug.LogError("Не определен тип " + shape);
                return null;
        }
    }

    public static Sprite SetShape(BlockingElementsShapeEnum shape, int option = 0)
    {
        ////в зависимости от типа
        if (shape == BlockingElementsShapeEnum.Empty)
        {
            return Resources.Load<Sprite>("Sprites/plus") as Sprite;
        }
        else
        {
            string dopString = "";
            if (option != 0)
            {
                dopString = "_" + option;
            }

            return Resources.Load<Sprite>("Sprites/BlockingElement/" + shape.ToString() + dopString) as Sprite;
        }
        
    }

    public static Sprite SetShape(InstrumentsEnum shape, bool mini = false)
    {
        ////в зависимости от типа
        if (shape == InstrumentsEnum.Empty)
        {
            return Resources.Load<Sprite>("Sprites/plus") as Sprite;
        }
        else
        {
            return Resources.Load("Sprites/Instruments/" + shape.ToString() + (mini ? "_mini" : ""), typeof(Sprite)) as Sprite;
        }        
    }

    public static Sprite SetShape(BorderEnum shape)
    {
        ////в зависимости от типа
        if (shape == BorderEnum.Empty)
        {
            return Resources.Load<Sprite>("Sprites/plus") as Sprite;
        }
        else
        {
            return Resources.Load<Sprite>("Sprites/Border/" + shape.ToString()) as Sprite;
        }
        
    }

    public static Sprite SetShape(AllShapeEnum shape, int option = 0)
    {
        //поиск в других коллекциях данное значение с перенаправлением к другому SetShape

        if (Enum.IsDefined(typeof(BlockingElementsShapeEnum), shape.ToString()))
        {
           return SetShape((BlockingElementsShapeEnum)Enum.Parse(typeof(BlockingElementsShapeEnum), shape.ToString()), option);
        }
        else if (Enum.IsDefined(typeof(ElementsShapeEnum), shape.ToString()))
        {
            return SetShape((ElementsShapeEnum)Enum.Parse(typeof(ElementsShapeEnum), shape.ToString()), option);
        }
        else if (Enum.IsDefined(typeof(BlockTypeEnum), shape.ToString()))
        {
            return SetShape((BlockTypeEnum)Enum.Parse(typeof(BlockTypeEnum), shape.ToString()), option);
        }
        else if (Enum.IsDefined(typeof(BehindElementsShapeEnum), shape.ToString()))
        {
            return SetShape((BehindElementsShapeEnum)Enum.Parse(typeof(BehindElementsShapeEnum), shape.ToString()), option);
        }
        return null;
    }
}
