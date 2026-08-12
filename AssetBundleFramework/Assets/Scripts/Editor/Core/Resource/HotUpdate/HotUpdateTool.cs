/*
 * Description:             HotUpdateTool.cs
 * Author:                  TONYTANG
 * Create Date:             2021//12/19
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TResource;
using UnityEditor;
using UnityEngine;

/// <summary>
/// HotUpdateTool.cs
/// 热更新工具
/// </summary>
public static class HotUpdateTool
{
    /// <summary>
    /// 打包平台文件夹名映射Map<打包平台,对应文件夹名>
    /// </summary>
    private static Dictionary<BuildTarget, string> BuildTargetFolderNameMap = new Dictionary<BuildTarget, string>
    {
        { BuildTarget.Android, BuildTarget.Android.ToString() },
        { BuildTarget.iOS, BuildTarget.iOS.ToString() },
        { BuildTarget.StandaloneWindows, BuildTarget.StandaloneWindows.ToString() },
        { BuildTarget.StandaloneOSX, BuildTarget.StandaloneOSX.ToString() },
    };

    /// <summary>
    /// 热更新文件后缀名黑名单列表
    /// </summary>
    private static List<string> HotUpdateFilePostFixBlackList = new List<string>
    {
        ".meta",
        ".manifest",
    };

    /// <summary>
    /// 更新热更新版本配置文件(ServerVersionConfig.json)
    /// </summary>
    /// <param name="hotupdateVersion"></param>
    /// <param name="hotupdateResourceVersion"></param>
    /// <returns></returns>
    public static bool UpdateHotUpdateVersionConfig(string hotupdateOutputFolderPath,
                                                    double hotupdateVersion,
                                                    int hotupdateResourceVersion)
    {
        Debug.Log($"热更新输出目录:{hotupdateOutputFolderPath}");
        if (!Directory.Exists(hotupdateOutputFolderPath))
        {
            Directory.CreateDirectory(hotupdateOutputFolderPath);
        }
        
        if (hotupdateVersion <= 0)
        {
            Debug.LogError($"填写的版本号:{hotupdateVersion}小于等于0无效，请填写有效的版本号!");
            return false;
        }

        // 矫正版本号
        hotupdateVersion = AssetBundleBuilder.GetCorrectVersionCode(hotupdateVersion);

        // 比对包内版本和资源版本号，判定是否设置版本和资源版本有问题
        VersionConfigModuleManager.Singleton.InitVerisonConfigData();
        var innerGameVersionConfig = VersionConfigModuleManager.Singleton.InnerGameVersionConfig;
        if(innerGameVersionConfig == null)
        {
            Debug.LogError($"获取不到包内版本配置文件，请检查代码流程!");
            return false;
        }
        if (innerGameVersionConfig.VersionCode > hotupdateVersion)
        {
            Debug.LogError($"包内版本号:{innerGameVersionConfig.VersionCode}大于热更新版本号:{hotupdateVersion}，请检查版本是否设置正确!");
            return false;
        }

        if (innerGameVersionConfig.ResourceVersionCode > hotupdateResourceVersion)
        {
            Debug.LogError($"包内资源版本号:{innerGameVersionConfig.ResourceVersionCode}大于热更新资源版本号:{hotupdateResourceVersion}，请检查填写的资源版本号是否正确!");
            return false;
        }
        // 输出最新的资源热更新版本信息文件(ServerVersionConfig.json)
        var utf8WithoutBom = new UTF8Encoding(false);
        var serverVersionFileFullName = Path.Combine(hotupdateOutputFolderPath, HotUpdateModuleManager.ServerVersionConfigFileName);
        var serverVersionConfig = new VersionConfig(hotupdateVersion, hotupdateResourceVersion);
        var serverversionconfigcontent = JsonUtility.ToJson(serverVersionConfig, true);
        File.WriteAllText(serverVersionFileFullName, serverversionconfigcontent, utf8WithoutBom);
        Debug.Log($"热更新最新版本号数据:{serverversionconfigcontent}");
        return true;
    }
}