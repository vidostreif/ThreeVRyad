﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using UnityEngine.SceneManagement;

public static class JsonSaveAndLoad
{
    private static Save save;
    private static bool saveIsChanged = false;
    private static bool saveFromFileLoad = false;//попытка загрузки из файла произведена
    private static string path;

    //// описание события
    //public static event ReloadSaveHandler ReloadSave;
    //// делегат для подписывающихся на событие обработчиков
    //public delegate void ReloadSaveHandler(EventArgs eventArgs);

    private static void LoadSaveFromFile()
    {
        if (!saveFromFileLoad)
        {
            if (save == null)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
            path = Path.Combine(Application.persistentDataPath, "Save.json");            
#else
                path = Path.Combine(Application.dataPath, "Save.json");
#endif
                if (File.Exists(path))
                {
                    try
                    {
                        save = JsonUtility.FromJson<Save>(VEncryption.Decrypt(File.ReadAllText(path)));
                        Debug.Log("Загрузка сохранения.");
                    }
                    catch (Exception)
                    {
                        Debug.Log("Загрузить сохранение из локального хранилища не удалось.");
                    }

                }
                else
                {
                    Debug.Log("Загрузить сохранение из локального хранилища не удалось.");
                }
            }

            ////загружаем данные из гугл сервиса

            ////!!! нужно дождаться окончания этой процедуры
            //GPGSManager.Auth((success) =>
            //{
            //    if (success)
            //    {
            //        GPGSManager.ReadSaveData(GPGSManager.DEFAULT_SAVE_NAME, (status, data) =>
            //        {
            //            if (status == GooglePlayGames.BasicApi.SavedGame.SavedGameRequestStatus.Success && data.Length > 0)
            //            {
            //                try
            //                {
            //                    save = JsonUtility.FromJson<Save>(VEncryption.Decrypt(Encoding.UTF8.GetString(data, 0, data.Length)));
            //                    //saveFromFileLoad = true;
            //                    Debug.Log("Загрузка сохранения из google.");
            //                }
            //                catch (Exception)
            //                {
            //                    Debug.Log("Загрузка сохранения из google не удалась.");
            //                }
            //            }
            //            else
            //            {
            //                Debug.Log("Загрузка сохранения из google не удалась.");
            //            }
            //        });
            //    }
            //    else
            //    {
            //        Debug.Log("Загрузка сохранения из google не удалась.");
            //    }                
            //});

            if (save == null)
            {
                Debug.Log("Создается новое сохранение");
                save = new Save();
            }
            saveFromFileLoad = true;

            //ReloadSettings();//перезагружаем настройки для классов
        }
    }

    private static void SetSaveToGPGS(string saveString)
    {
            //сохраняем данные в гугл сервис
            //GPGSManager.WriteSaveData(Encoding.UTF8.GetBytes(saveString));
    }

    public static void SetSaveToFile()
    {
        if (saveIsChanged)
        {
            string saveString = VEncryption.Encrypt(JsonUtility.ToJson(save));
            File.WriteAllText(path, saveString);
            //SetSaveToGPGS(saveString);
            saveIsChanged = false;
        }
    }

    public static void DeleteSave()
    {
        Debug.Log("Удаляем сохранения!");
        LoadSaveFromFile();
        //сохраняем все показанные подсказки и параметры настроек, остальное удаляем
        Save saveForSave = new Save();
        saveForSave.helpSave = save.helpSave;
        saveForSave.settingsSave = save.settingsSave;
        save = saveForSave;
        saveIsChanged = true;
        SetSaveToFile();
    }

    public static void DeleteHintSave()
    {
        Debug.Log("Удаляем статусы подсказок! Подсказки будут показаны заново.");
        LoadSaveFromFile();
        save.helpSave = new List<HelpSave>();
        HelpToPlayer.ReloadSave();
        saveIsChanged = true;
        SetSaveToFile();
    }

    //Сбросить настройки по умолчанию
    public static void DeleteSettingsSave()
    {
        Debug.Log("Сбрасываем настройки.");
        LoadSaveFromFile();
        save.settingsSave = GetStandartSettings();
        saveIsChanged = true;
        SetSaveToFile();
    }

    //загрузка сохранений уровней
    public static Save LoadSave()
    {
        LoadSaveFromFile();
        return save;
    }

    //запись сохранений уровней
    public static void RecordSave(List<Level> levelList, int region)
    {
        LoadSaveFromFile();
        //если в нашем сохранении недостаточно регионов
        if (save.regionSave.Count < region + 1)
        {
            //добавляем элементы в массив
            int regionSaveCount = save.regionSave.Count;
            for (int i = 0; i < region - regionSaveCount + 1; i++)
            {
                save.regionSave.Add(new RegionSave());
            }
        }
        //очищаем перед записью
        save.regionSave[region].levelSave.Clear();
        //записываем новые данные
        foreach (Level level in levelList)
        {
            save.regionSave[region].levelSave.Add(new LevelSave(level.Open, level.Passed, level.GiftIssued, level.Stars, level.Score));
        }

        saveIsChanged = true;
    }

    //запись сохранений магазина
    public static void RecordSave(Shop shop)
    {
        LoadSaveFromFile();
        save.shopSave.coins = shop.Coins;
        saveIsChanged = true;
    }

    //запись сохранений жизней
    public static void RecordSave(LifeManager lifeManager)
    {
        LoadSaveFromFile();
        save.lifeSave.life = lifeManager.Life;
        save.lifeSave.giftFirstImmortalityIssued = lifeManager.GiftFirstImmortalityIssued;
        save.lifeSave.timeToNextLifeLong = lifeManager.TimeToNextLife.ToFileTimeUtc();
        save.lifeSave.endTimeImmortalLong = lifeManager.EndTimeImmortal.ToFileTimeUtc();
        saveIsChanged = true;
    }

    //запись сохранений ежедневного подарка
    public static void RecordSave(DailyGiftManager dailyGiftManager)
    {
        LoadSaveFromFile();
        save.dailyGiftSave.lastGiftTimeIssuedLong = dailyGiftManager.LastGiftTimeIssued.ToFileTimeUtc();
        save.dailyGiftSave.numberOfGiftsIssuedToday = dailyGiftManager.NumberOfGiftsIssuedToday;     
        saveIsChanged = true;
    }

    //запись показанных подсказок
    public static void RecordSave(HintStatus[] hintsStatus)
    {
        LoadSaveFromFile();

        //очищаем перед записью
        save.helpSave.Clear();
        //записываем новые данные
        foreach (HintStatus hintStatus in hintsStatus)
        {
            save.helpSave.Add(new HelpSave(hintStatus.help.ToString(), hintStatus.status));
        }

        saveIsChanged = true;
    }

    //запись количества инструментов
    public static void RecordSave(Thing[] instruments)
    {
        LoadSaveFromFile();

        //очищаем перед записью
        save.instrumentsSave.Clear();
        //записываем новые данные
        foreach (Thing instrument in instruments)
        {
            save.instrumentsSave.Add(new InstrumentsSave(instrument.Type.ToString(), instrument.Quantity));
        }

        saveIsChanged = true;
    }

    //запись настроек
    public static void RecordSave(SettingsSave settings)
    {
        LoadSaveFromFile();
        //записываем новые данные
        save.settingsSave = settings;
        //сразу сохраняем в файл
        saveIsChanged = true;
        SetSaveToFile();

        //ReloadSettings();//перезагружаем настройки для классов
    }

    //получить значение настроек по умолчанию
    public static SettingsSave GetStandartSettings()
    {
        return new SettingsSave(true, true);
    }

    //обучение пройдено
    public static void TrainingCompleted() {
        LoadSaveFromFile();
        if (!save.trainingCompleted)
        {
            save.trainingCompleted = true;
            Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventTutorialComplete);
            //сразу сохраняем в файл
            saveIsChanged = true;
            SetSaveToFile();
        }
    }

    //отзыв оставлен
    public static void ReviewWritten()
    {
        LoadSaveFromFile();
        if (!save.reviewWritten)
        {
            save.reviewWritten = true;
            //Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.);
            //сразу сохраняем в файл
            saveIsChanged = true;
            SetSaveToFile();
        }
    }
}

