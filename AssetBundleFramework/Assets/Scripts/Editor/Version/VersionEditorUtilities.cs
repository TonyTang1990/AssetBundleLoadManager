/*
 * Description:             VersionEditorUtilities.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/21
 */

using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// VersionEditorUtilities.cs
/// 版本编辑器工具静态类
/// </summary>
public static class VersionEditorUtilities
{
    /// <summary>
    /// 最大资源版本号
    /// </summary>
    public const int MaxResourceVersionCode = 9999;

    /// <summary>
    /// 格式化版本号为*.**格式
    /// </summary>
    /// <param name="versionCode"></param>
    /// <returns></returns>
    public static (bool, double) FormatVersionCode(double versionCode)
    {
        // 版本号格式只允许*.**
        var versionString = versionCode.ToString("N2", CultureInfo.CreateSpecificCulture("en-US"));
        if (!double.TryParse(versionString, out versionCode))
        {
            Debug.LogError($"不支持的版本号:{versionCode},要求格式:*.**，请传入输入有效版本号值，执行打包失败!");
            return (false, versionCode);
        }
        return (true, versionCode);
    }

    /// <summary>
    /// 验证版本号是否有效
    /// </summary>
    /// <param name="versionCode"></param>
    /// <returns></returns>
    public static bool IsValideVersionCode(double versionCode)
    {
        return versionCode >= 1;
    }

    /// <summary>
    /// 验证资源版本号是否有效
    /// </summary>
    /// <param name="resourceVersionCode"></param>
    /// <returns></returns>
    public static bool IsValideResourceVersionCode(int resourceVersionCode)
    {
        return resourceVersionCode >= 1 && resourceVersionCode <= MaxResourceVersionCode;
    }

    /// <summary>
    /// 存储最新版本号信息到包内
    /// </summary>
    /// <param name="versionCode">版本号</param>
    public static bool SaveVersionCodeInnerConfig(double versionCode)
    {
        var innerVersionConfig = VersionUtilities.ReadInnerVersionConfig();
        var oldResourceVersionCode = innerVersionConfig != null ? innerVersionConfig.ResourceVersionCode : 1;
        return SaveInnerVersionConfig(versionCode, oldResourceVersionCode);
    }

    /// <summary>
    /// 存储最新资源版本号信息到包外
    /// </summary>
    /// <param name="resourceversioncode">资源版本号</param>
    public static bool SaveResoueceCodeInnerConfig(int resourceversioncode)
    {
        var innerVersionConfig = VersionUtilities.ReadInnerVersionConfig();
        var oldVersionCode = innerVersionConfig != null ? innerVersionConfig.VersionCode : 1;
        return SaveInnerVersionConfig(oldVersionCode, resourceversioncode);
    }

    /// <summary>
    /// 存储最新版本号和资源版本号信息到包内
    /// </summary>
    /// <param name="versionCode">版本号</param>
    /// <param name="resourceversioncode">资源版本号</param>
    public static bool SaveInnerVersionConfig(double versionCode = 1, int resourceversioncode = 1)
    {
        // 版本号格式只允许*.**
        (var result, var finalVersionCode) = FormatVersionCode(versionCode);
        if (!result)
        {
            Debug.LogError($"不支持的版本号:{versionCode},要求格式:*.**，请传入输入有效版本号值，执行打包失败!");
            return false;
        }
        if(resourceversioncode <= 0)
        {
            Debug.LogError($"不支持的资源版本号:{resourceversioncode},要求值>0，请传入输入有效资源版本号值，执行打包失败!");
            return false;
        }
        var innerVersionConfig = new VersionConfig(finalVersionCode, resourceversioncode);
        Debug.Log($"存储最新版本号:{finalVersionCode}和资源版本号:{resourceversioncode}到包内!");
        var innerVersionConfigSaveFileFullPath = VersionUtilities.GetInnerVersionConfigFullPath();
        Debug.Log($"innerVersionConfigSaveFileFullPath : {innerVersionConfigSaveFileFullPath}");

        innerVersionConfig.VersionCode = finalVersionCode;
        innerVersionConfig.ResourceVersionCode = resourceversioncode;
        Debug.Log($"innerVersionConfig: {innerVersionConfig}");

        var versionConfigData = JsonUtility.ToJson(innerVersionConfig);
        using (var verisionConfigFS = new StreamWriter(innerVersionConfigSaveFileFullPath, false, Encoding.UTF8))
        {
            verisionConfigFS.Write(versionConfigData);
        }
        return true;
    }
}