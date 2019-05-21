using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//панель настроек
public class SettingsPanel : MonoBehaviour
{
    //SettingsSave settingsSave;
    // Start is called before the first frame update
    void Start()
    {
        LoadSettingSaves();
    }

    private void LoadSettingSaves() {

        //отображаем значения
        Toggle showHintsToggle = transform.Find("ToggleShowHints").GetComponent<Toggle>();
        showHintsToggle.onValueChanged.RemoveAllListeners();
        showHintsToggle.isOn = SettingsController.ShowHints;        
        showHintsToggle.onValueChanged.AddListener(delegate { ChangeShowHints(showHintsToggle.isOn); });

        Toggle soundToggle = transform.Find("ToggleSound").GetComponent<Toggle>();
        soundToggle.onValueChanged.RemoveAllListeners();
        soundToggle.isOn = SettingsController.SoundOn;
        soundToggle.onValueChanged.AddListener(delegate { ChangeSound(soundToggle.isOn); });
    }

    //закрытие панели настроек
    public void CloseSettingsPanel() {
        Destroy(this.gameObject);
    }

    //удаление сохранений
    public void CreateQuestionDeleteSaves()
    {
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
        //удаляем сохранения
        JsonSaveAndLoad.DeleteSave();

        //перезапускаем сцену
        DontDestroyOnLoadManager.DestroyAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //переключение значения показывать подсказки
    public void ChangeShowHints(bool isOn)
    {
        LoadSettingSaves();
        SettingsController.ShowHints = isOn;
    }

    //переключение значения звука
    public void ChangeSound(bool isOn)
    {
        SettingsController.SoundOn = isOn;
    }
}
