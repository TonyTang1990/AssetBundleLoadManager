/*
 * Description:             PathUtilities.cs
 * Author:                  TONYTANG
 * Create Date:             2021//04/11
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// PathUtilities.cs
/// 路径静态工具类
/// </summary>
public static class PathUtilities
{
    /// <summary>
    /// 获取规范的路径
    /// </summary>
    public static string GetRegularPath(string path)
    {
        return path.Replace('\\', '/').Replace("\\", "/"); //替换为Linux路径格式
    }

    /// <summary>
    /// 获取指定路径的目录名
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static string GetFolderName(string path)
    {
        var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(path));
        return directoryInfo.Name;
    }

    /// <summary>
    /// 获取资源Asset相对路径
    /// </summary>
    /// <param name="folderfullpath"></param>
    /// <returns></returns>
    public static string GetAssetsRelativeFolderPath(string folderfullpath)
    {
        var projectpathprefix = Application.dataPath.Replace("Assets", string.Empty);
        if (folderfullpath.StartsWith(projectpathprefix))
        {
            var relativefolderpath = folderfullpath.Replace(projectpathprefix, string.Empty);
            return relativefolderpath;
        }
        else
        {
            Debug.LogError($"目录:{folderfullpath}不是项目有效路径,获取相对路径失败!");
            return string.Empty;
        }
    }
}