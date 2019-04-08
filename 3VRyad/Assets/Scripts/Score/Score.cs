using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour, IESaveAndLoad
{
    public static Score Instance; // Синглтон
    public int scoreForFirstStar; //Количество очков для поллучения первой звезды
    public int scoreForSecondStar;//Количество очков для поллучения второй звезды
    public int scoreForThirdStar;//Количество очков для поллучения третьей звезды
    private int score = 0;
    private int addScore = 0;
    private Text text;

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
        UpdateText();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    { 
        if (addScore > 50)
        {
            int i = addScore / 15;
            //double add = Math.Round((double)i / 5, MidpointRounding.AwayFromZero) * 5;
            score += i;
            addScore -= i;
            UpdateText();
        }
        else if (addScore > 0)
        {
            score += 1;
            addScore -= 1;
            UpdateText();
        }
    }

    public void ResetParameters()
    {
        //Сбрасываем значения
        score = 0;
        addScore = 0;
        UpdateText();
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

    //обнолвление текста
    private void UpdateText()
    {
        text.text = "Количество очков: " + score;
    }

    //количество полученных звезд
    public int NumberOfStarsReceived() {
        if (score + addScore >= scoreForThirdStar)
        {
            return 3;
        }
        else if (score + addScore >= scoreForSecondStar)
        {
            return 2;
        }
        else if (score + addScore >= scoreForFirstStar)
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

        mainXElement.Add(new XElement("scoreForFirstStar", scoreForFirstStar));
        mainXElement.Add(new XElement("scoreForSecondStar", scoreForSecondStar));
        mainXElement.Add(new XElement("scoreForThirdStar", scoreForThirdStar));

        return mainXElement;
    }

    public void RecoverFromXElement(XElement tasksXElement)
    {

        //восстанавливаем значения
        this.scoreForFirstStar = int.Parse(tasksXElement.Element("scoreForFirstStar").Value);
        this.scoreForSecondStar = int.Parse(tasksXElement.Element("scoreForSecondStar").Value);
        this.scoreForThirdStar = int.Parse(tasksXElement.Element("scoreForThirdStar").Value);

        if (Application.isPlaying)
        {
            //UpdateText();
        }
        else
        {
            text = transform.GetComponentInChildren<Text>();
            text.text = "Для первой " + scoreForFirstStar + "\nДля второй " + scoreForSecondStar + "\nДля третьей " + scoreForThirdStar;
        }
        ResetParameters();
    }
}
