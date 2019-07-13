using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
    private static GameObject panelInformation = Resources.Load(prefabFolder + "Canvas/Main menu/PanelInformation") as GameObject;
    private static GameObject panelInformationWithVideo = Resources.Load(prefabFolder + "Canvas/Main menu/PanelInformationWithVideo") as GameObject;
    private static GameObject panelDailyGift = Resources.Load(prefabFolder + "Canvas/Main menu/PanelDailyGift") as GameObject;    

    //магазин
    private static GameObject shopPanelPrefab = Resources.Load(prefabFolder + "Canvas/Shop/PanelShop") as GameObject;
    private static GameObject shopButtonPrefab = Resources.Load(prefabFolder + "Canvas/Shop/ShopButton") as GameObject;
    private static GameObject prefabButtonThing = Resources.Load(prefabFolder + "Canvas/GameCanvas/ButtonThing") as GameObject;
    private static GameObject panelShopConfirmation = Resources.Load(prefabFolder + "Canvas/Shop/PanelShopConfirmation") as GameObject;
    private static GameObject panelShopInformation = Resources.Load(prefabFolder + "Canvas/Shop/PanelShopInformation") as GameObject;
    private static GameObject prefabVideoBrowseButton = Resources.Load(prefabFolder + "Canvas/Shop/VideoBrowseButton") as GameObject;
    private static GameObject panelShopReceivingCoins = Resources.Load(prefabFolder + "Canvas/Shop/PanelShopReceivingCoins") as GameObject;
    private static GameObject dailyGiftButton = Resources.Load(prefabFolder + "Canvas/Shop/DailyGiftButton") as GameObject;
    private static GameObject imageCoin = Resources.Load(prefabFolder + "Canvas/GameCanvas/ImageCoin") as GameObject;
    private static GameObject cquirrel = Resources.Load(prefabFolder + "Canvas/Shop/Cquirrel") as GameObject;
    private static GameObject imageOpenBoxDailyGift = Resources.Load(prefabFolder + "Canvas/Shop/ImageOpenBoxDailyGift") as GameObject;
    private static GameObject videoBrowsePouchButton = Resources.Load(prefabFolder + "Canvas/Shop/VideoBrowsePouchButton") as GameObject;
    


    private static GameObject scoreElementPrefab = Resources.Load(prefabFolder + "Canvas/GameCanvas/ScoreElement") as GameObject;
    private static GameObject textInformationPrefab = Resources.Load(prefabFolder + "Canvas/GameCanvas/TextInformation") as GameObject;
    private static GameObject canvasHelpToPlayer = Resources.Load(prefabFolder + "Canvas/GameCanvas/CanHelpToPlayer") as GameObject;
    private static GameObject canvasStartGame = Resources.Load(prefabFolder + "Canvas/GameCanvas/CanStartGame") as GameObject;
    private static GameObject canvasEndGameMenu = Resources.Load(prefabFolder + "Canvas/GameCanvas/CanEndGame") as GameObject;

    ////элементы
    private static GameObject textCollectElement = Resources.Load(prefabFolder + "TextCollectElement") as GameObject;
    private static GameObject textLifeElement = Resources.Load(prefabFolder + "TextLifeElement") as GameObject;
    private static GameObject collectElement = Resources.Load(prefabFolder + "CollectElement") as GameObject;
    private static GameObject blockBacklight = Resources.Load(prefabFolder + "BlockBacklight") as GameObject;

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
    public static GameObject PanelInformation { get => panelInformation; }
    public static GameObject TextInformationPrefab { get => textInformationPrefab; }
    public static GameObject PanelInformationWithVideo { get => panelInformationWithVideo; }
    public static GameObject PanelDailyGift { get => panelDailyGift; }
    public static GameObject ImageCoin { get => imageCoin; }
    public static GameObject ScoreElementPrefab { get => scoreElementPrefab;}
    public static GameObject CanvasHelpToPlayer { get => canvasHelpToPlayer;}
    public static GameObject CanvasStartGame { get => canvasStartGame;}
    public static GameObject CanvasEndGameMenu { get => canvasEndGameMenu;}
    public static GameObject TextCollectElement { get => textCollectElement;}
    public static GameObject TextLifeElement { get => textLifeElement; }
    public static GameObject CollectElement { get => collectElement; }
    public static GameObject PrefabVideoBrowseButton { get => prefabVideoBrowseButton; }
    public static GameObject PanelShopReceivingCoins { get => panelShopReceivingCoins; }
    public static GameObject BlockBacklight { get => blockBacklight; }
    public static GameObject DailyGiftButton { get => dailyGiftButton; }
    public static GameObject Cquirrel { get => cquirrel; }
    public static GameObject ImageOpenBoxDailyGift { get => imageOpenBoxDailyGift; }
    public static GameObject VideoBrowsePouchButton { get => videoBrowsePouchButton; }
    

    public static void Preload()
    {
        //FieldInfo[] properties = PrefabBank.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        //foreach (FieldInfo property in properties)
        //{
        //    if (property.FieldType == typeof(string))
        //    {
        //        try
        //        {
        //            property.SetValue(this, string.Empty);
        //        }
        //        catch (Exception exception)
        //        {
        //            //Обрабатываем исключительную ситуацию, пишем логи
        //        }
        //    }
        //}
    }
    
}
