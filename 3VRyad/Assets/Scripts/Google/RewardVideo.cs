using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;

public class RewardVideo
{
    private RewardedAd rewardedAd;//параметры видео
    private float lastViewVideo = 0; //момент последнего просмотра видео
    private float pauseBetweenViews; //пауза между просмотрами
    private List<VideoBrowseButton> videoBrowseButtonList;
    private List<VideoBrowseButton> videoBrowseButtonListForDelete;
    private Action<Reward> actionSuccess; //действие в случае усспешного просмотра рекламы
    private string adAndroidId; //идентификатор рекламы за просмотр которой выдается вознаграждение
    private string adIOSId; //идентификатор рекламы за просмотр которой выдается вознаграждение
    private GameObject prefabButton;

    public RewardVideo(Action<Reward> actionSuccess, string adAndroidId, string adIOSId, GameObject prefabButton, float pauseBetweenViews = 0)
    {
        this.actionSuccess = actionSuccess;
        this.adAndroidId = adAndroidId;
        this.adIOSId = adIOSId;
        this.prefabButton = prefabButton;
        this.pauseBetweenViews = pauseBetweenViews;

#if UNITY_ANDROID
        string adUnitId = this.adAndroidId;
#elif UNITY_IPHONE
                            string adUnitId = adIOSId;
#else
                            string adUnitId = "unexpected_platform";
#endif


    this.rewardedAd = new RewardedAd(adUnitId);

        //// Get singleton reward based video ad reference.
        //this.rewardBasedVideoAd = RewardBasedVideoAd.Instance;

        //// Called when an ad request has successfully loaded.
        //rewardBasedVideoAd.OnAdLoaded += HandleRewardBasedVideoLoaded;
        //// Called when an ad request failed to load.
        //rewardBasedVideoAd.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
        //// Called when an ad is shown.
        //rewardBasedVideoAd.OnAdOpening += HandleRewardBasedVideoOpened;
        //// Called when the ad starts to play.
        //rewardBasedVideoAd.OnAdStarted += HandleRewardBasedVideoStarted;
        //// Called when the user should be rewarded for watching a video.
        //rewardBasedVideoAd.OnAdRewarded += HandleRewardBasedVideoRewarded;
        //// Called when the ad is closed.
        //rewardBasedVideoAd.OnAdClosed += HandleRewardBasedVideoClosed;
        //// Called when the ad click caused the user to leave the application.
        //rewardBasedVideoAd.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;

        // Called when an ad request has successfully loaded.
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        this.RequestRewardBasedVideoForCoin();

        videoBrowseButtonList = new List<VideoBrowseButton>();
        videoBrowseButtonListForDelete = new List<VideoBrowseButton>();
    }

    public VideoBrowseButton GetVideoBrowseButton(Transform transformParent)
    {
        VideoBrowseButton videoBrowseButton = new VideoBrowseButton();
        videoBrowseButtonList.Add(videoBrowseButton);
        videoBrowseButton.go = GameObject.Instantiate(prefabButton, transformParent);
        videoBrowseButton.button = videoBrowseButton.go.GetComponent<Button>();
        Transform textTimerTran = videoBrowseButton.go.transform.Find("TextTimer");
        videoBrowseButton.textTimer = textTimerTran.GetComponent<Text>();
        SupportFunctions.ChangeButtonAction(videoBrowseButton.button.transform, CreateBanerForFee);
        return videoBrowseButton;
    }

    //public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    //{
    //    Debug.Log("HandleRewardBasedVideoLoaded event received");
    //}

    ////неудалось загрузить видео
    //public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    //{
    //    Debug.Log(
    //        "HandleRewardBasedVideoFailedToLoad event received with message: "
    //                         + args.Message);
    //    this.RequestRewardBasedVideoForCoin();
    //}

    //public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    //{
    //    Debug.Log("HandleRewardBasedVideoOpened event received");
    //}

    //public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    //{
    //    Debug.Log("HandleRewardBasedVideoStarted event received");
    //}

    ////закрытие видео
    //public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    //{
    //    Debug.Log("HandleRewardBasedVideoClosed event received");
    //    this.RequestRewardBasedVideoForCoin();
    //}

