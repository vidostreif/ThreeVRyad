using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;

public class InstrumentsManager : MonoBehaviour
{
    public static InstrumentsManager Instance; // Синглтон
    public float distanceBetweenInstruments;
    public Instrument[] instruments;// список инструментов 
    
    void Awake()
    {
        // регистрация синглтона
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

        //загружаем сохранение
        Save save = JsonSaveAndLoad.LoadSave();
        foreach (InstrumentsSave instrumentsSave in save.instrumentsSave)
        {
            foreach (Instrument instrument in instruments)
            {
                if (instrumentsSave.instrumenTypeEnum == instrument.Type.ToString())
                {
                    instrument.AddQuantity(instrumentsSave.count);
                    break;
                }
            }
        }
        
    }
   
    //добавление количества инструментов из бандла
    public bool addinstruments(BundleShopV[] bundleShopV) {

        bool res = true;
        //добавляем бандл к инструментам
        foreach (BundleShopV item in bundleShopV)
        {
            bool found = false;
            foreach (Instrument instrument in instruments)
            {
                if (instrument.Type == item.type)
                {
                    instrument.AddQuantity(item.count);
                    Debug.Log("Добавили к инструменту " + instrument.Type +  ", " + item.count + " шт.");
                    found = true;
                    break;
                }
            }

            //если не нашли нужный инструмент
            if (!found)
            {
                res = false;
            }
        }

        //отнимаем обратно если не нашли все инструменты
        if (!res)
        {
            foreach (BundleShopV item in bundleShopV)
            {
                foreach (Instrument instrument in instruments)
                {
                    if (instrument.Type == item.type)
                    {
                        instrument.SubQuantity(item.count);
                        Debug.Log("Отняли у инструмента " + instrument.Type + ", " + item.count + " шт.");
                        break;
                    }
                }
            }
        }
        else
        {
            JsonSaveAndLoad.RecordSave(instruments);
        }

        return res;
    }
   
    //создание коллекции инструментов в магазине
    public void CreateInstrumentsOnShop(Transform panelTransform)
    {        
            if (panelTransform != null)
            {
                //смещение по x
                float startingXPoint = panelTransform.position.x - ((1 + distanceBetweenInstruments) * (instruments.Length - 1)) * 0.5f;

                for (int i = 0; i < instruments.Length; i++)
                {
                    GameObject go = Instantiate(PrefabBank.Instance.prefabInstrument, new Vector3(startingXPoint + (i * (1 + distanceBetweenInstruments)), panelTransform.position.y, panelTransform.position.z), Quaternion.identity, panelTransform);
                    instruments[i].CreateShopInstrumentButton(go);
                }
            }
            else
            {
                Debug.Log("Не нашли панель в магазине для отображения инструментов!");
            }
    }
        
}
