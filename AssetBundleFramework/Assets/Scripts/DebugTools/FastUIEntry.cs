/*
 * Description:             快速UI入口
 * Author:                  tanghuan
 * Create Date:             2018/02/26
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// 快速UI入口
/// </summary>
public class FastUIEntry : MonoBehaviour{

    /// <summary>
    /// 操作参数1
    /// </summary>
    private string m_TFInputParam1 = string.Empty;

    /// <summary>
    /// 快速UI显示宽度系数
    /// </summary>
    private const float mFastUIWidthFactor = 0.8f;

    /// <summary>
    /// 快速UI显示宽度系数
    /// </summary>
    private const float mFastUIHeightFactor = 1.0f;

    /// <summary>
    /// 可视化Log开关
    /// </summary>
    public static bool LogSwitch = true;

    /// <summary>
    /// 自定义GUI显示
    /// </summary>
    private GUIStyle mGUIDIY;    

    /// <summary>
    /// 最新一次获取的堆内存分配
    /// </summary>
    private long mHeapMemorySize;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        mGUIDIY = new GUIStyle();
        mGUIDIY.fontSize = 20;
        mGUIDIY.normal.textColor = Color.white;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width * mFastUIWidthFactor, Screen.height * (1.0f - mFastUIHeightFactor), Screen.width * (1.0f - mFastUIWidthFactor), Screen.height * mFastUIHeightFactor));
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("参数1:", mGUIDIY, GUILayout.Width(50.0f), GUILayout.MaxHeight(30.0f));
        m_TFInputParam1 = GUILayout.TextField(m_TFInputParam1, GUILayout.MaxWidth(90.0f), GUILayout.MaxHeight(30.0f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        LogSwitch = GUILayout.Toggle(LogSwitch, "可视化Log总开关", GUILayout.Width(150.0f));
        if (LogSwitch != VisibleLogUtility.getInstance().mVisibleLogSwitch)
        {
            VisibleLogUtility.getInstance().mVisibleLogSwitch = LogSwitch;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
