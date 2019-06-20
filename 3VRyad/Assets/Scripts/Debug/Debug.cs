using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngineInternal;

//public static class Debug
//{
//    public static bool isDebugBuild
//    {
//        get { return UnityEngine.Debug.isDebugBuild; }
//    }

//    public static void Log(object message)
//    {
//        if (Application.isPlaying && !MessageArray.debug)
//            return;
//        UnityEngine.Debug.Log(message);
//        AddMessageToScreen(message, Color.black);

//    }

//    public static void Log(object message, UnityEngine.Object context)
//    {
//        if (Application.isPlaying && !MessageArray.debug)
//            return;
//        UnityEngine.Debug.Log(message, context);
//        AddMessageToScreen(message, Color.black);
//    }

//    public static void LogError(object message)
//    {
//        if (Application.isPlaying && !MessageArray.debug)
//            return;
//        UnityEngine.Debug.LogError(message);
//        AddMessageToScreen(message, Color.red);
//    }

//    public static void LogError(object message, UnityEngine.Object context)
//    {
//        if (Application.isPlaying && !MessageArray.debug)
//            return;
//        UnityEngine.Debug.LogError(message, context);
//        AddMessageToScreen(message, Color.red);
//    }

//    public static void LogWarning(object message)
//    {
//        if (Application.isPlaying && !MessageArray.debug)
//            return;
//        UnityEngine.Debug.LogWarning(message.ToString());
//        AddMessageToScreen(message, Color.yellow);
//    }

//    public static void LogWarning(object message, UnityEngine.Object context)
//    {
//        if (Application.isPlaying && !MessageArray.debug)
//            return;
//        UnityEngine.Debug.LogWarning(message.ToString(), context);
//        AddMessageToScreen(message, Color.yellow);
//    }

//    public static void DrawLine(Vector3 start, Vector3 end, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
//    {
//        UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
//    }

//    public static void DrawRay(Vector3 start, Vector3 dir, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
//    {
//        UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
//    }

//    public static void Assert(bool condition)
//    {
//        if (!condition) throw new Exception();
//    }

//    private static void AddMessageToScreen(object message, Color color)
//    {
//        if (Application.isPlaying)
//        {
//            try
//            {
//                MessageArray.AddDebugMessage((string)message, color);
//            }
//            catch (Exception)
//            {
//                try
//                {
//                    MessageArray.AddDebugMessage(message.ToString(), color);
//                }
//                catch (Exception)
//                {
//                    Debug.Log("Ошибка вывода сообщения!");
//                }
//            }
//        }
//    }
//}
