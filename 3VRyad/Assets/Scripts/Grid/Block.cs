﻿using System.Collections;
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
            element.thisTransform.parent = thisTransform;
        }
    }

    public void CreatElement(GameObject prefabElement, ElementsShapeEnum shape, ElementsTypeEnum typeElementsEnum)
    {
        //создаем элемент у блока
        if (this.Type != BlockTypeEnum.Empty)
        {
            ////если уже есть элемент то удаляем его
            //if (element != null)
            //{
            //    DestroyImmediate(element.gameObject);
            //}

            //создаем новый элемент
            GameObject elementGameObject = Instantiate(prefabElement, new Vector3(thisTransform.position.x, thisTransform.position.y + 1, thisTransform.position.z), Quaternion.identity);
            Element curElement;
                    
            if (typeElementsEnum == ElementsTypeEnum.Standard)
            {                
                curElement = elementGameObject.AddComponent<Element>();
                curElement.InitialSettings(typeElementsEnum, false, false, true, false, HitTypeEnum.HitFromNearbyElement);
            }
            else if (typeElementsEnum == ElementsTypeEnum.CrushableWall)
            {                
                curElement = elementGameObject.AddComponent<ElementWall>();
                curElement.InitialSettings(typeElementsEnum, true, false, false, false, HitTypeEnum.Empty);
            }
            else if (typeElementsEnum == ElementsTypeEnum.ImmortalWall)
            {
                curElement = elementGameObject.AddComponent<ElementWall>();
                curElement.InitialSettings(typeElementsEnum, true, true, false, false, HitTypeEnum.Empty);
            }
            else if (typeElementsEnum == ElementsTypeEnum.BigFlask)
            {
                curElement = elementGameObject.AddComponent<ElementBigFlask>();
                curElement.InitialSettings(typeElementsEnum, false, false, false, true, HitTypeEnum.Explosion);
            }
            else if (typeElementsEnum == ElementsTypeEnum.MediumFlask)
            {
                curElement = elementGameObject.AddComponent<ElementMediumFlask>();
                curElement.InitialSettings(typeElementsEnum, false, false, false, true, HitTypeEnum.Explosion);
            }
            else if (typeElementsEnum == ElementsTypeEnum.SmallFlask)
            {
                curElement = elementGameObject.AddComponent<ElementSmallFlask>();
                curElement.InitialSettings(typeElementsEnum, false, false, false, true, HitTypeEnum.Explosion);
            }
            else
            {
                Debug.LogError("У блока " + this.name + " не удалось определить тип создаваемого элемента");
                DestroyImmediate(elementGameObject);
                return;
            }

            curElement.Shape = shape;
            //Добавляем в блок
            this.Element = curElement;

        }
    }

    public void DellElement() {
        if (element != null)
        {
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
                curElement.InitialSettings(typeElementsEnum);
            }
            else
            {
                Debug.LogError("У блока " + this.name + " не удалось определить тип создаваемого элемента на заднем плане");
                DestroyImmediate(elementGameObject);
                return;
            }

            curElement.Shape = shape;
            //Добавляем в блок
            this.BehindElement = curElement;
        }
    }

    public void DellBehindElement()
    {
        if (behindElement != null)
        {
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
    
    void Awake()
    {
        thisTransform = transform;
        //objectManagement = GetComponent<SpriteBank>();
        spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
    }
    	
	// Update is called once per frame
	void Update () {
        //если есть элемент и он не на позиции нашего блока то медлено премещаем его к блоку
        if (this.Element != null && this.Element.thisTransform.position != thisTransform.position && !this.Element.drag && !this.Element.Destroyed)
        {
            MainAnimator.Instance.AddElementForSmoothMove(this.Element.thisTransform, thisTransform.position, 1, SmoothEnum.InLine, smoothTime: 0.1f);
        }        
    }
}
