using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BehindElement : MonoBehaviour
{
    public Transform thisTransform;
    protected SpriteRenderer spriteRenderer;
    [SerializeField] protected bool destroyed;//признак что элемент был уничтожен
    [SerializeField] protected BehindElementsTypeEnum type;//тип элемента
    [SerializeField] protected BehindElementsShapeEnum shape;//форма элемента


    public bool Destroyed
    {
        get
        {
            return destroyed;
        }
    }//признак что элемент был уничтожен
    public BehindElementsTypeEnum Type
    {
        get
        {
            return type;
        }
    }
    public BehindElementsShapeEnum Shape
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

    public virtual void Awake()
    {
        thisTransform = transform;
        destroyed = false;
        spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        spriteRenderer.sortingLayerName = "BackgroundElements";
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    //установка настроек элементов
    public void InitialSettings(BehindElementsTypeEnum type)
    {
        this.type = type;
    }

    public void Hit() {
        if (!destroyed)
        {
                        //воздействие на соседние блоки
                        destroyed = true;
                        if (!Tasks.Instance.Collect(this))
                        {
                            AnimatorElement animatorElement = this.GetComponent<AnimatorElement>();
                            animatorElement.PlayDestroyAnimation();
                        }
        }
    }
}
