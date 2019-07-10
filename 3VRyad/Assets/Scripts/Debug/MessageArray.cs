using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageArray : MonoBehaviour
{
    public static MessageArray Instance; // Синглтон
    public static List<DebugMessage> debugMessage = new List<DebugMessage>();
    public static bool debug = false;
    [SerializeField] private bool debugMod = true;

    public RectTransform messageBlock;
    public int blockCount = 10;
    public float timeout = 0.2f;
    public float shift = 5;
    public float lifetime = 10;
    public bool moveUp = true;    

    private float curTimeout;
    private RectTransform[] tmp;
    private RectTransform clone;

    void Awake()
    {
        if (Instance)
        {
            Destroy(this); //Delete duplicate
            return;
        }
        else
        {
            Instance = this; //Make this object the only instance            
        }
        messageBlock.gameObject.SetActive(false);
        tmp = new RectTransform[blockCount];
        curTimeout = timeout;
        debug = debugMod;
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject); //Set as do not destroy
        }
#if UNITY_EDITOR
        debug = true;
#endif
    }

    public static void AddDebugMessage(string message, Color color) {
        if (debugMessage != null)
        {
            debugMessage.Add(new DebugMessage(message, color));
        }
    }

    void ClearArray()
    {
        bool active = false;
        foreach (DebugMessage debugMessage in debugMessage)
        {
            if (debugMessage.Message != string.Empty) active = true;
        }
        if (!active) debugMessage = new List<DebugMessage>();
    }

    void AddNewMessage(string text, Color color)
    {
        RectTransform block = Instantiate(messageBlock) as RectTransform;
        block.gameObject.SetActive(true);
        block.SetParent(transform, false);
        block.anchoredPosition = messageBlock.anchoredPosition;
        block.GetComponent<MessageBlock>().message = text;
        block.GetComponent<MessageBlock>().lifetime = lifetime;
        block.GetComponent<MessageBlock>().color = color;
        if (blockCount > 1)
        {
            for (int i = 0; i < tmp.Length; i++)
            {
                if (tmp[i])
                {
                    Vector3 move;
                    if (moveUp) move = new Vector3(tmp[i].anchoredPosition.x, tmp[i].anchoredPosition.y + tmp[i].sizeDelta.y + shift, 0);
                    else move = new Vector3(tmp[i].anchoredPosition.x, tmp[i].anchoredPosition.y - tmp[i].sizeDelta.y - shift, 0);
                    tmp[i].anchoredPosition = move;
                }
            }
            if (tmp[0]) Destroy(tmp[0].gameObject);
            for (int i = 0; i < tmp.Length; i++)
            {
                if (i < tmp.Length - 1) tmp[i] = tmp[i + 1];
            }
            tmp[blockCount - 1] = block;
        }
        else
        {
            if (clone) Destroy(clone.gameObject);
            clone = block;
        }

    }

    void Update()
    {
        if (debug)
        {
            if (debugMessage.Count > 0)
            {
                curTimeout += Time.deltaTime;
                if (curTimeout > timeout)
                {
                    for (int j = 0; j < debugMessage.Count; j++)
                    {
                        if (debugMessage[j].Message != string.Empty)
                        {
                            AddNewMessage(debugMessage[j].Message, debugMessage[j].Color);
                            debugMessage[j].Message = string.Empty;
                            curTimeout = 0;
                            return;
                        }
                    }
                    ClearArray();
                }
            }
            else
            {
                curTimeout = timeout;
            }
        }
       
    }

    void OnGUI()
    {
        float fps = 1.0f / Time.deltaTime;
        GUILayout.Label("FPS = " + (int)fps);

        string[] names = QualitySettings.names;
        GUILayout.BeginVertical();
        for (int i = 0; i < names.Length; i++)
        {
            if (GUILayout.Button(names[i]))
            {
                QualitySettings.SetQualityLevel(i, true);
            }
        }
        GUILayout.EndVertical();
    }
}

public class DebugMessage {

    string message;
    Color color;

    public DebugMessage(string message, Color color)
    {
        this.message = message;
        this.color = color;
    }

    public string Message
    {
        get
        {
            return message;
        }

        set
        {
            message = value;
        }
    }

    public Color Color
    {
        get
        {
            return color;
        }
    }
}