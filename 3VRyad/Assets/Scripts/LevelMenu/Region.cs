using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Region
{
    [HideInInspector]public string name;
    public string fullName;
    public Sprite sprite;
    [SerializeField] public List<Level> levelList = new List<Level>();    
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

    public void AddAction(GameObject elementGameObject)
    {
        //добавляем действие к кнопке
        Button = elementGameObject.GetComponent(typeof(Button)) as Button;
        Button.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
        Button.onClick.AddListener(delegate { Action(); });
    }

    //действие при нажатии
    public void Action()
    {
        Debug.Log(text);
        LevelMenu.Instance.CreateLevelMenu(this);
    }

    //обнолвление текста
    private void UpdateText()
    {
        text.text = "" + text;
    }
}
