using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class SpriteBank 
{
    private static string qSuffix = "@2x";
    //массивы предзагруженных спрайтов
    private static SpriteResurseArray[] spriteResurseArray = null;

    //предзагрузка всех картинок
    public static void Preload()
    {
        CreateSpritesList();
    }

    //создаем листы спрайтов
    private static void CreateSpritesList()
    {
        if (spriteResurseArray == null)
        {
            //определение префикса под нужное разрешение экрана
            qSuffix = GetQuality();

            spriteResurseArray = new SpriteResurseArray[6];
            SpriteResurse[] spriteResursesElementsShapeEnum = CreateSpriteResurseList(typeof(ElementsShapeEnum));
            int i = 0;
            foreach (var curEnum in Enum.GetValues(typeof(ElementsShapeEnum)))
            {
                Sprite sprite = SetShape((ElementsShapeEnum)Enum.Parse(typeof(ElementsShapeEnum), curEnum.ToString()));

                spriteResursesElementsShapeEnum[i] = new SpriteResurse(curEnum.ToString(), sprite);
                i++;
            }
            spriteResurseArray[0] = new SpriteResurseArray(typeof(ElementsShapeEnum), spriteResursesElementsShapeEnum);

            SpriteResurse[] spriteResursesBehindElementsShapeEnum = CreateSpriteResurseList(typeof(BehindElementsShapeEnum));
            i = 0;
            foreach (var curEnum in Enum.GetValues(typeof(BehindElementsShapeEnum)))
            {
                Sprite sprite = SetShape((BehindElementsShapeEnum)Enum.Parse(typeof(BehindElementsShapeEnum), curEnum.ToString()));

                spriteResursesBehindElementsShapeEnum[i] = new SpriteResurse(curEnum.ToString(), sprite);
                i++;
            }
            spriteResurseArray[1] = new SpriteResurseArray(typeof(BehindElementsShapeEnum), spriteResursesBehindElementsShapeEnum);

            SpriteResurse[] spriteResursesBlockTypeEnum = CreateSpriteResurseList(typeof(BlockTypeEnum));
            i = 0;
            foreach (var curEnum in Enum.GetValues(typeof(BlockTypeEnum)))
            {
                Sprite sprite = SetShape((BlockTypeEnum)Enum.Parse(typeof(BlockTypeEnum), curEnum.ToString()));

                spriteResursesBlockTypeEnum[i] = new SpriteResurse(curEnum.ToString(), sprite);
                i++;
            }
            spriteResurseArray[2] = new SpriteResurseArray(typeof(BlockTypeEnum), spriteResursesBlockTypeEnum);

            SpriteResurse[] spriteResursesBlockingElementsShapeEnum = CreateSpriteResurseList(typeof(BlockingElementsShapeEnum));
            i = 0;
            foreach (var curEnum in Enum.GetValues(typeof(BlockingElementsShapeEnum)))
            {
                Sprite sprite = SetShape((BlockingElementsShapeEnum)Enum.Parse(typeof(BlockingElementsShapeEnum), curEnum.ToString()));

                spriteResursesBlockingElementsShapeEnum[i] = new SpriteResurse(curEnum.ToString(), sprite);
                i++;
            }
            spriteResurseArray[3] = new SpriteResurseArray(typeof(BlockingElementsShapeEnum), spriteResursesBlockingElementsShapeEnum);

            SpriteResurse[] spriteResursesInstrumentsEnum = CreateSpriteResurseList(typeof(InstrumentsEnum));
            i = 0;
            foreach (var curEnum in Enum.GetValues(typeof(InstrumentsEnum)))
            {
                Sprite sprite = SetShape((InstrumentsEnum)Enum.Parse(typeof(InstrumentsEnum), curEnum.ToString()));

                spriteResursesInstrumentsEnum[i] = new SpriteResurse(curEnum.ToString(), sprite);
                i++;
            }
            spriteResurseArray[4] = new SpriteResurseArray(typeof(InstrumentsEnum), spriteResursesInstrumentsEnum);

            SpriteResurse[] spriteResursesBorderEnum = CreateSpriteResurseList(typeof(BorderEnum));
            i = 0;
            foreach (var curEnum in Enum.GetValues(typeof(BorderEnum)))
            {
                Sprite sprite = SetShape((BorderEnum)Enum.Parse(typeof(BorderEnum), curEnum.ToString()));

                spriteResursesBorderEnum[i] = new SpriteResurse(curEnum.ToString(), sprite);
                i++;
            }
            spriteResurseArray[5] = new SpriteResurseArray(typeof(BorderEnum), spriteResursesBorderEnum);
        }
    }

    private static string GetQuality()
    {
        int screenH = Screen.height;
        if (screenH > 1400)
            return "@4x";
        else if (screenH < 720)
            return "@1x";
        else
            return "@2x";
    }

    private static Sprite GetSprite(Type enumType, int spriteNumber) {
        CreateSpritesList();
        foreach (SpriteResurseArray item in spriteResurseArray)
        {
            if (item != null && item.EnumType == enumType)
            {
                return item.SpriteResurse[spriteNumber].Sprite;
            }
        }
        return null;
    }

    private static SpriteResurse[] CreateSpriteResurseList(Type type)
    {
        //создаем массив ресурсов
        int count = Enum.GetNames(type).Length;        
        return new SpriteResurse[count];
    }

    public static Sprite SetShape(ElementsShapeEnum shape, int option = 0, bool mini = false)
    {
        CreateSpritesList();
        ////в зависимости от типа
        if (shape == ElementsShapeEnum.Empty)
        {
            return Resources.Load<Sprite>("Sprites/plus") as Sprite;
        }
        else
        {
            //пробуем получить предзагруженный спрайт
            if (option == 0)
            {
                Sprite sprite = GetSprite(typeof(ElementsShapeEnum), (int)shape);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            //если не смогли получить предзагруженный спрайт или option != 0
            string dopString = "";
            if (option != 0)
            {
                dopString = "_" + option;
            }

            return Resources.Load<Sprite>("Sprites/Elements/" + shape.ToString() + dopString + (mini ? "@1x" : qSuffix)) as Sprite;
        }
    }

    public static Sprite SetShape(BehindElementsShapeEnum shape, int option = 0, bool mini = false)
    {
        CreateSpritesList();
        ////в зависимости от типа
        if (shape == BehindElementsShapeEnum.Empty)
        {
            return Resources.Load<Sprite>("Sprites/plus") as Sprite;
        }
        else
        {
            //пробуем получить предзагруженный спрайт
            if (option == 0)
            {
                Sprite sprite = GetSprite(typeof(BehindElementsShapeEnum), (int)shape);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            string dopString = "";
            if (option != 0)
            {
                dopString = "_" + option;
            }

            return Resources.Load<Sprite>("Sprites/BehindElements/" + shape.ToString() + dopString + (mini ? "@1x" : qSuffix)) as Sprite;
        }        
    }

    public static Sprite SetShape(BlockTypeEnum shape, int option = 0)
    {
        CreateSpritesList();
        ////в зависимости от типа
        if (shape == BlockTypeEnum.Empty)
        {
            return Resources.Load<Sprite>("Sprites/plus") as Sprite;
        }
        else
        {
            //пробуем получить предзагруженный спрайт
            if (option == 0)
            {
                Sprite sprite = GetSprite(typeof(BlockTypeEnum), (int)shape);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            string dopString = "";
            if (option != 0)
            {
                dopString = "_" + option;
            }

            //в зависимости от типа блока 
            switch (shape)
            {
                case BlockTypeEnum.Empty:
                    return null;
                case BlockTypeEnum.StandardBlock:
                    return Resources.Load<Sprite>("Sprites/Blocks/ground" + dopString + qSuffix) as Sprite;
                case BlockTypeEnum.Sliding:
                    return Resources.Load<Sprite>("Sprites/Blocks/Sliding" + dopString + qSuffix) as Sprite;
                default:
                    Debug.LogError("Не определен тип " + shape);
                    return null;
            }
        }


    }

    public static Sprite SetShape(BlockingElementsShapeEnum shape, int option = 0, bool mini = false)
    {
        CreateSpritesList();
        ////в зависимости от типа
        if (shape == BlockingElementsShapeEnum.Empty)
        {
            return Resources.Load<Sprite>("Sprites/plus") as Sprite;
        }
        else
        {
            //пробуем получить предзагруженный спрайт
            if (option == 0)
            {
                Sprite sprite = GetSprite(typeof(BlockingElementsShapeEnum), (int)shape);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            string dopString = "";
            if (option != 0)
            {
                dopString = "_" + option;
            }

            return Resources.Load<Sprite>("Sprites/BlockingElement/" + shape.ToString() + dopString + (mini ? "@1x" : qSuffix)) as Sprite;
        }
        
    }

    public static Sprite SetShape(InstrumentsEnum shape, bool mini = false)
    {
        CreateSpritesList();
        ////в зависимости от типа
        if (shape == InstrumentsEnum.Empty)
        {
            return Resources.Load<Sprite>("Sprites/plus") as Sprite;
        }
        else
        {
            //пробуем получить предзагруженный спрайт
            if (mini == false)
            {
                Sprite sprite = GetSprite(typeof(InstrumentsEnum), (int)shape);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            return Resources.Load("Sprites/Instruments/" + shape.ToString() + (mini ? "@1x" : qSuffix), typeof(Sprite)) as Sprite;
        }        
    }

    public static Sprite SetShape(SpritesEnum shape, bool mini = false)
    {
        CreateSpritesList();
        ////в зависимости от типа
        if (shape == SpritesEnum.Empty)
        {
            return Resources.Load<Sprite>("Sprites/plus") as Sprite;
        }
        else
        {
            //пробуем получить предзагруженный спрайт
            if (mini == false)
            {
                Sprite sprite = GetSprite(typeof(SpritesEnum), (int)shape);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            ////в зависимости от типа
            switch (shape)
            {
                case SpritesEnum.Button_Shop:
                    return Resources.Load<Sprite>("Sprites/interface/Buttons/" + shape.ToString()) as Sprite;
                case SpritesEnum.Button_Shop_Close:
                    return Resources.Load<Sprite>("Sprites/interface/Buttons/" + shape.ToString()) as Sprite;

                //case SpritesEnum.Targets_panel:
                //    return Resources.Load<Sprite>("Sprites/interface/" + shape.ToString() + qSuffix) as Sprite;
                default:
                    return Resources.Load("Sprites/interface/" + shape.ToString() + (mini ? "@1x" : qSuffix), typeof(Sprite)) as Sprite;
            }            
        }
    }

    public static Sprite SetShape(BorderEnum shape)
    {
        CreateSpritesList();
        ////в зависимости от типа
        if (shape == BorderEnum.Empty)
        {
            return Resources.Load<Sprite>("Sprites/plus") as Sprite;
        }
        else
        {
            //пробуем получить предзагруженный спрайт
            //if (option == 0)
            //{
                Sprite sprite = GetSprite(typeof(BorderEnum), (int)shape);
                if (sprite != null)
                {
                    return sprite;
                }
            //}

            return Resources.Load<Sprite>("Sprites/Border/" + shape.ToString() + qSuffix) as Sprite;
        }
        
    }

    public static Sprite SetShape(AllShapeEnum shape, int option = 0, bool mini = false)
    {
        //поиск в других коллекциях данное значение с перенаправлением к другому SetShape

        if (Enum.IsDefined(typeof(BlockingElementsShapeEnum), shape.ToString()))
        {
           return SetShape((BlockingElementsShapeEnum)Enum.Parse(typeof(BlockingElementsShapeEnum), shape.ToString()), option, mini);
        }
        else if (Enum.IsDefined(typeof(ElementsShapeEnum), shape.ToString()))
        {
            return SetShape((ElementsShapeEnum)Enum.Parse(typeof(ElementsShapeEnum), shape.ToString()), option, mini);
        }
        else if (Enum.IsDefined(typeof(BlockTypeEnum), shape.ToString()))
        {
            return SetShape((BlockTypeEnum)Enum.Parse(typeof(BlockTypeEnum), shape.ToString()), option);
        }
        else if (Enum.IsDefined(typeof(BehindElementsShapeEnum), shape.ToString()))
        {
            return SetShape((BehindElementsShapeEnum)Enum.Parse(typeof(BehindElementsShapeEnum), shape.ToString()), option, mini);
        }
        return null;
    }
}

public class SpriteResurseArray
{
    private Type enumType;
    private SpriteResurse[] spriteResurse;

    public Type EnumType { get => enumType; }
    public SpriteResurse[] SpriteResurse { get => spriteResurse; }

    public SpriteResurseArray(Type enumType, SpriteResurse[] spriteResurse)
    {
        this.enumType = enumType;
        this.spriteResurse = spriteResurse;
    }
}

public class SpriteResurse
{
    private string spriteName;
    private Sprite sprite;

    public string SpriteName { get => spriteName; }
    public Sprite Sprite { get => sprite; }

    public SpriteResurse(string spriteName, Sprite sprite)
    {
        this.spriteName = spriteName;
        this.sprite = sprite;
    }
}
