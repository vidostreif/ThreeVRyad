using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//панель настроек
public class SettingsPanel : MonoBehaviour
{
    SettingsSave settingsSave;
    // Start is called before the first frame update
    void Start()
    {
        LoadSettingSaves();
    }

    private void LoadSettingSaves() {
        //загружаем настройки
        settingsSave = JsonSaveAndLoad.LoadSave().SettingsSave;

        //отображаем значения
        Toggle showHintsToggle = transform.Find("ToggleShowHints").GetComponent<Toggle>();
        showHintsToggle.onValueChanged.RemoveAllListeners();
        showHintsToggle.isOn = settingsSave.showHints;        
        showHintsToggle.onValueChanged.AddListener(delegate { ChangeShowHints(showHintsToggle.isOn); });

        Toggle soundToggle = transform.Find("ToggleSound").GetComponent<Toggle>();
        soundToggle.onValueChanged.RemoveAllListeners();
        soundToggle.isOn = settingsSave.sound;
        soundToggle.onValueChanged.AddListener(delegate { ChangeSound(soundToggle.isOn); });
    }

    // Update is called once per frame
    void Update()
    {
        
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
            DeleteSettings();
        };
        SupportFunctions.CreateYesNoPanel(LevelMenu.Instance.transform, text, actionYes);
    }

    //удаляем сохранения
    public void DeleteSaves()
    {
        //удаляем сохранения
        JsonSaveAndLoad.DeleteSave();
        
        //перезапускаем сцену
        DontDestroyOnLoadManager.DestroyAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        //LevelMenu.Instance.Prepare();
        //Component[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(MonoBehaviour)) as Component[];
        //foreach (Component item in findeObjects)
        //{
        //    GameObject gameObject = item.gameObject;

        //    Destroy(item);
        //    gameObject.AddComponent<item.ToString()>();
        //}

        //LevelMenu.Instance.CreateRegionMenu();
    }

    //сбрасываем настройки
    public void DeleteSettings()
    {
        //удаляем сохранения
        JsonSaveAndLoad.DeleteSettingsSave();
        LoadSettingSaves();
    }

    //переключение значения показывать подсказки
    public void ChangeShowHints(bool isOn)
    {
        settingsSave.showHints = isOn;
        JsonSaveAndLoad.RecordSave(settingsSave);
    }

    //переключение значения звука
    public void ChangeSound(bool isOn)
    {
        settingsSave.sound = isOn;
        JsonSaveAndLoad.RecordSave(settingsSave);
    }
}
