using System.Collections;
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

    private Level lastLoadLevel;
    public string lastLoadXmlDocument;
    public string lastLoadFolder;
    public bool levelSaved = false;//левел в начале загрузки сохранен
    private GameObject panelRegions = null;
    private GameObject panelLevels = null;
    //private AsyncOperation async;

    public void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject); //Delete duplicate
            return;
        }
        else
        {
            Instance = this; //Make this object the only instance            
        }

        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject); //Set as do not destroy
        }

        ////заполнение regionsList из существующих файлов
        CreateRegionsListFromFiles();

        ////инициализируем гугл сервис
        //GPGSManager.Initialize(false);
        //JsonSaveAndLoad.LoadSaveFromFile();

#if UNITY_EDITOR
        //Если на сцене игры
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            ////если в игре
            if (Application.isPlaying)
            {
                if (lastLoadLevel == null && lastLoadXmlDocument != null && lastLoadFolder != null)
                {
                    lastLoadLevel = FoundLevel(lastLoadXmlDocument, lastLoadFolder);
                }
            }
        }
        else
        {
            levelSaved = true;
        }        
#endif
    }

    public void Start()
    {

#if UNITY_EDITOR
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            ////если не в игре
            if (!Application.isPlaying)
            {
                if (lastLoadXmlDocument != null && lastLoadFolder != null)
                {
                    LoadXml(FoundLevel(lastLoadXmlDocument, lastLoadFolder));
                }
            }
        }
