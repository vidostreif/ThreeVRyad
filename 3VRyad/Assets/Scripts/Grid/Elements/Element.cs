using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]// требуем наличие у объекта SpriteRenderer
//[RequireComponent(typeof(SpriteBank))]// требуем наличие у объекта ObjectManagement
[RequireComponent(typeof(ElementController))]// требуем наличие у объекта ElementController

/// наследоваться от интерфейса?
//описание поведения стандартного элемента
[ExecuteInEditMode]
public class Element : BaseElement
{
    public bool drag; //элемент перетаскивается
    public float speed = 0; //скорость
    [SerializeField] protected ElementsTypeEnum type;//тип элемента    
    [SerializeField] protected BlockingElement blockingElement;// блокирующий элемент
    [SerializeField] protected bool lockedForMove;//признак что элемент заблокирован для передвижения
    [SerializeField] protected bool createLine;//признак что элемент создает линию
    [SerializeField] protected bool activated;//признак что элемент активируемый
    [SerializeField] protected HitTypeEnum thisHitTypeEnum;//тип удара у элемента
    

    public override Position PositionInGrid
    {
        get
        {
            return positionInGrid;
        }

        set
        {
            positionInGrid = value;
            if (blockingElement != null)
            {
                blockingElement.PositionInGrid = value;
            }
        }
    }
    public bool LockedForMove {
        get
        {
            return lockedForMove;
        }
    }//признак что элемент заблокирован для передвижения
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
        spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        GetComponent<ElementController>().ThisElement = this;
    }

    public void Start()
    {
        AnimatorElement animatorElement = this.GetComponent<AnimatorElement>();
        animatorElement.PlayCreatureAnimation();
    }

    //установка настроек элементов
    public void InitialSettings(ElementsTypeEnum type, bool lockedForMove, bool immortal, bool createLine, bool activated, HitTypeEnum hitTypeEnum, int score) {
        this.type = type;
        this.lockedForMove = lockedForMove;
        this.immortal = immortal;
        this.createLine = createLine;
        this.activated = activated;
        this.thisHitTypeEnum = hitTypeEnum;
        this.score = score;
        DopSettings();
    }

    protected virtual void DopSettings()
    {
        soundDestroy = SoundsEnum.DestroyElement_3;
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.StandartHit, HitTypeEnum.Explosion, HitTypeEnum.Instrument };
    }

    //удар по элементу
    public override void Hit(HitTypeEnum hitType = HitTypeEnum.StandartHit, AllShapeEnum hitElementShape = AllShapeEnum.Empty)
    {
        if (!destroyed)
        {
            //если прямой удар или взрыв
            if (vulnerabilityTypeEnum.Contains(hitType))
            {
                //если стоит блокировка на элементе, то пытаемся ее снять
                if (BlockingElementExists())
                {
                    blockingElement.Hit();

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
                        ActionAfterHitting(hitType);
                    }
                }
            }
        }
    }

    //действие после удара
    protected virtual void ActionAfterHitting(HitTypeEnum hitType) {
        Position position = new Position(PositionInGrid.posX, PositionInGrid.posY);
        base.DestroyElement();

        if (hitType == HitTypeEnum.StandartHit || hitType == HitTypeEnum.Instrument)
            HitNeighboringBlocks(thisHitTypeEnum, position);
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
    
    public virtual void CreatBlockingElement(GameObject prefabBlockingElement, AllShapeEnum shape, BlockingElementsTypeEnum typeBlockingElementsEnum)
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

            if (typeBlockingElementsEnum == BlockingElementsTypeEnum.Liana)
            {
                curElement = blockingElementGameObject.AddComponent<BlockingElement>();
                curElement.InitialSettings(typeBlockingElementsEnum, false, 1, 100);
                lockedForMove = true;                
            }
            else if(typeBlockingElementsEnum == BlockingElementsTypeEnum.Spread)
            {
                curElement = blockingElementGameObject.AddComponent<SpreadBlockingElement>();
                curElement.InitialSettings(typeBlockingElementsEnum, false, 1, 300);
                curElement.MakeActionAfterMove(2);
                lockedForMove = true;
            }
            else
            {
                Debug.LogError("У элемента " + this.name + " не удалось определить тип создаваемого блокирующего элемента");
                DestroyImmediate(blockingElementGameObject);
                return;
            }

            curElement.Shape = shape;
            HelpToPlayer.AddHint(typeBlockingElementsEnum);
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
    protected virtual void HitNeighboringBlocks(HitTypeEnum hitTypeEnum, Position position)
{
    ////Находим позицию блока в сетке
    //Position gridPosition = this.PositionInGrid;
    //Определяем соседние блоки
    NeighboringBlocks neighboringBlocks = GridBlocks.Instance.GetNeighboringBlocks(position);

    foreach (Block neighboringBlock in neighboringBlocks.allBlockField)
    {
        if (neighboringBlock != null)
        {
                neighboringBlock.Hit(hitTypeEnum, this.shape);
        }
    }
}

}
