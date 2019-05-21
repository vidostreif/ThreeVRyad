//using Boo.Lang;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//вспомогательные функции 
public static class SupportFunctions
{
    //static GameObject yesNoPanelPrefab;

    public static void MixArray(Array arr) {
        for (int i = (arr.Length - 1); i >= 1; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var temp = arr.GetValue(j);
            arr.SetValue(arr.GetValue(i), j);
            arr.SetValue(temp, i);
        }
    }

    public static void MixArray<T>(List<T> list)
        {
            System.Random rand = new System.Random();

            for (int i = list.Count - 1; i >= 1; i--)
            {
                int j = rand.Next(i + 1);

                T tmp = list[j];
                list[j] = list[i];
                list[i] = tmp;
            }
        }

    public static void ChangeAlfa(SpriteRenderer spriteRenderer, float alfa) {
        //изменяем альфу спрайта
        //Color color = new Color();
        //color.a = 1f; // value between 0 and 1, where 1 is opaque
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alfa);
    }

    public static void ChangeAlfa(Image image, float alfa)
    {
        //изменяем альфу image
        //Color color = new Color();
        //color.a = 1f; // value between 0 and 1, where 1 is opaque
        image.color = new Color(image.color.r, image.color.g, image.color.b, alfa);
    }

    public static void ChangeAlfa(Text text, float alfa)
    {
        //изменяем альфу image
        //Color color = new Color();
        //color.a = 1f; // value between 0 and 1, where 1 is opaque
        text.color = new Color(text.color.r, text.color.g, text.color.b, alfa);
    }

    //создание панели вопроса
    public static void CreateYesNoPanel(Transform canvasTransform, string textQuestion, Action actionYesBut = null, Action actionNoBut = null)
    {
        GameObject yesNoPanelPrefab = GameObject.Instantiate(PrefabBank.YesNoPanelPrefab, canvasTransform);

        Transform textQuestionTransform = yesNoPanelPrefab.transform.Find("PanelQuestion/TextQuestion");
        Transform buttonYesTransform = yesNoPanelPrefab.transform.Find("PanelQuestion/ButtonYes");
        Transform buttonNoTransform = yesNoPanelPrefab.transform.Find("PanelQuestion/ButtonNo");

        Text TextQuestion = textQuestionTransform.GetComponent(typeof(Text)) as Text;
        Button buttonYes = buttonYesTransform.GetComponent(typeof(Button)) as Button;
        Button buttonNo = buttonNoTransform.GetComponent(typeof(Button)) as Button;

        TextQuestion.text = textQuestion;

        if (actionYesBut != null)
        {            
            buttonYes.onClick.AddListener(delegate { actionYesBut(); });
        }
        buttonYes.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
        buttonYes.onClick.AddListener(delegate { GameObject.Destroy(yesNoPanelPrefab); });
                
        if (actionNoBut != null)
        {            
            buttonNo.onClick.AddListener(delegate { actionNoBut(); });
        }
        buttonNo.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
        buttonNo.onClick.AddListener(delegate { GameObject.Destroy(yesNoPanelPrefab); });
    }

    //public static void DestroyYesNoPanel(GameObject yesNoPanelPrefab)
    //{
    //    GameObject.Destroy(yesNoPanelPrefab);
    //}
}
