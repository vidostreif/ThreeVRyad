//using UnityEngine;
//using System.Collections;

//[AddComponentMenu("Pool/PoolSetup")]
//public class PoolSetup : MonoBehaviour
//{//обертка для управления статическим классом PoolManager

//    public static PoolSetup Instance; // Синглтон
//    #region Unity scene settings
//    [SerializeField] private PoolManager.PoolPart[] pools;
//    #endregion

//    #region Methods
//    void OnValidate()
//    {
//        for (int i = 0; i < pools.Length; i++)
//        {
//            pools[i].name = pools[i].prefab.name;
//        }
//    }

//    void Awake()
//    {
//        if (Instance)
//        {
//            Destroy(this.gameObject); //Delete duplicate
//            return;
//        }
//        else
//        {
//            Instance = this; //Make this object the only instance            
//        }
//        if (Application.isPlaying)
//        {
//            DontDestroyOnLoadManager.DontDestroyOnLoad(gameObject); //Set as do not destroy            
//        }

//        Initialize();
//    }

//    void Initialize()
//    {
//        PoolManager.Initialize(pools, this.transform);
//    }
//    #endregion
//}