[Serializable]
public class Save
{
    public List<RegionSave> regionSave = new List<RegionSave>();
    public ShopSave shopSave = new ShopSave();
    public LifeSave lifeSave = new LifeSave();
    public DailyGiftSave dailyGiftSave = new DailyGiftSave();
    public List<HelpSave> helpSave = new List<HelpSave>();
    public List<InstrumentsSave> instrumentsSave = new List<InstrumentsSave>();
    public SettingsSave settingsSave = JsonSaveAndLoad.GetStandartSettings();
    public bool trainingCompleted = false;
    public bool reviewWritten = false;
}

[Serializable]
public class RegionSave
{
    public List<LevelSave> levelSave = new List<LevelSave>();
}

[Serializable]
public class LevelSave
{
    //public int region;
    //public int level;
    public bool open;//открыт
    public bool passed;//пройден
    public bool giftIssued;//есть подарок
    public int stars = 0;//количество звезд
    public int score = 0;//количество очков

    public LevelSave(bool open, bool passed, bool giftIssued, int stars, int score)
    {
        this.open = open;
        this.passed = passed;
        this.giftIssued = giftIssued;
        this.stars = stars;
        this.score = score;
    }
}

[Serializable]
public class ShopSave
{
    public int coins = 0;//количество монет
}

[Serializable]
public class LifeSave
{
    public int life = 0;
    public bool giftFirstImmortalityIssued;
    public long timeToNextLifeLong;
    public long endTimeImmortalLong;
}

[Serializable]
public class DailyGiftSave
{
    public long lastGiftTimeIssuedLong;
    public int numberOfGiftsIssuedToday = 0;
}

[Serializable]
public class HelpSave
{
    public string elementsTypeEnum;//тип
    public bool status;//показана

    public HelpSave(string elementsTypeEnum, bool status)
    {
        this.elementsTypeEnum = elementsTypeEnum;
        this.status = status;
    }
}

[Serializable]
public class InstrumentsSave
{
    public string instrumenTypeEnum;//тип
    public int count;//количество

    public InstrumentsSave(string instrumenTypeEnum, int count)
    {
        this.instrumenTypeEnum = instrumenTypeEnum;
        this.count = count;
    }
}

[Serializable]
public class SettingsSave
{
    public bool showHints;//показывать подсказки
    public bool sound;//проигрывать звуки

    public SettingsSave(bool showHints, bool sound)
    {
        this.showHints = showHints;
        this.sound = sound;
    }
}


