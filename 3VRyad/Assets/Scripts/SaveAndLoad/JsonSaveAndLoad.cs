using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;

public static class JsonSaveAndLoad
{
    private static Save save;
    private static bool saveIsChanged = false;
    private static bool saveFromFileLoad = false;//попытка загрузки из файла произведена
    private static string path;

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
            for (int i = 0; i < region - save.regionSave.Count + 1; i++)
            {
                save.regionSave.Add(new RegionSave());
            }
        }
        //очищаем перед записью
        save.regionSave[region].levelSave.Clear();
        //записываем новые данные
        foreach (Level level in levelList)
        {
            save.regionSave[region].levelSave.Add(new LevelSave(level.Open, level.Passed, level.Stars, level.Score));
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
    public static void RecordSave(Instrument[] instruments)
    {
        LoadSaveFromFile();

        //очищаем перед записью
        save.instrumentsSave.Clear();
        //записываем новые данные
        foreach (Instrument instrument in instruments)
        {
            save.instrumentsSave.Add(new InstrumentsSave(instrument.Type.ToString(), instrument.Quantity));
        }

        saveIsChanged = true;
    }
}

[Serializable]
public class Save
{
    public List<RegionSave> regionSave = new List<RegionSave>();
    public ShopSave shopSave = new ShopSave();
    public List<HelpSave> helpSave = new List<HelpSave>();
    public List<InstrumentsSave> instrumentsSave = new List<InstrumentsSave>();
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
    public int stars = 0;//количество звезд
    public int score = 0;//количество очков

    public LevelSave(bool open, bool passed, int stars, int score)
    {
        this.open = open;
        this.passed = passed;
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


