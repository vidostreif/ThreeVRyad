using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelMenu : MonoBehaviour
{

    [SerializeField] public List<Region> regionsList;
    //public Object curLevel = null;

    public void LoadXml(Object xmlDocument) {

    }

    public void SaveXml(Level inLevel)
    {
        foreach (Region region in regionsList)
        {
            foreach (Level level in region.levelList)
            {
                if (level == inLevel)
                {
                    SaveAndLoadScene.Instance.SaveXml(inLevel.xmlDocument.name, region.name);
                    break;
                }
            }
        }
    }

    public void CreateXml(Object xmlDocument)
    {

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
