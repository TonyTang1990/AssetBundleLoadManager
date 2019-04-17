/*
 * Description:             DIYLog..cs
 * Author:                  TONYTANG
 * Create Date:             2018/12/09
 */

using UnityEngine;

/// <summary>
/// DIYLog..cs
/// 自定义Log静态工具类(统一Log入口，方便管理)
/// </summary>
public static class DIYLog
{
    /// <summary>
    /// 自定义log分类
    /// </summary>
    private enum DIYLogLevel
    {
        DIY_Normal = 1,         // 普通Log
        DIY_Warning = 2,        // 警告Log
        DIY_Err = 3,            // 错误Log
    }

    /// <summary>
    /// 自定义Log开关
    /// </summary>
    public static bool mLogSwitch = true;

    /// <summary>
    /// 打印普通log
    /// </summary>
    /// <param name="msg"></param>
    public static void Log(string msg)
    {
        if (mLogSwitch)
        {
            Debug.Log(msg);
        }
    }

    /// <summary>
    /// 打印普通log
    /// </summary>
    /// <param name="msg"></param>
    public static void LogWarning(string msg)
    {
        if (mLogSwitch)
        {
            Debug.LogWarning(msg);
        }
    }

    /// <summary>
    /// 打印普通log
    /// </summary>
    /// <param name="msg"></param>
    public static void LogError(string msg)
    {
        if (mLogSwitch)
        {
            Debug.LogError(msg);
        }
    }
}