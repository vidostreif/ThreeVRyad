﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LevelMenu : MonoBehaviour
{
    public static LevelMenu Instance; // Синглтон
    [SerializeField] public List<Region> regionsList;

    [SerializeField] private Level lastLoadLevel;
    private GameObject canvasRegions = null;
    private GameObject canvasLevels = null;
    //private AsyncOperation async;

    public void Awake()
    {
        if (Instance)
        {
            Destroy(this); //Delete duplicate
            return;
        }
        else
        {
            Instance = this; //Make this object the only instance            
        }      
    }

    void Start()
    {
        ////заполнение regionsList из существующих файлов
        CreateRegionsListFromFiles();
        ////если в игре
        if (Application.isPlaying)
        {
            Prepare();
        }
    }

    private void Prepare() {
        DontDestroyOnLoad(gameObject); //Set as do not destroy
        //загрузить данных из сохранения
        JsonSaveAndLoad.LoadSave(regionsList);
        regionsList[0].levelList[0].open = true;
        lastLoadLevel = null;


        //если запустились на сцене меню
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            LoadMainMenu();
        }
    }

    private void CreateRegionsListFromFiles() {
        this.regionsList = new List<Region>();
        UnityEngine.Object xmlDocument = null;
        Level level = null;
        //MessageArray.message.Add("Создаем регионы:");
        int r = 0;        
        do
        {
            int l = 0;
            if (SaveAndLoadScene.Instance().LevelFileExist("Level_" + l, "Region_" + r))
            {
                //MessageArray.message.Add("Region_" + r);
                regionsList.Add(new Region());
                regionsList[r].name = "Region_" + r;
                do
                {
                    xmlDocument = SaveAndLoadScene.Instance().GetXmlDocument("Level_" + l, "Region_" + r);
                    if (xmlDocument != null)
                    {
                        level = new Level(xmlDocument);
                        regionsList[r].levelList.Add(level);
                    }
                    else
                    {
                        break;
                    }
                    l++;
                } while (true);
            }
            else
            {
                break;
            }
            r++;
        } while (true);
    }

    public Level LastLoadLevel
    {
        get
        {
            return lastLoadLevel;
        }
    }

    private Region FoundLevelInRegions(Level inLevel) {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    return regionsList[i];
                }
            }
        }
        return null;
    }

    public void LoadXml(Level inLevel) {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    SaveAndLoadScene.Instance().LoadXml(inLevel.xmlDocument.name, "Region_" + i);
                    lastLoadLevel = inLevel;
                    return;
                }
            }
        }
    }

#if UNITY_EDITOR
    public void SaveXml(Level inLevel)
    {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    inLevel.xmlDocument = SaveAndLoadScene.Instance().SaveXml(inLevel.xmlDocument.name, "Region_" + i);
                    return;
                }
            }
        }
    }
    public void CreateXml(Level inLevel)
    {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    //сначала пробуем загрузить
                    SaveAndLoadScene.Instance().LoadXml("Level_" + j, "Region_" + i);

                    inLevel.xmlDocument = SaveAndLoadScene.Instance().SaveXml("Level_" + j, "Region_" + i);
                    return;
                }
            }
        }
    }
