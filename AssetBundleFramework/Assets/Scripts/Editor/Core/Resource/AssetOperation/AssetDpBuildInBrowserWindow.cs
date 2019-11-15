/*
 * Description:             AssetDpBuildInBrowserWindow.cs
 * Author:                  TANGHUAN
 * Create Date:             2019//11/13
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Asset依赖内置资源查看窗口
/// </summary>
public class AssetDpBuildInBrowserWindow : EditorWindow
{
    /// <summary>
    /// 内置资源名字
    /// </summary>
    private const string BuildInResourceName = "Resources/unity_builtin_extra";

    /// <summary>
    /// 滚动位置
    /// </summary>
    private Vector2 uiScrollPos;

    /// <summary>
    /// 依赖内置资源的Asset映射map
    /// Key为选中Asset的路径，Value为Asset使用了内置Shader的节点名列表
    /// </summary>
    private Dictionary<string, List<string>> mReferenceBuildInAssetMap = new Dictionary<string, List<string>>();

    [MenuItem("Tools/Assets/统计内置资源引用Asset", false)]
    public static void dpAssetBrowser()
    {
        var assetdpbuildinwindow = EditorWindow.GetWindow<AssetDpBuildInBrowserWindow>();
        assetdpbuildinwindow.Show();
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("统计内置Shader引用Asset", GUILayout.MaxWidth(200.0f)))
        {
            analyzeBuildInShaderReferenceAsset();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginVertical();
        uiScrollPos = GUILayout.BeginScrollView(uiScrollPos);
        foreach (var referenceasset in mReferenceBuildInAssetMap)
        {
            showReferenceAssetUI(referenceasset.Key, referenceasset.Value);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 显示使用了内置Shader的资源信息UI
    /// </summary>
    /// <param name="assetpath"></param>
    /// <param name="nodenamelist"></param>
    private void showReferenceAssetUI(string assetpath, List<string> nodenamelist)
    {
        GUILayout.Label("Asset路径:");
        GUILayout.Label(assetpath);
        if(nodenamelist.Count > 0)
        {
            GUILayout.Label("使用了内置Shader的节点名:");
            foreach (var nodename in nodenamelist)
            {
                GUILayout.Label(nodename);
            }
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }

    /// <summary>
    /// 统计内置Shader引用Asset
    /// </summary>
    private void analyzeBuildInShaderReferenceAsset()
    {
        mReferenceBuildInAssetMap.Clear();
        var allmatfiles = Directory.GetFiles("Assets/", "*.mat", SearchOption.AllDirectories);
        var nodelist = new List<string>();
        foreach(var matfile in allmatfiles)
        {
            var matasset = AssetDatabase.LoadAssetAtPath<Material>(matfile);
            if(isUsingBuildInShader<Material>(matasset, ref nodelist))
            {
                tryAddBuildInReferenceAsset(matfile, nodelist);
            }
        }
        var allprefabfiles = Directory.GetFiles("Assets/", "*.prefab", SearchOption.AllDirectories);
        foreach (var prefabfile in allprefabfiles)
        {
            var prefabasset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabfile);
            if (isUsingBuildInShader<GameObject>(prefabasset, ref nodelist))
            {
                tryAddBuildInReferenceAsset(prefabfile, nodelist);
            }
        }
    }

    /// <summary>
    /// 指定材质是否使用了内置Shader
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="nodenamelist"></param>
    /// <returns></returns>
    private bool isUsingBuildInShader<T>(T asset, ref List<string> nodenamelist) where T : UnityEngine.Object
    {
        nodenamelist.Clear();
        if (typeof(T) == typeof(Material))
        {
            var mat = asset as Material;
            if (mat.shader != null)
            {
                return isBuildInShader(mat.shader);
            }
            else
            {
                return false;
            }
        }
        else if(typeof(T) == typeof(GameObject))
        {
            var prefab = asset as GameObject;
            var allrenders = prefab.GetComponentsInChildren<Renderer>();
            var isusingbuildinshader = false;
            foreach(var render in allrenders)
            {
                if(render.sharedMaterial != null && render.sharedMaterial.shader != null && isBuildInShader(render.sharedMaterial.shader))
                {
                    nodenamelist.Add(render.transform.name);
                    isusingbuildinshader = true;
                }
            }
            // UI组件
            var allgraphics = prefab.GetComponentsInChildren<Graphic>();
            foreach(var graphic in allgraphics)
            {
                if (graphic.material != null && graphic.material.shader != null && isBuildInShader(graphic.material.shader))
                {
                    nodenamelist.Add(graphic.transform.name);
                    isusingbuildinshader = true;
                }
            }
            return isusingbuildinshader;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 尝试添加Asset的内置Shader的引用信息
    /// </summary>
    /// <param name="assetpath"></param>
    /// <param name="nodenamelist"></param>
    /// <returns></returns>
    private void tryAddBuildInReferenceAsset(string assetpath, List<string> nodenamelist = null)
    {
        if(!mReferenceBuildInAssetMap.ContainsKey(assetpath))
        {
            mReferenceBuildInAssetMap.Add(assetpath, new List<string>());
        }
        foreach(var nodename in nodenamelist)
        {
            if (!string.IsNullOrEmpty(nodename))
            {
                mReferenceBuildInAssetMap[assetpath].Add(nodename);
            }
        }
    }

    /// <summary>
    /// 是否是内置Shader
    /// </summary>
    /// <param name="shader"></param>
    /// <returns></returns>
    private bool isBuildInShader(Shader shader)
    {
        var shaderassetpath = AssetDatabase.GetAssetPath(shader);
        return shaderassetpath.Contains(BuildInResourceName);
    }
}