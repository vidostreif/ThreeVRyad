using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainGameSceneScript : MonoBehaviour {

    private float timeUnpause; //время когда нужно снять игру с паузы
    public static MainGameSceneScript Instance; // Синглтон
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
#if UNITY_EDITOR
            if (!LevelMenu.Instance.levelSaved)
            {
                LevelMenu.Instance.SaveXml(LevelMenu.Instance.LastLoadLevel);
                LevelMenu.Instance.levelSaved = true;
            }            
#endif
            LevelMenu.Instance.LoadXml(LevelMenu.Instance.LastLoadLevel);
        }
        else
        {
            ResetScene();
            Debug.Log("MainGameSceneScript вызов ResetScene()");
        }
        Prepare();
    }

    public void Prepare()
    {
        Time.timeScale = 1;        
        BorderGrid.CircleGrid(GridBlocks.Instance);//обводка сетки
        GridBlocks.Instance.StartFilling();//стартовое заполнение элементами  
        GridBlocks.Instance.FoundNextMove();//поиск хода

        if (HelpFromGnome.Instance.helpEnum != HelpEnum.Empty)
        {
            HelpToPlayer.AddHint(HelpFromGnome.Instance.helpEnum);//подсказка
        }
        //если не создали первую подсказку для уровня, то выполняем ход
        if (!HelpToPlayer.CreateNextGameHelp())
        {
            GridBlocks.Instance.Move();//выполнение хода
        }              
    }

    public void CompleteGame()
    {
        Time.timeScale = 1;
        HelpToPlayer.ClearHintList();//очищаем список подсказок
        InstrumentPanel.Instance.DeactivateInstrument();//деактивируем инструмент

        CanvasMenu = Instantiate(PrefabBank.CanvasEndGameMenu);
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

        Transform gONextLevelButton = PanelMenu.transform.Find("NextLevelButton");

        //передача аналитики
        Firebase.Analytics.Parameter[] LevelEndParameters = {          
          new Firebase.Analytics.Parameter(
            Firebase.Analytics.FirebaseAnalytics.ParameterLevelName, LevelMenu.Instance.lastLoadFolder + "" + LevelMenu.Instance.LastLoadLevel.xmlDocument.name),
          new Firebase.Analytics.Parameter(
            Firebase.Analytics.FirebaseAnalytics.ParameterSuccess, Tasks.Instance.collectedAll.ToString()),          
        };
        Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLevelEnd, LevelEndParameters);

        string nameEvent;
        if (Tasks.Instance.collectedAll)
        {
            nameEvent = "Level_passed";
        }
        else
        {
            nameEvent = "Level_loss";
        }

        Firebase.Analytics.FirebaseAnalytics.LogEvent(
          nameEvent,
          new Firebase.Analytics.Parameter[] {
            new Firebase.Analytics.Parameter("Stars", Score.Instance.NumberOfStarsReceived()),
            new Firebase.Analytics.Parameter(
              Firebase.Analytics.FirebaseAnalytics.ParameterLevel, LevelMenu.Instance.lastLoadFolder + "" + LevelMenu.Instance.LastLoadLevel.xmlDocument.name),
            new Firebase.Analytics.Parameter(
              Firebase.Analytics.FirebaseAnalytics.ParameterScore, Score.Instance.getScore()),
          }
        );

        Firebase.Analytics.FirebaseAnalytics.LogEvent("Score", LevelMenu.Instance.lastLoadFolder + "" + LevelMenu.Instance.LastLoadLevel.xmlDocument.name, Score.Instance.getScore());

        //запускаем куротину для постепенного отображения элементов
        //если выполнили все задания
        if (Tasks.Instance.collectedAll)
        {
            //победа
            textEndGame.text = "Победа!";            
                        
            //Выдаем звезды
            int stars = Score.Instance.NumberOfStarsReceived();
            LevelPassedResult levelPassedResult = LevelMenu.Instance.SetLevelPassed(stars, Score.Instance.getScore());

            StartCoroutine(EndGameAnimationStars(PanelMenu, levelPassedResult));

            //показываем количество очков

            

            if (LevelMenu.Instance.NextLevelIsOpen())
            {
                Button nextLevelButton = gONextLevelButton.GetComponent<Button>();
                nextLevelButton.onClick.AddListener(delegate { NextLevel(); });
            }
            else
            {
                Destroy(gONextLevelButton.gameObject);
            }

            JsonSaveAndLoad.SetSaveToFile();
        }
        else
        {
            //удаляем звезды и текст количества очков

            //поражение
            textEndGame.text = "Поражение!";
            Destroy(gONextLevelButton.gameObject);
        }
    }

    //анимация выдачи звезд в конце уровня
    private IEnumerator EndGameAnimationStars(Transform panelMenu, LevelPassedResult levelPassedResult)
    {
        for (int i = 1; i <= levelPassedResult.stars; i++)
        {
            Transform starTransform = panelMenu.transform.Find("Star" + i);
            Image starImage = starTransform.GetComponent(typeof(Image)) as Image;
            SupportFunctions.ChangeAlfa(starImage, 1);
            //создаем эффект 
            GameObject psGO = GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/PSCollectAll") as GameObject, starTransform);
            //изменяем спрайт у эффекта
            ParticleSystem ps = psGO.GetComponent<ParticleSystem>();
            ps.textureSheetAnimation.AddSprite(starImage.sprite);

            //!!!Звук выдачи звезды

            //анимация передачи монет в магазин
            if (true)
            {

            }


            yield return new WaitForSeconds(0.3f);
        }
    }

    ////анимация набора очков в конце уровня
    //private IEnumerator EndGameAnimationScore()
    //{

    //}

    private void ResetScene()
    {
        //Сбрасываем значения  
        Time.timeScale = 1;
        HelpToPlayer.ClearHintList();//очищаем список подсказок
        MainAnimator.Instance.ClearAllMassive();
        Tasks.Instance.ResetParameters();
        Score.Instance.ResetParameters();
        SuperBonus.Instance.ResetParameters();
        InstrumentPanel.Instance.ResetParameters();
        MainAnimator.Instance.ClearAllMassive();
    }

    public void RestartLevel()
    {
        Destroy(CanvasMenu);
        if (LevelMenu.Instance.LastLoadLevel != null)
        {
            //MainAnimator.Instance.ClearAllMassive();
            LevelMenu.Instance.LoadLevel(LevelMenu.Instance.LastLoadLevel);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
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
