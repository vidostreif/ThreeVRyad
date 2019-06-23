using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Block : MonoBehaviour {

    public Transform thisTransform;
    [SerializeField] protected BlockTypeEnum type;
    [SerializeField] protected Element element;// элемент
    [SerializeField] protected GameObject arrowUpGO;// стрекла сверху
    [SerializeField] protected GameObject arrowDownGO;// стрелка снизу
    [SerializeField] protected BehindElement behindElement;// элемент на блоке за основным элементом    
    [SerializeField] protected bool generatorElements; //признак, что блок генерирует новые элементы
    [SerializeField] protected bool dontFillOnStart;//не заполнять при старте игры
    [SerializeField] protected GameObject crossBanGO;// крест запрета заполнения на старте
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Position positionInGrid;//позиция в сетке
    [SerializeField] protected bool dropping;//сбрасывающий блок
    [SerializeField] protected bool blocked;//заблокирован для действий

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
                    if (arrowUpGO == null)
                    {
                        arrowUpGO = Instantiate(Resources.Load("Prefabs/Arrow") as GameObject, new Vector3(thisTransform.position.x, thisTransform.position.y + 0.6f, thisTransform.position.z), Quaternion.identity);
                        arrowUpGO.transform.SetParent(thisTransform, true);
                    }
                    
                }
                else
                {
                    Debug.LogError("Блок " + this.name + " нельзя сделать генератором элементов!");
                }
            }
            else
            {
                generatorElements = value;
                DestroyImmediate(arrowUpGO);
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
            //if (this.Type != BlockTypeEnum.Empty)
            //{
                //если елемент не уничтожен
                if (value != null && !value.Destroyed)
                {
                    element = value;
                    element.PositionInGrid = positionInGrid;
                    element.thisTransform.parent = thisTransform;                    
                }
                else if (value == null)
                {
                    element = value;
                }
                else if (value.Destroyed)
                {
                   element = null;
                   return;
                }
            //}
            //else if (value == null)
            //{
            //    element = value;
            //    element.PositionInGrid = positionInGrid;
            //    element.thisTransform.parent = thisTransform;
            //}
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
            if (value != null)
            {
                behindElement = value;
                behindElement.PositionInGrid = positionInGrid;
                behindElement.thisTransform.parent = thisTransform;
            }
            else if (value == null)
            {
                behindElement = value;
            }
        }
    }
    public Position PositionInGrid
    {
        get
        {
            return positionInGrid;
        }

        set
        {
            positionInGrid = value;
            if (element != null)
            {
                element.PositionInGrid = value;
            }
            if (behindElement != null)
            {
                behindElement.PositionInGrid = value;
            }
        }
    }
    public bool Dropping
    {
        get
        {
            return dropping;
        }

        set
        {
            dropping = value;
            if (value == true)
            {
                if (arrowDownGO == null)
                {
                    arrowDownGO = Instantiate(Resources.Load("Prefabs/Arrow") as GameObject, new Vector3(thisTransform.position.x, thisTransform.position.y - 0.6f, thisTransform.position.z), Quaternion.identity);
                    arrowDownGO.transform.SetParent(thisTransform, true);
                }
            }
            else
            {
                DestroyImmediate(arrowDownGO);
            }
        }
    }
    public bool Blocked
    {
        get
        {
            return blocked;
        }

        set
        {
            blocked = value;
        }
    }
    public bool DontFillOnStart
    {
        get
        {
            return dontFillOnStart;
        }

        set
        {
            dontFillOnStart = value;
            //если ставим запрет, то создаем кресст внутри блока
            if (!Application.isPlaying)
            {
                if (dontFillOnStart && crossBanGO == null)
                {
                    crossBanGO = Instantiate(Resources.Load("Prefabs/CrossBan") as GameObject, thisTransform.position, Quaternion.identity);
                    crossBanGO.transform.SetParent(thisTransform, true);
                }
                else if(!dontFillOnStart)
                {
                    DestroyImmediate(crossBanGO);
                }                
            }
            else
            {
                Destroy(crossBanGO);
            }
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
            MainAnimator.Instance.AddElementForSmoothMove(this.Element.thisTransform, thisTransform.position, 1, SmoothEnum.InLineWithSlowdown, smoothTime: 0.1f);
        }
    }

    public Element CreatElement(GameObject prefabElement, AllShapeEnum shape, ElementsTypeEnum typeElementsEnum, AllShapeEnum dopShape = AllShapeEnum.Empty)
    {
        //создаем элемент у блока
        if (this.Type != BlockTypeEnum.Empty)
        {
            //если уже есть элемент то удаляем его
            if (element != null && !element.Destroyed)
            {
                //ElementsList.DellElement((AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), element.Shape.ToString()));
                DellElement();
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
                    
            if (typeElementsEnum == ElementsTypeEnum.StandardElement)
            {                
                curElement = elementGameObject.AddComponent<Element>();
                curElement.InitialSettings(typeElementsEnum, false, false, true, false, true, HitTypeEnum.HitFromNearbyElement ,1 , 100);
            }
            else if (typeElementsEnum == ElementsTypeEnum.CrushableWall)
            {                
                curElement = elementGameObject.AddComponent<ElementWall>();
                curElement.InitialSettings(typeElementsEnum, true, false, false, false, false, HitTypeEnum.Empty, 1, 200);
            }
            else if (typeElementsEnum == ElementsTypeEnum.ImmortalWall)
            {
                curElement = elementGameObject.AddComponent<ElementWall>();
                curElement.InitialSettings(typeElementsEnum, true, true, false, false, false, HitTypeEnum.Empty, 1, 0);
            }
            else if (typeElementsEnum == ElementsTypeEnum.BigFlask)
            {
                curElement = elementGameObject.AddComponent<ElementBigFlask>();
                curElement.InitialSettings(typeElementsEnum, false, false, false, true, true, HitTypeEnum.Explosion, 1, 1500);
            }
            else if (typeElementsEnum == ElementsTypeEnum.MediumFlask)
            {
                curElement = elementGameObject.AddComponent<ElementMediumFlask>();
                curElement.InitialSettings(typeElementsEnum, false, false, false, true, true, HitTypeEnum.Explosion, 1, 1000);
            }
            else if (typeElementsEnum == ElementsTypeEnum.SmallFlask)
            {
                curElement = elementGameObject.AddComponent<ElementSmallFlask>();
                curElement.InitialSettings(typeElementsEnum, false, false, false, true, true, HitTypeEnum.Explosion, 1, 500);
            }
            else if (typeElementsEnum == ElementsTypeEnum.SeedBarrel)
            {
                curElement = elementGameObject.AddComponent<SeedBarrelElement>();
                curElement.InitialSettings(typeElementsEnum, true, true, false, false, false, HitTypeEnum.Empty, 1, 1200);
                curElement.MakeCollector(dopShape, 8);
            }
            else if (typeElementsEnum == ElementsTypeEnum.Drop)
            {
                curElement = elementGameObject.AddComponent<Element>();
                curElement.InitialSettings(typeElementsEnum, false, false, false, false, false, HitTypeEnum.Empty, 1, 600);
                curElement.MakeDrop();
            }
            else if (typeElementsEnum == ElementsTypeEnum.WildPlant)
            {
                curElement = elementGameObject.AddComponent<WildPlantElement>();
                curElement.InitialSettings(typeElementsEnum, true, false, false, false, false, HitTypeEnum.Empty, 7, 1000);
                curElement.MakeActionAfterMove(1, false);
            }
            else
            {
                Debug.LogError("У блока " + this.name + " не удалось определить тип создаваемого элемента");
                DestroyImmediate(elementGameObject);
                return null;
            }

            curElement.Shape = shape;
            HelpToPlayer.AddHint(typeElementsEnum);
            //Добавляем в блок
            this.Element = curElement;
            return this.Element;
        }

        return null;
    }

    private void DellElement() {
        if (element != null)
        {
            //ElementsList.DellElement(element.Shape);
            DestroyImmediate(element.gameObject);
        }
        element = null;
    }

    //создает элемент на заднем плане из префаба, вида, типа элемента и на позиции указанного трансформа или позиции самого блока
    public BehindElement CreatBehindElement(GameObject prefabElement, AllShapeEnum shape, BehindElementsTypeEnum typeElementsEnum, Transform startTransform = null)
    {
        //создаем элемент у блока
        if (this.Type != BlockTypeEnum.Empty)
        {
            //определяем позицию для создания
            Vector3 startPosition;
            if (startTransform != null)
            {
                startPosition = startTransform.position;
            }
            else
            {
                startPosition = thisTransform.position;
            }

            //создаем новый элемент
            GameObject elementGameObject = Instantiate(prefabElement, startPosition, Quaternion.identity);
            BehindElement curElement;

            //если позиция элемента не совпадает с позицией нашего элемента, то перемещаем элемент к блоку
            if (elementGameObject.transform.position != thisTransform.position)
            {
                MainAnimator.Instance.AddElementForSmoothMove(elementGameObject.transform, thisTransform.position, 1, SmoothEnum.InLineWithSlowdown, smoothTime: 0.1f);
            }

            if (typeElementsEnum == BehindElementsTypeEnum.Grass)
            {
                curElement = elementGameObject.AddComponent<BehindElement>();
                curElement.InitialSettings(typeElementsEnum, false, 1, 200);
            }
            else if(typeElementsEnum == BehindElementsTypeEnum.Dirt)
            {
                curElement = elementGameObject.AddComponent<DirtBehindElement>();
                curElement.InitialSettings(typeElementsEnum, false, 1, 300);
                curElement.MakeActionAfterMove(0, true);
                if (ParticleSystemManager.Instance != null)
                {
                    ParticleSystemManager.Instance.CreatePSAsync(thisTransform, PSEnum.PSDirt, 3);
                }                
            }
            else
            {
                Debug.LogError("У блока " + this.name + " не удалось определить тип создаваемого элемента на заднем плане");
                DestroyImmediate(elementGameObject);
                return null;
            }

            curElement.Shape = shape;
            HelpToPlayer.AddHint(typeElementsEnum);
            //Добавляем в блок
            this.BehindElement = curElement;
            return this.BehindElement;
        }

        return null;
    }
    
    //удар по блоку
    public void Hit(HitTypeEnum hitTypeEnum = HitTypeEnum.StandartHit, AllShapeEnum hitElementShape = AllShapeEnum.Empty) {

        if (element != null && !element.Destroyed)
        {
            bool hitBack = element.HitBack;
            element.Hit(hitTypeEnum, hitElementShape);
            //если уничтожили элемент и он ударяет в ответ, то ударяем по элементу позади
            if (hitBack && (behindElement != null && !behindElement.Destroyed) && (element == null || element.Destroyed))
            {
                behindElement.Hit();
            }
        }
        //else if (behindElement != null && !behindElement.Destroyed)
        //{
        //    behindElement.Hit(hitTypeEnum, hitElementShape);
        //}
    }

}
