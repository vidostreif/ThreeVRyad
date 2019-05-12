using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//#if UNITY_EDITOR
[ExecuteInEditMode]
//#endif
public class PrefabBank : MonoBehaviour
{
    public static PrefabBank Instance; // Синглтон

    //интерфейс
    public GameObject levelsCanvasPrefab;
    public GameObject regionsCanvasPrefab;
    public GameObject settingsPanelPrefab;
    public GameObject yesNoPanelPrefab;
    public GameObject levelButtonPrefab;
    public GameObject regionButtonPrefab;
    public GameObject imageMainLoad;

    //магазин
    public GameObject shopPanelPrefab;
    public GameObject shopButtonPrefab;
    public GameObject prefabButtonThing;
    public GameObject panelShopConfirmation;

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
