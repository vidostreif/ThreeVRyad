using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance; // Синглтон
    private int life; //коилчество жизней
    [SerializeField] private int maxLife; //максимальное количество жизней
    [SerializeField] private int timeToGetOneLife; //количество минут для получения одной жизни 
    //private DateTime timeToGetOneLifeDateTime; //количество минут для получения одной жизни 
    private DateTime timeToNextLife; //время получения следующей жизни
    private DateTime endTimeImmortal; //время окончания бессметрия
    private int addMinutesTimeImmortal = 0;
    private Text textLive;
    private Image imageLive;
    private float LastArrayProcessingTime = 0;

    public int Life { get => life; }
    public DateTime TimeToNextLife { get => timeToNextLife; }
    public DateTime EndTimeImmortal {
        get {
            return endTimeImmortal.AddMinutes(addMinutesTimeImmortal);
        }
    }

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

        //timeToGetOneLifeDateTime = new DateTime(0, 0, 0, 0, timeToGetOneLife, 0);
        textLive = GetComponentInChildren<Text>();
        imageLive = GetComponentInChildren<Image>();
        LoadSave();
        StartCalculatingLives();
        UpdateText();
    }

    // Update is called once per frame
    void Update()
    {
        //пополняем жизни раз в указанное время
        if (LastArrayProcessingTime + 0.5f < Time.realtimeSinceStartup)
        {            
            if (life < maxLife && CheckTime.Realtime() >= timeToNextLife)
            {
                timeToNextLife = CheckTime.Realtime().AddMinutes(timeToGetOneLife);
                PlusLive();
                RecordSave();
            }
            LastArrayProcessingTime = Time.realtimeSinceStartup;
            UpdateText();
        }
    }

    //подсчет количества жизней и времени до получения следующей при старте игры
    private void StartCalculatingLives()
    {
        if (life < maxLife)
        {
            DateTime timeHasHassed = CheckTime.Realtime();
            //timeHasHassed = timeHasHassed.AddMinutes(-timeToGetOneLife);
            TimeSpan timeSpan = timeHasHassed.Subtract(timeToNextLife.AddMinutes(-timeToGetOneLife));

            double minutes = timeSpan.TotalMinutes;
            while (minutes > timeToGetOneLife && life < maxLife)
            {
                PlusLive();
                timeToNextLife = timeToNextLife.AddMinutes(timeToGetOneLife);
                Debug.Log(timeToNextLife.ToString());
                minutes -= timeToGetOneLife;
            }
            RecordSave();
        }
    }

    public void LiveFlew(int price) {
        if (price > addMinutesTimeImmortal)
        {
            price = addMinutesTimeImmortal;
        }
        endTimeImmortal = EndTimeImmortal.AddMinutes(price);
        addMinutesTimeImmortal -= price;
        UpdateText();
        if (price > 0)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Coin);
        }
    }

    //уменьшаем количество жизней
    public bool SubLive() {

        //если бессмертие, то не уменьшаем жизни
        if (EndTimeImmortal > CheckTime.Realtime())
        {
            return true;
        }
        else if (life > 0)
        {
            if (life == maxLife)
            {
                timeToNextLife = CheckTime.Realtime().AddMinutes(timeToGetOneLife);
            }
            life--;
            RecordSave();
            UpdateText();
            return true;
        }
        else
        {
            return false;
        }
    }

    private void PlusLive() {
        if (life < maxLife)
        {
            life++;                    
            UpdateText();            
        }
    }

    //добавление времени бессмертия из бандла
    public bool addTimeImmortal(int time)
    {
        if (EndTimeImmortal < CheckTime.Realtime())
        {
            endTimeImmortal = CheckTime.Realtime();
        }
        addMinutesTimeImmortal += time;
        if (EndTimeImmortal > CheckTime.Realtime())
        {
            life = maxLife;
        }
        RecordSave();
        return true;
    }

    //вознаграждение за просмотр рекламы
    public void AddLifeForViewingAds(Reward args)
    {
        if (args.Amount > 0 && life < maxLife)
        {
            PlusLive();
            RecordSave();
            //звук добавления
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.AddMove);
            //эффект                
            ParticleSystemManager.Instance.CreateCollectAllEffect(this.transform, imageLive);            
        }
    }

    private void LoadSave()
    {
        LifeSave lifeSave = JsonSaveAndLoad.LoadSave().lifeSave;
        DateTime dateTimeToNextLifeLong = DateTime.FromFileTimeUtc(lifeSave.timeToNextLifeLong);
        DateTime dateTimeEndTimeImmortal = DateTime.FromFileTimeUtc(lifeSave.endTimeImmortalLong);
        if (lifeSave.life == 0 && dateTimeToNextLifeLong == null)
        {
            life = maxLife;
            timeToNextLife = CheckTime.Realtime().AddMinutes(timeToGetOneLife);
            endTimeImmortal = new DateTime();
        }
        else
        {
            life = lifeSave.life > maxLife ? maxLife : lifeSave.life;
            timeToNextLife = dateTimeToNextLifeLong;
            endTimeImmortal = dateTimeEndTimeImmortal;
            if (endTimeImmortal > CheckTime.Realtime())
            {
                life = maxLife;
            }
        }
    }

    private void RecordSave()
    {
        JsonSaveAndLoad.RecordSave(this);
        JsonSaveAndLoad.SetSaveToFile();
    }

    private void UpdateText() {

        if (endTimeImmortal > CheckTime.Realtime())
        {
            TimeSpan dateTime = endTimeImmortal.Subtract(CheckTime.Realtime());
            textLive.text = (char)8734 + " (" + dateTime.Minutes + "m " + dateTime.Seconds + "s)";
        }
        else
        {
            string text = "" + life;
            if (life < maxLife)
            {
                TimeSpan dateTime = timeToNextLife.Subtract(CheckTime.Realtime());
                //int minutes = (int)(timeToNextLife - Time.realtimeSinceStartup) / 60;
                //int seconds = (int)(timeToNextLife - Time.realtimeSinceStartup - (minutes * 60));
                text += " (" + dateTime.Minutes + "m " + dateTime.Seconds + "s)";
            }

            textLive.text = text;
        }

        
    }
}
