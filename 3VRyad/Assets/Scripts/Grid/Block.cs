﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Block : MonoBehaviour {

    public Transform thisTransform;
    [SerializeField] protected BlockTypeEnum type;
    [SerializeField] protected Element element;// элемент
    [SerializeField] protected BehindElement behindElement;// элемент на блоке за основным элементом
    [SerializeField] protected bool generatorElements; //признак, что блок генерирует новые элементы
    [SerializeField] protected SpriteRenderer spriteRenderer;

    public BlockTypeEnum Type
    {
        get
        {
            return type;
        }

        set
        {
            type = value;
            spriteRenderer.sprite = SpriteBank.SetShape(value);
            //objectManagement.SetShape(value);
        }
    }
    
    public bool GeneratorElements
    {
        get
        {
            return generatorElements;
        }

        set
        {
            //если пытаемся установить положительное значение, то проверяем тип нашего блока
            if (value == true)
            {
                //создавать элементы может только стандартный блок
                if (type != BlockTypeEnum.Empty)
                {
                    generatorElements = value;
                }
                else
                {
                    Debug.LogError("Блок " + this.name + " нельзя сделать генератором элементов!");
                }
            }
            else
            {
                generatorElements = value;
            }
            
        }
    }
        
    public Element Element
    {
        get
        {
            return element;
        }
        set
        {
            //заменяем элемент у блока
            if (this.Type != BlockTypeEnum.Empty)
            {
                //если елемент не заблокирован
                if (value != null)
                {
                    element = value;
                    element.thisTransform.parent = thisTransform;                    
                }
                else if (value == null)
                {
                    element = value;
                }
                else
                {
                    Debug.LogError("Попытка замены элемента у заблокированного блока " + this.name);
                    return;
                }
            }
            else if (value == null)
            {
                element = value;
                element.thisTransform.parent = thisTransform;
            }
        }
    }

    public BehindElement BehindElement
    {
        get
        {
            return behindElement;
        }

        set
        {
            behindElement = value;
            behindElement.thisTransform.parent = thisTransform;
        }
    }

    void Awake()
    {
        thisTransform = transform;
        spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
    }

    // Update is called once per frame
    void Update()
    {
        //если есть элемент и он не на позиции нашего блока то медлено премещаем его к блоку
        if (MainAnimator.Instance != null && this.Element != null && this.Element.thisTransform.position != thisTransform.position && !this.Element.drag && !this.Element.Destroyed)
        {
            MainAnimator.Instance.AddElementForSmoothMove(this.Element.thisTransform, thisTransform.position, 1, SmoothEnum.InLine, smoothTime: 0.1f);
        }
    }

    public void CreatElement(GameObject prefabElement, ElementsShapeEnum shape, ElementsTypeEnum typeElementsEnum)
    {
        //создаем элемент у блока
        if (this.Type != BlockTypeEnum.Empty)
        {
            //если уже есть элемент то удаляем его
            if (element != null)
            {
                ElementsList.DellElement((AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), element.Shape.ToString()));
                DestroyImmediate(element.gameObject);
            }

            //если позиция создания элемента совпадает с позицией блока и блок создает новые элементы
            float addY = 0;
            if (/*position == thisTransform.position &&*/ generatorElements)
            {
                addY = 1;
            }

            //создаем новый элемент
            GameObject elementGameObject = Instantiate(prefabElement, new Vector3(thisTransform.position.x, thisTransform.position.y + addY, thisTransform.position.z), Quaternion.identity);
            Element curElement;
                    
            if (typeElementsEnum == ElementsTypeEnum.Standard)
            {                
                curElement = elementGameObject.AddComponent<Element>();
                curElement.InitialSettings(typeElementsEnum, false, false, true, false, HitTypeEnum.HitFromNearbyElement, 100);
            }
            else if (typeElementsEnum == ElementsTypeEnum.CrushableWall)
            {                
                curElement = elementGameObject.AddComponent<ElementWall>();
                curElement.InitialSettings(typeElementsEnum, true, false, false, false, HitTypeEnum.Empty, 200);
            }
            else if (typeElementsEnum == ElementsTypeEnum.ImmortalWall)
            {
                curElement = elementGameObject.AddComponent<ElementWall>();
                curElement.InitialSettings(typeElementsEnum, true, true, false, false, HitTypeEnum.Empty, 0);
            }
            else if (typeElementsEnum == ElementsTypeEnum.BigFlask)
            {
                curElement = elementGameObject.AddComponent<ElementBigFlask>();
                curElement.InitialSettings(typeElementsEnum, false, false, false, true, HitTypeEnum.Explosion, 1200);
            }
            else if (typeElementsEnum == ElementsTypeEnum.MediumFlask)
            {
                curElement = elementGameObject.AddComponent<ElementMediumFlask>();
                curElement.InitialSettings(typeElementsEnum, false, false, false, true, HitTypeEnum.Explosion, 600);
            }
            else if (typeElementsEnum == ElementsTypeEnum.SmallFlask)
            {
                curElement = elementGameObject.AddComponent<ElementSmallFlask>();
                curElement.InitialSettings(typeElementsEnum, false, false, false, true, HitTypeEnum.Explosion, 300);
            }
            else
            {
                Debug.LogError("У блока " + this.name + " не удалось определить тип создаваемого элемента");
                DestroyImmediate(elementGameObject);
                return;
            }

            curElement.Shape = shape;
            ElementsList.AddElement((AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), element.Shape.ToString())); 
            //Добавляем в блок
            this.Element = curElement;

        }
    }

    public void DellElement() {
        if (element != null)
        {
            ElementsList.DellElement((AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), element.Shape.ToString()));
            DestroyImmediate(element.gameObject);
        }
        element = null;
    }

    public void CreatBehindElement(GameObject prefabElement, BehindElementsShapeEnum shape, BehindElementsTypeEnum typeElementsEnum)
    {
        //создаем элемент у блока
        if (this.Type != BlockTypeEnum.Empty)
        {
            //создаем новый элемент
            GameObject elementGameObject = Instantiate(prefabElement, new Vector3(thisTransform.position.x, thisTransform.position.y, thisTransform.position.z), Quaternion.identity);
            BehindElement curElement;

            if (typeElementsEnum == BehindElementsTypeEnum.Standard)
            {
                curElement = elementGameObject.AddComponent<BehindElement>();
                curElement.InitialSettings(typeElementsEnum, false, 1, false, 0, 100);
            }
            else if(typeElementsEnum == BehindElementsTypeEnum.Dirt)
            {
                curElement = elementGameObject.AddComponent<DirtBehindElement>();
                curElement.InitialSettings(typeElementsEnum, false, 1, true, 2, 100);
            }
            else
            {
                Debug.LogError("У блока " + this.name + " не удалось определить тип создаваемого элемента на заднем плане");
                DestroyImmediate(elementGameObject);
                return;
            }

            curElement.Shape = shape;
            ElementsList.AddElement((AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), curElement.Shape.ToString()));
            //Добавляем в блок
            this.BehindElement = curElement;
        }
    }

    public void DellBehindElement()
    {
        if (behindElement != null)
        {
            ElementsList.DellElement((AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), behindElement.Shape.ToString()));
            DestroyImmediate(behindElement.gameObject);
        }
        behindElement = null;
    }
    
    //удар по блоку
    public void Hit(HitTypeEnum hitTypeEnum = HitTypeEnum.Standart, ElementsShapeEnum hitElementShape = ElementsShapeEnum.Empty) {

        if (element != null && !element.Destroyed)
        {
            element.Hit(hitTypeEnum, hitElementShape);
            //если уничтожили элемент то ударяем по элементу позади
            if ((behindElement != null && !behindElement.Destroyed) && (element == null || element.Destroyed))
            {
                behindElement.Hit();
            }
        }
    }

}
