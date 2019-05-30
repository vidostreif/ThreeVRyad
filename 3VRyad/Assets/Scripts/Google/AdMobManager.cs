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
    //идентификатор рекламы за просмотр которой выдается вознаграждение
    //private const string previewforFeeBanerID = "ca-app-pub-6280237892174167/9330414827";
    private RewardBasedVideoAd rewardBasedVideo;
    private float LastViewVideo = 0;
    private List<VideoBrowseButton> videoBrowseButtonList;
    private List<VideoBrowseButton> videoBrowseButtonListForDelete;
    private GameObject panelInfirmation; //панель информации

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

        // Get singleton reward based video ad reference.
        this.rewardBasedVideo = RewardBasedVideoAd.Instance;

        // Called when an ad request has successfully loaded.
        rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
        // Called when an ad request failed to load.
        rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
        // Called when an ad is shown.
        rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
        // Called when the ad starts to play.
        rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
        // Called when the user should be rewarded for watching a video.
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        // Called when the ad is closed.
        rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        // Called when the ad click caused the user to leave the application.
        rewardBasedVideo.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;

        this.RequestRewardBasedVideo();

        videoBrowseButtonList = new List<VideoBrowseButton>();
        videoBrowseButtonListForDelete = new List<VideoBrowseButton>();
    }

    public void Update()
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
                        if (LastViewVideo != 0 && LastViewVideo + 60 > Time.time)
                        {
                            itemVideoBrowseButton.button.interactable = false;
                            itemVideoBrowseButton.textTimer.text = "" + ((int)LastViewVideo + 60 - (int)Time.time);
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

    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardBasedVideoLoaded event received");
    }

    //неудалось загрузить видео
    public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log(
            "HandleRewardBasedVideoFailedToLoad event received with message: "
                             + args.Message);
        this.RequestRewardBasedVideo();
    }

    public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardBasedVideoOpened event received");
    }

    public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardBasedVideoStarted event received");
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardBasedVideoClosed event received");
        this.RequestRewardBasedVideo();
    }

    //если пользователь успешно посмотрел видео
    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        Debug.Log(
            "HandleRewardBasedVideoRewarded event received for "
                        + amount.ToString() + " " + type);

        //если вознаграждение монеты
        //if (type == "Coin")
        //{
            Shop.Instance.AddCoinsForViewingAds(args);
        //}

        LastViewVideo = Time.time;
    }

    public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardBasedVideoLeftApplication event received");
    }

    //предварительная загрузка видео
    private void RequestRewardBasedVideo()
    {
        #if UNITY_ANDROID
            //string adUnitId = "ca-app-pub-6280237892174167/9330414827";
        string adUnitId = "ca-app-pub-3940256099942544/5224354917";
        #elif UNITY_IPHONE
                    string adUnitId = "ca-app-pub-3940256099942544/1712485313";
        #else
                    string adUnitId = "unexpected_platform";
        #endif

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        this.rewardBasedVideo.LoadAd(request, adUnitId);
    }

    //создание рекламы за вознаграждение
    public void CreateBanerForFee() {
        if (rewardBasedVideo.IsLoaded())
        {
            rewardBasedVideo.Show();
        }
        else
        {
            SupportFunctions.CreateInformationPanel("Не удалось загрузить рекламное видео!", this.transform);
        }
    }

    //создание кнопки просмотра видео
    public VideoBrowseButton GetVideoBrowseButton(Transform transformParent) {
        VideoBrowseButton videoBrowseButton = new VideoBrowseButton();
        videoBrowseButtonList.Add(videoBrowseButton);
        videoBrowseButton.go = Instantiate(PrefabBank.PrefabVideoBrowseButton, transformParent);
        videoBrowseButton.button = videoBrowseButton.go.GetComponent<Button>();
        Transform textTimerTran = videoBrowseButton.go.transform.Find("TextTimer");
        videoBrowseButton.textTimer = textTimerTran.GetComponent<Text>();
        SupportFunctions.ChangeButtonAction(videoBrowseButton.button.transform, CreateBanerForFee);
        return videoBrowseButton;
    }
}

public class VideoBrowseButton {
    public GameObject go;
    public Button button;
    public Text textTimer;
}
