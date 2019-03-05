using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelMenu : MonoBehaviour
{
    public static LevelMenu Instance; // Синглтон
    [SerializeField] private bool active;
    [SerializeField] public List<Region> regionsList;
    private Level lastLoadLevel = null;

    private GameObject canvasRegions = null;
    private GameObject canvasLevels = null;
    //private AsyncOperation async;


    public void OnEnable()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);


        //!!!сделать заполнение regionsList из существующих файлов


        //загрузить данных из сохранения
        JsonSaveAndLoad.LoadSave(regionsList);
        regionsList[0].levelList[0].open = true;
    }

    public void Awake()
    {
        if (Instance) DestroyImmediate(this);
    }

    //void Awake()
    //{
    //    //if (active)
    //    //{
    //        // регистрация синглтона
    //        if (Instance != null)
    //        {
    //            Debug.LogError("Несколько экземпляров LevelMenu!");
    //        }

    //        Instance = this;

            
    //    //}
    //    //else
    //    //{
    //    //    Destroy(this);
    //    //}
        
    //}

    public Level LastLoadLevel
    {
        get
        {
            return lastLoadLevel;
        }
    }

    private Level FoundLevelInRegionsList(Level inLevel) {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    return regionsList[i].levelList[j];
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
                    SaveAndLoadScene.Instance.LoadXml(inLevel.xmlDocument.name, "Region_" + i);
                    lastLoadLevel = inLevel;
                    break;
                }
            }
        }
    }

    public void SaveXml(Level inLevel)
    {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    inLevel.xmlDocument = SaveAndLoadScene.Instance.SaveXml(inLevel.xmlDocument.name, "Region_" + i);
                    break;
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
                    SaveAndLoadScene.Instance.LoadXml("Level_" + j, "Region_" + i);

                    inLevel.xmlDocument = SaveAndLoadScene.Instance.SaveXml("Level_" + j, "Region_" + i);
                    break;
                }
            }
        }
    }

    public void GetXmlDocument(Level inLevel)
    {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    inLevel.xmlDocument = SaveAndLoadScene.Instance.GetXmlDocument("Level_" + j, "Region_" + i);
                    break;
                }
            }
        }
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
                        break;
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
                        LoadXml(regionsList[i].levelList[j]);
                        break;
                    }
                    if (regionsList[i].levelList[j] == lastLoadLevel)
                    {
                        found = true;
                    }
                }
            }
        }
    }

    public void LoadLevel(Level inLevel) {
        StartCoroutine(curLoadLevel(inLevel));
    }

    private void SaveNameScene(Level inLevel) {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    //SaveAndLoadScene.Instance.LoadXml(inLevel.xmlDocument.name, "Region_" + i);

                    GameMetaData.GetInstance().SetString("name_scene", inLevel.xmlDocument.name);
                    GameMetaData.GetInstance().SetString("folder_scene", "Region_" + i);
                    break;
                }
            }
        }
    }

    private IEnumerator curLoadLevel(Level inLevel) {
        //загружаем уровень
        //SceneManager.LoadScene("SampleScene");
        //SaveNameScene(inLevel);
        SceneManager.LoadSceneAsync("SampleScene");
        lastLoadLevel = inLevel;
        Destroy(canvasRegions);
        Destroy(canvasLevels);
        yield return true;
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

        if (SaveAndLoadScene.Instance.xmlDocument != null && levelMenu.LastLoadLevel != null)
        {
            if (GUILayout.Button("Сохранить"))
            {                
                levelMenu.SaveXml(levelMenu.LastLoadLevel);
            }
        }
    }
}
#endif
