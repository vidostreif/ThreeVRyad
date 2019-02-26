using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainSceneScript : MonoBehaviour {

    private float timeUnpause; //время когда нужно снять игру с паузы
    public static MainSceneScript Instance; // Синглтон
    public GameObject prefabCanvasEndGameMenu;

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
        GridBlocks.Instance.StartFilling();
        GridBlocks.Instance.Move();
    }

    public void CompleteGame() {

        GameObject CanvasMenu = Instantiate(prefabCanvasEndGameMenu);
        Transform PanelMenu = CanvasMenu.transform.Find("Panel");
        Transform gOtextEndGame = PanelMenu.transform.Find("TextEndGame");
        Text textEndGame = gOtextEndGame.GetComponent(typeof(Text)) as Text;

        //добавляем действие к кнопке
        Transform gORestartButton = PanelMenu.transform.Find("RestartButton");
        Button restartButton = gORestartButton.GetComponent<Button>();
        restartButton.onClick.AddListener(delegate { RestartLevel(); });

        //если выполнили все задания
        if (Tasks.Instance.collected)
        {
            //победа
            textEndGame.text = "Победа!";
        }
        else
        {
            //поражение
            textEndGame.text = "Поражение!";
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
