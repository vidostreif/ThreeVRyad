using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelMenu : MonoBehaviour
{
    public static LevelMenu Instance; // Синглтон
    [SerializeField] public List<Region> regionsList;
    private Level lastLoadLevel = null;
    //private AsyncOperation async;

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров LevelMenu!");
        }

        Instance = this;
    }

    private void Update()
    {
        //if ()
        //    async.allowSceneActivation = true;
        //LoadXml(inLevel);
    }

    public Level LastLoadLevel
    {
        get
        {
            return lastLoadLevel;
        }
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
                    SaveAndLoadScene.Instance.LoadXml(inLevel.xmlDocument.name, "Region_" + i);

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
        SaveNameScene(inLevel);
        SceneManager.LoadSceneAsync("SampleScene");
        yield return true;
        //async.allowSceneActivation = false;
        ////восстанавливаем настройки уровня
        ////LoadXml(inLevel);
        //async.allowSceneActivation = true;
    }

    public void CreateLevelMenu(Region region)
    {
        Transform LevelsCountTransform = transform.Find("Levels");
        foreach (Level level in region.levelList)
        {
            GameObject elementGameObject = Instantiate(PrefabBank.Instance.levelButtonPrefab, LevelsCountTransform);
            //Image image = elementGameObject.GetComponent(typeof(Image)) as Image;
            //image.sprite = SpriteBank.SetShape(instruments[i].Type);
            //instruments[i].Image = image;
            elementGameObject.GetComponentInChildren<Text>().text = level.xmlDocument.name;
            level.AddAction(elementGameObject);
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
