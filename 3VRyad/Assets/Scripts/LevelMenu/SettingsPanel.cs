using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//панель настроек
public class SettingsPanel : MonoBehaviour
{

    Image banHintsImage; //картинка запрета подсказок
    Image banSoundImage; //картинка запрета звука
    //SettingsSave settingsSave;
    // Start is called before the first frame update
    void Start()
    {
        LoadSettingSaves();
    }

    private void LoadSettingSaves() {

        Transform panelSetings = transform.Find("PanelSetings");
        //отображаем значения
        Toggle showHintsToggle = panelSetings.Find("ToggleShowHints").GetComponent<Toggle>();
        showHintsToggle.onValueChanged.RemoveAllListeners();
        showHintsToggle.isOn = SettingsController.ShowHints;        
        showHintsToggle.onValueChanged.AddListener(delegate { ChangeShowHints(showHintsToggle.isOn); });
        
        banHintsImage = showHintsToggle.transform.Find("BanImage").GetComponent<Image>();
        HideImage(SettingsController.ShowHints, banHintsImage);

        Toggle soundToggle = panelSetings.Find("ToggleSound").GetComponent<Toggle>();
        soundToggle.onValueChanged.RemoveAllListeners();
        soundToggle.isOn = SettingsController.SoundOn;
        soundToggle.onValueChanged.AddListener(delegate { ChangeSound(soundToggle.isOn); });

        banSoundImage = soundToggle.transform.Find("BanImage").GetComponent<Image>();
        HideImage(SettingsController.SoundOn, banSoundImage);
    }

    //закрытие панели настроек
    public void CloseSettingsPanel() {
        Destroy(this.gameObject);
    }

    //удаление сохранений
    public void CreateQuestionDeleteSaves()
    {
        SoundManager.Instance.PlayClickButtonSound();
        //создаем запрос на удаление сохранений
        string text = "Вы действительно хотите удалить все сохранения?";
        Action actionYes = delegate
        {
            CreateQuestionDeleteHints(DeleteSaves);
        };
        SupportFunctions.CreateYesNoPanel(LevelMenu.Instance.transform, text, actionYes);
    }

    //запрос на удаление статусов подсказок. На вход может принимать дополнительное действие, которое будет выполняться после
    public void CreateQuestionDeleteHints(Action afterAction = null)
    {
        //запрос на удаление статусов подсказок
        string text = "Показать все подсказки заново?";
        Action actionYes = delegate
        {
            //удаляем статусы подсказок
            JsonSaveAndLoad.DeleteHintSave();
            afterAction();
        };
        Action actionNo = delegate
        {
            afterAction();
        };
        SupportFunctions.CreateYesNoPanel(LevelMenu.Instance.transform, text, actionYes, actionNo);
    }

    public void CreateQuestionDeleteSettings()
    {
        //создаем запрос на удаление сохранений
        string text = "Вы действительно хотите сбросить все настойки по умолчанию?";
        Action actionYes = delegate
        {
            SettingsController.DeleteSettings();
        };
        SupportFunctions.CreateYesNoPanel(LevelMenu.Instance.transform, text, actionYes);
    }

    //удаляем сохранения
    public static void DeleteSaves()
    {
        SoundManager.Instance.PlayClickButtonSound();
        //удаляем сохранения
        JsonSaveAndLoad.DeleteSave();

        //перезапускаем сцену
        DontDestroyOnLoadManager.DestroyAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //переключение значения показывать подсказки
    public void ChangeShowHints(bool isOn)
    {
        //LoadSettingSaves();
        SoundManager.Instance.PlayClickButtonSound();
        SettingsController.ShowHints = isOn;
        HideImage(isOn, banHintsImage);
    }

    //переключение значения звука
    public void ChangeSound(bool isOn)
    {
        SoundManager.Instance.PlayClickButtonSound();
        SettingsController.SoundOn = isOn;
        HideImage(isOn, banSoundImage);
    }

    //скрыть отобразить картинку
    private void HideImage(bool show, Image image) {

        int i = 1;
        if (show)
        {
            i = 0;
        }
        SupportFunctions.ChangeAlfa(image, i);
    }
}
