using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneSettings : MonoBehaviour
{
    public static SceneSettings Instance; // Синглтон
    private bool setingsHidden = true;

    private Transform buttonExit;
    private Transform buttonSound;
    private Transform buttonRestart;
    private Transform buttonSetings;
    Image banSoundImage; //картинка запрета звука

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров SceneSettings!");
        }
        Instance = this;

        buttonSound = transform.Find("ButtonSound");        
        buttonExit = transform.Find("ButtonExit");
        buttonRestart = transform.Find("ButtonRestart");
        buttonSetings = transform.Find("ButtonSetings");
        banSoundImage = buttonSound.Find("BanImage").GetComponent<Image>();
    }

    public void RestartScene() {
        SoundManager.Instance.PlaySoundInternal(SoundsEnum.ClickButton);
        HideOrShowSetings(false);
        MainGameSceneScript.Instance.RequestRestartLevel();
        HideSetings();
    }

    public void ExitScene()
    {
        SoundManager.Instance.PlaySoundInternal(SoundsEnum.ClickButton);
        HideOrShowSetings(false);
        MainGameSceneScript.Instance.RequestExitToMenu();
    }

    public void SoundSwitch()
    {
        SoundManager.Instance.PlaySoundInternal(SoundsEnum.ClickButton);
        //включение-выключение звука
        if (!SettingsController.SoundOn)
        {          
            SettingsController.SoundOn = true;
        }
        else
        {
            SettingsController.SoundOn = false;
        }

        //показываем или скрываем закрывающую картинку
        HideImage(SettingsController.SoundOn, banSoundImage);
    }

    //показываем или скрываем кнопки
    public void SetingsSwitch()
    {
        SoundManager.Instance.PlaySoundInternal(SoundsEnum.ClickButton);
        if (setingsHidden)
        {
            HideOrShowSetings(true);
        }
        else
        {
            HideOrShowSetings(false);
        }                  
    }

    public void HideSetings() {
        if (!setingsHidden)
        {
            HideOrShowSetings(false);
        }
    }

    private void HideOrShowSetings(bool show) {

        Vector2 startAnchordPosition = buttonSetings.GetComponent<RectTransform>().anchoredPosition;
        float offset;
        if (show)
        {
            offset = 1;
            setingsHidden = false;
        }
        else
        {
            offset = 0;
            setingsHidden = true;
        }

        RectTransform rectButtonExit = buttonExit.GetComponent<RectTransform>();
        rectButtonExit.anchoredPosition = new Vector2(startAnchordPosition.x - offset * rectButtonExit.rect.width * 1.2f, startAnchordPosition.y);
        SupportFunctions.ChangeAlfa(buttonExit.GetComponent<Image>(), offset);
        buttonExit.GetComponent<Button>().interactable = !setingsHidden;

        RectTransform rectButtonSound = buttonSound.GetComponent<RectTransform>();
        rectButtonSound.anchoredPosition = new Vector2(startAnchordPosition.x, startAnchordPosition.y + offset * rectButtonSound.rect.height * 1.2f);
        SupportFunctions.ChangeAlfa(buttonSound.GetComponent<Image>(), offset);
        buttonSound.GetComponent<Button>().interactable = !setingsHidden;

        //отображаем картинку выключения звука        
        if (!setingsHidden)
        {
            HideImage(SettingsController.SoundOn, banSoundImage);
        }
        else
        {
            HideImage(true, banSoundImage);
        }        

        RectTransform rectbuttonRestart = buttonRestart.GetComponent<RectTransform>();
        rectbuttonRestart.anchoredPosition = new Vector2(startAnchordPosition.x - offset * rectbuttonRestart.rect.width * 0.95f, startAnchordPosition.y + offset * rectbuttonRestart.rect.height * 0.95f);
        SupportFunctions.ChangeAlfa(buttonRestart.GetComponent<Image>(), offset);
        buttonRestart.GetComponent<Button>().interactable = !setingsHidden;
    }

    //скрыть отобразить картинку
    private void HideImage(bool show, Image image)
    {
        int i = 1;
        if (show)
        {
            i = 0;
        }
        SupportFunctions.ChangeAlfa(image, i);
    }
}
