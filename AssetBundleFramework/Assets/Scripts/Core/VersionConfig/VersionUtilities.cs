/*
 * Description:             VersionUtilities.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/21
 */

using System.IO;
using UnityEngine;

/// <summary>
/// VersionUtilities.cs
/// 版本静态工具类
/// </summary>
public static class VersionUtilities
{
    /// <summary>
    /// 获取包内版本配置文件相对路径(无后缀)
    /// </summary>
    /// <returns></returns>
    public static string GetInnerVersionConfigRelativePath()
    {
        return Path.Combine(VersionConst.ConfigFolderPath, VersionConst.VersionConfigFileName);
    }
    
    /// <summary>
    /// 获取包内版本配置文件全路径(含后缀)
    /// </summary>
    /// <returns></returns>
    public static string GetInnerVersionConfigFullPath()
    {
        var versionConfigRelativePath = GetInnerVersionConfigRelativePath();
        return Path.Combine(Application.dataPath, "Resources", $"{versionConfigRelativePath}.json");;
    }

    /// <summary>
    /// 获取包外版本配置文件全路径
    /// </summary>
    /// <returns></returns>
    public static string GetOutterVersionConfigFolderPath()
    {
        return Path.Combine(Application.persistentDataPath, VersionConst.ConfigFolderPath);
    }

    /// <summary>
    /// 获取包外版本配置文件全路径(含后缀)
    /// </summary>
    /// <returns></returns>
    public static string GetOtterVersionConfigFullPath()
    {
        var outterVersionConfigFolderPath = GetOutterVersionConfigFolderPath();
        return Path.Combine(outterVersionConfigFolderPath, $"{VersionConst.VersionConfigFileName}.json");
    }

    /// <summary>
    /// 读取包内版本配置文件
    /// </summary>
    /// <returns></returns>
    public static VersionConfig ReadInnerVersionConfig()
    {
        //读取包内的版本信息
        var innerVersionConfigFileRelativePath = GetInnerVersionConfigRelativePath();
        var versionconfigasset = Resources.Load<TextAsset>(innerVersionConfigFileRelativePath);
        if (versionconfigasset != null)
        {
            Debug.Log($"包内版本信息文件:{innerVersionConfigFileRelativePath}");
            var content = versionconfigasset.text;
            Debug.Log($"content : {content}");
            var innerVersionConfig = JsonUtility.FromJson<VersionConfig>(content);
            Debug.Log($"VersionCode : {innerVersionConfig.VersionCode} ResourceVersionCode : {innerVersionConfig.ResourceVersionCode}");
            return innerVersionConfig;
        }
        Debug.LogWarning($"包内游戏配置版本信息文件 : {innerVersionConfigFileRelativePath}不存在!无法读取!");
        return null;
    }

    /// <summary>
    /// 读取包外版本配置文件
    /// </summary>
    /// <returns></returns>
    public static VersionConfig ReadOutterVersionConfig()
    {
        var outterVersionConfigFullPath = GetOtterVersionConfigFullPath();
        if (File.Exists(outterVersionConfigFullPath))
        {
            Debug.Log($"包外版本信息文件:{outterVersionConfigFullPath}");
            var content = File.ReadAllText(outterVersionConfigFullPath);
            Debug.Log($"content : {content}");
            var outterVersionConfig = JsonUtility.FromJson<VersionConfig>(content);
            Debug.Log($"VersionCode : {outterVersionConfig.VersionCode} ResourceVersionCode : {outterVersionConfig.ResourceVersionCode}");
            return outterVersionConfig;
        }
        Debug.LogWarning($"包外游戏配置版本信息文件 : {outterVersionConfigFullPath}不存在!无法读取!");
        return null;
    }
}