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
        CanvasMenu.GetComponent<Canvas>().worldCamera = Camera.main;
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
              Firebase.Analytics.FirebaseAnalytics.ParameterScore, Score.Instance.getScore())
          }
        );

        if (Tasks.Instance.collectedAll)
        {
            int stars = Score.Instance.NumberOfStarsReceived();

            Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "Stars" + stars,
            new Firebase.Analytics.Parameter[] {
            new Firebase.Analytics.Parameter(
              Firebase.Analytics.FirebaseAnalytics.ParameterLevel, LevelMenu.Instance.lastLoadFolder + "" + LevelMenu.Instance.LastLoadLevel.xmlDocument.name),
            }
            );
        }

        //запускаем куротину для постепенного отображения элементов
        //если выполнили все задания
        if (Tasks.Instance.collectedAll)
        {
            //победа
            textEndGame.text = "Победа!";            
                        
            //Выдаем звезды
            int stars = Score.Instance.NumberOfStarsReceived();
            LevelPassedResult levelPassedResult = LevelMenu.Instance.SetLevelPassed(stars, Score.Instance.getScore());

            //показываем звезды
            StartCoroutine(EndGameAnimationStars(PanelMenu, levelPassedResult));

            //показываем количество очков
            StartCoroutine(EndGameAnimationScore(PanelMenu, levelPassedResult));

            //показываем подарки
            StartCoroutine(EndGameAnimationGift(PanelMenu, levelPassedResult));

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
        //номер звезды с которой начинаем выдавать монеты
        int numberStarForCoin = levelPassedResult.stars - levelPassedResult.plusStars + 1;

        //количество монет за одну звезду
        int coinsForOneStar = 0;
        if (levelPassedResult.plusStars > 0)
        {
            coinsForOneStar = levelPassedResult.plusCoins / levelPassedResult.plusStars;
        }

        //Находим монету в магазине
        GameObject imageCoinsGO = GameObject.Find("ImageCoins");

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

            //анимация выдачи монет за звезды
            if (numberStarForCoin <= i)
            {
                //создаем монеты
                for (int j = 0; j < coinsForOneStar; j++)
                {
                    GameObject coinGO = GameObject.Instantiate(Resources.Load("Prefabs/Canvas/GameCanvas/ImageCoin") as GameObject, starTransform);
                    //перемещаем чуть выше
                    MainAnimator.Instance.AddElementForSmoothMove(coinGO.transform, new Vector3(starTransform.position.x - 0.5f + j * 0.5f, starTransform.position.y + 1, starTransform.position.z), 1, SmoothEnum.InLineWithSlowdown, 0.05f, false, true);
                    //перемещаем к монете в магазине
                    MainAnimator.Instance.AddElementForSmoothMove(coinGO.transform, imageCoinsGO.transform.position, 1, SmoothEnum.InLineWithSlowdown, 0.05f, true, true);
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    ////анимация набора очков в конце уровня
    private IEnumerator EndGameAnimationScore(Transform panelMenu, LevelPassedResult levelPassedResult)
    {
        Transform textScoreTransform = panelMenu.transform.Find("TextScore");
        Text textScore = textScoreTransform.GetComponent(typeof(Text)) as Text;
        Image imageNewScore = textScoreTransform.transform.Find("ImageNewScore").GetComponent<Image>();

        int score = levelPassedResult.score;
        int newScore = 0;
        int plusScore = levelPassedResult.plusScore;
        int oldScore = levelPassedResult.score - levelPassedResult.plusScore;

        bool createdNewScore = false;
        do
        {
            if (score > 50)
            {
                int i = score / 15;
                newScore += i;
                score -= i;
                textScore.text = "" + newScore;
            }
            else if (score > 0)
            {
                newScore += 1;
                score -= 1;
                textScore.text = "Score: " + newScore;
            }

            //показываем newscore если увеличили рекорд
            if (!createdNewScore && newScore > oldScore)
            {
                createdNewScore = true;
                //показываем надпись new
                SupportFunctions.ChangeAlfa(imageNewScore, 1);

                //создаем эффект 
                GameObject psGO = GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/PSCollectAll") as GameObject, imageNewScore.transform);
                //изменяем спрайт у эффекта
                ParticleSystem ps = psGO.GetComponent<ParticleSystem>();
                ps.textureSheetAnimation.AddSprite(imageNewScore.sprite);
            }

            yield return new WaitForEndOfFrame();
        } while (score > 0);
    }

    //анимация выдачи звезд в конце уровня
    private IEnumerator EndGameAnimationGift(Transform panelMenu, LevelPassedResult levelPassedResult)
    {
        int giftLength = 0;
        if (levelPassedResult.gift.coins > 0)
        {
            giftLength++;
        }
        if (levelPassedResult.gift.bundel.Length > 0)
        {
            giftLength += levelPassedResult.gift.bundel.Length;
        }

        Transform panelGiftTransform = panelMenu.transform.Find("PanelGift");

        //если есть подарки то продолжаем
        if (giftLength > 0)
        {
            //смещение по x
            float startingXPoint = panelGiftTransform.position.x - ((1 + 0.5f) * (giftLength-1)) * 0.5f;

            int startPoint = 0;
            if (levelPassedResult.gift.coins > 0)
            {
                startPoint = 1;
                //Находим монету в магазине
                GameObject imageCoinsGO = GameObject.Find("ImageCoins");

                //показываем монету среди подарков
                GameObject giftCoinGO = Instantiate(PrefabBank.PrefabButtonThing, new Vector3(startingXPoint, panelGiftTransform.position.y, panelGiftTransform.position.z), Quaternion.identity, panelGiftTransform);
                giftCoinGO.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/coin") as Sprite;
                giftCoinGO.GetComponentInChildren<Text>().text = "+" + levelPassedResult.gift.coins;

                //создаем монеты
                for (int j = 0; j < levelPassedResult.gift.coins; j++)
                {
                    GameObject coinGO = GameObject.Instantiate(Resources.Load("Prefabs/Canvas/GameCanvas/ImageCoin") as GameObject, giftCoinGO.transform);

                    //перемещаем к монете в магазине
                    MainAnimator.Instance.AddElementForSmoothMove(coinGO.transform, imageCoinsGO.transform.position, 1, SmoothEnum.InLineWithSlowdown, 0.05f, true, true);

                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForSeconds(0.3f);
            }

            //показываем подарки
            for (int i = 0; i < levelPassedResult.gift.bundel.Length; i++)
            {
                if (levelPassedResult.gift.bundel[i].count > 0)
                {
                    GameObject go = Instantiate(PrefabBank.PrefabButtonThing, new Vector3(startingXPoint + ((i + startPoint) * (1 + 0.5f)), panelGiftTransform.position.y, panelGiftTransform.position.z), Quaternion.identity, panelGiftTransform);
                    go.GetComponent<Image>().sprite = SpriteBank.SetShape(levelPassedResult.gift.bundel[i].type);
                    go.GetComponentInChildren<Text>().text = "+" + levelPassedResult.gift.bundel[i].count;
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            Destroy(panelGiftTransform.gameObject);
        }
        
        
    }

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
