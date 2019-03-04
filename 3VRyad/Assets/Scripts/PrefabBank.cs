﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabBank : MonoBehaviour
{
    public static PrefabBank Instance; // Синглтон
    public GameObject levelsCanvasPrefab;
    public GameObject regionsCanvasPrefab;
    public GameObject levelButtonPrefab;
    public GameObject regionButtonPrefab;

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
