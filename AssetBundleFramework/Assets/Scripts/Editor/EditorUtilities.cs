/*
 * Description:             EditorUtilities.cs
 * Author:                  TONYTANG
 * Create Date:             2021//04/11
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// EditorUtilities.cs
/// 编辑器静态工具类
/// </summary>
public static class EditorUtilities
{
    private static MethodInfo _clearConsoleMethod;
    private static MethodInfo ClearConsoleMethod
    {
        get
        {
            if (_clearConsoleMethod == null)
            {
                Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                System.Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                _clearConsoleMethod = logEntries.GetMethod("Clear");
            }
            return _clearConsoleMethod;
        }
    }

    #region 路径相关
    /// <summary>
    /// 获取项目工程路径
    /// </summary>
    public static string GetProjectPath()
    {
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        return PathUtilities.GetRegularPath(projectPath);
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
    #endregion

    /// <summary>
    /// 清空控制台
    /// </summary>
    public static void ClearUnityConsole()
    {
        ClearConsoleMethod.Invoke(new object(), null);
    }
    
    /// <summary>
    /// 是否是数字
    /// </summary>
    public static bool IsNumber(string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;
        string pattern = @"^\d*$";
        return Regex.IsMatch(content, pattern);
    }
}