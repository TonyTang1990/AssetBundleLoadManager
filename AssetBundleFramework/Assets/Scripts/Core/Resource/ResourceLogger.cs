/*
 * Description:             ResourceLogger.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/30
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ResourceLogger.cs
/// 资源log打印工具
/// 封装一层Logger，为了单独控制资源的调试Log打印，避免资源Log过多导致卡顿
/// </summary>
public class ResourceLogger
{
    /// <summary>
    /// 资源Log开关本地存储Key
    /// </summary>
    private static string LogSwitchPrefsKey = "ResourceLogSwitch";

    /// <summary>
    /// 资源Log打印开关
    /// </summary>
    public static bool LogSwitch
    {
        get
        {
            return mLogSwitch;
        }
        set
        {
            var prevalue = mLogSwitch;
            mLogSwitch = value;
            PlayerPrefs.SetInt(LogSwitchPrefsKey, mLogSwitch == true ? 1 : 0);
            if(prevalue != mLogSwitch)
            {
                Debug.Log(string.Format("当前资源开关:{0}", mLogSwitch));
            }
        }
    }
    private static bool mLogSwitch = PlayerPrefs.GetInt(LogSwitchPrefsKey, 0) == 0 ? false : true;

    /// <summary>
    /// 打印普通Log信息
    /// </summary>
    /// <param name="content"></param>
    public static void log(string content)
    {
        if (LogSwitch)
        {
            Debug.Log(content);
        }
    }

    /// <summary>
    /// 打印警告信息
    /// </summary>
    /// <param name="content"></param>
    public static void logWar(string content)
    {
        if (LogSwitch)
        {
            Debug.LogWarning(content);
        }
    }

    /// <summary>
    /// 打印错误信息
    /// </summary>
    /// <param name="content"></param>
    public static void logErr(string content)
    {
        if (LogSwitch)
        {
            Debug.LogError(content);
        }
    }
}
