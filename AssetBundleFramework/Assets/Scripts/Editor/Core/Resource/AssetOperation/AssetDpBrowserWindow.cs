/*
 * Description:             Asset依赖资源查看窗口
 * Author:                  tanghuan
 * Create Date:             2018/02/26
 */

using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Asset依赖资源查看窗口
/// </summary>
public class AssetDpBrowserWindow : EditorWindow
{
    /// <summary>
    /// 依赖资源映射map
    /// Key为选中Asset的路径，Value依赖Asset的路径列表
    /// </summary>
    private Dictionary<string, List<string>> dpAssetInfoMap = new Dictionary<string, List<string>>();

    /// <summary>
    /// 滚动位置
    /// </summary>
    private Vector2 uiScrollPos;

    /// <summary>
    /// 是否包含间接引用
    /// </summary>
    private bool includeIndirectDp = false;

    /// <summary>
    /// 资源后缀过滤
    /// </summary>
    private string postFixFilter = string.Empty;

    [MenuItem("Tools/Assets/查看选中Asset依赖资源 %#B", false, 100)]
    public static void dpAssetBrowser()
    {
        var dpassetbrowser = EditorWindow.GetWindow<AssetDpBrowserWindow>();
        dpassetbrowser.Show();
    }

    void Awake()
    {
        refreshSelections();
    }

    void OnEnable()
    {
        refreshSelections();
    }

    private void refreshSelections()
    {
        dpAssetInfoMap.Clear();
        var selections = Selection.GetFiltered<Object>(SelectionMode.Assets);
        foreach (var selection in selections)
        {
            var selectionassetpath = AssetDatabase.GetAssetPath(selection);
            var dpassets = AssetDatabase.GetDependencies(selectionassetpath, includeIndirectDp);
            if (!dpAssetInfoMap.ContainsKey(selectionassetpath))
            {
                dpAssetInfoMap.Add(selectionassetpath, new List<string>());
            }
            foreach(var dpasset in dpassets)
            {
                dpAssetInfoMap[selectionassetpath].Add(dpasset);
            }
        }
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical();
        foreach(var dpassetinfo in dpAssetInfoMap)
        {
            showAssetDpUI(dpassetinfo.Key, dpassetinfo.Value);
        }
        GUILayout.BeginHorizontal();
        includeIndirectDp = GUILayout.Toggle(includeIndirectDp, "是否包含间接引用");
        GUILayout.Label("资源后缀过滤:");
        postFixFilter = GUILayout.TextField(postFixFilter, GUILayout.MaxWidth(200.0f));
        if (GUILayout.Button("刷新选中Asset", GUILayout.MaxWidth(200.0f)))
        {
            refreshSelections();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 显示依赖资源信息UI
    /// </summary>
    /// <param name="assetpath"></param>
    /// <param name="dpassetpath"></param>
    private void showAssetDpUI(string assetpath, List<string> dpassetpathlist)
    {
        GUILayout.BeginVertical();
        uiScrollPos = GUILayout.BeginScrollView(uiScrollPos, GUILayout.MaxWidth(2000.0f), GUILayout.MaxHeight(800.0f));
        GUILayout.Label("主Asset路径:");
        GUILayout.Label(assetpath);
        GUILayout.Label("依赖Asset路径:");
        foreach (var dpassetpath in dpassetpathlist)
        {
            if(postFixFilter.Equals(string.Empty))
            {
                GUILayout.Label(dpassetpath);
            }
            else
            {
                if(dpassetpath.EndsWith(postFixFilter))
                {
                    GUILayout.Label(dpassetpath);
                }
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }
}
