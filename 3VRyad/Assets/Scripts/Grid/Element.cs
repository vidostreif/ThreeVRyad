using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]// требуем наличие у объекта SpriteRenderer
//[RequireComponent(typeof(SpriteBank))]// требуем наличие у объекта ObjectManagement
[RequireComponent(typeof(ElementController))]// требуем наличие у объекта ElementController

/// наследоваться от интерфейса?
//описание поведения стандартного элемента
[ExecuteInEditMode]
public class Element : MonoBehaviour
{
    public bool drag; //элемент перетаскивается
    [SerializeField] protected ElementsTypeEnum type;//тип элемента
    [SerializeField] protected ElementsShapeEnum shape;//форма элемента
    [SerializeField] protected BlockingElement blockingElement;// блокирующий элемент
    public Transform thisTransform;
    [SerializeField] protected bool lockedForMove;//признак что элемент заблокирован для передвижения
    [SerializeField] protected bool destroyed;//признак что элемент был уничтожен
    [SerializeField] protected bool immortal;//признак бессмертия
    [SerializeField] protected bool createLine;//признак что элемент создает линию
    [SerializeField] protected bool activated;//признак что элемент активируемый
    [SerializeField] protected HitTypeEnum thisHitTypeEnum;//тип удара у блока
    //protected SpriteBank objectManagement;
    protected SpriteRenderer spriteRenderer;


    public bool LockedForMove {
        get
        {
            return lockedForMove;
        }
    }//признак что элемент заблокирован для передвижения
    public bool Destroyed {
        get
        {
            return destroyed;
        }
    }//признак что элемент был уничтожен
    public bool Immortal {
        get
        {
            return immortal;
        }
    }//признак бессмертия
    public bool CreateLine {
        get
        {
            return createLine;
        }
    }//признак что элемент создает линию
    public bool Activated
    {
        get
        {
            return activated;
        }
    }//признак что элемент активируемый
    public ElementsTypeEnum Type {
        get
        {
            return type;
        }
    }
    public ElementsShapeEnum Shape
    {
        get
        {
            return shape;
        }

        set
        {
            shape = value;
            spriteRenderer.sprite = SpriteBank.SetShape(value);
        }
    }
    public HitTypeEnum HitTypeEnum
    {
        get
        {
            return thisHitTypeEnum;
        }
    }

    public virtual void Awake()
    {
        drag = false;
        thisTransform = transform;
        destroyed = false;
        //objectManagement = GetComponent<SpriteBank>();
        spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        GetComponent<ElementController>().ThisElement = this;
    }
        
    //установка настроек элементов
    public void InitialSettings(ElementsTypeEnum type, bool lockedForMove, bool immortal, bool createLine, bool activated, HitTypeEnum hitTypeEnum) {
        this.type = type;
        this.lockedForMove = lockedForMove;
        this.immortal = immortal;
        this.createLine = createLine;
        this.activated = activated;
        this.thisHitTypeEnum = hitTypeEnum;
        DopSettings();
    }

    protected virtual void DopSettings()
    {

    }

    //удар элементу
    public virtual void Hit(HitTypeEnum hitType = HitTypeEnum.Standart, ElementsShapeEnum hitElementShape = ElementsShapeEnum.Empty)
    {
        if (!destroyed)
        {
            //если прямой удар или взрыв
            if (hitType == HitTypeEnum.Standart || hitType == HitTypeEnum.Explosion)
            {
                //Debug.LogWarning("Удар по элементу " + this.transform.parent.name);
                //если стоит блокировка на элементе, то пытаемся ее снять
                if (BlockingElementExists())
                {
                    blockingElement = blockingElement.Hit();

                    //если уничтожили блокирующий элемент
                    if (blockingElement.Destroyed)
                    {
                        lockedForMove = false;
                    }
                }
                //если элемент не заблокирован, то уничтожаем элемент        
                else
                {
                    //если элемент не бессмертный
                    if (!Immortal)
                    {
                        //воздействие на соседние блоки
                        destroyed = true;
                        if (hitType == HitTypeEnum.Standart)
                            HitNeighboringBlocks(thisHitTypeEnum);
                        if (!Tasks.Instance.Collect(this))
                        {
                            AnimatorElement animatorElement = this.GetComponent<AnimatorElement>();
                            animatorElement.PlayDestroyAnimation();
                        }
                    }
                }
            }
        }

    }

