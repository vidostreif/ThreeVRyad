using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CheckTime

{
    private static DateTime dateTime;
    private static bool checkGlobalTime = false;

    public static DateTime Realtime() {
        if (!checkGlobalTime)
        {
            dateTime = CheckGlobalTime();
            Debug.Log("Global UTC time: " + dateTime);
            checkGlobalTime = true;
        }
        
        return dateTime.AddSeconds(Time.realtimeSinceStartup);
    }

    private static DateTime CheckGlobalTime()
    {
        //var www = new WWW("https://google.com");
        //int meter = 100;
        //while (!www.isDone && www.error == null && meter > 0) {
        //    Thread.Sleep(1);
        //    meter--;
        //}

        //if (meter > 0)
        //{
        //    var str = www.responseHeaders["Date"];
        //    DateTime newDateTime;

        //    if (!DateTime.TryParse(str, out newDateTime))
        //        return DateTime.MinValue;

        //    return newDateTime.ToUniversalTime();
        //}
        //else
        //{
            return DateTime.UtcNow;
        //}
                
    }
}
