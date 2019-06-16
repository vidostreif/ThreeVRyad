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

        rewardVideoForLife = new RewardVideo(LifeManager.Instance.AddLifeForViewingAds, "ca-app-pub-3940256099942544/5224354917", "", PrefabBank.PrefabVideoBrowseButton, 300, 15);
    }

    public void Update()
    {
#if !UNITY_EDITOR
        //обрабатываем не чаще двух раз в секунду
        if (LastArrayProcessingTime + 0.5f < Time.time)
        {
            LastArrayProcessingTime = Time.time;
            rewardVideoForCoin.ProcessingOfButtonArrays();
            rewardVideoForMove.ProcessingOfButtonArrays();
        }
#endif
    }

    //создание кнопки просмотра видео
    public VideoBrowseButton GetVideoBrowseButton(Transform transformParent, VideoForFeeEnum videoForFeeEnum) {
        if (videoForFeeEnum == VideoForFeeEnum.ForCoin)
        {
            return rewardVideoForCoin.GetVideoBrowseButton(transformParent);
        }
        else if (videoForFeeEnum == VideoForFeeEnum.ForMove)
        {
            return rewardVideoForMove.GetVideoBrowseButton(transformParent);
        }
        else if (videoForFeeEnum == VideoForFeeEnum.ForLive)
        {
            return rewardVideoForLife.GetVideoBrowseButton(transformParent);
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



