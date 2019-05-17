using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.UI;
using UnityEditor;

[System.Serializable]
public class Target
{
    private Image image;
    private GameObject gameObject;
    public AllShapeEnum elementsShape;//какой вид элементов собераем
    [SerializeField] private bool collectEverything;//признак, что нужно собрать все элементы на поле
    [SerializeField] private int goal;//необходимо собрать
    List<Transform> transformsInTransitList = new List<Transform>();
    private int itemsInTransit = 0;//элементы в пути
    private Text text;
    private bool collected = false; //признак, что коллекция собрана

    public Target(AllShapeEnum elementsShape, int goal, bool collectEverything)
    {
        this.elementsShape = elementsShape;
        this.collectEverything = collectEverything;
        this.goal = goal;
    }
    public int Goal
    {
        get
        {
            return goal;
        }
    }
    public bool CollectEverything
    {
        get
        {
            return collectEverything;
        }
    }
    public bool Collected
    {
        get
        {
            return collected;
        }
    }
    public void UpdateGoal() {
        //если нужно собрать все элементы на поле, то проверяем поле и обновляем счетчик
        if (collectEverything)
        {
            goal = ElementsList.GetAmountOfThisShapeElemets(elementsShape);
            UpdateText();
        }
    }

    //обнолвление текста
    private void UpdateText()
    {
        text.text = "" + goal;
    }

    public Text Text
    {
        get
        {
            return text;
        }

        set
        {
            text = value;
            UpdateGoal();
            UpdateText();
        }
    }

    public Image Image
    {
        get
        {
            return image;
        }

        set
        {
            image = value;
        }
    }

    public GameObject GameObject
    {
        get
        {
            return gameObject;
        }

        set
        {
            gameObject = value;
        }
    }

    //если элемент подошел то возвращаем истину
    public bool Collect(AllShapeEnum Shape, Transform transformElement) {
        if (Shape == elementsShape)
        {
            
            if (goal - itemsInTransit > 0)
            {
                //foreach (Transform item in transformsInTransitList)
                //{
                //    if (item == transformElement)
                //    {
                //        Debug.Log("Найден дубликат");
                //    }
                //}
                transformsInTransitList.Add(transformElement);
                itemsInTransit++;
                Check();
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    //проверяем собрали ли коллекцию
    private void Check() {
        if (goal - itemsInTransit <= 0)
        {
            collected = true;            
        }
    }

    public void ItemReached(Transform transformElement) {

        bool found = false;
        foreach (Transform item in transformsInTransitList)
        {
            if (item == transformElement)
            {
                found = true;
            }
        }

        if (found)
        {
            transformsInTransitList.Remove(transformElement);
            goal--;
            itemsInTransit--;
            if (goal <= 0)
            {
                //создаем эффект 
                GameObject psGO = GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/PSCollect") as GameObject, gameObject.transform);

                //Color32 color = AverageColorFromTexture(image.sprite.texture);
                //изменяем цвет
                ParticleSystem ps = psGO.GetComponent<ParticleSystem>();
                ps.textureSheetAnimation.AddSprite(image.sprite);
                //var col = ps.colorOverLifetime;
                //col.enabled = true;
                //Gradient grad = new Gradient();
                //grad.SetKeys(new GradientColorKey[] { new GradientColorKey(color, 0.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 0.7f), new GradientAlphaKey(0.0f, 1.0f) });
                //col.color = grad;
            }
            UpdateGoal();
        }        
    }

    //Color32 AverageColorFromTexture(Texture2D tex)
    //{
    //    Color32[] texColors = tex.GetPixels32();

    //    int total = texColors.Length;

    //    float r = 0;
    //    float g = 0;
    //    float b = 0;        

    //    for (int i = 0; i < total; i++)
    //    {
    //        if (texColors[i].r == 0 && texColors[i].g == 0 && texColors[i].b == 0)
    //        { }
    //        else
    //        {
    //            r += texColors[i].r;

    //            g += texColors[i].g;

    //            b += texColors[i].b;
    //        }
    //    }

    //    float sum = r+g+b;

    //    //float r = 0;
    //    //float g = 0;
    //    //float b = 0;
    //    float m = 1;
    //    if (r > g && r > b)
    //    {
    //        m = r/sum * 255;
    //        m = 255 / m;
    //        //r = sum;
    //    }
    //    else if (g > r && g > b)
    //    {
    //        m = g / sum * 255;
    //        m = 255 / m;
    //        //g = sum;
    //    }
    //    else
    //    {
    //        m = b / sum * 255;
    //        m = 255 / m;
    //        //b = sum;
    //    }
    //    float ri = r / sum * 255 * m;
    //    float gi = g / sum * 255 * m;
    //    float bi = b / sum * 255 * m;

    //    if (ri > 255)
    //    {
    //        ri = 255;
    //    }
    //    if (gi > 255)
    //    {
    //        gi = 255;
    //    }
    //    if (bi > 255)
    //    {
    //        bi = 255;
    //    }


    //    return new Color32((byte)ri, (byte)gi, (byte)bi, 1);

    //}
}


