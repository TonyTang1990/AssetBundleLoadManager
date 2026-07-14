/*
 * Description:             EditorAssetInfoPath.cs
 * Author:                  TONYTANG
 * Create Date:             2026//07/13
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EditorAssetInfoPath.cs
/// EditorAssetInfoPath类,用于存储编辑器Asset路径信息
/// </summary>
public static class EditorAssetInfoPath
{
    /// <summary>
    /// EditorAssetInfoAsset名(含后缀)
    /// </summary>
    public const string EditorAssetInfoAssetName = "EditorAssetInfoAsset.asset";

    /// <summary>
    /// EditorAssetInfo Asset的输出目录相对路径
    /// </summary>
    public const string EditorAssetInfoAssetFolderRelativePath = "Res/editorAssetInfoAsset";

    /// <summary>
    /// 获取EditorAssetInfo Asset的名字(含后缀)
    /// </summary>
    /// <returns></returns>
    public static string GetEditorAssetInfoAssetName()
    {
        return EditorAssetInfoAssetName;
    }

    /// <summary>
    /// 获取EditorAssetInfo Asset所在目录全路径
    /// </summary>
    /// <returns></returns>
    public static string GetEditorAssetInfoFolderFullPath()
    {
        return $"{Application.dataPath}/{EditorAssetInfoAssetFolderRelativePath}";
    }

    /// <summary>
    /// 获取EditorAssetInfo文件相对路径
    /// </summary>
    /// <returns></returns>
    public static string GetEditorAssetInfoAssetFileRelativePath()
    {
        return $"Assets/{EditorAssetInfoAssetFolderRelativePath}/{GetEditorAssetInfoAssetName()}";
    }
}