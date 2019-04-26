using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Instrument
{
    [SerializeField] private InstrumentsEnum type;//какой вид инструмента
    private int quantity;//доступное количество
    [SerializeField] private bool allow;//разрешен на уровне
    
    private GameObject pSSelect;//наложенный эффект выделения
    private InstrumentButton gameInstrumentButton; //кнопка инструмента в игре
    private InstrumentButton shopInstrumentButton; //кнопка инструмента в магазине

    public InstrumentButton GameInstrumentButton
    {
        get
        {
            return gameInstrumentButton;
        }
    }
    public InstrumentButton ShopInstrumentButton
    {
        get
        {
            return shopInstrumentButton;
        }
    }
    public int Quantity
    {
        get
        {
            return quantity;
        }
    }
    public InstrumentsEnum Type
    {
        get
        {
            return type;
        }
    }
    public GameObject PSSelect
    {
        get
        {
            return pSSelect;
        }

        set
        {
            pSSelect = value;
        }
    }
    public bool Allow
    {
        get
        {
            return allow;
        }

        set
        {
            allow = value;
        }
    }

    public void CreateGameInstrumentButton(GameObject go)
    {
        if (gameInstrumentButton != null)
        {
            GameObject.Destroy(gameInstrumentButton.GameObject);
        }
        gameInstrumentButton = new InstrumentButton(go, type);        
        UpdateText();

        //если разрешен на уровне
        if (Allow)
        {
            AddOnGameAction(gameInstrumentButton);
        }
        else
        {
            SupportFunctions.ChangeAlfa(gameInstrumentButton.Image, 0.2f);

            //!!!сверху повесить замок и действие открытия магазина
        }
    }

    public void CreateShopInstrumentButton(GameObject go)
    {
        if (shopInstrumentButton != null)
        {
            GameObject.Destroy(shopInstrumentButton.GameObject);
        }
        shopInstrumentButton = new InstrumentButton(go, type);
        UpdateText();

        AddOnShopAction(gameInstrumentButton);
    }

    public void AddOnGameAction(InstrumentButton InstrumentButton) {
        //добавляем действие к кнопке
        InstrumentButton.Button = InstrumentButton.GameObject.GetComponent(typeof(Button)) as Button;
        InstrumentButton.Button.onClick.AddListener(delegate { ActionOnGame(); });
    }

    public void AddOnShopAction(InstrumentButton InstrumentButton)
    {
        //добавляем действие к кнопке
        InstrumentButton.Button = InstrumentButton.GameObject.GetComponent(typeof(Button)) as Button;
        InstrumentButton.Button.onClick.AddListener(delegate { ActionOnShop(); });
    }

    //действие при нажатии
    public void ActionOnGame() {
        if (quantity > 0)
        {
            Debug.Log(Type);
            InstrumentsManager.Instance.PreparInstrument(this);
        }        
    }

    public void ActionOnShop()
    {
        
    }

    //обнолвление текста
    private void UpdateText()
    {
        //для игрового отображения
        if (allow && gameInstrumentButton != null)
        {
            gameInstrumentButton.UpdateText("" + quantity);
        }
        else if (gameInstrumentButton != null)
        {
            gameInstrumentButton.UpdateText("");
        }

        //для отображения в магазине
        if (shopInstrumentButton != null)
        {
            shopInstrumentButton.UpdateText("" + quantity);
        }
    }

    public void SubQuantity(int count = 1)
    {
        if (count <= quantity)
        {
            quantity -= count;            
        }
        else
        {
            quantity = 0;
        }
        UpdateText();
    }

    public void AddQuantity(int count = 1)
    {
        quantity += count;
        UpdateText();
    }
}

public class InstrumentButton
{
    private GameObject gameObject;//объект инструмента
    private Image image;
    private Text text;
    private Button button;

    public InstrumentButton(GameObject go, InstrumentsEnum type)
    {
        this.GameObject = go;
        Image image = GameObject.GetComponent(typeof(Image)) as Image;
        image.sprite = SpriteBank.SetShape(type);
        Image = image;
        Text = image.GetComponentInChildren<Text>();
    }

    public GameObject GameObject
    {
        get
        {
            return gameObject;
        }

        set
        {
            gameObject = value;
        }
    }
    public Text Text
    {
        get
        {
            return text;
        }

        set
        {
            text = value;
            //UpdateText();
        }
    }
    public Image Image
    {
        get
        {
            return image;
        }

        set
        {
            image = value;
        }
    }
    public Button Button
    {
        get
        {
            return button;
        }

        set
        {
            button = value;
        }
    }

    //обнолвление текста
    public void UpdateText(string str)
    {
        if (text != null)
        {
            text.text = str;
        }            
    }


}
