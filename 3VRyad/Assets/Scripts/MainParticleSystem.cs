using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainParticleSystem : MonoBehaviour
{
    public static MainParticleSystem Instance; // Синглтон
    public GameObject pSMagicalTail;
    public GameObject pSSelect;
    public GameObject pSAddPowerSuperBonus;
    public GameObject pSBeatsSuperBonus;

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
            DontDestroyOnLoad(gameObject); //Set as do not destroy
        }
        //// регистрация синглтона
        //if (Instance != null)
        //{
        //    Debug.LogError("Несколько экземпляров MainParticleSystem!");
        //}

        //Instance = this;
    }
}
