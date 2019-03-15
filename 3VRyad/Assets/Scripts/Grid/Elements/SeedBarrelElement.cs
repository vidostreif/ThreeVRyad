using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedBarrelElement : Element
{
    //делаем элемент коллекционером
    public override void MakeCollector(AllShapeEnum collectShape, int numberOfElementCollected)
    {
        base.MakeCollector(collectShape, numberOfElementCollected);
        string collectName = "collectElement";
        Transform collectTransform = transform.Find(collectName);
        if (collectTransform != null)
        {
            DestroyImmediate(collectTransform.gameObject);
        }
        //создаем рисунок в центре нашего объекта
        GameObject collectObg = new GameObject();
        collectObg.name = collectName;
        collectObg.transform.SetParent(transform, false);
        SpriteRenderer collectSpRender = collectObg.AddComponent<SpriteRenderer>();
        collectSpRender.sprite = SpriteBank.SetShape(collectShape);
        collectSpRender.sortingLayerName = "Elements";
        collectSpRender.sortingOrder = 100;
        collectObg.transform.localScale = new Vector3(0.5f, 0.5f);
    }
}
