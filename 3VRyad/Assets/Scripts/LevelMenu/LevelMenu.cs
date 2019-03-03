using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelMenu : MonoBehaviour
{
    [SerializeField] public List<Region> regionsList;
    private Level lastLoadLevel = null;

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
