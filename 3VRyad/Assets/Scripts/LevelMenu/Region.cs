using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Region
{
    public string name;
    [SerializeField] public List<Level> levelList = new List<Level>();
}
