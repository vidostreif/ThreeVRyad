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

[ExecuteInEditMode]
public class SaveAndLoadScene : MonoBehaviour {

    //public Grid grid;
    //public Tasks tasks;
    public string SceneName;
    [SerializeField] public UnityEngine.Object xmlDocument;
    // Use this for initialization
    void Start () {
        //grid = GameObject.Find("Grid").GetComponent<Grid>();
        //tasks = GameObject.Find("Tasks").GetComponent<Tasks>();
        //xDocument = new UnityEngine.Object();
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    public void SaveXml(string name = "null")
    {
        if (name == "null")
        {
            SceneName = this.xmlDocument.name;
        }

        string datapath = Application.dataPath + "/Resources/SaveScenes/" + SceneName + ".xml";

        XElement root = new XElement("root");

        //сохраняем данные из других классов
        var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.GetInterfaces().Contains(typeof(IESaveAndLoad))
                          && t.GetConstructor(Type.EmptyTypes) != null
                        select Activator.CreateInstance(t) as IESaveAndLoad;

        //List<GameObject> objectsToInteract = new List<GameObject>();//список найденных объектов  

        foreach (var instance in instances)
        {
            Type component = instance.GetClassName();
            IESaveAndLoad[] findeObjects = FindObjectsOfType(component) as IESaveAndLoad[]; //находим всех объекты с компонентом и создаём массив из них

            //!!! потом можноо переделать что бы сохранялись все объекты даже если тип объектов несколько. Сохранять можно по имени.
            if (findeObjects.GetLength(0) > 1)
            {
                Debug.LogError("Объектов типа: " + component.ToString() + ", больше одного на сцене! Сохранение прервано.");
                return;
            }

            foreach (var currentObject in findeObjects) //для каждого объекта в массиве
            {
                root.Add(currentObject.GetXElement()); // where Foo is a method of ISomething 
            }            
        }


        XDocument xDocument = new XDocument(root);
        File.WriteAllText(datapath, xDocument.ToString());
        Debug.Log("Сохранили в " + datapath);

        xmlDocument = Resources.Load("SaveScenes/" + SceneName, typeof(UnityEngine.Object)) as UnityEngine.Object;
    }

    public void LoadXml()
    {
        SceneName = this.xmlDocument.name;
        string datapath = Application.dataPath + "/Resources/SaveScenes/" + SceneName + ".xml";

        XElement root = null;

        if (!File.Exists(datapath))
        {
            Debug.Log("Файл не найден!");
        }
        else
        {
            root = XDocument.Parse(File.ReadAllText(datapath)).Element("root");
        }

        if (root == null)
        {
            Debug.Log("Не удалось загрузить файл!");
            return;
        }

        GenerateScene(root);

    }

    private void GenerateScene(XElement root) {


        //grid.RecoverFromXElement(root.Element("Grid"));
        //tasks.RecoverFromXElement(root.Element("tasks"));

        //List<GameObject> objectsToInteract = new List<GameObject>();//список найденных объектов 

        foreach (XElement ListXElement in root.Elements())
        {
            Type component = Type.GetType(ListXElement.Name.ToString());
            IESaveAndLoad[] findeObjects = FindObjectsOfType(component) as IESaveAndLoad[]; //находим всех объекты с компонентом и создаём массив из них

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


#if UNITY_EDITOR
[CustomEditor(typeof(SaveAndLoadScene))]
public class SaveAndLoadSceneEditor : Editor
{
    //Grid grid;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //this.grid = GameObject.Find("Grid").GetComponent<Grid>();
        SaveAndLoadScene saveAndLoadScene = (SaveAndLoadScene)target;

        SaveAndLoadScene myScript = (SaveAndLoadScene)target;

        
        //string datapath = Application.dataPath + "/Resources/SaveScenes/" + saveAndLoadScene.SceneName + ".xml";

        if (GUILayout.Button("Save"))
        {
            myScript.SaveXml();
        }


        //окно выбора файла!!!


        if (GUILayout.Button("Load"))
        {
            myScript.LoadXml();
        }

        if (GUILayout.Button("Save as"))
        {
            myScript.SaveXml(saveAndLoadScene.SceneName);
        }

        saveAndLoadScene.SceneName = EditorGUILayout.TextField("Имя файла", saveAndLoadScene.SceneName);
        EditorUtility.SetDirty(saveAndLoadScene);

        //if (GUILayout.Button("Dell"))
        //{
        //    myScript.DeXml(datapath);
        //}
    }

}
#endif
