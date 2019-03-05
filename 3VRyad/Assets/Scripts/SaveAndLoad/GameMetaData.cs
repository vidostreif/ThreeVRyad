using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//передает данные между сценами
public class GameMetaData
{
    private Dictionary<string, string> string_parameters; //сохраненные параметры
    private static GameMetaData Instance;

    public GameMetaData()
    {
        string_parameters = new Dictionary<string, string>();
    }

    public string GetString(string parameter)
    {
        if (!string_parameters.ContainsKey(parameter))
        {
            string_parameters.Add(parameter, "");
        }

        return string_parameters[parameter];
    }

    public void SetString(string parameter, string value)
    {
        if (string_parameters.ContainsKey(parameter))
        {
            string_parameters[parameter] = value;
        }
        else
        {
            string_parameters.Add(parameter, value);
        }
    }

    public static GameMetaData GetInstance()
    {
        if (Instance == null)
            Instance = new GameMetaData();

        return Instance;
    }
}
