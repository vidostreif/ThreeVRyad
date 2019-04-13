using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class JsonSaveAndLoad 
{
    private static Save save;
    private static bool saveIsChanged = false;
    private static bool saveFromFileLoad = false;//попытка загрузки из файла произведена
    private static string path;

    private static void GateSaveFromFile()
    {
        if (!saveFromFileLoad)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, "Save.json");
#else
            path = Path.Combine(Application.dataPath, "Save.json");
#endif
            if (File.Exists(path))
            {
                save = JsonUtility.FromJson<Save>(File.ReadAllText(path));
                Debug.Log("Загрузка сохранения.");
            }
            else
            {
                save = new Save();
            }
        }

        saveFromFileLoad = true;
    }

    public static void SetSaveToFile()
    {
        if (saveIsChanged)
        {
            File.WriteAllText(path, JsonUtility.ToJson(save));
            saveIsChanged = false;
        }        
    }

    //загрузка сохранений уровней
    public static Save LoadSave() {
        GateSaveFromFile();
        return save;
    }

    //запись сохранений уровней
    public static void RecordSave(List<Level> levelList, int region)
    {
        GateSaveFromFile();
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
        GateSaveFromFile();
        save.shopSave.coins = shop.Coins;
        saveIsChanged = true;
    }

    //запись показанных подсказок
    public static void RecordSave(HintStatus[] hintsStatus)
    {
        GateSaveFromFile();
        
        //очищаем перед записью
        save.helpSave.Clear();
        //записываем новые данные
        foreach (HintStatus hintStatus in hintsStatus)
        {
            save.helpSave.Add(new HelpSave(hintStatus.help.ToString(), hintStatus.status));
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

