using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SettingsController
{
    private static SettingsSave settingsSave;
    
    private static void LoadSave()
    {
        if (settingsSave == null)
        {
            settingsSave = JsonSaveAndLoad.LoadSave().settingsSave;
        }        
    }

    private static void RecordSave()
    {
        JsonSaveAndLoad.RecordSave(settingsSave);
    }
    
    //сбрасываем настройки
    public static void DeleteSettings()
    {
        //удаляем сохранения
        JsonSaveAndLoad.DeleteSettingsSave();
        settingsSave = null;
        LoadSave();
    }

    //работа с подсказками
    public static bool ShowHints {
        get
        {
            LoadSave();
            return settingsSave.showHints;
        }
        set
        {
            LoadSave();
            settingsSave.showHints = value;
            RecordSave();
        }
    }

    //работа со звуком
    public static bool SoundOn
    {
        get
        {
            LoadSave();
            return settingsSave.sound;
        }
        set
        {
            LoadSave();
            settingsSave.sound = value;
            SoundManager.Instance.SoundMute(!settingsSave.sound);
            RecordSave();
        }
    }
}