#endif

        if (Application.isPlaying)
        {
            Prepare(); 
        }
    }

    private void Prepare() {
        //DontDestroyOnLoad(gameObject); //Set as do not destroy
        //загрузить данных из сохранения
        LoadSave();
        regionsList[0].levelList[0].SetLevelOpend();
        //lastLoadLevel = null;

        //если запустились на сцене меню
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            LoadMainMenu();
        }
    }

    private void LoadSave() {
        Save save = JsonSaveAndLoad.LoadSave();
        if (save.regionSave.Count > 0)
        {
            for (int r = 0; r < regionsList.Count; r++)
            {
                for (int l = 0; l < regionsList[r].levelList.Count; l++)
                {
                    if (save.regionSave.Count > r && save.regionSave[r].levelSave.Count > l)
                    {
                        regionsList[r].levelList[l].LoadSave(save.regionSave[r].levelSave[l].open, save.regionSave[r].levelSave[l].passed,
                            save.regionSave[r].levelSave[l].stars, save.regionSave[r].levelSave[l].score);
                    }
                }
            }
        }
    }

    private void CreateRegionsListFromFiles() {
        this.regionsList = new List<Region>();
        UnityEngine.Object xmlDocument = null;
        Level level = null;
        
        int r = 0;        
        do
        {
            int l = 0;
            if (SaveAndLoadScene.Instance().LevelFileExist("Level_" + l, "Region_" + r))
            {                
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

    private Level FoundLevel(string xmlName, string regionName)
    {
        for (int i = 0; i < regionsList.Count; i++)
        {
            if (regionsList[i].name == regionName)
            {
                for (int j = 0; j < regionsList[i].levelList.Count; j++)
                {
                    if (regionsList[i].levelList[j].xmlDocument.name == xmlName)
                    {
                        return regionsList[i].levelList[j];
                    }
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
                    lastLoadXmlDocument = inLevel.xmlDocument.name;
                    lastLoadFolder = "Region_" + i;
#if UNITY_EDITOR
                    //сохраняем параметры в редакторе
                    if (!Application.isPlaying)
                    {
                        EditorUtility.SetDirty(this);
                    }
#endif
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
        StartCoroutine(CurLoadLevel(inLevel));
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
                        if (regionsList[i].levelList[j].Open)
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
                        if (regionsList[i].levelList[j].Open)
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
                        regionsList[i].levelList[j].SetLevelOpend();
                        JsonSaveAndLoad.RecordSave(regionsList[i].levelList, i);
                        return;
                    }
                    if (regionsList[i].levelList[j] == lastLoadLevel)
                    {
                        found = true;
                        //если последний уровень в регионе
                        if ((j + 1) == regionsList[i].levelList.Count)
                        {
                            JsonSaveAndLoad.RecordSave(regionsList[i].levelList, i);
                        }
                    }
                }
            }
        }
    }

    public void SetLevelPassed(int stars, int score)
    {
        if (lastLoadLevel != null)
        {
            lastLoadLevel.SetLevelPassed(stars, score);
            SetOpenNextLevel();
        }        
    }

    private IEnumerator CurLoadLevel(Level inLevel) {
        //загружаем уровень
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            LoadXml(inLevel);
            MainGameSceneScript.Instance.Prepare();
        }
        else
        {
            lastLoadLevel = inLevel;
            Destroy(panelRegions);
            Destroy(panelLevels);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SampleScene");
            //создаем изображение для отображения загрузки
            GameObject imageMainLoadGO = Instantiate(PrefabBank.Instance.imageMainLoad, transform);
            Image imageLoad = imageMainLoadGO.transform.Find("ImageLoad").GetComponent<Image>();
            //ожидаем загрузки уровня
            float progress = 0;
            while (!asyncLoad.isDone)
            {
                progress = asyncLoad.progress / 0.9f;
                imageLoad.fillAmount = progress;
                yield return null;
            }
            Destroy(imageMainLoadGO);
        }        
        //async.allowSceneActivation = false;
        ////восстанавливаем настройки уровня
        ////LoadXml(inLevel);
        //async.allowSceneActivation = true;
    }

    public void CreateLevelMenu(Region region)
    {
        if (panelRegions != null)
        {
            Destroy(panelRegions);
        }
        panelLevels = Instantiate(PrefabBank.Instance.levelsCanvasPrefab, transform);

        //настройки кнопки назад
        Transform returnButtonTransform = panelLevels.transform.Find("ReturnButton");
        //добавляем действие к кнопке
        Button returnButton = returnButtonTransform.GetComponent(typeof(Button)) as Button;
        returnButton.onClick.AddListener(delegate { CreateRegionMenu(); });

        //список уровней
        Transform LevelsCountTransform = panelLevels.transform.Find("Viewport/Content");
        int levelNumber = 1;
        foreach (Level level in region.levelList)
        {
            GameObject levelGameObject = Instantiate(PrefabBank.Instance.levelButtonPrefab, LevelsCountTransform);
            //Image image = elementGameObject.GetComponent(typeof(Image)) as Image;
            //image.sprite = SpriteBank.SetShape(instruments[i].Type);
            //instruments[i].Image = image;

            //Выдаем звезды
            for (int i = 1; i <= level.Stars; i++)
            {
                Transform starTransform = levelGameObject.transform.Find("Star" + i);
                Image starImage = starTransform.GetComponent(typeof(Image)) as Image;
                starImage.color = Color.white;
                //SupportFunctions.ChangeAlfa(starImage, 1);
            }

            levelGameObject.GetComponentInChildren<Text>().text = levelNumber.ToString();
            if (level.Open)
            {
                SupportFunctions.ChangeAlfa(levelGameObject.GetComponentInChildren<Text>(), 1);
            }
            level.GetButtonFrom(levelGameObject);
            levelNumber++;
        }
    }

    public void CreateRegionMenu()
    {
        if (panelLevels != null)
        {
            Destroy(panelLevels);
        }
        panelRegions = Instantiate(PrefabBank.Instance.regionsCanvasPrefab, transform);
        Transform contentTransform = panelRegions.transform.Find("Viewport/Content");

        //список регионов
        foreach (Region region in regionsList)
        {
            GameObject regionelementGameObject = Instantiate(PrefabBank.Instance.regionButtonPrefab, contentTransform);
            Transform textNameTransform = regionelementGameObject.transform.Find("TextName");
            textNameTransform.GetComponentInChildren<Text>().text = region.name;

            Transform textStarsTransform = regionelementGameObject.transform.Find("TextStars");

            int stars = 0;
            int allStars = 0;
            foreach (Level level in region.levelList)
            {
                stars += level.Stars;
                allStars += 3;
            }
            textStarsTransform.GetComponentInChildren<Text>().text = stars + " / " + allStars;

            region.AddAction(regionelementGameObject);
        }
    }

    public void LoadMainMenu()
    {
        StartCoroutine(CurLoadMainMenu());
    }

    private IEnumerator CurLoadMainMenu()
    {
        //если на ходимся не на сцене меню, то сначала подгружаем сцену
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");
            //yield return true;
            //ожидаем загрузки уровня
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

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

//#if UNITY_EDITOR
//[CustomEditor(typeof(LevelMenu))]
//public class LevelMenuEditor : Editor
//{
//    LevelMenu levelMenu;
//    public override void OnInspectorGUI()
//    {
//        levelMenu = (LevelMenu)target;
//        DrawDefaultInspector();

//        if (SaveAndLoadScene.Instance().lastLoadXmlDocument != null && levelMenu.LastLoadLevel != null)
//        {
//            if (GUILayout.Button("Сохранить"))
//            {
//                levelMenu.SaveXml(levelMenu.LastLoadLevel);
//            }
//        }
//    }
//}
//#endif
