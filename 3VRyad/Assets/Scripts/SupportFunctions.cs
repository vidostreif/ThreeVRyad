using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
