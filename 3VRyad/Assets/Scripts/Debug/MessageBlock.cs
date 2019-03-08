using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MessageBlock : MonoBehaviour
{

    public Text _text;
    public Color color;
    public string message;
    public float lifetime;

    void Start()
    {
        _text.text = message;
        _text.color = color;
        Destroy(gameObject, lifetime);
    }
}
