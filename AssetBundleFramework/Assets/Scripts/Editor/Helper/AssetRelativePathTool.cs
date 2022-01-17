/*
 * Description:             AssetRelativePathTool.cs
 * Author:                  TANGHUAN
 * Create Date:             2020/03/13
 */

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 快速获取Asset相对路径工具
/// </summary>
public class AssetRelativePathTool : MonoBehaviour
{
    [MenuItem("Assets/快速获取Asset相对路径/含后缀 &#q", false, 20000)]
    public static void GetAssetRelativePathWithPostfix()
    {
        var assets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets | SelectionMode.TopLevel);
        var allRelativePath = GetAllAssetRelativePath(assets, true);
        Debug.Log($"{allRelativePath}");
        EditorGUIUtility.systemCopyBuffer = allRelativePath;
    }

    [MenuItem("Assets/快速获取Asset相对路径/无后缀 &#w", false, 20000)]
    public static void GetAssetRelativePathWithoutPostfix()
    {
        var assets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets | SelectionMode.TopLevel);
        Debug.Log($"当前选中Asset数量:{assets.Length}");
        var allRelativePath = GetAllAssetRelativePath(assets, false);
        Debug.Log($"{allRelativePath}");
        EditorGUIUtility.systemCopyBuffer = allRelativePath;
    }

    //[MenuItem("Assets/快速获取Asset相对路径/小写含后缀 &#e", false, 20000)]
    //public static void GetAssetRelativePathWithPostfix()
    //{
    //    var assets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets | SelectionMode.TopLevel);
    //    var allRelativePath = GetAllAssetRelativePath(assets, true, true);
    //    Debug.Log($"{allRelativePath}");
    //    EditorGUIUtility.systemCopyBuffer = allRelativePath;
    //}

    //[MenuItem("Assets/快速获取Asset相对路径/小写无后缀 &#r", false, 20000)]
    //public static void GetAssetRelativePathWithoutPostfix()
    //{
    //    var assets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets | SelectionMode.TopLevel);
    //    Debug.Log($"当前选中Asset数量:{assets.Length}");
    //    var allRelativePath = GetAllAssetRelativePath(assets, false, true);
    //    Debug.Log($"{allRelativePath}");
    //    EditorGUIUtility.systemCopyBuffer = allRelativePath;
    //}

    /// <summary>
    /// 获取Asset的相对路径
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="withPostfix">是否包含后缀名</param>
    /// <param name="toLowerPath">是否全小写路径</param>
    private static string GetAllAssetRelativePath(UnityEngine.Object[] assets, bool withPostfix = false, bool toLowerPath = false)
    {
        if (assets == null)
        {
            Debug.LogError($"传递空Assets，获取相对路径失败!");
            return string.Empty;
        }
        var allRelativePaths = string.Empty;
        for (int i = 0, length = assets.Length; i < length; i++)
        {
            var assetrelativepath = GetAssetRelativePath(assets[i], withPostfix);
            if (i != (length - 1))
            {
                assetrelativepath = assetrelativepath + Environment.NewLine;
            }
            allRelativePaths += assetrelativepath;
        }
        return !toLowerPath ? allRelativePaths : allRelativePaths.ToLower();
    }

    /// <summary>
    /// 获取Asset的相对路径
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="withPostfix">是否包含后缀名</param>
    private static string GetAssetRelativePath(UnityEngine.Object asset, bool withPostfix = false)
    {
        Debug.Assert(asset != null, "不允许传空Asset,获取Asset相对路径失败!");
        var assetRelativePath = AssetDatabase.GetAssetPath(asset);
        if (withPostfix == false)
        {
            var extension = Path.GetExtension(assetRelativePath);
            assetRelativePath = assetRelativePath.Substring(0, assetRelativePath.Length - extension.Length);
        }
        return assetRelativePath;
    }
}