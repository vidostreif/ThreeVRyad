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
        GameObject collectObg = Instantiate(PrefabBank.CollectElement);
        collectObg.name = collectName;
        collectObg.transform.SetParent(transform, false);

        SpriteRenderer collectSpRender = collectObg.GetComponent<SpriteRenderer>();
        collectSpRender.sprite = SpriteBank.SetShape(collectShape);
        SpriteRenderer[] collectSpRenders = collectObg.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer item in collectSpRenders)
        {
            item.sprite = SpriteBank.SetShape(collectShape);
        }
        //collectSpRender.sortingLayerName = "Elements";
        //collectSpRender.sortingOrder = 100;
        //collectObg.transform.localScale = new Vector3(0.60f, 0.60f);
        //collectObg.transform.localPosition = new Vector3(collectObg.transform.localPosition.x, -0.23f , collectObg.transform.localPosition.z);

        //создаем текст
        string TextCollectName = "TextCollectElement";
        Transform TextCollectTransform = transform.Find(TextCollectName);
        if (TextCollectTransform != null)
        {
            DestroyImmediate(TextCollectTransform.gameObject);
        }
        GameObject TextCollectObg = Instantiate(PrefabBank.TextCollectElement);
        TextCollectObg.name = TextCollectName;
        TextCollectObg.transform.SetParent(transform, false);
        collectText = TextCollectObg.GetComponent<TextMesh>();
        collectText.text = numberOfElementCollected.ToString();
    }

    //добавляем в коллекцию элемент
    public override bool AddToCollection(AllShapeEnum elementShape, Transform elementTransform)
    {
        bool i = base.AddToCollection(elementShape, elementTransform);
        if (i)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.SeedBarrel_collect);
        }
        if (collectText == null)
        {
            collectText = GetComponentInChildren<TextMesh>();
        }
        collectText.text = numberOfElementCollected.ToString();
        return i;
    }
}
