using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Instrument
{
    [SerializeField] private InstrumentsEnum type;//какой вид инструмента
    [SerializeField] private int quantity;//доступное количество
    [SerializeField] private bool allow;//разрешен на уровне
    private GameObject gameObject;//объект инструмента
    private GameObject pSSelect;//наложенный эффект выделения
    private Image image;
    private Text text;
    private Button button;

    public Text Text
    {
        get
        {
            return text;
        }

        set
        {
            text = value;
            UpdateText();
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
    public InstrumentsEnum Type
    {
        get
        {
            return type;
        }
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

    public void AddAction(GameObject elementGameObject) {
        //добавляем действие к кнопке
        Button = elementGameObject.GetComponent(typeof(Button)) as Button;
        Button.onClick.AddListener(delegate { Action(); });
    }

    //действие при нажатии
    public void Action() {
        if (quantity > 0)
        {
            Debug.Log(Type);
            InstrumentsManager.Instance.PreparInstrument(this);
        }
        
    }

    //обнолвление текста
    private void UpdateText()
    {
        if (allow)
        {
            text.text = "" + quantity;
        }
        else
        {
            text.text = "";
        }
        
    }

    public void SubQuantity()
    {
            quantity--;
            UpdateText();
    }
}
