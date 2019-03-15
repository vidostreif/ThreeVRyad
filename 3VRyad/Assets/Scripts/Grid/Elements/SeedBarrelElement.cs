using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedBarrelElement : Element
{
    private TextMesh collectText;
    //делаем элемент коллекционером
    public override void MakeCollector(AllShapeEnum collectShape, int numberOfElementCollected)
    {
        base.MakeCollector(collectShape, numberOfElementCollected);
        //создаем рисунок
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
        collectObg.transform.localScale = new Vector3(0.7f, 0.7f);

        //создаем текст
        string TextCollectName = "TextCollectElement";
        Transform TextCollectTransform = transform.Find(TextCollectName);
        if (TextCollectTransform != null)
        {
            DestroyImmediate(TextCollectTransform.gameObject);
        }
        GameObject TextCollectObg = new GameObject();
        TextCollectObg.name = TextCollectName;
        TextCollectObg.transform.SetParent(transform, false);

        MeshRenderer collectMeshRenderer = TextCollectObg.AddComponent<MeshRenderer>();
        collectMeshRenderer.sortingLayerName = "Elements";
        collectMeshRenderer.sortingOrder = 101;

        collectText = TextCollectObg.AddComponent<TextMesh>();
        collectText.text = numberOfElementCollected.ToString();
        collectText.fontSize = 50;
        collectText.anchor = TextAnchor.MiddleCenter;
        TextCollectObg.transform.localScale = new Vector3(0.08f, 0.08f);
    }

    //добавляем в коллекцию элемент
    public override bool AddToCollection(AllShapeEnum elementShape, Transform elementTransform)
    {
        bool i = base.AddToCollection(elementShape, elementTransform);
        if (collectText == null)
        {
            collectText = GetComponentInChildren<TextMesh>();
        }
        collectText.text = numberOfElementCollected.ToString();
        return i;
    }
}
