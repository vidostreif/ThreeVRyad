using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlockingElement : MonoBehaviour {
        
    public Transform thisTransform;
    //protected SpriteBank objectManagement;
    protected SpriteRenderer spriteRenderer;
    [SerializeField] protected BlockingElementsShapeEnum shape;//форма элемента
    [SerializeField] protected BlockingElementsTypeEnum type;//тип элемента
    [SerializeField] protected bool destroyed;
    [SerializeField] protected int life = 1;
    [SerializeField] protected bool immortal;


    public bool Destroyed {
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
    public BlockingElementsTypeEnum Type
    {
        get
        {
            return type;
        }
    }
    public BlockingElementsShapeEnum Shape
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
       
    //установка настроек элементов
    public void InitialSettings(BlockingElementsTypeEnum type, int life, bool immortal)
    {
        this.type = type;
        this.life = life;
        this.immortal = immortal;
    }

    //удар элементу
    public BlockingElement Hit()
    {
        //если не неразрушаемый
        if (!this.immortal)
        {
            life--;
        }
        //если елемент убили, то возвращаем null
        if (life <= 0)
        {
            Tasks.Instance.Collect(this);
            AnimatorElement animatorElement = this.GetComponent<AnimatorElement>();
            animatorElement.PlayDestroyAnimation();
            destroyed = true;
        }
        return this;
    }

    void Awake()
    {
        thisTransform = transform;
        destroyed = false;
        //objectManagement = GetComponent<SpriteBank>();
        spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
