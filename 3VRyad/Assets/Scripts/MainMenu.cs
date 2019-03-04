using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LevelMenu.Instance.CreateLevelMenu(LevelMenu.Instance.regionsList[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
