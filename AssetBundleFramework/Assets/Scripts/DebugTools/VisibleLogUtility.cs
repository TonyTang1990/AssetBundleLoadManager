/*
 * Description:             VisibleLogUtility.cs
 * Author:                  TONYTANG
 * Create Date:             2018/08/08
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 简单的可视化UI Log显示
/// </summary>
public class VisibleLogUtility : SingletonMonoBehaviourTemplate<VisibleLogUtility> {

    public bool mVisibleLogSwitch = false;

    private List<string> mLogList;

    private Vector2 scrollPosition;

    private Rect mLogArea;

    /// <summary>
    /// 自定义GUI显示
    /// </summary>
    private GUIStyle mGUIDIY;

    /// <summary>
    /// Log显示宽度系数
    /// </summary>
    private const float mLogWidthFactor = 0.8f;

    /// <summary>
    /// Log显示宽度系数
    /// </summary>
    private const float mLogHeightFactor = 1.0f;

    void Awake()
    {
        mLogList = new List<string>();
        scrollPosition = Vector2.zero;
        mLogArea = new Rect(0.0f, 0.0f, Screen.width * mLogWidthFactor, Screen.height * mLogHeightFactor);

        mGUIDIY = new GUIStyle();
        mGUIDIY.fontSize = 20;
        mGUIDIY.normal.textColor = Color.white;
    }

    public void HandleLog(string logString, string stackTrace, LogType type)
    {
        if(mVisibleLogSwitch == false)
        {
            return;
        }

        mLogList.Add(logString);
        if(type == LogType.Error)
        {
            mLogList.Add(stackTrace);
        }
    }

    void OnGUI()
    {
        if (mVisibleLogSwitch == false)
        {
            return;
        }

        GUILayout.BeginArea(mLogArea);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.MinWidth(Screen.width * mLogWidthFactor), GUILayout.MinHeight(Screen.height * mLogHeightFactor));
        for(int i = 0; i < mLogList.Count; i++)
        {
            GUILayout.Label(mLogList[i], mGUIDIY);
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void OnDestroy()
    {
        mLogList = null;
    }
}
