/*
 * Description:             ExtractBuildInResourceWindow.cs
 * Author:                  TANGHUAN
 * Create Date:             2019//11/13
 */

using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 内置资源提取功能窗口
/// </summary>
public class ExtractBuildInResourceWindow : EditorWindow
{
    /// <summary>
    /// 内置资源路径
    /// </summary>
    private const string BuildInResourcePath = "Resources/unity_builtin_extra";

    [MenuItem("Tools/Assets/提取内置资源", false)]
    public static void dpAssetBrowser()
    {
        var exbuildinreswindow = EditorWindow.GetWindow<ExtractBuildInResourceWindow>();
        exbuildinreswindow.Show();
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("提取内置资源", GUILayout.MaxWidth(200.0f)))
        {
            extractBuildInResource();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 提取内置资源
    /// </summary>
    private void extractBuildInResource()
    {
        DIYLog.Log("此功能未完成!");
        //DIYLog.Log("BuildInResourcePath:" + BuildInResourcePath);
        //UnityEngine.Object[] unityassets = AssetDatabase.LoadAllAssetsAtPath(BuildInResourcePath);
        //DIYLog.Log("unityassets.Length = " + unityassets.Length);
    }
}