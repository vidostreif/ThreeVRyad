using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneSettings : MonoBehaviour
{
    private bool setingsHidden = true;

    private Transform buttonExit;
    private Transform buttonSound;
    private Transform buttonRestart;
    private Transform buttonSetings;

    void Awake()
    {
        buttonSound = transform.Find("ButtonSound");
        buttonExit = transform.Find("ButtonExit");
        buttonRestart = transform.Find("ButtonRestart");
        buttonSetings = transform.Find("ButtonSetings");
    }

    public void RestartScene() {
        HideOrShowSetings(false);
        MainSceneScript.Instance.RestartLevel();
    }

    public void ExitScene()
    {
        HideOrShowSetings(false);
        MainSceneScript.Instance.ExitToMenu();
    }

    public void SoundSwitch()
    {
        //!!!включение-выключение звука

    }

    //показываем или скрываем кнопки
    public void SetingsSwitch()
    {          
        if (setingsHidden)
        {
            HideOrShowSetings(true);
        }
        else
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

        RectTransform rectbuttonRestart = buttonRestart.GetComponent<RectTransform>();
        rectbuttonRestart.anchoredPosition = new Vector2(startAnchordPosition.x - offset * rectbuttonRestart.rect.width * 0.95f, startAnchordPosition.y + offset * rectbuttonRestart.rect.height * 0.95f);
        SupportFunctions.ChangeAlfa(buttonRestart.GetComponent<Image>(), offset);
        buttonRestart.GetComponent<Button>().interactable = !setingsHidden;
    }
}
