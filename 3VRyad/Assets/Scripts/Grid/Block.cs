using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Block : MonoBehaviour {

    [SerializeField] protected BlockTypeEnum type;
    public Transform thisTransform;
    public Element element;// элемент
    [SerializeField] protected bool generatorElements; //признак, что блок генерирует новые элементы
    //protected SpriteBank objectManagement;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    //private float smoothTime = 15f;//скорость перемещения элемента к блоку
    //private float yvelocity = 0.0f;


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

                    //если растояние меньше размеров блока сетки, то перемещаем моментально на позицию блока
                    //расчитываем вектор смещения
                    Vector3 vector = element.thisTransform.position - thisTransform.position;
                    //вычисляем расстояние на которое смещаем объект
                    float distance = vector.magnitude;

                    if (Grid.Instance != null && distance < Grid.Instance.blockSize)
                    {
                        element.thisTransform.position = thisTransform.position;
                    }
                    
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
            }
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
                DestroyImmediate(element.gameObject);
            }

            //создаем новый элемент
            GameObject elementGameObject = Instantiate(prefabElement, thisTransform.position, Quaternion.identity);
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
            else if (typeElementsEnum == ElementsTypeEnum.Bomb)
            {
                curElement = elementGameObject.AddComponent<ElementBomb>();
                curElement.InitialSettings(typeElementsEnum, false, false, false, true, HitTypeEnum.Explosion);
            }
            else if (typeElementsEnum == ElementsTypeEnum.Dynamite)
            {
                curElement = elementGameObject.AddComponent<ElementDynamite>();
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

    //удар по блоку
    public void Hit(HitTypeEnum hitTypeEnum = HitTypeEnum.Standart, ElementsShapeEnum hitElementShape = ElementsShapeEnum.Empty) {

        if (element != null)
        {
            Element = element.Hit(hitTypeEnum, hitElementShape);
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
        if (this.Element != null && this.Element.thisTransform.position != thisTransform.position && !this.Element.drag)
        {
            MainAnimator.Instance.AddElementForSmoothMove(this.Element.thisTransform, thisTransform.position, 1);
            //float newPositionX = Mathf.SmoothDamp(Element.thisTransform.position.x, thisTransform.position.x, ref yVelocity, smoothTime);
            //float newPositionY = Mathf.SmoothDamp(Element.thisTransform.position.y, thisTransform.position.y, ref yVelocity, smoothTime);
            //Element.thisTransform.position = new Vector3(newPositionX, newPositionY, thisTransform.position.z);
            //Element.thisTransform.position = Vector3.Lerp(Element.thisTransform.position, thisTransform.position, Time.deltaTime * smoothTime);
        }        
    }
}
