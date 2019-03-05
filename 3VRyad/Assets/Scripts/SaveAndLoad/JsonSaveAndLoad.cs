using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class JsonSaveAndLoad 
{
    private static Save save;
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

    private static void SetSaveToFile()
    {
        File.WriteAllText(path, JsonUtility.ToJson(save));
    }

    //загрузка сохранений уровней
    public static void LoadSave(List<Region> regionsList) {
        GateSaveFromFile();
        if (save.regionSave.Count > 0)
        {
            for (int r = 0; r < regionsList.Count; r++)
            {
                for (int l = 0; l < regionsList[r].levelList.Count; l++)
                {
                        if (save.regionSave.Count > r && save.regionSave[r].levelSave.Count > l)
                        {
                            regionsList[r].levelList[l].open = save.regionSave[r].levelSave[l].open;
                            regionsList[r].levelList[l].passed = save.regionSave[r].levelSave[l].passed;
                        }
                }
            }
        }
    }

    //запись сохранений уровней
    public static void RecordSave(List<Level> levelList, int region, bool setSaveToFile = true)
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
            save.regionSave[region].levelSave.Add(new LevelSave(level.open, level.passed));
        }

        if (setSaveToFile)
            SetSaveToFile();

    }
}

[Serializable]
public class Save
{
    public List<RegionSave> regionSave = new List<RegionSave>();
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

    public LevelSave(bool open, bool passed)
    {
        this.open = open;
        this.passed = passed;
    }
}
