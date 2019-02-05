using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.IO;
using System.Xml.Linq;


[ExecuteInEditMode]
public class SaveAndLoadScene : MonoBehaviour {

    public Grid grid;
    public string SceneName;
    [SerializeField] public UnityEngine.Object xmlDocument;
    // Use this for initialization
    void Start () {
        grid = GameObject.Find("Grid").GetComponent<Grid>();
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

        root.Add(grid.GetXElement());

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

        grid.RecoverFromXElement(root.Element("grid"));
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