#endif

    public void GetXmlDocument(Level inLevel)
    {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    inLevel.xmlDocument = SaveAndLoadScene.Instance().GetXmlDocument("Level_" + j, "Region_" + i);
                    return;
                }
            }
        }
    }
    
    private void SaveNameScene(Level inLevel)
    {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    //SaveAndLoadScene.Instance.LoadXml(inLevel.xmlDocument.name, "Region_" + i);

                    GameMetaData.GetInstance().SetString("name_scene", inLevel.xmlDocument.name);
                    GameMetaData.GetInstance().SetString("folder_scene", "Region_" + i);
                    return;
                }
            }
        }
    }

    public bool ItLastLevelOnRegion(Level inLevel) {

        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    //если последний уровень в регионе
                    if ((j + 1) == regionsList[i].levelList.Count)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }                
            }
        }
        return false;
    }

    public void AddLevelOnRegion(Level inLevel)
    {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    regionsList[i].levelList.Add(new Level());
                }
            }
        }
    }

    public void DellLevelOnRegion(Level inLevel)
    {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    regionsList[i].levelList.Remove(inLevel);

                    //!!!Добавить удаление файла
                    return;
                }
            }
        }
    }

    //работа с переходом между уровнями
    public void LoadLevel(Level inLevel) {
        StartCoroutine(curLoadLevel(inLevel));
    }

    //загрузить следующий уровень
    public void LoadNextLevel()
    {
        if (lastLoadLevel != null)
        {
            bool found = false;
            for (int i = 0; i < regionsList.Count; i++)
            {
                for (int j = 0; j < regionsList[i].levelList.Count; j++)
                {
                    if (found)
                    {
                        //если уровень открыт
                        if (regionsList[i].levelList[j].open)
                        {
                                LoadLevel(regionsList[i].levelList[j]);
                            return;
                        }                        
                    }
                    if (regionsList[i].levelList[j] == lastLoadLevel)
                    {
                        found = true;
                    }
                }
            }
        }
    }

    public bool NextLevelIsOpen()
    {
        if (lastLoadLevel != null)
        {
            bool found = false;
            for (int i = 0; i < regionsList.Count; i++)
            {
                for (int j = 0; j < regionsList[i].levelList.Count; j++)
                {
                    if (found)
                    {
                        //если уровень открыт
                        if (regionsList[i].levelList[j].open)
                        {
                            return true;
                        }
                        return false;
                    }
                    if (regionsList[i].levelList[j] == lastLoadLevel)
                    {
                        found = true;
                    }
                }
            }
        }
        return false;
    }

    private void SetOpenNextLevel()
    {
        if (lastLoadLevel != null)
        {
            bool found = false;
            for (int i = 0; i < regionsList.Count; i++)
            {
                for (int j = 0; j < regionsList[i].levelList.Count; j++)
                {
                    if (found)
                    {
                        regionsList[i].levelList[j].open = true;
                        JsonSaveAndLoad.RecordSave(regionsList[i].levelList, i);
                        return;
                    }
                    if (regionsList[i].levelList[j] == lastLoadLevel)
                    {
                        found = true;
                        //если последний уровень в регионе
                        if ((j + 1) == regionsList[i].levelList.Count)
                        {
                            JsonSaveAndLoad.RecordSave(regionsList[i].levelList, i, false);
                        }
                    }
                }
            }
        }
    }

    public void SetLevelPassed()
    {
        lastLoadLevel.passed = true;
        SetOpenNextLevel();
    }

    private IEnumerator curLoadLevel(Level inLevel) {
        //загружаем уровень
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            LoadXml(inLevel);
            MainSceneScript.Instance.Prepare();
        }
        else
        {
            lastLoadLevel = inLevel;
            Destroy(canvasRegions);
            Destroy(canvasLevels);
            SceneManager.LoadSceneAsync("SampleScene");
            yield return true;
        }
        
        //async.allowSceneActivation = false;
        ////восстанавливаем настройки уровня
        ////LoadXml(inLevel);
        //async.allowSceneActivation = true;
    }

    public void CreateLevelMenu(Region region)
    {
        if (canvasRegions != null)
        {
            Destroy(canvasRegions);
        }
        canvasLevels = Instantiate(PrefabBank.Instance.levelsCanvasPrefab, transform);

        //настройки кнопки назад
        Transform returnButtonTransform = canvasLevels.transform.Find("ReturnButton");
        //добавляем действие к кнопке
        Button returnButton = returnButtonTransform.GetComponent(typeof(Button)) as Button;
        returnButton.onClick.AddListener(delegate { CreateRegionMenu(); });

        //список уровней
        Transform LevelsCountTransform = canvasLevels.transform.Find("Viewport/Content");
        foreach (Level level in region.levelList)
        {
            GameObject elementGameObject = Instantiate(PrefabBank.Instance.levelButtonPrefab, LevelsCountTransform);
            //Image image = elementGameObject.GetComponent(typeof(Image)) as Image;
            //image.sprite = SpriteBank.SetShape(instruments[i].Type);
            //instruments[i].Image = image;
            elementGameObject.GetComponentInChildren<Text>().text = level.xmlDocument.name;
            level.GetButtonFrom(elementGameObject);            
        }
    }

    public void CreateRegionMenu()
    {
        if (canvasLevels != null)
        {
            Destroy(canvasLevels);
        }
        canvasRegions = Instantiate(PrefabBank.Instance.regionsCanvasPrefab, transform);
        Transform contentTransform = canvasRegions.transform.Find("Viewport/Content");

        //список регионов
        foreach (Region region in regionsList)
        {
            GameObject regionelementGameObject = Instantiate(PrefabBank.Instance.regionButtonPrefab, contentTransform);
            //Image image = elementGameObject.GetComponent(typeof(Image)) as Image;
            //image.sprite = SpriteBank.SetShape(instruments[i].Type);
            //instruments[i].Image = image;
            regionelementGameObject.GetComponentInChildren<Text>().text = region.name;
            region.AddAction(regionelementGameObject);
        }
    }

    public void LoadMainMenu()
    {
        //если на ходимся не на сцене меню, то сначала подгружаем сцену
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            SceneManager.LoadSceneAsync("MainMenu");
        }        
        //если выходим из уровня то загружаем регион с этим уровнем
        if (lastLoadLevel != null)
        { 
            CreateLevelMenu(FoundLevelInRegions(lastLoadLevel));
        }
        else //иначе показываем все основное меню с регионами
        {
            CreateRegionMenu();
        }        
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelMenu))]
public class LevelMenuEditor : Editor
{
    LevelMenu levelMenu;
    public override void OnInspectorGUI()
    {
        levelMenu = (LevelMenu)target;
        DrawDefaultInspector();

        if (SaveAndLoadScene.Instance().xmlDocument != null && levelMenu.LastLoadLevel != null)
        {
            if (GUILayout.Button("Сохранить"))
            {
                levelMenu.SaveXml(levelMenu.LastLoadLevel);
            }
        }
    }
}
#endif
