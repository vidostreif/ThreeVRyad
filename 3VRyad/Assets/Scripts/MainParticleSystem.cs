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
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров MainParticleSystem!");
        }

        Instance = this;
    }
}
