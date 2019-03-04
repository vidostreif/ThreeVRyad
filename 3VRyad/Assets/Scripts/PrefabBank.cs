using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabBank : MonoBehaviour
{
    public static PrefabBank Instance; // Синглтон
    public GameObject levelButtonPrefab;

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров PrefabBank!");
        }
        Instance = this;
    }
}
