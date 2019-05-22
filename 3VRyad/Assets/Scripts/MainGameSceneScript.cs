using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainGameSceneScript : MonoBehaviour {

    private float timeUnpause; //время когда нужно снять игру с паузы
    public static MainGameSceneScript Instance; // Синглтон
    private GameObject CanvasMenu;

    bool animationStarsComplete = false;
    bool animationScoreComplete = false;
    bool animationGiftComplete = false;

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

    public IEnumerator CompleteGame()
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
        restartButton.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
        restartButton.onClick.AddListener(delegate { RestartLevel(); });
        restartButton.interactable = false;

        Transform gOExitButton = PanelMenu.transform.Find("ExitButton");
        Button exitButton = gOExitButton.GetComponent<Button>();
        exitButton.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
        exitButton.onClick.AddListener(delegate { ExitToMenu(); });
        exitButton.interactable = false;

        Transform gONextLevelButton = PanelMenu.transform.Find("NextLevelButton");
        Button nextLevelButton = gONextLevelButton.GetComponent<Button>();
        nextLevelButton.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
        nextLevelButton.onClick.AddListener(delegate { NextLevel(); });
        nextLevelButton.interactable = false;

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
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Victory);
            GameObject psGO = GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/Rocket") as GameObject, CanvasMenu.transform);
            psGO.transform.position = new Vector3(0, -3, 0);
            //Выдаем звезды
            int stars = Score.Instance.NumberOfStarsReceived();
            LevelPassedResult levelPassedResult = LevelMenu.Instance.SetLevelPassed(stars, Score.Instance.getScore());

            //показываем звезды
            animationStarsComplete = false;
            StartCoroutine(EndGameAnimationStars(PanelMenu, levelPassedResult));

            //показываем количество очков
            animationScoreComplete = false;
            StartCoroutine(EndGameAnimationScore(PanelMenu, levelPassedResult));

            //показываем подарки
            animationGiftComplete = false;
            StartCoroutine(EndGameAnimationGift(PanelMenu, levelPassedResult));

            if (!LevelMenu.Instance.NextLevelIsOpen())
            {
                Destroy(gONextLevelButton.gameObject);
            }

            JsonSaveAndLoad.SetSaveToFile();
        }
        else
        {
            //animationStarsComplete = true;
            //animationScoreComplete = true;
            //animationGiftComplete = true;
            
            //поражение
            textEndGame.text = "Поражение!";
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Defeat);
            Destroy(gONextLevelButton.gameObject);
            //удаляем текст количества очков и панель подарков
            Destroy(PanelMenu.transform.Find("PanelGift").gameObject);
            Destroy(PanelMenu.transform.Find("TextScore").gameObject);
        }

        //ждем до тех пор
        do
        {
            //если все анимации закончили свои действия активируем кнопки
            if ((animationStarsComplete && animationScoreComplete && animationGiftComplete) || !Tasks.Instance.collectedAll)
            {
                restartButton.interactable = true;
                exitButton.interactable = true;
                if (Tasks.Instance.collectedAll && LevelMenu.Instance.NextLevelIsOpen())
                {
                    nextLevelButton.interactable = true;
                }
                break;
            }
            yield return new WaitForSeconds(0.2f);
        } while (true);
    }

    //анимация выдачи звезд в конце уровня
    private IEnumerator EndGameAnimationStars(Transform panelMenu, LevelPassedResult levelPassedResult)
    {
        yield return new WaitForSeconds(0.4f);
        //номер звезды с которой начинаем выдавать монеты
        int numberStarForCoin = levelPassedResult.stars - levelPassedResult.plusStars + 1;

        //количество монет за одну звезду
        int coinsForOneStar = 0;
        if (levelPassedResult.plusStars > 0)
        {
            coinsForOneStar = levelPassedResult.plusCoins / levelPassedResult.plusStars;
        }

        //Находим монету в магазине
        GameObject shopImageCoinsGO = GameObject.Find("ImageCoins");

        for (int i = 1; i <= levelPassedResult.stars; i++)
        {
            Transform starTransform = panelMenu.transform.Find("Star" + i);
            Image starImage = starTransform.GetComponent(typeof(Image)) as Image;
            SupportFunctions.ChangeAlfa(starImage, 1);
            //создаем эффект 
            GameObject psGO = GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/PSCollectAll") as GameObject, starTransform);
            GameObject.Destroy(psGO, 4);
            //изменяем спрайт у эффекта
            ParticleSystem ps = psGO.GetComponent<ParticleSystem>();
            ps.textureSheetAnimation.AddSprite(starImage.sprite);

            //!!!Звук выдачи звезды
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Star);
            //если выдаем третью звезду
            if (i == 3)
            {
                SoundManager.Instance.PlaySoundInternal(SoundsEnum.Applause, true);
            }

            //анимация выдачи монет за звезды
            if (numberStarForCoin <= i)
            {
                //создаем монеты
                for (int j = 0; j < coinsForOneStar; j++)
                {
                    GameObject coinGO = GameObject.Instantiate(Resources.Load("Prefabs/Canvas/GameCanvas/ImageCoin") as GameObject, starTransform.position, Quaternion.identity, shopImageCoinsGO.transform);
                    //перемещаем чуть выше
                    MainAnimator.Instance.AddElementForSmoothMove(coinGO.transform, new Vector3(starTransform.position.x - 0.5f + j * 0.5f, starTransform.position.y + 1, starTransform.position.z), 1, SmoothEnum.InLineWithSlowdown, 0.05f, false, true);
                    //перемещаем к монете в магазине
                    MainAnimator.Instance.AddElementForSmoothMove(coinGO.transform, shopImageCoinsGO.transform.position, 1, SmoothEnum.InLineWithOneSpeed, 1.35f, true, true, delegate { Shop.Instance.CoinFlew(1); });
                }
            }

            yield return new WaitForSeconds(0.7f);
        }

        animationStarsComplete = true;
    }

    ////анимация набора очков в конце уровня
    private IEnumerator EndGameAnimationScore(Transform panelMenu, LevelPassedResult levelPassedResult)
    {
        Transform textScoreTransform = panelMenu.transform.Find("TextScore");
        Text textScore = textScoreTransform.GetComponent(typeof(Text)) as Text;
        Image imageNewScore = textScoreTransform.transform.Find("ImageNewScore").GetComponent<Image>();

        int score = levelPassedResult.score;
        int newScore = 0;
        textScore.text = "Score: " + newScore;
        //int plusScore = levelPassedResult.plusScore;
        int oldScore = levelPassedResult.score - levelPassedResult.plusScore;

        bool createdNewScore = false;
        yield return new WaitForSeconds(0.2f);
        do
        {
            if (score > 50)
            {
                int i = score / 15;
                newScore += i;
                score -= i;
                textScore.text = "Score: " + newScore;
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
                GameObject psGO = GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/PSCollect") as GameObject, imageNewScore.transform);
                GameObject.Destroy(psGO, 4);
                //изменяем спрайт у эффекта
                ParticleSystem ps = psGO.GetComponent<ParticleSystem>();
                ps.textureSheetAnimation.AddSprite(imageNewScore.sprite);
            }

            yield return new WaitForEndOfFrame();
        } while (score > 0);

        animationScoreComplete = true;
    }

    //анимация выдачи подарков в конце уровня
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
            yield return new WaitForSeconds(0.4f);
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Magic);
            yield return new WaitForSeconds(0.8f);
            //находим наш сундук
            Transform imageOpenGiftBoxTransform = panelGiftTransform.Find("ImageOpenGiftBox");

            //смещение по x
            float startingXPoint = panelGiftTransform.position.x - ((1 + 0.5f) * (giftLength-1)) * 0.5f;

            //показываем подарки
            for (int i = 0; i < levelPassedResult.gift.bundel.Length; i++)
            {
                if (levelPassedResult.gift.bundel[i].count > 0)
                {
                    SoundManager.Instance.PlaySoundInternal(SoundsEnum.Ring_1);
                    GameObject go = Instantiate(PrefabBank.PrefabButtonThing, new Vector3(imageOpenGiftBoxTransform.position.x, imageOpenGiftBoxTransform.position.y - 0.5f, imageOpenGiftBoxTransform.position.z), Quaternion.identity, panelGiftTransform);
                    go.GetComponent<Image>().sprite = SpriteBank.SetShape(levelPassedResult.gift.bundel[i].type);
                    go.GetComponentInChildren<Text>().text = "+" + levelPassedResult.gift.bundel[i].count;
                    //перемещаем на свою позицию
                    MainAnimator.Instance.AddElementForSmoothMove(go.transform, new Vector3(startingXPoint + (i * (1 + 0.5f)), panelGiftTransform.position.y, panelGiftTransform.position.z), 1, SmoothEnum.InLineWithSlowdown, 0.05f, false, true);

                }
                yield return new WaitForSeconds(0.3f);
            }

            //int startPoint = 0;
            if (levelPassedResult.gift.coins > 0)
            {
                //startPoint = 1;
                //Находим монету в магазине
                GameObject shopImageCoinsGO = GameObject.Find("ImageCoins");

                SoundManager.Instance.PlaySoundInternal(SoundsEnum.Ring_1);

                //показываем монету среди подарков
                //создаем рядом с сундуком
                GameObject giftCoinGO = Instantiate(PrefabBank.PrefabButtonThing, new Vector3(imageOpenGiftBoxTransform.position.x, imageOpenGiftBoxTransform.position.y - 0.5f, imageOpenGiftBoxTransform.position.z), Quaternion.identity, panelGiftTransform);
                giftCoinGO.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/coin") as Sprite;
                giftCoinGO.GetComponentInChildren<Text>().text = "+" + levelPassedResult.gift.coins;

                //перемещаем на свою позицию
                MainAnimator.Instance.AddElementForSmoothMove(giftCoinGO.transform, new Vector3(startingXPoint + (levelPassedResult.gift.bundel.Length * (1 + 0.5f)), panelGiftTransform.position.y, panelGiftTransform.position.z), 1, SmoothEnum.InLineWithSlowdown, 0.05f, false, true);

                //создаем монеты рядом с монетой подарком
                for (int j = 0; j < levelPassedResult.gift.coins; j++)
                {
                    GameObject coinGO = GameObject.Instantiate(Resources.Load("Prefabs/Canvas/GameCanvas/ImageCoin") as GameObject, giftCoinGO.transform.position, Quaternion.identity, shopImageCoinsGO.transform);

                    //перемещаем к монете в магазин
                    MainAnimator.Instance.AddElementForSmoothMove(coinGO.transform, shopImageCoinsGO.transform.position, 1, SmoothEnum.InLineWithOneSpeed, 1.05f, true, true, delegate { Shop.Instance.CoinFlew(1); });

                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForSeconds(0.2f);
            }

            
        }
        else
        {
            Destroy(panelGiftTransform.gameObject);
        }

        animationGiftComplete = true;
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
