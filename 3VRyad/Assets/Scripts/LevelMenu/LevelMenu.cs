using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelMenu : MonoBehaviour
{

    [SerializeField] public List<Region> regionsList;
    
    public void LoadXml(Level inLevel) {
        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList[i].levelList.Count; j++)
            {
                if (regionsList[i].levelList[j] == inLevel)
                {
                    SaveAndLoadScene.Instance.LoadXml(inLevel.xmlDocument.name, "Region_" + i);
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

//#if UNITY_EDITOR
//[CustomEditor(typeof(LevelMenu))]
//public class LevelMenuEditor : Editor
//{
//    Grid grid;

//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();
//        LevelMenu levelMenu = (LevelMenu)target;

//        SaveAndLoadScene myScript = (SaveAndLoadScene)target;

//        string datapath = Application.dataPath + "/Resources/SaveScenes/" + saveAndLoadScene.SceneName + ".xml";

//        foreach (Region region in levelMenu.regionsList)
//        {
//            foreach (Level level in region.levelList)
//            {
//                level.xmlDocument = EditorGUILayout.ObjectField("Имя файла", level.xmlDocument, typeof(Object), false);

//                if (level.xmlDocument != null)
//                {
//                    if (levelMenu.curLevel != level.xmlDocument)
//                    {
//                        if (GUILayout.Button("Загрузить"))
//                        {
//                            levelMenu.LoadXml(level.xmlDocument);
//                            levelMenu.curLevel = level.xmlDocument;
//                        }
//                    }
//                    else
//                    {
//                        if (GUILayout.Button("Сохранить"))
//                        {
//                            levelMenu.SaveXml(level.xmlDocument);
//                        }
//                    }

//                }
//                else
//                {
//                    if (GUILayout.Button("Создать"))
//                    {
//                        levelMenu.CreateXml(level.xmlDocument);
//                        levelMenu.curLevel = level.xmlDocument;
//                    }
//                }

//            }
//        }

//        EditorUtility.SetDirty(levelMenu);
//    }

//}
//#endif
