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
    /// 执行热更新准备任务
    /// </summary>
    /// <param name="assetBundleFolderPath">包内资源目录</param>
    /// <param name="hotupdateOutputFolderPath">热更新目录</param>
    /// <param name="buildTarget">热更新平台</param>
    /// <param name="hotupdateResourceVersion">热更新版本号</param>
    /// <param name="hotupdateVersion">热更新资源版本号</param>
    /// <returns></returns>
    public static bool DoHotUpdatePreparationTask(string assetBundleFolderPath, string hotupdateOutputFolderPath, BuildTarget buildTarget, string hotupdateVersion, int hotupdateResourceVersion)
    {
        string buildTargetFolderName = string.Empty;
        if (!BuildTargetFolderNameMap.ContainsKey(buildTarget))
        {
            Debug.LogError($"不支持的热更新平台:{buildTarget.ToString()},执行热更新准备工作失败!");
            return false;
        }
        buildTargetFolderName = BuildTargetFolderNameMap[buildTarget];

        hotupdateOutputFolderPath = Path.Combine(hotupdateOutputFolderPath, BuildTargetFolderNameMap[buildTarget]);
        Debug.Log($"热更新输出目录:{hotupdateOutputFolderPath}");
        if (!Directory.Exists(hotupdateOutputFolderPath))
        {
            Directory.CreateDirectory(hotupdateOutputFolderPath);
        }
        
        double hotUpdateVersionCode = 0f;
        int resourcenumber = hotupdateResourceVersion;
        if (!double.TryParse(hotupdateVersion, out hotUpdateVersionCode))
        {
            Debug.LogError($"填写的版本号:{hotupdateVersion}无效，请填写有效的版本号!");
            return false;
        }

        if (hotUpdateVersionCode <= 0)
        {
            Debug.LogError($"填写的版本号:{hotUpdateVersionCode}小于等于0无效，请填写有效的版本号!");
            return false;
        }

        // 比对包内版本和资源版本号，判定是否设置版本和资源版本有问题
        VersionConfigModuleManager.Singleton.initVerisonConfigData();
        if (VersionConfigModuleManager.Singleton.InnerGameVersionConfig.VersionCode != hotUpdateVersionCode)
        {
            Debug.LogError($"包内版本号:{VersionConfigModuleManager.Singleton.InnerGameVersionConfig.VersionCode}和热更新版本号:{hotUpdateVersionCode}不一致，请检查版本是否设置正确!");
            return false;
        }

        if (VersionConfigModuleManager.Singleton.InnerGameVersionConfig.ResourceVersionCode >= hotupdateResourceVersion)
        {
            Debug.LogError($"包内资源版本号:{VersionConfigModuleManager.Singleton.InnerGameVersionConfig.ResourceVersionCode}大于等于热更新资源版本号:{hotupdateResourceVersion}，请检查填写的资源版本号是否正确!");
            return false;
        }

        var platformAssetBundlePath = Path.Combine(assetBundleFolderPath, buildTargetFolderName);
        if(!Directory.Exists(platformAssetBundlePath))
        {
            Debug.LogError($"AssetBundle目录:{platformAssetBundlePath}不存在，请检查是否已经打包完成或传入正确的路径!");
            return false;
        }

        // 输出最新的资源热更新版本信息文件(ServerVersionConfig.json)
        var serverversionfilefullname = Path.Combine(hotupdateOutputFolderPath, HotUpdateModuleManager.ServerVersionConfigFileName);
        using (var sw = new StreamWriter(serverversionfilefullname, false))
        {
            var serverversionconfigcontent = "{" + "\"VersionCode\":" + hotUpdateVersionCode + ",\"ResourceVersionCode\":" + resourcenumber + "}";
            sw.WriteLine(serverversionconfigcontent);
            Debug.Log($"热更新最新版本号数据:{serverversionconfigcontent}");
        }

        // 复制所有最新的AB文件到对应热更新版本目录下
        var versionupdatefilefolderpath = Path.Combine(hotupdateOutputFolderPath, hotUpdateVersionCode.ToString());
        versionupdatefilefolderpath = Path.Combine(versionupdatefilefolderpath, hotupdateResourceVersion.ToString());
        // 确保热更新目录最新且存在
        FolderUtilities.RecreateSpecificFolder(versionupdatefilefolderpath);
        FileUtilities.CopyFolderToFolder(platformAssetBundlePath, versionupdatefilefolderpath);

        // 复制最新的包内AssetBundleMD5.txt文件到资源目录下
        var innerAssetBundleMd5FilePath = Path.Combine(Application.dataPath, "Resources");
        innerAssetBundleMd5FilePath = Path.Combine(innerAssetBundleMd5FilePath, ResourceConstData.AssetBundleMd5InfoFileName);
        if(!File.Exists(innerAssetBundleMd5FilePath))
        {
            Debug.LogError($"包内:{innerAssetBundleMd5FilePath}文件不存在,请检查流程!");
            return false;
        }
        FileUtilities.CopyFileToFolder(innerAssetBundleMd5FilePath, versionupdatefilefolderpath);
        return true;
    }
}