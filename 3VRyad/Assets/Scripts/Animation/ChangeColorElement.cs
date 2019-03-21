using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColorElement 
{
    public Image image;
    public Color newColor;
    public float speed;

    public ChangeColorElement(Image image, Color newColor, float speed)
    {
        this.image = image;
        this.newColor = newColor;
        this.speed = speed;
    }
}
