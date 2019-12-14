/*
 * Description:             BaseEditorWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2019/12/14
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BaseEditorWindow.cs
/// 编辑器窗口基类抽象
/// </summary>
public class BaseEditorWindow : EditorWindow
{
    protected virtual void OnEnable()
    {
        InitData();
    }

    protected virtual void OnDisable()
    {
        SaveData();
    }

    protected virtual void OnDestroy()
    {
        SaveData();
    }

    /// <summary>
    /// 初始化窗口数据
    /// </summary>
    protected virtual void InitData()
    {
        
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    protected virtual void SaveData()
    {

    }

}