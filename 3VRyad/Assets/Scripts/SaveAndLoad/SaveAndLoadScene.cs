using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;

//[ExecuteInEditMode]
public class SaveAndLoadScene
{
    private static SaveAndLoadScene instance; // Синглтон
    private string SceneName;
    private string allSaveFolder = "SaveScenes";
    public UnityEngine.Object xmlDocument = null;

    public static SaveAndLoadScene Instance()
    {
        if (instance == null)
            instance = new SaveAndLoadScene();

        return instance;
    }

#if UNITY_EDITOR
    public UnityEngine.Object SaveXml(string name = "null", string folder = "")
    {
        string datapath = GetDatapath(name, folder);

        XElement root = new XElement("root");

        //!!!Переделать - получать все объекты на сцене и в них уже искать IESaveAndLoad

        //сохраняем данные из других классов
        var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.GetInterfaces().Contains(typeof(IESaveAndLoad))
                          && t.GetConstructor(Type.EmptyTypes) != null
                        select Activator.CreateInstance(t) as IESaveAndLoad;

        foreach (var instance in instances)
        {
            Type component = instance.GetClassName();
            IESaveAndLoad[] findeObjects = UnityEngine.Object.FindObjectsOfType(component) as IESaveAndLoad[]; //находим всех объекты с компонентом и создаём массив из них

            //!!! потом можноо переделать что бы сохранялись все объекты даже если тип объектов несколько. Сохранять можно по имени.
            if (findeObjects.GetLength(0) > 1)
            {
                Debug.LogError("Объектов типа: " + component.ToString() + ", больше одного на сцене! Сохранение прервано.");
                return null;
            }

            foreach (var currentObject in findeObjects) //для каждого объекта в массиве
            {
                root.Add(currentObject.GetXElement()); // where Foo is a method of ISomething 
            }            
        }
        
        XDocument xDocument = new XDocument(root);
        File.WriteAllText(datapath, xDocument.ToString());
        
        Debug.Log("Сохранили в " + datapath);

        //var bytes = RawDeserializeEx(System.IO.File.ReadAllBytes(datapath), typeof(UnityEngine.Object));

        AssetDatabase.Refresh();

        xmlDocument = Resources.Load(allSaveFolder + "/" + folder + "/" + name, typeof(UnityEngine.Object)) as UnityEngine.Object;
        return xmlDocument;
    }
#endif

    public void LoadXml(string name = "null", string folder = "")
    {
        //string datapath = GetDatapath(name, folder);

        XElement root = null;

        //if (!File.Exists(datapath))
        //{
        //    Debug.Log("Файл не найден!");
        //}
        //else
        //{
            //string fileXmlPath = Application.dataPath;
            //fileXmlPath += "/Data/Spells.xml";
            TextAsset txt = Resources.Load<TextAsset>(allSaveFolder + "/" + folder + "/" + name);
            //XmlDocument xml = new XmlDocument();
            //xml.LoadXml(ta.text);

            root = XDocument.Parse(txt.text).Element("root");
        //}

        if (root == null)
        {
            Debug.Log("Не удалось загрузить файл!");
            return;
        }

        GenerateScene(root);
        xmlDocument = Resources.Load(allSaveFolder + "/" + folder + "/" + name, typeof(UnityEngine.Object)) as UnityEngine.Object;
    }

    public UnityEngine.Object GetXmlDocument(string name = "null", string folder = "")
    {
        return Resources.Load(allSaveFolder + "/" + folder + "/" + name, typeof(UnityEngine.Object)) as UnityEngine.Object;
    }

    public bool LevelFileExist(string name, string folder)
    {
        TextAsset txt = Resources.Load<TextAsset>(allSaveFolder + "/" + folder + "/" + name);
        if (txt != null)
        {
            XElement root = null;
            root = XDocument.Parse(txt.text).Element("root");
            if (root != null)
            {
                return true;
            }
        }        
        return false;
    }

    private string GetDatapath(string name = "null", string folder = "") {
        if (name == "null")
        {
            SceneName = this.xmlDocument.name;
        }
        else
        {
            SceneName = name;
        }
        if (folder != "")
        {
            folder = folder + "/";
        }

        string curFolder = Application.dataPath + "/Resources/" + allSaveFolder + "/" + folder;
        System.IO.Directory.CreateDirectory(curFolder);

        return curFolder + SceneName + ".xml";
    }

    private void GenerateScene(XElement root) {


        //grid.RecoverFromXElement(root.Element("Grid"));
        //tasks.RecoverFromXElement(root.Element("tasks"));

        //List<GameObject> objectsToInteract = new List<GameObject>();//список найденных объектов 

        foreach (XElement ListXElement in root.Elements())
        {
            Type component = Type.GetType(ListXElement.Name.ToString());
            IESaveAndLoad[] findeObjects = UnityEngine.Object.FindObjectsOfType(component) as IESaveAndLoad[]; //находим всех объекты с компонентом и создаём массив из них

            //!!! потом можноо переделать что бы загружались все объекты даже если тип объектов несколько. Загружать можно по имени.
            if (findeObjects.GetLength(0) > 1)
            {
                Debug.LogError("Объектов типа: " + component.ToString() + ", больше одного на сцене! Загрузка прервана.");
                return;
            }

            foreach (var currentObject in findeObjects) //для каждого объекта в массиве
            {
                currentObject.RecoverFromXElement(ListXElement); 
            }
        }
    }

}


//#if UNITY_EDITOR
//[CustomEditor(typeof(SaveAndLoadScene))]
//public class SaveAndLoadSceneEditor : Editor
//{
//    //Grid grid;

//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();
//        //this.grid = GameObject.Find("Grid").GetComponent<Grid>();
//        SaveAndLoadScene saveAndLoadScene = (SaveAndLoadScene)target;

//        SaveAndLoadScene myScript = (SaveAndLoadScene)target;

        
//        //string datapath = Application.dataPath + "/Resources/SaveScenes/" + saveAndLoadScene.SceneName + ".xml";

//        if (GUILayout.Button("Save"))
//        {
//            myScript.SaveXml();
//        }


//        //окно выбора файла!!!


//        if (GUILayout.Button("Load"))
//        {
//            myScript.LoadXml();
//        }

//        if (GUILayout.Button("Save as"))
//        {
//            myScript.SaveXml(saveAndLoadScene.SceneName);
//        }

//        saveAndLoadScene.SceneName = EditorGUILayout.TextField("Имя файла", saveAndLoadScene.SceneName);
//        EditorUtility.SetDirty(saveAndLoadScene);

//        //if (GUILayout.Button("Dell"))
//        //{
//        //    myScript.DeXml(datapath);
//        //}
//    }

//}
//#endif
