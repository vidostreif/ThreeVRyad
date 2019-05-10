using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColorElement 
{
    public Image image;
    public SpriteRenderer spriteRenderer;
    public Color standartColor;
    public Color startColor;
    public Color newColor;
    public float speed;
    public bool inCycle;
    public int numberOfCycles;

    public ChangeColorElement(Image image, Color newColor, float speed, bool inCycle, int numberOfCycles)
    {
        this.image = image;
        this.standartColor = image.color;
        this.spriteRenderer = null;
        this.startColor = image.color;
        this.newColor = newColor;
        this.speed = speed;
        this.inCycle = inCycle;
        this.numberOfCycles = numberOfCycles;
    }

    public ChangeColorElement(Image image, Color newColor, float speed)
    {
        this.image = image;
        this.standartColor = image.color;
        this.spriteRenderer = null;
        this.startColor = image.color;
        this.newColor = newColor;
        this.speed = speed;
        this.inCycle = false;
        this.numberOfCycles = 0;
    }

    public ChangeColorElement(SpriteRenderer spriteRenderer, Color newColor, float speed, bool inCycle, int numberOfCycles)
    {
        this.image = null;
        this.spriteRenderer = spriteRenderer;
        this.standartColor = spriteRenderer.color;
        this.startColor = image.color;
        this.newColor = newColor;
        this.speed = speed;
        this.inCycle = inCycle;
        this.numberOfCycles = numberOfCycles;
    }

    public ChangeColorElement(SpriteRenderer spriteRenderer, Color newColor, float speed)
    {
        this.image = null;
        this.spriteRenderer = spriteRenderer;
        this.standartColor = spriteRenderer.color;
        this.startColor = image.color;
        this.newColor = newColor;
        this.speed = speed;
        this.inCycle = false;
        this.numberOfCycles = 0;
    }
}
