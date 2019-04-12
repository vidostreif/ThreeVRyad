﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabBank : MonoBehaviour
{
    public static PrefabBank Instance; // Синглтон

    //интерфейс
    public GameObject levelsCanvasPrefab;
    public GameObject regionsCanvasPrefab;
    public GameObject levelButtonPrefab;
    public GameObject regionButtonPrefab;

    public GameObject scoreElementPrefab;

    public GameObject canvasHelpToPlayer;
    public GameObject canvasStartGame;
    public GameObject canvasEndGameMenu;

    void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject); //Delete duplicate
            return;
        }
        else
        {
            Instance = this; //Make this object the only instance            
        }
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject); //Set as do not destroy
        }
        //// регистрация синглтона
        //if (Instance != null)
        //{
        //    Debug.LogError("Несколько экземпляров PrefabBank!");
        //}
        //Instance = this;
    }
}
