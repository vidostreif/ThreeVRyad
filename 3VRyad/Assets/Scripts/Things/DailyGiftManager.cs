using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//выдача подарков каждый день начиная с 8 часов по гринвичу
public class DailyGiftManager : MonoBehaviour
{
    public static DailyGiftManager Instance; // Синглтон
    private DateTime lastGiftTimeIssued; //время выдачи предыдущего подарка
    private DateTime nextGiftTimeIssued; //время выдачи следующего подарка
    private int numberOfGiftsIssuedToday; //количество выданных подарков сегодня
    private GameObject dailyGiftPanel;
    private GameObject dailyGiftBut; //кнопка
    private GameObject panelCquirrelPlace;//панель с белкой
    private Text TextTimeUntilNextDailyGift;
    private Transform dailyGiftButtonPlace;
    private float lastProcessingTime = 0;

    private VideoBrowseButton[] videoBrowseButtonList;

    public DateTime LastGiftTimeIssued { get => lastGiftTimeIssued; }
    public int NumberOfGiftsIssuedToday { get => numberOfGiftsIssuedToday; }

    // Start is called before the first frame update
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
            DontDestroyOnLoadManager.DontDestroyOnLoad(gameObject); //Set as do not destroy            
        }
        dailyGiftButtonPlace = transform.Find("DailyGiftButtonPlace");
        TextTimeUntilNextDailyGift = dailyGiftButtonPlace.Find("TextTimeUntilNextDailyGift").GetComponent<Text>();
        LoadSave();
        DetermineMomentOfIssuingNextGift();
        CreateCquirrel();
    }

    // Update is called once per frame
    void Update()
    {
        //обрабатываем не чаще одного раз в секунду
        if (lastProcessingTime + 1f < Time.realtimeSinceStartup)
        {
            if (JsonSaveAndLoad.LoadSave().trainingCompleted)
            {
                if (TodayReceivedAllDailyGift())
                {
                    if (dailyGiftBut != null)
                    {
                        Destroy(dailyGiftBut);
                    }

                    TextTimeUntilNextDailyGift.text = SupportFunctions.GetStringTime(TimeUntilNextDailyGift());
                }
                else
                {
                    if (dailyGiftBut == null)
                    {
                        dailyGiftBut = GameObject.Instantiate(PrefabBank.DailyGiftButton, dailyGiftButtonPlace);
                        SupportFunctions.ChangeButtonAction(dailyGiftBut.transform, delegate { CreateDailyGiftPanel(); });
                        TextTimeUntilNextDailyGift.text = "";
                    }
                }
            }
            else
            {
                if (dailyGiftBut != null)
                {
                    Destroy(dailyGiftBut);
                }
                TextTimeUntilNextDailyGift.text = "";
            }            
        }
    }

    private void DetermineMomentOfIssuingNextGift() {
        //определяем момен выдачи следующего подарка
        DateTime realtime = CheckTime.Realtime();
        DateTime realtime2hours = new DateTime(realtime.Year, realtime.Month, realtime.Day).AddHours(2);//находим 8 час сегодняшнего дня
        TimeSpan timeSpan = realtime2hours.Subtract(lastGiftTimeIssued);
        TimeSpan differenceWith8Hours = realtime2hours.Subtract(realtime);

        // если выдали предыдущий подарок раньше 8 часов сегодняшнего дня, а сейчас уже больше 8 часов
        if (timeSpan.TotalSeconds > 0 && differenceWith8Hours.TotalSeconds < 0)
        {
            nextGiftTimeIssued = realtime2hours;
            numberOfGiftsIssuedToday = 0;
        }
        else if (timeSpan.TotalSeconds > 0 && differenceWith8Hours.TotalSeconds > 0)
        {
            nextGiftTimeIssued = realtime2hours;
        }
        else if (timeSpan.TotalSeconds < 0 && differenceWith8Hours.TotalSeconds < 0)
        {
            nextGiftTimeIssued = realtime2hours.AddDays(1);
        }
    }

    //показываем белку
    public void CreateCquirrel() {
        DelCquirrel();
        if (!TodayReceivedAllDailyGift() && JsonSaveAndLoad.LoadSave().trainingCompleted)
        {
            GameObject PanelCquirrelPlace = GameObject.Find("PanelCquirrelPlace");
            panelCquirrelPlace = Instantiate(PrefabBank.Cquirrel, PanelCquirrelPlace.transform);
            SupportFunctions.ChangeButtonAction(panelCquirrelPlace.transform, CreateDailyGiftPanel);
        }        
    }

    //удаляем белку
    public void DelCquirrel()
    {
        Destroy(panelCquirrelPlace);
    }

    //выдача рандомного подарка
    private void GivingOutRandomGift(int place)
    {
        if (numberOfGiftsIssuedToday < 3)
        {
            int randomNumber = UnityEngine.Random.Range(1, 3 + ThingsManager.Instance.InstrumentsCount());

            int randomFactor = UnityEngine.Random.Range(1, 5); //множитель подарка
            if (randomFactor == 1)
            {
                randomFactor = 2;
            }
            else
            {
                randomFactor = 1;
            }

            Debug.Log("randomNumber " + randomNumber);
            if (dailyGiftPanel != null)
            {
                Transform dailyGiftPanelTransform = dailyGiftPanel.transform.Find("VideoPlace_" + place);
                //если выпала единица, то выдаем 15 монет
                if (randomNumber == 1)
                {
                    Shop.Instance.AddCoins(15 * randomFactor, dailyGiftPanel.transform, dailyGiftPanelTransform.transform.position);
                }
                else if (randomNumber == 2)//если выпала двойка, то дарим бесконечные жизни
                {
                    LifeManager.Instance.addTimeImmortal(30 * randomFactor, dailyGiftPanel.transform, dailyGiftPanelTransform.transform.position);
                }
                else
                {
                    randomNumber -= 2;
                    //Debug.Log("randomGiftNumber " + randomNumber);
                    Thing thing = ThingsManager.Instance.GetThing(randomNumber);
                    //Debug.Log("thing " + thing.Type);
                    if (thing != null)
                    {
                        ThingsManager.Instance.addinstruments(thing.Type, 1 * randomFactor, dailyGiftPanel.transform, dailyGiftPanelTransform.transform.position);
                    }
                }
                numberOfGiftsIssuedToday++;

                lastGiftTimeIssued = CheckTime.Realtime();
                DetermineMomentOfIssuingNextGift();
                RecordSave();
            }
        }
    }

    //подтверждение просмотра первого видео
    public void ConfirmationOfViewingVideo_1(Reward args)
    {
        if (args.Amount > 0)
        {
            Destroy(videoBrowseButtonList[0].go);
            GivingOutRandomGift(1);
        }
    }

    //подтверждение просмотра второго видео
    public void ConfirmationOfViewingVideo_2(Reward args)
    {
        if (args.Amount > 0)
        {
            Destroy(videoBrowseButtonList[1].go);
            GivingOutRandomGift(2);
        }
    }

    //подтверждение просмотра третьего видео
    public void ConfirmationOfViewingVideo_3(Reward args)
    {
        if (args.Amount > 0)
        {
            Destroy(videoBrowseButtonList[2].go);
            GivingOutRandomGift(3);
        }
    }

    public void CreateDailyGiftPanel() {
        DetermineMomentOfIssuingNextGift();
        int maxNumber = 3;
        DelCquirrel();
        if (numberOfGiftsIssuedToday < maxNumber) {
            DestroyDailyGiftPanel();
            ThingsManager.Instance.PreloadSprites();
            videoBrowseButtonList = new VideoBrowseButton[maxNumber];
            Transform transformParent = GameObject.Find("CanvasShop").transform;
            dailyGiftPanel = GameObject.Instantiate(PrefabBank.PanelDailyGift, transformParent);
            //dailyGiftPanel.transform.Find("TextConfirmation").GetComponent<Text>().text = str;

                if (numberOfGiftsIssuedToday < maxNumber - 0)
                {
                    int n = 1;
                    VideoBrowseButton videoBrowseButton = AdMobManager.Instance.GetVideoBrowseButton(dailyGiftPanel.transform.Find("VideoPlace_" + n), VideoForFeeEnum.ForDailyGift, DailyGiftManager.Instance.ConfirmationOfViewingVideo_1);
                    videoBrowseButtonList[n - 1] = videoBrowseButton;
                }

            if (numberOfGiftsIssuedToday < maxNumber - 1)
            {
                int n = 2;
                    VideoBrowseButton videoBrowseButton = AdMobManager.Instance.GetVideoBrowseButton(dailyGiftPanel.transform.Find("VideoPlace_" + n), VideoForFeeEnum.ForDailyGift, DailyGiftManager.Instance.ConfirmationOfViewingVideo_2);
                    videoBrowseButtonList[n - 1] = videoBrowseButton; 
            }

            if (numberOfGiftsIssuedToday < maxNumber - 2)
            {
                int n = 3;
                    VideoBrowseButton videoBrowseButton = AdMobManager.Instance.GetVideoBrowseButton(dailyGiftPanel.transform.Find("VideoPlace_" + n), VideoForFeeEnum.ForDailyGift, DailyGiftManager.Instance.ConfirmationOfViewingVideo_3);
                    videoBrowseButtonList[n - 1] = videoBrowseButton;  
            }

            SupportFunctions.ChangeButtonAction(dailyGiftPanel.transform.Find("ButtonOk"), DestroyDailyGiftPanel);
        }
        else
        {
            TimeSpan timeSpan = TimeUntilNextDailyGift();
            SupportFunctions.CreateInformationPanel("До получения новых подарков осталось " + timeSpan.Hours + "h " + timeSpan.Minutes + "m");
        }
        
    }

    public void DestroyDailyGiftPanel()
    {
        GameObject.Destroy(dailyGiftPanel);
    }

    //сегодня получили все подарки?
    public bool TodayReceivedAllDailyGift()
    {
        DetermineMomentOfIssuingNextGift();
        if (numberOfGiftsIssuedToday < 3)
        {
            return false;
        }
        else
        {
            return true;
        }

    }

    //сколко времени до следующего подарка
    public TimeSpan TimeUntilNextDailyGift()
    {
        DetermineMomentOfIssuingNextGift();
        return nextGiftTimeIssued.Subtract(CheckTime.Realtime());      
    }

    private void LoadSave()
    {
        DailyGiftSave dailyGiftSave = JsonSaveAndLoad.LoadSave().dailyGiftSave;
        lastGiftTimeIssued = DateTime.FromFileTimeUtc(dailyGiftSave.lastGiftTimeIssuedLong);
        numberOfGiftsIssuedToday = dailyGiftSave.numberOfGiftsIssuedToday;
    }

    private void RecordSave()
    {
        JsonSaveAndLoad.RecordSave(this);
        JsonSaveAndLoad.SetSaveToFile();
    }
}
