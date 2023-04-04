/*
 * Description:             FolderUtilities.cs
 * Author:                  TONYTANG
 * Create Date:             2021//12/26
 */

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

/// <summary>
/// FolderUtilities.cs
/// 目录静态工具类
/// </summary>
public static class FolderUtilities 
{
    /// <summary>
    /// 本机打开特定目录(暂时只用于Windows)
    /// </summary>
    /// <param name="folderPath"></param>
    public static void OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(folderPath, "explorer.exe");
            Process.Start(startInfo);
        }
        else
        {
            UnityEngine.Debug.LogError(string.Format("{0} Directory does not exist!", folderPath));
        }
    }

    /// <summary>
    /// 检查指定目录是否存在，不存在创建一个
    /// </summary>
    public static void CheckAndCreateSpecificFolder(string folderpath)
    {
        if (!Directory.Exists(folderpath))
        {
            Directory.CreateDirectory(folderpath);
        }
    }

    /// <summary>
    /// 无论目录是否存在都删除所有文件重新创建一个目录
    /// </summary>
    public static void RecreateSpecificFolder(string folderpath)
    {
        if (Directory.Exists(folderpath))
        {
            Directory.Delete(folderpath, true);
        }
        Directory.CreateDirectory(folderpath);
    }

    /// <summary>
    /// 清空文件夹
    /// </summary>
    /// <param name="folderPath">要清理的文件夹路径</param>
    public static void ClearFolder(string directoryPath)
    {
        if (Directory.Exists(directoryPath) == false)
            return;

        // 删除文件
        string[] allFiles = Directory.GetFiles(directoryPath);
        for (int i = 0; i < allFiles.Length; i++)
        {
            File.Delete(allFiles[i]);
        }

        // 删除文件夹
        string[] allFolders = Directory.GetDirectories(directoryPath);
        for (int i = 0; i < allFolders.Length; i++)
        {
            Directory.Delete(allFolders[i], true);
        }
    }
}