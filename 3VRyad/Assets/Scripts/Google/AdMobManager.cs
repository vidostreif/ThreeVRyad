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

        //Action<Reward> actionForCoin;
        //actionForCoin = Shop.Instance.AddCoinsForViewingAds;
        //string adUnitId = "ca-app-pub-6280237892174167/9330414827";

        //actionSuccess = AddMovesOnEndGAme;

        rewardVideoForCoin = new RewardVideo(Shop.Instance.AddCoinsForViewingAds, "ca-app-pub-3940256099942544/5224354917", "", PrefabBank.PrefabVideoBrowseButton, 60, 0);

        rewardVideoForMove = new RewardVideo(AdMobManager.Instance.AddMovesOnEndGAme, "ca-app-pub-3940256099942544/5224354917", "", PrefabBank.PrefabVideoBrowseButton, 300, 30);

        rewardVideoForLife = new RewardVideo(LifeManager.Instance.AddLifeForViewingAds, "ca-app-pub-3940256099942544/5224354917", "", PrefabBank.PrefabVideoBrowseButton, 300, 20);

        //определяем время загрузки видео для ежедневного подарка
        int timeLoadVideoForDailyGift = 10;
        if (DailyGiftManager.Instance.TodayReceivedAllDailyGift())
        {
            timeLoadVideoForDailyGift = (int)DailyGiftManager.Instance.TimeUntilNextDailyGift().TotalSeconds - 30;
            if (timeLoadVideoForDailyGift < 10)
            {
                timeLoadVideoForDailyGift = 10;
            }
        }     
        
        rewardVideoForDailyGift = new RewardVideo(DailyGiftManager.Instance.ConfirmationOfViewingFirstVideo, "ca-app-pub-3940256099942544/5224354917", "", PrefabBank.PrefabVideoBrowseButton, 0, timeLoadVideoForDailyGift);
    }

    public void Update()
    {
#if !UNITY_EDITOR
        //обрабатываем не чаще двух раз в секунду
        if (LastArrayProcessingTime + 0.5f < Time.realtimeSinceStartup)
        {
            LastArrayProcessingTime = Time.realtimeSinceStartup;
            rewardVideoForCoin.ProcessingOfButtonArrays();
            rewardVideoForMove.ProcessingOfButtonArrays();
            rewardVideoForLife.ProcessingOfButtonArrays();
            rewardVideoForDailyGift.ProcessingOfButtonArrays();
        }
#endif
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

    public void AddMovesOnEndGAme(Reward args) {
        if (Tasks.Instance != null)
        {
            Tasks.Instance.AddMovesOnEndGAme(args);
        }
    }
}



