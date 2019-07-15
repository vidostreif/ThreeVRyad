using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Класс Object Pooling — сам пул, который выдает свободные объекты по требованию и создает новые при нехватке.
[AddComponentMenu("Pool/ObjectPooling")]
public class ObjectPooling
{
    #region Data
    private List<GameObject> objects;
    Transform objectsParent;
    //public int replenishment;//пополнение в цикл (0,1сек)

    public List<GameObject> Objects { get => objects;}
    #endregion

    //инициализация пула
    #region Interface
    public void Initialize(int count, GameObject sample, Transform objects_parent)
    {
        //GameObject go = GameObject.Instantiate(new GameObject(), objects_parent);
        objects = new List<GameObject>();
        objectsParent = objects_parent;
        for (int i = 0; i < count; i++)
        {
            AddObject(sample, objects_parent); //создаем объекты до указанного количества
        }
    }

    //выдача объекта
    public GameObject GetObjectToRent()
    {
        for (int i = 1; i < Objects.Count; i++)
        {
            if (Objects[i].gameObject != null)
            {
                if (Objects[i].gameObject.activeInHierarchy == false)
                {
                    return Objects[i];
                }
            }
            else
            {
                AddObject(Objects[0], objectsParent, i);
            }
           
        }
        AddObject(Objects[0], objectsParent);
        return Objects[Objects.Count - 1];
    }

    //выдача объекта
    public GameObject GetObjectForever()
    {
        GameObject go;
        for (int i = Objects.Count - 1; i > 0; i--)
        {
            if (Objects[i].gameObject != null)
            {
                if (Objects[i].gameObject.activeInHierarchy == false)
                {
                    go = Objects[i];
                    Objects.RemoveRange(i, 1);
                    return go;                    
                }
            }
        }
        AddObject(Objects[0], objectsParent);
        go = Objects[Objects.Count - 1];
        Objects.RemoveRange(Objects.Count - 1, 1);
        return go;
    }

    //расширить массив
    public void ExpandArray(int count) {
        for (int i = 0; i < count; i++)
        {
            i++;
            AddObject(Objects[0], objectsParent);
        }
    }
    #endregion

    //добавление объекта в пул
    #region Methods
    void AddObject(GameObject sample, Transform objects_parent, int position = -1)
    {
        GameObject temp;
        temp = GameObject.Instantiate(sample.gameObject);        
        temp.transform.SetParent(objects_parent);
        if (position == -1)
        {
            Objects.Add(temp);
            temp.name = sample.name + "_" + Objects.Count;
        }
        else
        {
            Objects[position] = temp;
            temp.name = sample.name + "_" + (position+1);
        }
        
        if (temp.GetComponent<Animator>())
            temp.GetComponent<Animator>().StartPlayback();
        temp.SetActive(false);
    }
    #endregion
}
