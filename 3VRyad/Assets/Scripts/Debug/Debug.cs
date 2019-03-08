using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngineInternal;

public static class Debug 
{
    public static bool isDebugBuild
    {
        get { return UnityEngine.Debug.isDebugBuild; }
    }

    public static void Log(object message)
    {
        if (MessageArray.debug)
        {
            UnityEngine.Debug.Log(message);
            MessageArray.AddDebugMessage(message.ToString(), Color.black);
        }
    }

    public static void Log(object message, UnityEngine.Object context)
    {
        if (MessageArray.debug)
        {
            UnityEngine.Debug.Log(message, context);
            MessageArray.AddDebugMessage(message.ToString(), Color.black);
        }
    }

    public static void LogError(object message)
    {
        if (MessageArray.debug)
        {
            UnityEngine.Debug.LogError(message);
            MessageArray.AddDebugMessage(message.ToString(), Color.red);
        }
    }

    public static void LogError(object message, UnityEngine.Object context)
    {
        if (MessageArray.debug)
        {
            UnityEngine.Debug.LogError(message, context);
            MessageArray.AddDebugMessage(message.ToString(), Color.red);
        }
    }

    public static void LogWarning(object message)
    {
        if (MessageArray.debug)
        {
            UnityEngine.Debug.LogWarning(message.ToString());
            MessageArray.AddDebugMessage(message.ToString(), Color.yellow);
        }
    }

    public static void LogWarning(object message, UnityEngine.Object context)
    {
        if (MessageArray.debug)
        {
            UnityEngine.Debug.LogWarning(message.ToString(), context);
            MessageArray.AddDebugMessage(message.ToString(), Color.yellow);
        }
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
    {
        UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
    }

    public static void DrawRay(Vector3 start, Vector3 dir, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
    {
        UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
    }

    public static void Assert(bool condition)
    {
        if (!condition) throw new Exception();
    }
}
