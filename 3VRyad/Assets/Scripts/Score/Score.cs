using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour, IESaveAndLoad
{
    public static Score Instance; // Синглтон
    
    private int score = 0;
    private int addScore = 0;
    private Text text;
    private Image imagePanelImage;

    public int scoreFor1Star; //Количество очков для поллучения первой звезды
    private Image imageStar1;
    private bool imageStar1shown;

    public int scoreFor2Star;//Количество очков для поллучения второй звезды
    private Image imageStar2;
    private bool imageStar2shown;

    public int scoreFor3Star;//Количество очков для поллучения третьей звезды
    private Image imageStar3;
    private bool imageStar3shown;


    public int getScore()
    {
            return score + addScore;
    }

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров Score!");
        }

        Instance = this;

        text = transform.GetComponentInChildren<Text>();
        ResetParameters();
    }

    // Update is called once per frame
    void FixedUpdate()
    { 
        if (addScore > 50)
        {
            int i = addScore / 15;
            //double add = Math.Round((double)i / 5, MidpointRounding.AwayFromZero) * 5;
            score += i;
            addScore -= i;
            UpdateText();
            UpdateLineAndStars();
        }
        else if (addScore > 0)
        {
            score += 1;
            addScore -= 1;
            UpdateText();
            UpdateLineAndStars();
        }
    }

    public void ResetParameters()
    {
        //Сбрасываем значения
        score = 0;
        addScore = 0;
        if (Application.isPlaying)
        {
            UpdateText();
        }
        else
        {
            text.text = "1: " + scoreFor1Star + " 2: " + scoreFor2Star + " 3: " + scoreFor3Star;
        }

        //визуализируем звезды
        Transform panelImage = transform.Find("PanelImage");
        RectTransform rectTransformPanelImage = panelImage.GetComponent<RectTransform>();
        imagePanelImage = panelImage.GetComponent<Image>();
        Transform star1 = panelImage.Find("Star1");
        RectTransform rectTransformStar1 = star1.GetComponent<RectTransform>();
        imageStar1 = star1.GetComponent<Image>();
        SupportFunctions.ChangeAlfa(imageStar1, 0.2f);
        imageStar1shown = false;
        Transform star2 = panelImage.Find("Star2");
        RectTransform rectTransformStar2 = star2.GetComponent<RectTransform>();
        imageStar2 = star2.GetComponent<Image>();
        SupportFunctions.ChangeAlfa(imageStar2, 0.2f);
        imageStar2shown = false;
        Transform star3 = panelImage.Find("Star3");
        RectTransform rectTransformStar3 = star3.GetComponent<RectTransform>();
        imageStar3 = star3.GetComponent<Image>();
        SupportFunctions.ChangeAlfa(imageStar3, 0.2f);
        imageStar3shown = false;

        rectTransformStar2.localPosition = new Vector3(rectTransformPanelImage.rect.xMin + (rectTransformPanelImage.rect.width * (float)scoreFor2Star / (float)scoreFor3Star), rectTransformStar3.localPosition.y, 0);
        rectTransformStar1.localPosition = new Vector3(rectTransformPanelImage.rect.xMin + (rectTransformPanelImage.rect.width * (float)scoreFor1Star / (float)scoreFor3Star), rectTransformStar3.localPosition.y, 0);
        UpdateLineAndStars();
    }

    public void CreateScoreElement(Vector3 position, int score)
    {
        GameObject scoreElement = Instantiate(PrefabBank.Instance.scoreElementPrefab, transform);
        scoreElement.transform.position = position;
        Text scoreElementText = scoreElement.GetComponent<Text>();
        scoreElementText.text = score.ToString();
        scoreElementText.fontSize = scoreElementText.fontSize * (score/100);
        MainAnimator.Instance.AddElementForSmoothMove(scoreElement.transform, transform.position, 1, SmoothEnum.InLineWithAcceleration, smoothTime: 1, destroyAfterMoving: true);
        AddScore(score);
    }

    private void AddScore(int addScore) {
        this.addScore += addScore;
    }

    //обновляем отображение линии и звезд
    private void UpdateLineAndStars()
    {
        imagePanelImage.fillAmount = (float)score / (float)scoreFor3Star;
        //показываем звезды
        if (score > scoreFor1Star && !imageStar1shown)
        {
            //!!!создать эффект взрыва отображения звезды

            SupportFunctions.ChangeAlfa(imageStar1, 1);
            imageStar1shown = true;
        }

        if (score > scoreFor2Star && !imageStar2shown)
        {
            //!!!создать эффект взрыва отображения звезды

            SupportFunctions.ChangeAlfa(imageStar2, 1);
            imageStar2shown = true;
        }

        if (score > scoreFor3Star && !imageStar3shown)
        {
            //!!!создать эффект взрыва отображения звезды

            SupportFunctions.ChangeAlfa(imageStar3, 1);
            imageStar3shown = true;
        }
    }

    //обнолвление текста
    private void UpdateText()
    {
        text.text = "Количество очков: " + score;
    }

    //количество полученных звезд
    public int NumberOfStarsReceived() {
        if (score + addScore >= scoreFor3Star)
        {
            return 3;
        }
        else if (score + addScore >= scoreFor2Star)
        {
            return 2;
        }
        else if (score + addScore >= scoreFor1Star)
        {
            return 1;
        }
        return 0;
    }

    //сохранение и заргрузка
    public Type GetClassName()
    {
        return this.GetType();
    }
    //передаем данные о настройках в xml формате
    public XElement GetXElement()
    {
        XElement mainXElement = new XElement(this.GetType().ToString());

        mainXElement.Add(new XElement("scoreForFirstStar", scoreFor1Star));
        mainXElement.Add(new XElement("scoreForSecondStar", scoreFor2Star));
        mainXElement.Add(new XElement("scoreForThirdStar", scoreFor3Star));

        return mainXElement;
    }

    public void RecoverFromXElement(XElement tasksXElement)
    {

        //восстанавливаем значения
        this.scoreFor1Star = int.Parse(tasksXElement.Element("scoreForFirstStar").Value);
        this.scoreFor2Star = int.Parse(tasksXElement.Element("scoreForSecondStar").Value);
        this.scoreFor3Star = int.Parse(tasksXElement.Element("scoreForThirdStar").Value);

        if (Application.isPlaying)
        {
            //UpdateText();
        }
        else
        {
            text = transform.GetComponentInChildren<Text>();
            text.text = "Для первой " + scoreFor1Star + "\nДля второй " + scoreFor2Star + "\nДля третьей " + scoreFor3Star;
        }
        ResetParameters();
    }
}
