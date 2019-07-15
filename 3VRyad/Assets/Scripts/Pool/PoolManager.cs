using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Pool/PoolManager")]
//управляет пулами различных объектов
public class PoolManager : MonoBehaviour
{
    #region Unity scene settings
    [SerializeField] private PoolPart[] poolsSetup;
    #endregion
    public static PoolManager Instance; // Синглтон
    private PoolPart[] pools;
    private List<RentalGO> rentalGOList;
    private bool returnRentalGOIdle;
    private bool quantityRecoveryGOIdle;
    private GameObject objectsParent;

    [System.Serializable]
    public struct PoolPart
    {
        public string name; //имя префаба
        public GameObject prefab; //сам префаб, как образец
        public int count; //количество объектов при инициализации пула
        public int replenishment;//пополнение в цикл (0,1сек)
        public ObjectPooling ferula; //сам пул
    }

    #region Methods

    //добавление нового пула
    public void PoolsSetupAddPool(string name, GameObject prefab, int count)
    {
        if (count < 1)
        {
            count = 1;
        }
            //перенос данных в новый пул
            PoolPart[] newPools = new PoolPart[pools.Length + 1];
            for (int i = 0; i < pools.Length; i++)
            {
                newPools[i] = pools[i];
            }

            newPools[newPools.Length - 1].count = count;
            newPools[newPools.Length - 1].name = name;
            newPools[newPools.Length - 1].prefab = prefab;
            newPools[newPools.Length - 1].ferula = new ObjectPooling(); //создаем свой пул для каждого префаба
            newPools[newPools.Length - 1].ferula.Initialize(count, prefab, objectsParent.transform); //инициализируем пул заданным количество объектов

            pools = newPools;
        
        
    }

    void Awake()
    {
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

        rentalGOList = new List<RentalGO>();
        Initialize(poolsSetup, this.transform);
    }

    //инициализация всех пулов с указанием родителя
    private void Initialize(PoolPart[] newPools, Transform parent)
    {
        pools = newPools;
        objectsParent = new GameObject();
        objectsParent.name = "Pool"; //создаем на сцене объект Pool, чтобы не заслонять иерархию
        if (Application.isPlaying)
        {
            DontDestroyOnLoadManager.DontDestroyOnLoad(objectsParent); //Set as do not destroy            
        }
        //objectsParent.transform.parent = parent;
        for (int i = 0; i < pools.Length; i++)
        {
            if (pools[i].prefab != null)
            {
                pools[i].ferula = new ObjectPooling(); //создаем свой пул для каждого префаба
                pools[i].ferula.Initialize(pools[i].count, pools[i].prefab, objectsParent.transform); //инициализируем пул заданным количество объектов
            }
        }
    }

    private void ReturnRentalGO()
    {
        if (!returnRentalGOIdle)
        {
            StartCoroutine(CurReturnRentalGO());
        }
    }

    //куротина для возврата арендованных объектов
    private IEnumerator CurReturnRentalGO()
    {
        returnRentalGOIdle = true;
        while (rentalGOList != null && rentalGOList.Count > 0)
        {
            yield return new WaitForSeconds(0.1f);
            List<RentalGO> rentalGOListForDel = new List<RentalGO>();
            foreach (RentalGO item in rentalGOList)
            {
                //если прошло время аренды
                if (item.returnTime < Time.time)
                {
                    ReturnObjectToPool(item.GO);
                    rentalGOListForDel.Add(item);
                }
            }

            //удаляем из массива
            foreach (RentalGO item in rentalGOListForDel)
            {
                rentalGOList.Remove(item);
            }
        }

        returnRentalGOIdle = false;
    }

    //восстановление количества объектов в масссивах
    private void QuantityRecoveryGO()
    {
        if (!quantityRecoveryGOIdle)
        {
            StartCoroutine(CurQuantityRecoveryGO());
        }
    }

    //куротина для возврата арендованных объектов
    private IEnumerator CurQuantityRecoveryGO()
    {
        quantityRecoveryGOIdle = true;
        bool repeat = true;
        while (pools != null && pools.Length > 0 && repeat)
        {
            repeat = false;
            yield return new WaitForSeconds(0.1f);

            foreach (PoolPart item in pools)
            {
                //заполняем массив если в нем меньше объектов чем было при старте
                if (item.replenishment > 0 && item.ferula.Objects.Count < item.count)
                {
                    int replenishment = item.replenishment;
                    if (item.replenishment > item.count - item.ferula.Objects.Count)
                    {
                        replenishment = item.count - item.ferula.Objects.Count;
                    }
                    item.ferula.ExpandArray(replenishment);
                    repeat = true;
                }
            }
        }
        quantityRecoveryGOIdle = false;
    }

    //выдача объекта в аренду
    public GameObject GetObjectToRent(string name, Vector3 position, Transform parent, float rentalTime = 0)
    {
        GameObject result = null;
        if (pools != null)
        {
            for (int i = 0; i < pools.Length; i++)
            {
                if (string.Compare(pools[i].name, name) == 0)
                {
                    result = pools[i].ferula.GetObjectToRent().gameObject;
                    result.transform.SetParent(parent, false);
                    result.transform.position = position;
                    //result.transform.rotation = rotation;
                    result.SetActive(true);

                    //добавляем в массив аренды
                    if (rentalTime > 0)
                    {
                        rentalGOList.Add(new RentalGO(result, Time.time + rentalTime));
                        ReturnRentalGO();
                    }

                    return result;
                }
            }
        }
        return result; //если такого объекта нет в пулах, вернет null
    }

    //выдача объекта навсегда
    public GameObject GetObjectForever(string name, Vector3 position, Transform parent, float rentalTime = 0)
    {
        GameObject result = null;
        if (pools != null)
        {
            for (int i = 0; i < pools.Length; i++)
            {
                if (string.Compare(pools[i].name, name) == 0)
                {
                    result = pools[i].ferula.GetObjectForever().gameObject;
                    result.transform.SetParent(parent, false);
                    result.transform.position = position;
                    //result.transform.rotation = rotation;
                    result.SetActive(true);

                    //запускаем авто восстановление массива
                    QuantityRecoveryGO();

                    return result;
                }
            }
        }
        return result; //если такого объекта нет в пулах, вернет null
    }

    //возврат объекта в пул с возможной задеркой
    public void ReturnObjectToPool(GameObject GO, float delay = 0)
    {
        if (delay == 0)
        {
            if (GO != null)
            {
                GO.transform.SetParent(objectsParent.transform, false);
                GO.SetActive(false);
            }
        }
        else
        {
            rentalGOList.Add(new RentalGO(GO, Time.time + delay));
            ReturnRentalGO();
        }
        
    }

    //вернуть все объеты в пулы
    public void ReturnAllObjectToPool()
    {
        foreach (PoolPart item in pools)
        {
            foreach (GameObject GOItem in item.ferula.Objects)
            {
                ReturnObjectToPool(GOItem);
            }
        }
    }

    #endregion
}

//объек в аренде который должен быть возвращен через определенное количество времени
public class RentalGO
{
    public GameObject GO;
    public float returnTime;

    public RentalGO(GameObject gO, float returnTime)
    {
        GO = gO;
        this.returnTime = returnTime;
    }
}
