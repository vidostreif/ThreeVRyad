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
    private DateTime timeToNextLife; //время получения следующей жизни
    private DateTime endTimeImmortal; //время окончания бессметрия
    private int addMinutesTimeImmortal = 0;
    private float LastArrayProcessingTime = 0;
    private bool giftFirstImmortalityIssued;//подарок - первое бессмертие выдано

    private Transform thisTransform;
    private Transform startParent;
    private Animation thisAnimation;
    private Text textLive;
    private Text textLiveTime;
    private Image imageLive;

    public int Life { get => life; }
    public Transform StartParent { get => startParent; }
    public Transform ThisTransform { get => thisTransform; }
    public DateTime TimeToNextLife { get => timeToNextLife; }
    public DateTime EndTimeImmortal {
        get {
            return endTimeImmortal.AddMinutes(addMinutesTimeImmortal);
        }
    }

    public bool GiftFirstImmortalityIssued { get => giftFirstImmortalityIssued;}

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
        textLive = transform.Find("TextLive").GetComponent<Text>();
        textLiveTime = transform.Find("TextLiveTime").GetComponent<Text>();        
        imageLive = transform.Find("ImageLive").GetComponent<Image>();
        thisAnimation = transform.Find("ImageLive").GetComponent<Animation>();
        thisTransform = transform;
        startParent = thisTransform.parent;
        LoadSave();
        StartCalculatingLives();
        UpdateText();
    }

    private void Start()
    {
        //startPosition = thisTransform.position;
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
                AddLive();
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
                AddLive();
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
        endTimeImmortal = endTimeImmortal.AddMinutes(price);
        addMinutesTimeImmortal -= price;

        UpdateText();
        if (price > 0 && !thisAnimation.IsPlaying("Life_add"))
        {            
            thisAnimation.Play("Life_add");
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Life_add);
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
            thisAnimation.Play("Life_sub");
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Life_sub);
            RecordSave();
            UpdateText();
            if (life == 0 && !giftFirstImmortalityIssued)
            {
                GameObject informationPanelGO = SupportFunctions.CreateInformationPanel("У тебя закончились жизни! В дальнейшем ты сможешь получить бессмертие из подарков или купить в магазине. А пока прими от меня подарочек!");
                addTimeImmortal(60, informationPanelGO.transform, new Vector3(0, 1.5f, 0), new Vector3(0, 2.5f, 0));
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AddLive() {
        if (life < maxLife)
        {
            life++;
            thisAnimation.Play("Life_add");
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Life_add);
            UpdateText();            
        }
    }

    //добавление времени бессмертия
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

    //добавление времени бессмертия с доп параметрами
    public void addTimeImmortal(int time, Transform curTransform, Vector3 position, Vector3 newPosition)
    {
        addTimeImmortal(time);
        StartCoroutine(Shop.Instance.CreateLivesAnimation(position, curTransform, time, newPosition: newPosition));
    }

    public bool Immortal()
    {
        if (EndTimeImmortal > CheckTime.Realtime())
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    //вознаграждение за просмотр рекламы
    public void AddLifeForViewingAds(Reward args)
    {
        if (args.Amount > 0 && life < maxLife)
        {
            AddLive();
            RecordSave();
            //эффект                
            ParticleSystemManager.Instance.CreateCollectAllEffect(imageLive.transform, SpriteBank.SetShape(SpritesEnum.Life, true));            
        }
    }

    private void LoadSave()
    {
        LifeSave lifeSave = JsonSaveAndLoad.LoadSave().lifeSave;
        giftFirstImmortalityIssued = lifeSave.giftFirstImmortalityIssued;
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
            textLive.text = "" + (char)8734;
            TimeSpan dateTime = endTimeImmortal.Subtract(CheckTime.Realtime());
            textLiveTime.text = SupportFunctions.GetStringTime(dateTime);
        }
        else
        {
            textLive.text = "" + life;
            if (life < maxLife)
            {
                TimeSpan dateTime = timeToNextLife.Subtract(CheckTime.Realtime());
                textLiveTime.text = SupportFunctions.GetStringTime(dateTime);
            }
            else
            {
                textLiveTime.text = "ВСЕ";
            }
        }

        
    }
}
