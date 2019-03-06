using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainSceneScript : MonoBehaviour {

    private float timeUnpause; //время когда нужно снять игру с паузы
    public static MainSceneScript Instance; // Синглтон
    public GameObject prefabCanvasEndGameMenu;
    private GameObject CanvasMenu;

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
        //восстанавливаем настройки сцены
        if (LevelMenu.Instance.LastLoadLevel != null)
        {
            LevelMenu.Instance.LoadXml(LevelMenu.Instance.LastLoadLevel);
        }
        Prepare();
    }

    public void Prepare() {
        Tasks.Instance.UpdateMovesText();
        Tasks.Instance.CreateCollectedElements();
        BorderGrid.CircleGrid(GridBlocks.Instance);
        GridBlocks.Instance.StartFilling();
        GridBlocks.Instance.Move();
    }

    public void CompleteGame()
    {
        CanvasMenu = Instantiate(prefabCanvasEndGameMenu);
        Transform PanelMenu = CanvasMenu.transform.Find("Panel");
        Transform gOtextEndGame = PanelMenu.transform.Find("TextEndGame");
        Text textEndGame = gOtextEndGame.GetComponent(typeof(Text)) as Text;

        //добавляем действие к кнопкам
        Transform gORestartButton = PanelMenu.transform.Find("RestartButton");
        Button restartButton = gORestartButton.GetComponent<Button>();
        restartButton.onClick.AddListener(delegate { RestartLevel(); });

        Transform gOExitButton = PanelMenu.transform.Find("ExitButton");
        Button exitButton = gOExitButton.GetComponent<Button>();
        exitButton.onClick.AddListener(delegate { ExitToMenu(); });

        //если выполнили все задания
        if (Tasks.Instance.collectedAll)
        {
            //победа
            textEndGame.text = "Победа!";
            LevelMenu.Instance.SetLevelPassed();

            Transform gONextLevelButton = PanelMenu.transform.Find("NextLevelButton");
            if (LevelMenu.Instance.NextLevelIsOpen())
            {
                Button nextLevelButton = gONextLevelButton.GetComponent<Button>();
                nextLevelButton.onClick.AddListener(delegate { NextLevel(); });
            }
            else
            {
                Destroy(gONextLevelButton.gameObject);
            }
        }
        else
        {
            //поражение
            textEndGame.text = "Поражение!";
        }
    }

    public void RestartLevel()
    {
        Destroy(CanvasMenu);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        LevelMenu.Instance.LoadLevel(LevelMenu.Instance.LastLoadLevel);
    }

    public void NextLevel()
    {
        Destroy(CanvasMenu);
        LevelMenu.Instance.LoadNextLevel();
    }

    public void ExitToMenu()
    {
        LevelMenu.Instance.LoadMainMenu();
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
