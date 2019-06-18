using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;

public class ThingsManager : MonoBehaviour
{
    public static ThingsManager Instance; // Синглтон
    public float distanceBetweenInstruments;
    private Thing[] instruments;// список инструментов 
    
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
            DontDestroyOnLoadManager.DontDestroyOnLoad(gameObject); //Set as do not destroy
        }

        //загружаем сохранение
        Save save = JsonSaveAndLoad.LoadSave();
        //создаем список инструментов и проставляем данные
        instruments = new Thing[Enum.GetNames(typeof(InstrumentsEnum)).Length - 1];
        for (int i = 0; i < instruments.Length; i++)
        {
            int quantity = 0;
            InstrumentsEnum instrumentsEnum = (InstrumentsEnum)Enum.GetValues(typeof(InstrumentsEnum)).GetValue(i + 1);

            //если в сохранении нет ни 

            foreach (InstrumentsSave instrumentsSave in save.instrumentsSave)
            {
                if (instrumentsSave.instrumenTypeEnum == instrumentsEnum.ToString())
                {
                    quantity = instrumentsSave.count;
                    break;
                }
            }
            instruments[i] = new Thing(instrumentsEnum, quantity);
        }
    }
   
    //добавление количества инструментов из бандла
    public bool addinstruments(BundleShopV[] bundleShopV) {

        bool res = true;
        //добавляем бандл к инструментам
        foreach (BundleShopV item in bundleShopV)
        {
            bool found = false;
            foreach (Thing instrument in instruments)
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
                foreach (Thing instrument in instruments)
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

    //добавление количества инструментов из бандла
    public void addinstruments(InstrumentsEnum type, int count, Transform curTransform, Vector3 position)
    {
        //добавляем указанное количество к инструментам
            foreach (Thing instrument in instruments)
            {
                if (instrument.Type == type)
                {
                    instrument.AddQuantity(count);
                    StartCoroutine(Shop.Instance.CreateThingAnimation(position, curTransform, type, count));
                    break;
                }
            }

        JsonSaveAndLoad.RecordSave(instruments);
        JsonSaveAndLoad.SetSaveToFile();
    }

    //количество инструментов
    public int InstrumentsCount() {
        if (instruments != null)
        {
            return instruments.Length;
        }
        else
        {
            return 0;
        }
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
                if (instruments[i].Type != InstrumentsEnum.Empty)
                {
                    GameObject go = Instantiate(PrefabBank.PrefabButtonThing, new Vector3(startingXPoint + (i * (1 + distanceBetweenInstruments)), panelTransform.position.y, panelTransform.position.z), Quaternion.identity, panelTransform);
                    instruments[i].CreateShopThingButton(go);
                }
                    
                }
            }
            else
            {
                Debug.Log("Не нашли панель в магазине для отображения инструментов!");
            }
    }

    //предзагрузка спрайтов
    public void PreloadSprites() {
        foreach (Thing thingItem in instruments)
        {
            SpriteBank.SetShape(thingItem.Type);
            SpriteBank.SetShape(thingItem.Type, true);
        }
    }

    public Thing GetThing(InstrumentsEnum type) {
        if (instruments != null)
        {
            foreach (Thing item in instruments)
            {
                if (item.Type == type)
                {
                    return item;
                }
            }
        }
        return null;
    }

    public Thing GetThing(int count)
    {
        if (instruments != null && instruments.Length >= count)
        {
            return instruments[count - 1];
        }
        return null;
    }

    public void RecordSave()
    {
        JsonSaveAndLoad.RecordSave(instruments);
    }

    public void CountAllNumber()
    {
        foreach (var item in instruments)
        {
            item.CountNumber();
        }
    }
}
