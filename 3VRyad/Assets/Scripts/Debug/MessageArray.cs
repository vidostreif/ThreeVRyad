using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageArray : MonoBehaviour
{

    public static List<string> message;

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
        messageBlock.gameObject.SetActive(false);
        message = new List<string>();
        tmp = new RectTransform[blockCount];
        curTimeout = timeout;
    }

    void ClearArray()
    {
        bool active = false;
        foreach (string mess in message)
        {
            if (mess != string.Empty) active = true;
        }
        if (!active) message = new List<string>();
    }

    void AddNewMessage(string text)
    {
        RectTransform block = Instantiate(messageBlock) as RectTransform;
        block.gameObject.SetActive(true);
        block.SetParent(transform);
        block.anchoredPosition = messageBlock.anchoredPosition;
        block.GetComponent<MessageBlock>().message = text;
        block.GetComponent<MessageBlock>().lifetime = lifetime;
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
        if (message.Count > 0)
        {
            curTimeout += Time.deltaTime;
            if (curTimeout > timeout)
            {
                for (int j = 0; j < message.Count; j++)
                {
                    if (message[j] != string.Empty)
                    {
                        AddNewMessage(message[j]);
                        message[j] = string.Empty;
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