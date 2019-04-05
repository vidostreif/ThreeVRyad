using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapScrolling : MonoBehaviour
{
    [Range(1,50)]
    [Header("Управление")]
    public int panCount;
    [Range(0, 500)]
    public int panOffset;
    [Range(0f, 20f)]
    public float snapSpeed;
    [Range(0f, 20f)]
    public float scaleOffset;
    public Vector2[] pansScale;
    [Header("Префабы")]
    public GameObject panPrefab;

    private GameObject[] instPans;
    private RectTransform contentRect;
    private bool isScrolling = false;
    private Vector2 contentVector; 
    public int selectedPanId;
    // Start is called before the first frame update
    void Start()
    {
        contentRect = GetComponent<RectTransform>();
        instPans = new GameObject[panCount];
        //pansPos = new Vector2[panCount];
        for (int i = 0; i < panCount; i++)
        {
            instPans[i] = Instantiate(panPrefab, transform, false);
            if (i == 0) continue;
            //instPans[i].transform.localPosition = new Vector2(instPans[i - 1].transform.localPosition.x + instPans[i].transform.GetComponent<RectTransform>().sizeDelta.x + panOffset, instPans[i].transform.localPosition.y);
            //pansPos[i] = -instPans[i].transform.localPosition;
            
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float nearestPos = float.MaxValue;
        float distance = 0;
        Transform transformPan = null;
        int i = 0;
        foreach (GameObject item in instPans)
        {
            distance = Mathf.Abs(item.transform.position.x);
            if (distance < nearestPos)
            {
                nearestPos = distance;
                transformPan = item.transform;
                selectedPanId = i;
            }
            //float scale = Mathf.Clamp(1 / (distance / panOffset) * scaleOffset, );
            i++;
        }
        if (isScrolling)
        {
            return;
        }
        contentVector.x = Mathf.SmoothStep(contentRect.anchoredPosition.x, -instPans[selectedPanId].GetComponent<RectTransform>().anchoredPosition.x + (contentRect.sizeDelta.x*0.5f), snapSpeed * Time.fixedDeltaTime);
        contentRect.anchoredPosition = contentVector;
    }

    public void Scrolling(bool scroll) {

        isScrolling = scroll;
    }
}
