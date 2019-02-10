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
        //в зависимости от типа
        switch (shape)
        {
            case ElementsShapeEnum.Empty:
                //удаляем спрайт!!!
                return null;
            case ElementsShapeEnum.Carrot:
                //устанавливаем спрайт
                return Resources.Load<Sprite>("Sprites/star") as Sprite;
            case ElementsShapeEnum.Watermelon:
                return Resources.Load<Sprite>("Sprites/watermelon") as Sprite;
            case ElementsShapeEnum.Apple:
                return Resources.Load<Sprite>("Sprites/apple") as Sprite;
            case ElementsShapeEnum.Strawberry:
                return Resources.Load<Sprite>("Sprites/strawberry") as Sprite;
            case ElementsShapeEnum.Banana:
                return Resources.Load<Sprite>("Sprites/banana") as Sprite;
            case ElementsShapeEnum.Wall:
                return Resources.Load<Sprite>("Sprites/wall") as Sprite;
            case ElementsShapeEnum.BigFlask:
                return Resources.Load<Sprite>("Sprites/BigFlask") as Sprite;
            case ElementsShapeEnum.MediumFlask:
                return Resources.Load<Sprite>("Sprites/MediumFlask") as Sprite;
            case ElementsShapeEnum.SmallFlask:
                return Resources.Load<Sprite>("Sprites/SmallFlask") as Sprite;
            case ElementsShapeEnum.Orange:
                return Resources.Load<Sprite>("Sprites/orange") as Sprite;
            case ElementsShapeEnum.Camomile:
                return Resources.Load<Sprite>("Sprites/camomile") as Sprite;
            case ElementsShapeEnum.Plum:
                return Resources.Load<Sprite>("Sprites/plum") as Sprite;
            default:
                Debug.LogError("Не определен тип " + shape);
                return null;
                
        }
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
        //в зависимости от типа элемента 
        switch (shape)
        {
            case BlockingElementsShapeEnum.Empty:
                //удаляем спрайт!!!
                return null;
            case BlockingElementsShapeEnum.Liana:
                //устанавливаем спрайт
                return Resources.Load<Sprite>("Sprites/liana") as Sprite;
            default:
                Debug.LogError("Не определен тип " + shape);
                return null;
        }
    }

    public static Sprite SetShape(BorderEnum shape)
    {
        //в зависимости от типа элемента 
        switch (shape)
        {
            case BorderEnum.InAngle:
                return Resources.Load<Sprite>("Sprites/Border/inAngel") as Sprite;
            case BorderEnum.OutAngle:
                return Resources.Load<Sprite>("Sprites/Border/outAngel") as Sprite;
            case BorderEnum.Line:
                //устанавливаем спрайт
                return Resources.Load<Sprite>("Sprites/Border/line") as Sprite;
            default:
                Debug.LogError("Не определен тип " + shape);
                return null;
        }
    }

    public static Sprite SetShape(AllShapeEnum shape)
    {
        //поиск в других коллекциях данное значение с перенаправлением к другому SetShape
        //BlockingElementsShapeEnum value2 = Enum.TryParse(value.ToString(), out Enum2 outValue) ? outValue : Enum2.Unknown;

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
        return null;
    }
}
