using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMes : MonoBehaviour
{
    public string mes = "Сообщение!";
    public static DebugMes Instance; // Синглтон

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров DebugMes!");
        }
        Instance = this;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(330, 120, 100, 20), mes);
    }
}