    public virtual void Activate() {

    }

    public virtual BlockingElement BlockingElement
    {
        get
        {
            return blockingElement;
        }
        set
        {                
                if (value != null)
                {
                    blockingElement = value;
                    blockingElement.transform.parent = thisTransform;

                    ////если растояние меньше размеров блока сетки, то перемещаем моментально на позицию блока
                    ////расчитываем вектор смещения
                    //Vector3 vector = blockingElement.transform.position - thisTransform.position;
                    ////вычисляем расстояние на которое смещаем объект
                    //float distance = vector.magnitude;

                    //if (distance < Grid.Instance.blockSize)
                    //{
                        blockingElement.transform.position = thisTransform.position;
                //}

                lockedForMove = true;
                }
                else if (value == null)
                {
                    blockingElement = value;
                lockedForMove = false;
            }
        }
    }


    public virtual void CreatBlockingElement(GameObject prefabBlockingElement, BlockingElementsShapeEnum shape, BlockingElementsTypeEnum typeBlockingElementsEnum)
    {
        //создаем элемент у блока
        if (this.Type != ElementsTypeEnum.Empty)
        {
            //если уже есть элемент то удаляем его
            if (this.blockingElement != null)
            {
                DestroyImmediate(this.blockingElement.gameObject);
            }

            //создаем новый элемент
            GameObject blockingElementGameObject = Instantiate(prefabBlockingElement, thisTransform.position, Quaternion.identity);
            BlockingElement curElement;

            if (typeBlockingElementsEnum == BlockingElementsTypeEnum.Standard)
            {
                curElement = blockingElementGameObject.AddComponent<BlockingElement>();
                curElement.InitialSettings(typeBlockingElementsEnum, 1, false);
                lockedForMove = true;
            }
            //else if (typeBlockingElementsEnum == BlockingElementsTypeEnum.CrushableWall)
            //{
            //    curElement = elementGameObject.AddComponent<ElementWall>();
            //    curElement.InitialSettings(typeBlockingElementsEnum, true, false, false);
            //}
            //else if (typeBlockingElementsEnum == BlockingElementsTypeEnum.ImmortalWall)
            //{
            //    curElement = elementGameObject.AddComponent<ElementWall>();
            //    curElement.InitialSettings(typeBlockingElementsEnum, true, true, false);
            //}
            else
            {
                Debug.LogError("У элемента " + this.name + " не удалось определить тип создаваемого блокирующего элемента");
                DestroyImmediate(blockingElementGameObject);
                return;
            }

            curElement.Shape = shape;
            //Добавляем в элемент
            this.BlockingElement = curElement;
        }        
    }

    //проверка на существование элемента
    protected bool BlockingElementExists() {
        //если элемент существет и небыл уничтожен
        if (blockingElement != null && !blockingElement.Destroyed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //ударяем по соседним блокам
    protected virtual void HitNeighboringBlocks(HitTypeEnum hitTypeEnum)
{
    //Находим позицию блока в сетке
    Position gridPosition = Grid.Instance.FindPosition(this);
    //Определяем соседние блоки
    NeighboringBlocks neighboringBlocks = Grid.Instance.DeterminingNeighboringBlocks(gridPosition);

    foreach (Block neighboringBlock in neighboringBlocks.allBlockField)
    {
        if (neighboringBlock != null)
        {
                neighboringBlock.Hit(hitTypeEnum, this.shape);
        }
    }
}

}
