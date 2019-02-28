using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Instrument
{
    private Image image;
    public InstrumentsEnum elementsType;//какой вид элементов собераем
    private Text text;
    private Button button;
    public int number;
    [SerializeField] private int quantity;//доступное количество

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

    public void AddAction(GameObject elementGameObject) {
        //добавляем действие к кнопке
        Button = elementGameObject.GetComponent(typeof(Button)) as Button;
        Button.onClick.AddListener(delegate { Action(); });
    }

    //действие при нажатии
    public void Action() {
        if (quantity > 0)
        {
            Debug.Log(elementsType);
            InstrumentsManager.Instance.PreparInstrument(this);
        }
        
    }
    //обнолвление текста
    private void UpdateText()
    {
        text.text = "" + quantity;
    }

    public void SubQuantity()
    {
            quantity--;
            UpdateText();
    }
}
