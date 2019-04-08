using System.Collections;
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