    ////если пользователь успешно посмотрел видео
    //public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    //{
    //    string type = args.Type;
    //    double amount = args.Amount;
    //    Debug.Log(
    //        "HandleRewardBasedVideoRewarded event received for "
    //                    + amount.ToString() + " " + type);

    //    //выполняем прописанный делегат
    //    if (actionSuccess != null)
    //    {
    //        if (actionSuccess.Method != null && actionSuccess.Target != null)
    //        {
    //            actionSuccess(args);
    //        }
    //    }
    //    lastViewVideo = Time.time;
    //}

    //public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    //{
    //    Debug.Log("HandleRewardBasedVideoLeftApplication event received");
    //}


    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardedAdLoaded event received");
    }
    //неудалось загрузить видео
    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        Debug.Log(
            "HandleRewardedAdFailedToLoad event received with message: "
                             + args.Message);
        this.RequestRewardBasedVideoForCoin();
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardedAdOpening event received");
    }
    //ошибка показа
    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        Debug.Log(
            "HandleRewardedAdFailedToShow event received with message: "
                             + args.Message);
        this.RequestRewardBasedVideoForCoin();
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardedAdClosed event received");
    }

    //если пользователь успешно посмотрел видео
    public void HandleUserEarnedReward(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        Debug.Log(
            "HandleRewardedAdRewarded event received for "
                        + amount.ToString() + " " + type);


        //выполняем прописанный делегат
        if (actionSuccess != null)
        {
            if (actionSuccess.Method != null && actionSuccess.Target != null)
            {
                actionSuccess(args);
            }
        }
        lastViewVideo = Time.time;
    }
       

    //предварительная загрузка видео для получения вознаграждения
    private void RequestRewardBasedVideoForCoin()
    {
//#if UNITY_ANDROID
//        string adUnitId = adAndroidId;
//#elif UNITY_IPHONE
//                            string adUnitId = adIOSId;
//#else
//                            string adUnitId = "unexpected_platform";
//#endif

        //// Create an empty ad request.
        //AdRequest request = new AdRequest.Builder().Build();
        //// Load the rewarded video ad with the request.
        //this.rewardBasedVideoAd.LoadAd(request, adUnitId);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);
    }

    //создание рекламы за вознаграждение
    public void CreateBanerForFee()
    {
        if (rewardedAd.IsLoaded())
        {
            rewardedAd.Show();
        }
        else
        {
            SupportFunctions.CreateInformationPanel("Видео еще не загрузилось!");
        }
    }

    public void ProcessingOfButtonArrays()
    {
        if (videoBrowseButtonList == null)
        {
            videoBrowseButtonList = new List<VideoBrowseButton>();
            videoBrowseButtonListForDelete = new List<VideoBrowseButton>();
        }

        //обрабатывем все кнопки
        if (videoBrowseButtonList.Count > 0)
        {
            foreach (VideoBrowseButton itemVideoBrowseButton in videoBrowseButtonList)
            {
                if (itemVideoBrowseButton.go != null)
                {
                    if (itemVideoBrowseButton.button != null && itemVideoBrowseButton.textTimer != null)
                    {
                        if (lastViewVideo != 0 && lastViewVideo + pauseBetweenViews > Time.time)
                        {
                            itemVideoBrowseButton.button.interactable = false;
                            itemVideoBrowseButton.textTimer.text = "" + ((int)lastViewVideo + pauseBetweenViews - (int)Time.time);
                        }
                        else
                        {
                            itemVideoBrowseButton.button.interactable = true;
                            itemVideoBrowseButton.textTimer.text = "";
                        }
                    }
                }
                else
                {
                    //добавляем для удаления
                    videoBrowseButtonListForDelete.Add(itemVideoBrowseButton);
                }
            }

            //удаляем уничтоженные кнопки
            foreach (VideoBrowseButton itemVideoBrowseButton in videoBrowseButtonListForDelete)
            {
                videoBrowseButtonList.Remove(itemVideoBrowseButton);
            }
            videoBrowseButtonListForDelete.Clear();
        }
    }
}

public class VideoBrowseButton
{
    public GameObject go;
    public Button button;
    public Text textTimer;
}

