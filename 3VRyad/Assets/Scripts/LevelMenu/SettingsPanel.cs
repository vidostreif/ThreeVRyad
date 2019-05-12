using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//панель настроек
public class SettingsPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //закрытие панели настроек
    public void CloseSettingsPanel() {
        Destroy(this.gameObject);
    }

    //удаление сохранений
    public void CreateQuestionDeleteSaves()
    {
        //создаем запрос на удаление сохранений
        string text = "Вы точно хотите удалить все сохранения?";
        //Action actionYes = delegate
        //{
        //    JsonSaveAndLoad.DeleteSave();
        //};
        SupportFunctions.CreateYesNoPanel(LevelMenu.Instance.transform, text, DeleteSaves);
    }

    public void DeleteSaves()
    {
        //удаляем сохранения
        JsonSaveAndLoad.DeleteSave();

        
        //LevelMenu.Instance.Prepare();
        //Component[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(MonoBehaviour)) as Component[];
        //foreach (Component item in findeObjects)
        //{
        //    GameObject gameObject = item.gameObject;

        //    Destroy(item);
        //    gameObject.AddComponent<item.ToString()>();
        //}

        //LevelMenu.Instance.CreateRegionMenu();
    }
}
