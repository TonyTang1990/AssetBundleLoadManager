/*
 * Description:             NativeMessageHandler.cs
 * Author:                  TONYTANG
 * Create Date:             2018/08/10
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// NativeMessageHandler.cs
/// 原生消息相应处理器
/// </summary>
public class NativeMessageHandler : MonoBehaviour {

    /// <summary>
    /// 原生消息数据显示文本
    /// </summary>
    public Text TxtNativeOutput;

    public static NativeMessageHandler Singleton { get; private set; }

    void Awake()
    {
        Singleton = this;
    }

    /// <summary>
    /// 接收原生消息
    /// </summary>
    /// <param name="msg"></param>
    public void resUnityMsg(string msg)
    {
        Debug.Log(string.Format("resUnityMsg : {0}", msg));
        if (TxtNativeOutput != null)
        {
            TxtNativeOutput.text = msg;
        }
    }

    /// <summary>
    /// 接收原生Log
    /// </summary>
    /// <param name="msg"></param>
    public void resJavaLog(string log)
    {
        Debug.Log(string.Format("Java Log : {0}", log));
    }
}