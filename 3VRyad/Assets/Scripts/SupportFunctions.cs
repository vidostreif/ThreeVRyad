//using Boo.Lang;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//вспомогательные функции 
public static class SupportFunctions
{
    public static void MixArray(Array arr) {
        for (int i = (arr.Length - 1); i >= 1; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var temp = arr.GetValue(j);
            arr.SetValue(arr.GetValue(i), j);
            arr.SetValue(temp, i);
        }
    }

    //public static void MixArray(List<MonoBehaviour> arr)
    //{
    //    for (int i = (arr.Count - 1); i >= 1; i--)
    //    {
    //        int j = UnityEngine.Random.Range(0, i + 1);
    //        var temp = arr[j];
    //        arr[j] = arr[i];
    //        arr[i] = temp;
    //    }
    //}

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
}
