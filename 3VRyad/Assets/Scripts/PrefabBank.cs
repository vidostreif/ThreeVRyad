using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//#if UNITY_EDITOR
//[ExecuteInEditMode]
//#endif
public static class PrefabBank
{
    //public static PrefabBank Instance; // Синглтон
    private static string prefabFolder = "Prefabs/";

    //интерфейс
    private static GameObject levelsCanvasPrefab = Resources.Load(prefabFolder + "Canvas/Main menu/Levels") as GameObject;
    private static GameObject regionsCanvasPrefab = Resources.Load(prefabFolder + "Canvas/Main menu/Regions") as GameObject;
    private static GameObject startScreenPrefab = Resources.Load(prefabFolder + "Canvas/Main menu/StartScreen") as GameObject;
    private static GameObject settingsPanelPrefab = Resources.Load(prefabFolder + "Canvas/Main menu/PanelSettingsMain") as GameObject;
    private static GameObject yesNoPanelPrefab = Resources.Load(prefabFolder + "Canvas/PanelYesNo") as GameObject;
    private static GameObject levelButtonPrefab = Resources.Load(prefabFolder + "Canvas/Main menu/LevelButton") as GameObject;
    private static GameObject regionButtonPrefab = Resources.Load(prefabFolder + "Canvas/Main menu/RegionButton") as GameObject;
    private static GameObject imageMainLoad = Resources.Load(prefabFolder + "Canvas/Main menu/ImageMainLoad") as GameObject;

    //магазин
    private static GameObject shopPanelPrefab = Resources.Load(prefabFolder + "Canvas/Shop/PanelShop") as GameObject;
    private static GameObject shopButtonPrefab = Resources.Load(prefabFolder + "Canvas/Shop/ShopButton") as GameObject;
    private static GameObject prefabButtonThing = Resources.Load(prefabFolder + "Canvas/GameCanvas/ButtonThing") as GameObject;
    private static GameObject panelShopConfirmation = Resources.Load(prefabFolder + "Canvas/Shop/PanelShopConfirmation") as GameObject;
    private static GameObject panelShopInformation = Resources.Load(prefabFolder + "Canvas/Shop/PanelShopConfirmation") as GameObject;
    
    private static GameObject scoreElementPrefab = Resources.Load(prefabFolder + "Canvas/GameCanvas/ScoreElement") as GameObject;

    private static GameObject canvasHelpToPlayer = Resources.Load(prefabFolder + "Canvas/GameCanvas/CanHelpToPlayer") as GameObject;
    private static GameObject canvasStartGame = Resources.Load(prefabFolder + "Canvas/GameCanvas/CanStartGame") as GameObject;
    private static GameObject canvasEndGameMenu = Resources.Load(prefabFolder + "Canvas/GameCanvas/CanEndGame") as GameObject;

    ////элементы
    private static GameObject textCollectElement = Resources.Load(prefabFolder + "TextCollectElement") as GameObject;
    private static GameObject collectElement = Resources.Load(prefabFolder + "CollectElement") as GameObject;

    public static GameObject LevelsCanvasPrefab { get => levelsCanvasPrefab;}
    public static GameObject RegionsCanvasPrefab { get => regionsCanvasPrefab;}
    public static GameObject StartScreenPrefab { get => startScreenPrefab; }
    public static GameObject SettingsPanelPrefab { get => settingsPanelPrefab;}
    public static GameObject YesNoPanelPrefab { get => yesNoPanelPrefab;}
    public static GameObject LevelButtonPrefab { get => levelButtonPrefab;}
    public static GameObject RegionButtonPrefab { get => regionButtonPrefab;}
    public static GameObject ImageMainLoad { get => imageMainLoad;}
    public static GameObject ShopPanelPrefab { get => shopPanelPrefab;}
    public static GameObject ShopButtonPrefab { get => shopButtonPrefab;}
    public static GameObject PrefabButtonThing { get => prefabButtonThing;}
    public static GameObject PanelShopConfirmation { get => panelShopConfirmation;}
    public static GameObject PanelShopInformation { get => panelShopInformation; }
    public static GameObject ScoreElementPrefab { get => scoreElementPrefab;}
    public static GameObject CanvasHelpToPlayer { get => canvasHelpToPlayer;}
    public static GameObject CanvasStartGame { get => canvasStartGame;}
    public static GameObject CanvasEndGameMenu { get => canvasEndGameMenu;}
    public static GameObject TextCollectElement { get => textCollectElement;}
    public static GameObject CollectElement { get => collectElement; }

    //private void LoadAllPrefab() {
    //    levelsCanvasPrefab = Resources.Load(prefabFolder + "Arrow") as GameObject;
    //}


    //void Awake()
    //{
    //    if (Instance)
    //    {
    //        Destroy(this.gameObject); //Delete duplicate
    //        return;
    //    }
    //    else
    //    {
    //        Instance = this; //Make this object the only instance            
    //    }
    //    if (Application.isPlaying)
    //    {
    //        DontDestroyOnLoad(gameObject); //Set as do not destroy
    //    }
    //    //// регистрация синглтона
    //    //if (Instance != null)
    //    //{
    //    //    Debug.LogError("Несколько экземпляров PrefabBank!");
    //    //}
    //    //Instance = this;
    //}
}
