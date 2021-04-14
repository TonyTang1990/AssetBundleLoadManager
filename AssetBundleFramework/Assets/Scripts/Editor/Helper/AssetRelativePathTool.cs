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
        if (assets.Length > 0)
        {
            var allrelativepath = GetAllAssetRelativePath(assets, true);
            Debug.Log($"{allrelativepath}");
            EditorGUIUtility.systemCopyBuffer = allrelativepath;
        }
        else
        {
            Debug.LogError("未选中有效Asset!");
        }
    }

    [MenuItem("Assets/快速获取Asset相对路径/无后缀 &#w", false, 20000)]
    public static void GetAssetRelativePathWithoutPostfix()
    {
        var assets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets | SelectionMode.TopLevel);
        Debug.Log($"当前选中Asset数量:{assets.Length}");
        if (assets.Length > 0)
        {
            var allrelativepath = GetAllAssetRelativePath(assets, false);
            Debug.Log($"{allrelativepath}");
            EditorGUIUtility.systemCopyBuffer = allrelativepath;
        }
        else
        {
            Debug.LogError("未选中有效Asset!");
        }
    }


    /// <summary>
    /// 获取Asset的相对路径
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="withpostfix">是否包含后缀名</param>
    private static string GetAllAssetRelativePath(UnityEngine.Object[] assets, bool withpostfix = false)
    {
        Debug.Assert(assets != null, "不允许传空Assets!");
        var allrelativepaths = string.Empty;
        for (int i = 0, length = assets.Length; i < length; i++)
        {
            var assetrelativepath = GetAssetRelativePath(assets[i], withpostfix);
            if (i != (length - 1))
            {
                assetrelativepath = assetrelativepath + Environment.NewLine;
            }
            allrelativepaths += assetrelativepath;
        }
        return allrelativepaths;
    }

    /// <summary>
    /// 获取Asset的相对路径
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="withpostfix">是否包含后缀名</param>
    private static string GetAssetRelativePath(UnityEngine.Object asset, bool withpostfix = false)
    {
        Debug.Assert(asset != null, "不允许传空Asset!");
        var assetpath = AssetDatabase.GetAssetPath(asset);
        var assetrelativepath = assetpath;
        //var assetrelativepath = assetpath.Substring(7);
        //if (!assetrelativepath.StartsWith("Resources/"))
        //{
        //    Debug.LogWarning("默认只允许访问Resources目录里的资源!");
        //}
        //else
        //{
        //    assetrelativepath = assetrelativepath.Substring(10);
        //}
        if (withpostfix == false)
        {
            var extension = Path.GetExtension(assetrelativepath);
            assetrelativepath = assetrelativepath.Substring(0, assetrelativepath.Length - extension.Length);
        }
        return assetrelativepath;
    }
}