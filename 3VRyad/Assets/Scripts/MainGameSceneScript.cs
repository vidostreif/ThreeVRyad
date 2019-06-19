using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainGameSceneScript : MonoBehaviour {

    private float timeUnpause; //время когда нужно снять игру с паузы
    public static MainGameSceneScript Instance; // Синглтон
    private GameObject CanvasMenu;

    private bool animationStarsIdle = false;
    private bool animationScoreIdle = false;
    private bool animationGiftIdle = false;
    private bool completeGameIdle = false;    

    //private IEnumerator animationStarsCompleteCur;
    //private IEnumerator animationScoreCompleteCur;
    //private IEnumerator animationGiftCompleteCur;

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
        GridBlocks.Instance.NextActionElementsAfterMove();//поиск следующего хода для активируемых элементов

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

    public void DestroyCanvasMenu() {

        StartCoroutine(CurDestroyCanvasMenu());
    }

    private IEnumerator CurDestroyCanvasMenu()
    {
        //ожидание завершения всех куротин
        do
        {
            if (animationStarsIdle || animationScoreIdle || animationGiftIdle || completeGameIdle)
            {
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                break;
            }
        } while (true);

        
        Destroy(CanvasMenu);
    }

    public IEnumerator CompleteGame(bool victory, bool nextMoveExists)
    {
        completeGameIdle = true;
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
        restartButton.onClick.AddListener(delegate { RestartLevelInEndGame(); });
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
        if (victory)
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
              Firebase.Analytics.FirebaseAnalytics.ParameterScore, Score.Instance.getScore)
          }
        );

        if (victory)
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

        //yield return new WaitForSeconds(0.15f);
        //запускаем куротину для постепенного отображения элементов
        //если выполнили все задания
        if (victory)
        {
            //победа
            textEndGame.text = "Победа!";
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Victory);
            GameObject psGO = GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/Rocket") as GameObject, CanvasMenu.transform);
            psGO.transform.position = new Vector3(-4, -5, 0);
            GameObject psGO2 = GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/Rocket") as GameObject, CanvasMenu.transform);
            psGO2.transform.position = new Vector3(4, -5, 0);
            //Выдаем звезды
            int stars = Score.Instance.NumberOfStarsReceived();
            LevelPassedResult levelPassedResult = LevelMenu.Instance.SetLevelPassed(stars, Score.Instance.getScore);

            //показываем звезды
            StartCoroutine(EndGameAnimationStars(PanelMenu, levelPassedResult));

            //показываем количество очков
            StartCoroutine(EndGameAnimationScore(PanelMenu, levelPassedResult));

            //показываем подарки
            StartCoroutine(EndGameAnimationGift(PanelMenu, levelPassedResult));

            if (!LevelMenu.Instance.NextLevelIsOpen())
            {
                Destroy(gONextLevelButton.gameObject);
            }
            else
            {
                SupportFunctions.ChangeAlfa(gONextLevelButton.GetComponent<Image>(), 1);
            }

            JsonSaveAndLoad.SetSaveToFile();
        }
        else
        {
            //если лвл опциональный, то открываем следующий лвл
            if (LevelSettings.Instance.Optional)
            {
                LevelMenu.Instance.SetOpenNextLevel();
            }
            else
            {
                LifeManager.Instance.SubLive();
            }

            //поражение
            if (!nextMoveExists)//если не смогли найти следующий ход
            {
                textEndGame.text = "Это тупик!";
                if (!LevelMenu.Instance.NextLevelIsOpen())
                {
                    Destroy(gONextLevelButton.gameObject);
                }
            }
            else
            {
                //если еще не добавляли ходов
                if (!Tasks.Instance.addMovesOnEndGAme && !LevelSettings.Instance.Optional)
                {
                    textEndGame.text = "У вас закончились ходы! Добавим за просмотр видео?";
                    //создаем кнопку видео на месте загрузки следующего уровня    
                    //SupportFunctions.ChangeAlfa(gONextLevelButton.GetComponent<Image>(), 0);
                    AdMobManager.Instance.GetVideoBrowseButton(gONextLevelButton, VideoForFeeEnum.ForMove);
                }
                else
                {
                    textEndGame.text = "У вас закончились ходы!";
                    if (!LevelMenu.Instance.NextLevelIsOpen())
                    {
                        Destroy(gONextLevelButton.gameObject);
                    }
                }              
            }
            
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Defeat);
            //Destroy(gONextLevelButton.gameObject);
            //удаляем текст количества очков и панель подарков
            Destroy(PanelMenu.transform.Find("PanelGift").gameObject);
            Destroy(PanelMenu.transform.Find("TextScore").gameObject);
        }

        //ждем до тех пор
        do
        {
            //если уничтожили канвас
            if (CanvasMenu == null)
            {
                break;
            }
            //если все анимации закончили свои действия активируем кнопки
            if (!animationStarsIdle && !animationScoreIdle && !animationGiftIdle)
            {
                restartButton.interactable = true;
                exitButton.interactable = true;
                if (victory && LevelMenu.Instance.NextLevelIsOpen())
                {
                    nextLevelButton.interactable = true;
                }
                break;
            }
            yield return new WaitForSeconds(0.2f);
        } while (true);

        completeGameIdle = false;
    }

    //анимация выдачи звезд в конце уровня
    private IEnumerator EndGameAnimationStars(Transform panelMenu, LevelPassedResult levelPassedResult)
    {
        animationStarsIdle = true;
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
            ParticleSystemManager.Instance.CreateCollectAllEffect(starTransform, starImage);
            //Звук выдачи звезды
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

        animationStarsIdle = false;
    }

    ////анимация набора очков в конце уровня
    private IEnumerator EndGameAnimationScore(Transform panelMenu, LevelPassedResult levelPassedResult)
    {
        animationScoreIdle = true;
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
                ParticleSystemManager.Instance.CreateCollectEffect(imageNewScore.transform, imageNewScore);
            }

            yield return new WaitForEndOfFrame();
        } while (score > 0);

        animationScoreIdle = false;
    }

    //анимация выдачи подарков в конце уровня
    private IEnumerator EndGameAnimationGift(Transform panelMenu, LevelPassedResult levelPassedResult)
    {
        animationGiftIdle = true;
        //находим панель для выдачи подарков
        Transform panelGiftTransform = panelMenu.transform.Find("PanelGift");
        //находим закрытый ящик 
        Transform CloseGiftTransform = panelMenu.transform.Find("ImageCloseGiftBox");
        if (levelPassedResult.giftOptions.displayBox)
        {
            if (levelPassedResult.giftOptions.BoxOpen)
            {
                Gift gift = levelPassedResult.giftOptions.gift;
                int giftLength = 0;
                if (gift.Coins > 0)
                {
                    giftLength++;
                }
                if (gift.TimeImmortalLives > 0)
                {
                    giftLength++;
                }
                if (gift.Bundel.Length > 0)
                {
                    giftLength += gift.Bundel.Length;
                }

                //если есть подарки то продолжаем
                if (giftLength > 0)
                {
                    yield return new WaitForSeconds(0.2f);
                    SupportFunctions.ChangeAlfa(panelGiftTransform.GetComponent<Image>(), 1);
                    yield return new WaitForSeconds(0.2f);
                    SoundManager.Instance.PlaySoundInternal(SoundsEnum.Magic);
                    yield return new WaitForSeconds(0.8f);
                    //находим наш сундук
                    Transform imageOpenGiftBoxTransform = panelGiftTransform.Find("ImageOpenGiftBox");

                    //смещение по x
                    float startingXPoint = panelGiftTransform.position.x - ((1 + 0.5f) * (giftLength - 1)) * 0.5f;

                    //показываем подарки
                    for (int i = 0; i < gift.Bundel.Length; i++)
                    {
                        if (gift.Bundel[i].count > 0)
                        {
                            Vector3 startPosition = new Vector3(imageOpenGiftBoxTransform.position.x, imageOpenGiftBoxTransform.position.y - 0.5f, imageOpenGiftBoxTransform.position.z);
                            Vector3 newPosition = new Vector3(startingXPoint + (i * (1 + 0.5f)), panelGiftTransform.position.y, panelGiftTransform.position.z);

                            yield return StartCoroutine(Shop.Instance.CreateThingAnimation(startPosition, panelGiftTransform, gift.Bundel[i].type, gift.Bundel[i].count, newPosition));
                        }
                        yield return new WaitForSeconds(0.1f);
                    }

                    float numberGifts = gift.Bundel.Length;
                    //показываем монеты
                    if (gift.Coins > 0)
                    {
                        Vector3 startPosition = new Vector3(imageOpenGiftBoxTransform.position.x, imageOpenGiftBoxTransform.position.y - 0.5f, imageOpenGiftBoxTransform.position.z);
                        Vector3 newPosition = new Vector3(startingXPoint + (numberGifts * (1 + 0.5f)), panelGiftTransform.position.y, panelGiftTransform.position.z);

                        yield return StartCoroutine(Shop.Instance.CreateCoinAnimation(startPosition, panelGiftTransform, gift.Coins, newPosition));

                        numberGifts++;
                        yield return new WaitForSeconds(0.1f);
                    }

                    //показываем жизни
                    if (gift.TimeImmortalLives > 0)
                    {
                        Vector3 startPosition = new Vector3(imageOpenGiftBoxTransform.position.x, imageOpenGiftBoxTransform.position.y - 0.5f, imageOpenGiftBoxTransform.position.z);
                        Vector3 newPosition = new Vector3(startingXPoint + (numberGifts * (1 + 0.5f)), panelGiftTransform.position.y, panelGiftTransform.position.z);

                        yield return StartCoroutine(Shop.Instance.CreateLivesAnimation(startPosition, panelGiftTransform, gift.TimeImmortalLives, newPosition));

                        yield return new WaitForSeconds(0.1f);
                    }

                    yield return new WaitForSeconds(0.15f);
                }
                else
                {
                    Destroy(panelGiftTransform.gameObject);
                    Destroy(CloseGiftTransform.gameObject);
                }
            }
            else
            {
                //находим закрытый ящик и показываем его
                //SupportFunctions.ChangeAlfa(CloseGiftTransform.GetComponent<Image>(), 1);
                CloseGiftTransform.GetComponent<Animation>().Play();               
                Destroy(panelGiftTransform.gameObject);
                yield return new WaitForSeconds(1.3f);
            }
            
        }
        else
        {
            Destroy(panelGiftTransform.gameObject);
            Destroy(CloseGiftTransform.gameObject);
        }      

        animationGiftIdle = false;
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

    //запрос на перезапуск сцены во время игры
    public void RequestRestartLevel() {

        if (LifeManager.Instance.Immortal())
        {
            RestartLevel();
        }
        else if (LifeManager.Instance.Life > 1)
        {
            SupportFunctions.CreateYesNoPanel(GameObject.Find("GameHelper").transform, "При перезапуске игры вы потеряете жизнь. Перезапустить?", RestartLevelInGame);
        }
        else
        {
            SupportFunctions.CreateInformationPanelWithVideo("Вам нужно больше одной жизни, что бы перезапустить уровень во время игры. Подождите немного или посмотрите видео за одну жизнь!", VideoForFeeEnum.ForLive, Shop.Instance.transform);
        }        
    }

    //перезапуск сцены с уменьшением количества жизней
    public void RestartLevelInGame()
    {
        if (LifeManager.Instance.Life > 1)
        {
            LifeManager.Instance.SubLive();
            RestartLevel();            
        }
        else
        {
            SupportFunctions.CreateInformationPanelWithVideo("Вам нужно больше одной жизни, что бы перезапустить уровень во время игры. Подождите немного или посмотрите видео за одну жизнь!", VideoForFeeEnum.ForLive, Shop.Instance.transform);
        }
    }

    //перезапуск сцены по окончанию игры
    public void RestartLevelInEndGame()
    {
        if (LifeManager.Instance.Life > 0)
        {
            RestartLevel();
        }
        else
        {
            SupportFunctions.CreateInformationPanelWithVideo("У вас недостаточно жизней, что бы перезапустить уровень. Подождите немного или посмотрите видео за одну жизнь!", VideoForFeeEnum.ForLive, Shop.Instance.transform);
        }
    }

    //перезапуск сцены
    public void RestartLevel()
    {
            Destroy(CanvasMenu);        

            //if (LevelMenu.Instance.LastLoadLevel != null)
            //{
                LevelMenu.Instance.LoadLevel(LevelMenu.Instance.LastLoadLevel);
            //}
            //else
            //{
            //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            //}     
    }

    public void NextLevel()
    {
        if (LifeManager.Instance.Life > 0)
        {
            Destroy(CanvasMenu);
            LevelMenu.Instance.LoadNextLevel();
        }
        else
        {
            SupportFunctions.CreateInformationPanelWithVideo("У вас недостаточно жизней, что бы перейти на следующий уровень. Подождите немного или посмотрите видео за одну жизнь!", VideoForFeeEnum.ForLive, Shop.Instance.transform);
        }       
    }

    //запрос на выход в меню в время игры
    public void RequestExitToMenu()
    {
        if (LifeManager.Instance.Immortal())
        {
            ExitToMenu();
        }
        else if (LifeManager.Instance.Life > 0)
        {
            SupportFunctions.CreateYesNoPanel(GameObject.Find("GameHelper").transform, "При выходе в меню вы потеряете жизнь. Выйти?", ExitToMenuInGame);
        }
        else
        {
            ExitToMenu();
        }        
    }

    //выход в меню с уменьшением количества жизней
    public void ExitToMenuInGame()
    {
        LifeManager.Instance.SubLive();
        ExitToMenu();
    }

    //выход в меню
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
