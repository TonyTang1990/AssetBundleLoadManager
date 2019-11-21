/*
 * Description:             ResourceUtilities.cs
 * Author:                  TANGHUAN
 * Create Date:             2019//11/21
 */

using System;
using UnityEditor;

/// <summary>
/// 编辑器资源静态工具类
/// </summary>
public static class EditorResourceUtilities
{
    /// <summary>
    /// 内置资源名字
    /// </summary>
    private const string BuildInResourceName = "Resources/unity_builtin_extra";

    /// <summary>
    /// 是否是内置资源
    /// </summary>
    /// <param name="shader"></param>
    /// <returns></returns>
    public static bool isBuildInResource(UnityEngine.Object asset)
    {
        var assetpath = AssetDatabase.GetAssetPath(asset);
        return assetpath.Contains(BuildInResourceName);
    }
}