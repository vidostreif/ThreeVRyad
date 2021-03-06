﻿using System.Collections;
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

    private GameObject PSRocket1;
    private GameObject PSRocket2;

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
        StartCoroutine(CurPrepare());
    }

    private IEnumerator CurPrepare()
    {
        Time.timeScale = 1;
        transform.Find("BackGround").GetComponent<Image>().sprite = LevelMenu.Instance.GetBackGroundSprite(LevelMenu.Instance.LastLoadLevel);
        BorderGrid.CircleGrid(GridBlocks.Instance);//обводка сетки
        GridBlocks.Instance.StartFilling();//стартовое заполнение элементами  
        yield return StartCoroutine(GridBlocks.Instance.FoundNextMove(true));//поиск хода
        GridBlocks.Instance.NextActionElementsAfterMove();//поиск следующего хода для активируемых элементов

        if (HelpFromGnome.Instance.helpEnum != HelpEnum.Empty)
        {
            HelpToPlayer.AddHint(HelpFromGnome.Instance.helpEnum);//подсказка
        }
        //если не создали первую подсказку для уровня, то выполняем ход
        if (!HelpToPlayer.CreateNextGameHelp())
        {
            yield return new WaitForSeconds(1f);
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

        PoolManager.Instance.ReturnObjectToPool(PSRocket1);
        PoolManager.Instance.ReturnObjectToPool(PSRocket2);
        DailyGiftManager.Instance.DelCquirrel();
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

        Transform gOTextLevel = PanelMenu.transform.Find("TextLevel");
        gOTextLevel.GetComponent<Text>().text = (LevelMenu.Instance.LastLoadLevel.regionNumber + 1) + "-" + (LevelMenu.Instance.LastLoadLevel.levelNumber + 1);

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

        //показываем белку
        DailyGiftManager.Instance.CreateCquirrel();

        VideoBrowseButton videoBrowseButton = null;
        //запускаем куротину для постепенного отображения элементов
        //если выполнили все задания
        if (victory)
        {
            //показываем кнопку отзыва если пройдено обучение и на кнопку еще ни разу не нажимали
            if (JsonSaveAndLoad.LoadSave().trainingCompleted && !JsonSaveAndLoad.LoadSave().reviewWritten)
            {
                Transform transformPanelReview = PanelMenu.transform.Find("PanelReview");
                transformPanelReview.GetComponent<Animation>().Play();

                Button transformPanelReviewButton =  transformPanelReview.Find("ButtonReview").GetComponent<Button>();
                transformPanelReviewButton.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
                transformPanelReviewButton.onClick.AddListener(delegate { Application.OpenURL("https://play.google.com/store/apps/details?id=ru.VIDOCompany"); JsonSaveAndLoad.ReviewWritten(); });
            }
            //победа
            textEndGame.text = "Победа!";
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Victory);
            PSRocket1 = ParticleSystemManager.Instance.CreatePS(CanvasMenu.transform, PSEnum.PSRocket);
            PSRocket1.transform.position = new Vector3(-4, -5, 0);
            PSRocket2 = ParticleSystemManager.Instance.CreatePS(CanvasMenu.transform, PSEnum.PSRocket);
            PSRocket2.transform.position = new Vector3(4, -5, 0);
            //Выдаем звезды
            int stars = Score.Instance.NumberOfStarsReceived();
            LevelPassedResult levelPassedResult = LevelMenu.Instance.SetLevelPassed(stars, Score.Instance.getScore);

            //показываем звезды
            StartCoroutine(EndGameAnimationStars(PanelMenu, levelPassedResult));

            //показываем количество очков
            StartCoroutine(EndGameAnimationScore(PanelMenu, levelPassedResult));

            //показываем подарки
            StartCoroutine(EndGameAnimationGift(PanelMenu, levelPassedResult));            

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
            }
            else
            {
                //если еще не добавляли ходов
                if (!Tasks.Instance.addMovesOnEndGAme && !LevelSettings.Instance.Optional)
                {
                    textEndGame.text = "У вас закончились ходы! Добавим за просмотр видео?";
                    //создаем кнопку видео на месте загрузки следующего уровня    
                    videoBrowseButton = AdMobManager.Instance.GetVideoBrowseButton(gONextLevelButton, VideoForFeeEnum.ForMove);
                }
                else
                {
                    textEndGame.text = "У вас закончились ходы!";
                    
                }              
            }
            
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Defeat);
            //Destroy(gONextLevelButton.gameObject);
            //удаляем текст количества очков и панель подарков
            Destroy(PanelMenu.transform.Find("PanelGift").gameObject);
            Destroy(PanelMenu.transform.Find("TextScore").gameObject);
        }

        //если нет кнопки видео, то показываем кнопку следующего уровня
        if (videoBrowseButton == null)
        {
            if (!LevelMenu.Instance.NextLevelIsOpen())
            {
                SupportFunctions.ChangeAlfa(gONextLevelButton.GetComponent<Image>(), 0.5f);
                nextLevelButton.onClick.AddListener(delegate { SupportFunctions.CreateInformationPanel("Следующий уровень еще не открыт!", CanvasMenu.transform); });
            }
            else
            {
                SupportFunctions.ChangeAlfa(gONextLevelButton.GetComponent<Image>(), 1);
                nextLevelButton.onClick.AddListener(delegate { NextLevel(); });
            }
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
                //if (LevelMenu.Instance.NextLevelIsOpen())
                //{
                    nextLevelButton.interactable = true;
                //}
                break;
            }
            yield return new WaitForSeconds(0.1f);
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
        animationStarsIdle = false;
        for (int i = 1; i <= levelPassedResult.stars; i++)
        {
            //прерываем если уничтожили меню
            if (panelMenu == null)
            {
                yield break;
            }
            Transform starTransform = panelMenu.transform.Find("Star" + i);
            Image starImage = starTransform.GetComponent(typeof(Image)) as Image;
            SupportFunctions.ChangeAlfa(starImage, 1);
            //создаем эффект 
            ParticleSystemManager.Instance.CreateCollectAllEffect(starTransform, SpriteBank.SetShape(SpritesEnum.Star, true));
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
                    GameObject coinGO = GameObject.Instantiate(PrefabBank.ImageCoin, starTransform.position, Quaternion.identity, shopImageCoinsGO.transform);
                    //перемещаем чуть выше
                    MainAnimator.Instance.AddElementForSmoothMove(coinGO.transform, new Vector3(starTransform.position.x - 0.5f + j * 0.5f, starTransform.position.y + 1, starTransform.position.z), 1, SmoothEnum.InLineWithSlowdown, 0.05f, false, true);
                    //перемещаем к монете в магазине
                    MainAnimator.Instance.AddElementForSmoothMove(coinGO.transform, shopImageCoinsGO.transform.position, 1, SmoothEnum.InLineWithOneSpeed, 1.35f, true, true, delegate { Shop.Instance.CoinFlew(1); });
                }
            }

            yield return new WaitForSeconds(0.7f);
        }

        //animationStarsIdle = false;
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
        animationScoreIdle = false;
        do
        {
            //прерываем если уничтожили меню
            if (panelMenu == null)
            {
                yield break;
            }
            if (score > 50)
            {
                int i = score / 15;
                newScore += i;
                score -= i;
            }
            else if (score > 0)
            {
                newScore += 1;
                score -= 1;            
            }

            if (textScore != null)
            {
                textScore.text = "Score: " + newScore;
            }
            //else
            //{
            //    yield break;
            //}

            //показываем newscore если увеличили рекорд
            if (!createdNewScore && newScore > oldScore)
            {
                createdNewScore = true;
                //показываем надпись new
                if (imageNewScore != null)
                {
                    SupportFunctions.ChangeAlfa(imageNewScore, 1);
                    ParticleSystemManager.Instance.CreateCollectEffect(imageNewScore.transform, imageNewScore);
                }
                //else
                //{
                //    yield break;
                //}                
            }

            yield return new WaitForEndOfFrame();
        } while (score > 0);

        //animationScoreIdle = false;
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
        Tasks.Instance.CreateCollectedElements();
        Score.Instance.ResetParameters();
        SuperBonus.Instance.ResetParameters();
        InstrumentPanel.Instance.ResetParameters();
        //MainAnimator.Instance.ClearAllMassive();
    }

    //запрос на перезапуск сцены во время игры
    public void RequestRestartLevel() {

        //если у нас бессмертие или наш левел опциональный
        if (LifeManager.Instance.Immortal() || (LifeManager.Instance.Life > 0 && LevelSettings.Instance.Optional))
        {
            RestartLevel();
        }
        else if (LifeManager.Instance.Life > 1)
        {
            SupportFunctions.CreateYesNoPanel(GameObject.Find("GameHelper").transform, "При перезапуске игры вы потеряете жизнь. Перезапустить?", RestartLevelInGame);
        }
        else
        {
            SupportFunctions.CreateInformationPanelWithVideoAndShopButton("Тебе нужно больше одной жизни, что бы перезапустить уровень во время игры. Подожди немного, зайди в магазин за бессмертием или посмотри видео за одну жизнь!", VideoForFeeEnum.ForLive, Shop.Instance.transform);
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
            SupportFunctions.CreateInformationPanelWithVideoAndShopButton("Тебе нужно больше одной жизни, что бы перезапустить уровень во время игры. Подожди немного, зайди в магазин за бессмертием или посмотри видео за одну жизнь!", VideoForFeeEnum.ForLive, Shop.Instance.transform);
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
            SupportFunctions.CreateInformationPanelWithVideoAndShopButton("У тебя недостаточно жизней, что бы перезапустить уровень. Подожди немного, зайди в магазин за бессмертием или посмотри видео за одну жизнь!", VideoForFeeEnum.ForLive, Shop.Instance.transform);
        }
    }

    //перезапуск сцены
    public void RestartLevel()
    {
        DestroyCanvasMenu();        

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
            DestroyCanvasMenu();
            LevelMenu.Instance.LoadNextLevel();
        }
        else
        {
            SupportFunctions.CreateInformationPanelWithVideoAndShopButton("У тебя недостаточно жизней, что бы перейти на следующий уровень. Подожди немного, зайди в магазин за бессмертием или посмотри видео за одну жизнь!", VideoForFeeEnum.ForLive, Shop.Instance.transform);
        }       
    }

    //запрос на выход в меню в время игры
    public void RequestExitToMenu()
    {
        if (LifeManager.Instance.Immortal() || LevelSettings.Instance.Optional)
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
        ExitToMenu();
        LifeManager.Instance.SubLive();
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
