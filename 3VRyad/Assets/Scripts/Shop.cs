using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public static Shop Instance; // Синглтон
    private int coins = 0; //валюта магазина
    private int exchangeRate = 3; //курс обмена звезд на коины
    private Text textCoins;

    public int Coins
    {
        get
        {
            return coins;
        }
    }

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
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Save save = JsonSaveAndLoad.LoadSave();
        coins = save.shopSave.coins;

        Transform gOTextCoins = transform.Find("TextCoins");
        textCoins = gOTextCoins.GetComponent(typeof(Text)) as Text;
        UpdateTextCoins();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExchangeStarsForCoins(Level level, int stars) {

        if (level.Stars < stars )
        {
            coins += (stars - level.Stars) * exchangeRate;
            JsonSaveAndLoad.RecordSave(this);
            UpdateTextCoins();
        }
    }

    //обнолвление текста
    private void UpdateTextCoins()
    {
        textCoins.text = coins.ToString();
    }
}
