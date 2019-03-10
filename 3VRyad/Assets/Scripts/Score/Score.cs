using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public static Score Instance; // Синглтон
    private int score = 0;
    private int addScore = 0;
    private Text text;

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров Score!");
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        text = transform.GetComponentInChildren<Text>();
        UpdateText();
    }

    // Update is called once per frame
    void Update()
    {
        //if (addScore > 1000)
        //{
        //    double i = addScore / 15;
        //    double add = Math.Round((double)i/100, MidpointRounding.AwayFromZero) * 100;
        //    score += (int)add;
        //    addScore -= (int)add;
        //    UpdateText();
        //}
        //else if (addScore > 500)
        //{
        //    double i = addScore / 15;
        //    double add = Math.Round((double)i / 50, MidpointRounding.AwayFromZero) * 50;
        //    score += (int)add;
        //    addScore -= (int)add;
        //    UpdateText();
        //}
        //else 
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

    public void CreatreScoreElement(Vector3 position, int score)
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
}
