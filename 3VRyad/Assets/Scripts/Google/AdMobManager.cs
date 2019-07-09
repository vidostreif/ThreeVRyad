using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;

//менеджер рекламы
public class AdMobManager : MonoBehaviour
{
    public static AdMobManager Instance; // Синглтон
    public float LastArrayProcessingTime = 5;

    private RewardVideo rewardVideoForCoin; //просмотр рекламы за монеты
    private RewardVideo rewardVideoForMove; //просмотр рекламы за ходы
    private RewardVideo rewardVideoForLife; //просмотр рекламы за жизни
    private RewardVideo rewardVideoForDailyGift; //просмотр рекламы за ежедневный подарок

    //public Action<Reward> actionSuccess;

    public void Awake()
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
            DontDestroyOnLoadManager.DontDestroyOnLoad(gameObject); //Set as do not destroy            
        }
    }

    public void Start()
    {
#if UNITY_ANDROID
        string appId = "ca-app-pub-6280237892174167~8971322173";
#elif UNITY_IPHONE
                    string appId = "ca-app-pub-3940256099942544~1458002511";
#else
                    string appId = "unexpected_platform";
#endif

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);

        //тестовый код рекламы ca-app-pub-3940256099942544/5224354917

        rewardVideoForCoin = new RewardVideo(AdMobManager.Instance.AddCoinsForViewingAds, "ca-app-pub-6280237892174167/9330414827", "", PrefabBank.PrefabVideoBrowseButton, SpriteBank.SetShape(SpritesEnum.Coin), 60, 30);

        rewardVideoForMove = new RewardVideo(AdMobManager.Instance.AddMovesOnEndGAme, "ca-app-pub-6280237892174167/5734011639", "", PrefabBank.PrefabVideoBrowseButton, SpriteBank.SetShape(SpritesEnum.Move), 300, 120);

        rewardVideoForLife = new RewardVideo(AdMobManager.Instance.AddLifeForViewingAds, "ca-app-pub-6280237892174167/8374640938", "", PrefabBank.PrefabVideoBrowseButton, SpriteBank.SetShape(SpritesEnum.Life), 300, 60);

        ////определяем время загрузки видео для ежедневного подарка
        //int timeLoadVideoForDailyGift = 10;
        //if (DailyGiftManager.Instance.TodayReceivedAllDailyGift())
        //{
        //    timeLoadVideoForDailyGift = (int)DailyGiftManager.Instance.TimeUntilNextDailyGift().TotalSeconds - 30;
        //    if (timeLoadVideoForDailyGift < 10)
        //    {
        //        timeLoadVideoForDailyGift = 10;
        //    }
        //}     
        
        rewardVideoForDailyGift = new RewardVideo(AdMobManager.Instance.ConfirmationOfViewingVideo_1, "ca-app-pub-6280237892174167/9113007538", "", PrefabBank.PrefabVideoBrowseButton, SpriteBank.SetShape(SpritesEnum.Daily_Gift), 5, 0);
    }

    public void Update()
    {
        //обрабатываем не чаще двух раз в секунду
        if (LastArrayProcessingTime + 0.5f < Time.realtimeSinceStartup)
        {
            LastArrayProcessingTime = Time.realtimeSinceStartup;
            rewardVideoForCoin.ProcessingOfButtonArrays();
            rewardVideoForMove.ProcessingOfButtonArrays();
            rewardVideoForLife.ProcessingOfButtonArrays();
            rewardVideoForDailyGift.ProcessingOfButtonArrays();
        }
    }

    //создание кнопки просмотра видео
    public VideoBrowseButton GetVideoBrowseButton(Transform transformParent, VideoForFeeEnum videoForFeeEnum, Action<Reward> newAction = null) {
        if (videoForFeeEnum == VideoForFeeEnum.ForCoin)
        {
            return rewardVideoForCoin.GetVideoBrowseButton(transformParent, newAction);
        }
        else if (videoForFeeEnum == VideoForFeeEnum.ForMove)
        {
            return rewardVideoForMove.GetVideoBrowseButton(transformParent, newAction);
        }
        else if (videoForFeeEnum == VideoForFeeEnum.ForLive)
        {
            return rewardVideoForLife.GetVideoBrowseButton(transformParent, newAction);
        }
        else if (videoForFeeEnum == VideoForFeeEnum.ForDailyGift)
        {
            return rewardVideoForDailyGift.GetVideoBrowseButton(transformParent, newAction);
        }
        return null;
    }

    public void AddCoinsForViewingAds(Reward args)
    {
        if (Shop.Instance != null)
        {
            Shop.Instance.AddCoinsForViewingAds(args);
        }
    }    

    public void AddMovesOnEndGAme(Reward args) {
        if (Tasks.Instance != null)
        {
            Tasks.Instance.AddMovesOnEndGAme(args);
        }
    }

    public void AddLifeForViewingAds(Reward args)
    {
        if (LifeManager.Instance != null)
        {
            LifeManager.Instance.AddLifeForViewingAds(args);
        }
    }

    public void ConfirmationOfViewingVideo_1(Reward args)
    {
        if (DailyGiftManager.Instance != null)
        {
            DailyGiftManager.Instance.ConfirmationOfViewingVideo_1(args);
        }
    }
}



