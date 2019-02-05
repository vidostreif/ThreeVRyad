using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneScript : MonoBehaviour {

    //private bool pause; //признак что игра на паузе
    private float timeUnpause; //время когда нужно снять игру с паузы
    public static MainSceneScript Instance; // Синглтон

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров MainSceneScript!");
        }

        Instance = this;
    }

    void Start()
    {
        Grid.Instance.StartFilling();
        Grid.Instance.Move();
    }

    // Update is called once per frame
    void Update () {

	}

    public void Pause(float time)
    {
        StartCoroutine(Waiter(time));        
    }

    IEnumerator Waiter(float time)
    {
        //Wait for seconds
        yield return new WaitForSeconds(time);
    }
}
