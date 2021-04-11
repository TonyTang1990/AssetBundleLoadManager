/*
 * Description:             PathUtilities.cs
 * Author:                  TONYTANG
 * Create Date:             2021//04/11
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PathUtilities.cs
/// 路径静态工具类
/// </summary>
public static class PathUtilities
{
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