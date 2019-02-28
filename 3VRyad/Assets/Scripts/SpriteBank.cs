using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteBank : MonoBehaviour
{
    public static SpriteBank Instance; // Синглтон

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров SpriteBank!");
        }
        Instance = this;
    }

    public static Sprite SetShape(ElementsShapeEnum shape)
    {
        ////в зависимости от типа
        //switch (shape)
        //{
        //    case ElementsShapeEnum.Empty:
        //        //удаляем спрайт!!!
        //        return null;
        //    case ElementsShapeEnum.Carrot:
        //        //устанавливаем спрайт
        //        return Resources.Load<Sprite>("Sprites/Elements/star") as Sprite;
        //    case ElementsShapeEnum.Watermelon:
        //        return Resources.Load<Sprite>("Sprites/Elements/watermelon") as Sprite;
        //    case ElementsShapeEnum.Apple:
        //        return Resources.Load<Sprite>("Sprites/Elements/apple") as Sprite;
        //    case ElementsShapeEnum.Strawberry:
        //        return Resources.Load<Sprite>("Sprites/Elements/strawberry") as Sprite;
        //    case ElementsShapeEnum.Banana:
        //        return Resources.Load<Sprite>("Sprites/Elements/banana") as Sprite;
        //    case ElementsShapeEnum.Wall:
        //        return Resources.Load<Sprite>("Sprites/Elements/wall") as Sprite;
        //    case ElementsShapeEnum.BigFlask:
        //        return Resources.Load<Sprite>("Sprites/Elements/BigFlask") as Sprite;
        //    case ElementsShapeEnum.MediumFlask:
        //        return Resources.Load<Sprite>("Sprites/Elements/MediumFlask") as Sprite;
        //    case ElementsShapeEnum.SmallFlask:
        //        return Resources.Load<Sprite>("Sprites/Elements/SmallFlask") as Sprite;
        //    case ElementsShapeEnum.Orange:
        //        return Resources.Load<Sprite>("Sprites/Elements/orange") as Sprite;
        //    case ElementsShapeEnum.Camomile:
        //        return Resources.Load<Sprite>("Sprites/Elements/camomile") as Sprite;
        //    case ElementsShapeEnum.Plum:
        //        return Resources.Load<Sprite>("Sprites/Elements/plum") as Sprite;
        //    case ElementsShapeEnum.Mushroom:
                return Resources.Load<Sprite>("Sprites/Elements/" + shape.ToString()) as Sprite;
                
        //    default:
        //        Debug.LogError("Не определен тип " + shape);
        //        return null;
                
        //}
    }

    public static Sprite SetShape(BehindElementsShapeEnum shape)
    {
        ////в зависимости от типа
        //switch (shape)
        //{
        //    case BehindElementsShapeEnum.Empty:
        //        //удаляем спрайт!!!
        //        return null;
        //    case BehindElementsShapeEnum.Grass:
        //        //устанавливаем спрайт
        //        return Resources.Load<Sprite>("Sprites/BehindElements/Grass") as Sprite;
        //    case BehindElementsShapeEnum.Dirt:
        //        //устанавливаем спрайт
        //        return Resources.Load<Sprite>("Sprites/BehindElements/Dirt") as Sprite;
        //    default:
        //        Debug.LogError("Не определен тип " + shape);
        //        return null;

        //}

        return Resources.Load<Sprite>("Sprites/BehindElements/" + shape.ToString()) as Sprite;
    }

    public static Sprite SetShape(BlockTypeEnum shape)
    {

        //в зависимости от типа блока 
        switch (shape)
        {
            case BlockTypeEnum.Empty:
                return null;
            case BlockTypeEnum.Standard:
                return Resources.Load<Sprite>("Sprites/ground") as Sprite;
            default:
                Debug.LogError("Не определен тип " + shape);
                return null;
        }
    }

    public static Sprite SetShape(BlockingElementsShapeEnum shape)
    {
        ////в зависимости от типа элемента 
        //switch (shape)
        //{
        //    case BlockingElementsShapeEnum.Empty:
        //        //удаляем спрайт!!!
        //        return null;
        //    case BlockingElementsShapeEnum.Liana:
        //        //устанавливаем спрайт
        //        return Resources.Load<Sprite>("Sprites/BlockingElement/liana") as Sprite;
        //    default:
        //        Debug.LogError("Не определен тип " + shape);
        //        return null;
        //}

        return Resources.Load<Sprite>("Sprites/BehindElements/" + shape.ToString()) as Sprite;
    }

    public static Sprite SetShape(InstrumentsEnum shape)
    {
        ////в зависимости от типа
        //switch (shape)
        //{
        //    case InstrumentsEnum.Empty:
        //        //удаляем спрайт!!!
        //        return null;
        //    case InstrumentsEnum.Shovel:
        //        //устанавливаем спрайт
        //        return Resources.Load<Sprite>("Sprites/Instruments/Shovel") as Sprite;
        //    case InstrumentsEnum.Hoe:
        //        return Resources.Load<Sprite>("Sprites/Instruments/Hoe") as Sprite;
        //    case InstrumentsEnum.Vortex:
        //        return Resources.Load<Sprite>("Sprites/Instruments/Vortex") as Sprite;
        //    case InstrumentsEnum.Repainting:
        //        return Resources.Load<Sprite>("Sprites/Instruments/Repainting") as Sprite;
        //    default:
        //        Debug.LogError("Не определен тип " + shape);
        //        return null;

        //}

        return Resources.Load<Sprite>("Sprites/Instruments/" + shape.ToString()) as Sprite;
    }

    public static Sprite SetShape(BorderEnum shape)
    {
        ////в зависимости от типа элемента 
        //switch (shape)
        //{
        //    case BorderEnum.InAngle:
        //        return Resources.Load<Sprite>("Sprites/Border/inAngel") as Sprite;
        //    case BorderEnum.OutAngle:
        //        return Resources.Load<Sprite>("Sprites/Border/outAngel") as Sprite;
        //    case BorderEnum.Line:
        //        //устанавливаем спрайт
        //        return Resources.Load<Sprite>("Sprites/Border/line") as Sprite;
        //    default:
        //        Debug.LogError("Не определен тип " + shape);
        //        return null;
        //}

        return Resources.Load<Sprite>("Sprites/Border/" + shape.ToString()) as Sprite;
    }

    public static Sprite SetShape(AllShapeEnum shape)
    {
        //поиск в других коллекциях данное значение с перенаправлением к другому SetShape

        if (Enum.IsDefined(typeof(BlockingElementsShapeEnum), shape.ToString()))
        {
           return SetShape((BlockingElementsShapeEnum)Enum.Parse(typeof(BlockingElementsShapeEnum), shape.ToString()));
        }
        else if (Enum.IsDefined(typeof(ElementsShapeEnum), shape.ToString()))
        {
            return SetShape((ElementsShapeEnum)Enum.Parse(typeof(ElementsShapeEnum), shape.ToString()));
        }
        else if (Enum.IsDefined(typeof(BlockTypeEnum), shape.ToString()))
        {
            return SetShape((BlockTypeEnum)Enum.Parse(typeof(BlockTypeEnum), shape.ToString()));
        }
        else if (Enum.IsDefined(typeof(BehindElementsShapeEnum), shape.ToString()))
        {
            return SetShape((BehindElementsShapeEnum)Enum.Parse(typeof(BehindElementsShapeEnum), shape.ToString()));
        }
        return null;
    }
}
