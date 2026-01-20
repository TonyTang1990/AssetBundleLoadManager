/*
 * Description:             EditorUtilities.cs
 * Author:                  TONYTANG
 * Create Date:             2021//04/11
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// EditorUtilities.cs
/// 编辑器静态工具类
/// </summary>
public static class EditorUtilities
{
    private static MethodInfo _clearConsoleMethod;
    private static MethodInfo ClearConsoleMethod
    {
        get
        {
            if (_clearConsoleMethod == null)
            {
                Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                System.Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                _clearConsoleMethod = logEntries.GetMethod("Clear");
            }
            return _clearConsoleMethod;
        }
    }

    /// <summary>
    /// 清空控制台
    /// </summary>
    public static void ClearUnityConsole()
    {
        ClearConsoleMethod.Invoke(new object(), null);
    }
    
    /// <summary>
    /// 是否是数字
    /// </summary>
    public static bool IsNumber(string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;
        string pattern = @"^\d*$";
        return Regex.IsMatch(content, pattern);
    }
    
    /// <summary>
    /// 调用私有的静态方法
    /// </summary>
    /// <param name="type">类的类型</param>
    /// <param name="method">类里要调用的方法名</param>
    /// <param name="parameters">调用方法传入的参数</param>
    public static object InvokeNonPublicStaticMethod(System.Type type, string method, params object[] parameters)
    {
        var methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
        if (methodInfo == null)
        {
            UnityEngine.Debug.LogError($"{type.FullName} not found method : {method}");
            return null;
        }
        return methodInfo.Invoke(null, parameters);
    }

    /// <summary>
    /// 聚焦Unity游戏窗口
    /// </summary>
    public static void FocusUnityGameWindow()
    {
        System.Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.GameView");
        EditorWindow.GetWindow(T, false, "GameView", true);
    }
    
    /// <summary>
    /// 显示进度框
    /// </summary>
    public static void DisplayProgressBar(string tips, int progressValue, int totalValue)
    {
        EditorUtility.DisplayProgressBar("进度", $"{tips} : {progressValue}/{totalValue}", (float)progressValue / totalValue);
    }
    
    /// <summary>
    /// 隐藏进度框
    /// </summary>
    public static void ClearProgressBar()
    {
        EditorUtility.ClearProgressBar();
    }
}