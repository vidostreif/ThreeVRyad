using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;

public class RewardVideo
{
    private RewardedAd rewardedAd;//параметры видео
    private bool newAdPrepared;//новый экземпляр класса рекламы подготовлен
    private float lastTryLoadVideo; //момент последнеЙ попытки загрузить видео
    int firstLoadDelay;
    private float lastViewVideo = 0; //момент последнего просмотра видео
    private float pauseBetweenViews; //пауза между просмотрами
    private VideoBrowseButton lastActivVideoBrowseButton; //последняя активированная кнопка видео
    private List<VideoBrowseButton> videoBrowseButtonList;
    private List<VideoBrowseButton> videoBrowseButtonListForDelete;
    private Action<Reward> actionSuccess; //действие в случае усспешного просмотра рекламы
    private string adAndroidId; //идентификатор рекламы за просмотр которой выдается вознаграждение
    private string adIOSId; //идентификатор рекламы за просмотр которой выдается вознаграждение
    private GameObject prefabButton;

    public RewardVideo(Action<Reward> actionSuccess, string adAndroidId, string adIOSId, GameObject prefabButton, float pauseBetweenViews = 0, int firstLoadDelay = 0)
    {
        this.actionSuccess = actionSuccess;
        this.adAndroidId = adAndroidId;
        this.adIOSId = adIOSId;
        this.prefabButton = prefabButton;
        this.pauseBetweenViews = pauseBetweenViews;
        this.firstLoadDelay = firstLoadDelay;
        this.lastTryLoadVideo = 0;

        PrepareNewAd();

        videoBrowseButtonList = new List<VideoBrowseButton>();
        videoBrowseButtonListForDelete = new List<VideoBrowseButton>();
    }

    public VideoBrowseButton GetVideoBrowseButton(Transform transformParent, Action<Reward> newAction)
    {
        VideoBrowseButton videoBrowseButton = new VideoBrowseButton();
        videoBrowseButtonList.Add(videoBrowseButton);
        //если заранее прописанный актион иначе прописываем новый
        if (newAction == null)
        {
            videoBrowseButton.actionSuccess = actionSuccess;
        }
        else
        {
            videoBrowseButton.actionSuccess = newAction;
        }        
        videoBrowseButton.go = GameObject.Instantiate(prefabButton, transformParent);
        videoBrowseButton.button = videoBrowseButton.go.GetComponent<Button>();
        Transform textTimerTran = videoBrowseButton.go.transform.Find("TextTimer");
        videoBrowseButton.textTimer = textTimerTran.GetComponent<Text>();
        SupportFunctions.ChangeButtonAction(videoBrowseButton.button.transform, delegate { CreateBanerForFee(videoBrowseButton); });
        firstLoadDelay = 0;
        lastTryLoadVideo = 0;
        return videoBrowseButton;
    }
        
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
        //this.RequestRewardBasedVideoForCoin();
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
        this.PrepareNewAd();
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardedAdClosed event received");
        this.PrepareNewAd();
    }

    private void PrepareNewAd() {
#if UNITY_ANDROID
                string adUnitId = this.adAndroidId;
#elif UNITY_IPHONE
                                    string adUnitId = adIOSId;
#else
                                    string adUnitId = "unexpected_platform";
#endif

        this.rewardedAd = new RewardedAd(adUnitId);

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

        newAdPrepared = true;
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
        if (actionSuccess != null && lastActivVideoBrowseButton != null)
        {
            if (actionSuccess.Method != null && actionSuccess.Target != null)
            {
                lastActivVideoBrowseButton.actionSuccess(args);
            }
        }
        lastViewVideo = Time.realtimeSinceStartup;
        //RequestRewardBasedVideoForCoin();
    }

    //предварительная загрузка видео для получения вознаграждения
    private void RequestRewardBasedVideoForCoin()
    {        
        //если первая загрузка и нет задержки, то загружаем немедленно. Если первая загрузка и есть задержка, то ждем пока не наступит время
        //иначе пытаемся загрузить видео не чаще одного раза в минуту
        if ((lastTryLoadVideo == 0 && (firstLoadDelay == 0 || firstLoadDelay < Time.time)) || lastTryLoadVideo + 60 < Time.realtimeSinceStartup)
        {
            lastTryLoadVideo = Time.realtimeSinceStartup;
            //// Create an empty ad request.
            //AdRequest request = new AdRequest.Builder().Build();
            //// Load the rewarded video ad with the request.
            //this.rewardBasedVideoAd.LoadAd(request, adUnitId);
            Debug.Log("Запуск загрузки видео");
            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();
            // Load the rewarded ad with the request.
            this.rewardedAd.LoadAd(request);
        }        
    }

    //создание рекламы за вознаграждение
    public void CreateBanerForFee(VideoBrowseButton videoBrowseButton)
    {
#if !UNITY_EDITOR
        if (rewardedAd.IsLoaded())
        {
            newAdPrepared = false;
            lastActivVideoBrowseButton = videoBrowseButton;
            rewardedAd.Show();            
        }
        else
        {
            SupportFunctions.CreateInformationPanel("Видео еще не загрузилось!");
        }

        //сбрасываем, для немедленной попытки загрузить видео.
        firstLoadDelay = 0;
        lastTryLoadVideo = 0;
#else
        lastActivVideoBrowseButton = videoBrowseButton;
        Reward args = new Reward();
        args.Type = "Test type";
        args.Amount = 1;
        HandleUserEarnedReward(videoBrowseButton, args);
#endif
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
                        if (lastViewVideo != 0 && lastViewVideo + pauseBetweenViews > Time.realtimeSinceStartup)
                        {
                            itemVideoBrowseButton.button.interactable = false;
                            itemVideoBrowseButton.textTimer.text = "" + ((int)lastViewVideo + pauseBetweenViews - (int)Time.realtimeSinceStartup);
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
#if !UNITY_EDITOR
        //проверяем загрузку видео
        if (newAdPrepared && !rewardedAd.IsLoaded())
        {
            RequestRewardBasedVideoForCoin();
        }
#endif
    }
}

public class VideoBrowseButton
{
    public GameObject go;
    public Button button;
    public Text textTimer;
    public Action<Reward> actionSuccess; //действие в случае усспешного просмотра рекламы
}

