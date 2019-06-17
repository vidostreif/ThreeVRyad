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

    private VideoBrowseButton firstVideoBrowseButton;
    private VideoBrowseButton secondVideoBrowseButton;
    private VideoBrowseButton thirdVideoBrowseButton;

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
        LoadSave();

        DetermineMomentOfIssuingNextGift();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DetermineMomentOfIssuingNextGift() {
        //определяем момен выдачи следующего подарка
        DateTime realtime = CheckTime.Realtime();
        realtime = new DateTime(realtime.Year, realtime.Month, realtime.Day).AddHours(8);//находим 8 час сегодняшнего дня
        TimeSpan timeSpan = realtime.Subtract(lastGiftTimeIssued);

        // если выдали предыдущий подарок раньше 8 часов сегодняшнего дня
        if (timeSpan.TotalSeconds > 0)
        {
            nextGiftTimeIssued = realtime;
            numberOfGiftsIssuedToday = 0;
        }
        else
        {
            nextGiftTimeIssued = realtime.AddDays(1);
        }
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
                    Debug.Log("randomGiftNumber " + randomNumber);
                    Thing thing = ThingsManager.Instance.GetThing(randomNumber);
                    Debug.Log("thing " + thing.Type);
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
    public void ConfirmationOfViewingFirstVideo(Reward args)
    {
        if (args.Amount > 0)
        {
            Destroy(firstVideoBrowseButton.go);
            GivingOutRandomGift(1);
        }
    }

    //подтверждение просмотра второго видео
    public void ConfirmationOfViewingSecondVideo(Reward args)
    {
        if (args.Amount > 0)
        {
            Destroy(secondVideoBrowseButton.go);
            GivingOutRandomGift(2);
        }
    }

    //подтверждение просмотра третьего видео
    public void ConfirmationOfViewingThirdVideo(Reward args)
    {
        if (args.Amount > 0)
        {
            Destroy(thirdVideoBrowseButton.go);
            GivingOutRandomGift(3);
        }
    }

    public void CreateDailyGiftPanel() {
        DetermineMomentOfIssuingNextGift();
        if (numberOfGiftsIssuedToday < 3) {
            DestroyDailyGiftPanel();
            Transform transformParent = GameObject.Find("CanvasShop").transform;
            dailyGiftPanel = GameObject.Instantiate(PrefabBank.PanelDailyGift, transformParent);
            //dailyGiftPanel.transform.Find("TextConfirmation").GetComponent<Text>().text = str;

            if (numberOfGiftsIssuedToday < 3)
            {
               GameObject ButtonGo = dailyGiftPanel.transform.Find("Button1").gameObject;
#if !UNITY_EDITOR
                firstVideoBrowseButton = AdMobManager.Instance.GetVideoBrowseButton(dailyGiftPanel.transform.Find("VideoPlace_1"), VideoForFeeEnum.ForDailyGift, DailyGiftManager.Instance.ConfirmationOfViewingFirstVideo);   
                //GameObject.Destroy(Button1Go);                
#else
                SupportFunctions.ChangeButtonAction(ButtonGo.transform, delegate { GivingOutRandomGift(1); });
                SupportFunctions.ChangeAlfa(ButtonGo.GetComponent<Image>(), 1);
#endif
            }

            if (numberOfGiftsIssuedToday < 2)
            {
                GameObject ButtonGo = dailyGiftPanel.transform.Find("Button2").gameObject;
#if !UNITY_EDITOR
                firstVideoBrowseButton = AdMobManager.Instance.GetVideoBrowseButton(dailyGiftPanel.transform.Find("VideoPlace_2"), VideoForFeeEnum.ForDailyGift, DailyGiftManager.Instance.ConfirmationOfViewingSecondVideo);   
                //GameObject.Destroy(Button1Go);                
#else
                SupportFunctions.ChangeButtonAction(ButtonGo.transform, delegate { GivingOutRandomGift(2); });
                SupportFunctions.ChangeAlfa(ButtonGo.GetComponent<Image>(), 1);
#endif
            }

            if (numberOfGiftsIssuedToday < 1)
            {
                GameObject ButtonGo = dailyGiftPanel.transform.Find("Button3").gameObject;
#if !UNITY_EDITOR
                firstVideoBrowseButton = AdMobManager.Instance.GetVideoBrowseButton(dailyGiftPanel.transform.Find("VideoPlace_3"), VideoForFeeEnum.ForDailyGift, DailyGiftManager.Instance.ConfirmationOfViewingThirdVideo);   
                //GameObject.Destroy(Button1Go);                
#else
                SupportFunctions.ChangeButtonAction(ButtonGo.transform, delegate { GivingOutRandomGift(3); });
                SupportFunctions.ChangeAlfa(ButtonGo.GetComponent<Image>(), 1);
#endif
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
