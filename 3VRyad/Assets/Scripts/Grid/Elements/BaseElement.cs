using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseElement : MonoBehaviour
{
    public Transform thisTransform;
    protected SpriteRenderer spriteRenderer;
    protected AnimatorElement animatorElement;
    [SerializeField] protected Position positionInGrid;//позиция в сетке
    [SerializeField] protected AllShapeEnum shape;//форма элемента
    [SerializeField] protected HitTypeEnum[] vulnerabilityTypeEnum;//уязвимость к типам удара
    [SerializeField] protected GameObject DopPS;
    [SerializeField] protected bool destroyed = false;//признак что элемент был уничтожен
    [SerializeField] protected int life; //количество жизней
    protected TextMesh lifeText;
    [SerializeField] protected int score;//количество очков за уничтожение элемента
    [SerializeField] protected bool scoreScale;//очки приумножаются при уничтожении одинаковых элементов в один ход
    [SerializeField] protected bool immortal;//признак бессмертия
    //[SerializeField] protected bool returnToPool;//вернуть в пул

    [SerializeField] protected bool actionAfterMove = false;//признак активируемости по окончанию хода    
    [SerializeField] protected int actionDelay;//задержка перед активированием    
    [SerializeField] protected int activationMove;//следующий ход для активации
    [SerializeField] protected bool singleItemActivated = false;//признак что активируется только один элемент из всех с таким типом и внешностью
    [SerializeField] protected int nextProcessedMoveForAction;//следующий обработанный ход для этой группы элементов    

    [SerializeField] protected bool collector = false;//признак что элемент коллекционирует другие элементы
    [SerializeField] protected AllShapeEnum collectShape;//форма коллекционированого элемента
    [SerializeField] protected int numberOfElementCollected;//количество коллекционируемых элементов

    [SerializeField] protected bool drop = false;//сбрасываемый элемент    

    public AnimatorElement AnimatElement
    {
        get
        {
            if (animatorElement == null)
            {
                animatorElement = transform.GetComponent<AnimatorElement>();
            }
            return animatorElement;
        }
    }
    public virtual Position PositionInGrid
    {
        get
        {
            return positionInGrid;
        }

        set
        {
            positionInGrid = value;
            UpdateOrderLayer();
        }
    }
    public bool Destroyed
    {
        get
        {
            return destroyed;
        }
    }
    public bool Immortal
    {
        get
        {
            return immortal;
        }
    }//признак бессмертия
    public int Life
    {
        get
        {
            return life;
        }
    }//признак бессмертия
    public bool ActionAfterMove
    {
        get
        {
            return actionAfterMove;
        }
    }
    //public bool ReturnToPool
    //{
    //    get
    //    {
    //        return returnToPool;
    //    }
    //}
    public AllShapeEnum Shape
    {
        get
        {
            return shape;
        }

        set
        {
            if (shape != AllShapeEnum.Empty)
            {
                ElementsList.DellElement(shape);
            }
            shape = value;
            SoundManager.Instance.PlayCreateElement(shape);
            ElementsList.AddElement(shape);
            UpdateSprite();
        }
    }
    public bool Collector
    {
        get
        {
            return collector;
        }
    }
    public AllShapeEnum CollectShape
    {
        get
        {
            return collectShape;
        }
        set
        {
            MakeCollector(value, this.numberOfElementCollected);
        }
    }
    public int NumberOfElementCollected
    {
        get
        {
            return numberOfElementCollected;
        }
    }
    public bool Drop
    {
        get
        {
            return drop;
        }
    }
    public int ActivationMove { get => activationMove; set => activationMove = value; }
    public bool SingleItemActivated { get => singleItemActivated; }
    public int NextProcessedMoveForAction { get => nextProcessedMoveForAction; set => nextProcessedMoveForAction = value; }

    public void Start()
    {
        animatorElement = this.GetComponent<AnimatorElement>();
        animatorElement.PlayCreatureAnimation();
        if (!Application.isPlaying)
        {
            UpdateSprite();
        }        
    }

    //минус жизнь
    public virtual bool SubLife()
    {
        if (!this.immortal)
        {
            if (life > 0)
            {
                life--;
                if (life > 0)
                {
                    AnimatElement.PlayIdleAnimation();
                }
                SoundManager.Instance.PlayHitElement(shape);
                if (lifeText != null)
                {
                    lifeText.text = Life.ToString();
                }
            }

            //если убили
            if (life == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    //делаем элемент активным после хода
    public virtual void MakeActionAfterMove(int actionDelay, bool singleItemActivated)
    {
        this.actionAfterMove = true;
        this.singleItemActivated = singleItemActivated;
        //this.preliminarySearchToActivate = preliminarySearchToActivate;
        this.actionDelay = actionDelay;
        this.nextProcessedMoveForAction = -1;
        //if (singleItemActivated || !Application.isPlaying)
        //{            
            this.ActivationMove = -1;
        //}
        //else
        //{
        //   this.ActivationMove = Tasks.Instance.RealMoves + 1 + actionDelay;
        //}
        
        UpdateSprite();        
    }

    //делаем элемент коллекционером
    public virtual void MakeCollector(AllShapeEnum collectShape, int numberOfElementCollected) {
        this.collector = true;
        this.collectShape = collectShape;
        this.numberOfElementCollected = numberOfElementCollected;
    }

    //делаем элемент сбрасываемым
    public virtual void MakeDrop()
    {
        this.drop = true;
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.Drop };
    }

    //добавляем в коллекцию элемент
    public virtual bool AddToCollection(AllShapeEnum elementShape, Transform elementTransform)
    {
        if (!this.destroyed && this.collector && elementShape == this.collectShape && this.numberOfElementCollected > 0)
        {
            //перемещаем элемент к себе
            MainAnimator.Instance.AddElementForSmoothMove(elementTransform, this.transform.position, 10, SmoothEnum.InLineWithAcceleration, 0.1f, true);
            //уменьшаем в два раза
            elementTransform.localScale = elementTransform.localScale * 0.7f;
            AnimatElement.PlayIdleAnimation();
            //создаем эффект
            //SoundManager.Instance.PlaySoundInternal(SoundsEnum.DestroyElement_1);

            ParticleSystemManager.Instance.CreateCollectEffect(gameObject.transform, SpriteBank.SetShape(collectShape, mini: true));

            numberOfElementCollected--;
            //если собрали колецию
            if (numberOfElementCollected == 0)
            {
                DestroyElement();
            }
            return true;
        }
        return false;
    }

    public virtual void Hit(HitTypeEnum hitType, AllShapeEnum hitElementShape)
    { }

    //действие после хода
    public virtual IEnumerator PerformActionAfterMove()
    {
        yield break;
    }

    //поиск следующего хода
    public virtual bool FoundNextActionAfterMove()
    {
        return false;
    }

    protected virtual void DestroyElement()
    {        
        destroyed = true;
        //StopAllCoroutines();
        //if (returnToPool)
        //{
        //    foreach (Transform child in this.transform)
        //    {
        //        Destroy(child.gameObject);
        //    }
        //}

        ElementsList.DellElement(shape);
        Score.Instance.AddScore(transform.position, score, scoreScale);
        PoolManager.Instance.ReturnObjectToPool(DopPS);
        //if (animatorElement == null)
        //{
        //    animatorElement = this.GetComponent<AnimatorElement>();
        //}
        //animatorElement.playIdleAnimationRandomTime = false;
        //SuperBonus.Instance.CreatePowerSuperBonus(transform.position, score);
        AnimatElement.StopAllAnimation();

        SoundManager.Instance.PlayDestroyElement(shape);      

        //определяем есть ли вокруг элементы коллекционирующие наш вид элемента
        bool addToCollection = false;
        Block[] blocksAround = GridBlocks.Instance.GetAroundBlocks(this.PositionInGrid);
        foreach (Block item in blocksAround)
        {
            if (BlockCheck.ThisBlockWithCollectorElementAndNoBlockingElement(item))
            {
                //если добавили элемент в коллекцию то выходим
                if (item.Element.AddToCollection(this.shape, this.transform))
                {
                    addToCollection = true;
                }  
            }
        }
        //проверяем по заданиям
        if (!addToCollection && !Tasks.Instance.Collect(shape, transform))
        {
            AnimatElement.PlayDestroyAnimation();            
        }
        else
        {
            AnimatElement.PlayIdleAnimation();
            //удаляем из блока
            Block block = GridBlocks.Instance.GetBlock(this.positionInGrid);
            if (block != null)
            {
                if (block.BehindElement == this)
                {
                    block.BehindElement = null;
                }
                if (block.Element == this)
                {
                    block.Element = null;
                }
            }
        }
        this.PositionInGrid = null;
    }

    protected virtual void UpdateSprite(int option = 0)
    {
        spriteRenderer.sprite = SpriteBank.SetShape(shape, option);
        //UpdateOrderLayer();
    }

    protected virtual void UpdateOrderLayer()
    {
        if (positionInGrid != null && spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = -positionInGrid.posY;
        }        
    }

}
