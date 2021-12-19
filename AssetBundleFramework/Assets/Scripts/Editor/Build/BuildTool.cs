/*
 * Description:             BuildTool.cs
 * Author:                  TONYTANG
 * Create Date:             2021//12/19
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BuildTool.cs
/// 打包工具
/// </summary>
public static class BuildTool 
{
    /// <summary>
    /// 执行打包
    /// </summary>
    /// <param name="buildOutputPath">打包输出目录</param>
    /// <param name="buildTarget">打包平台</param>
    /// <param name="versionCode">版本号</param>
    /// <param name="resourceVersionCode">资源版本号</param>
    /// <param name="isDevelopment">是否打开发包</param>
    public static void DoBuild(string buildOutputPath, BuildTarget buildTarget, double versionCode, int resourceVersionCode, bool isDevelopment = false)
    {
        Debug.Log("BuildTool.DoBuild()");
        // 版本号格式只允许*.*
        var versionString = versionCode.ToString("N1", CultureInfo.CreateSpecificCulture("en-US"));
        if (!double.TryParse(versionString, out versionCode))
        {
            Debug.LogError($"不支持的版本号:{versionCode},请传入输入有效版本号值!");
            return;
        }
        if (string.IsNullOrEmpty(buildOutputPath))
        {
            buildOutputPath = $"{Application.dataPath}/../../../Build/";
        }
        Debug.Log($"buildOutputPath:{buildOutputPath}");
        if (!string.IsNullOrEmpty(buildOutputPath))
        {
            if (!Directory.Exists(buildOutputPath))
            {
                Directory.CreateDirectory(buildOutputPath);
            }
            var buildtargetgroup = GetCorrespondingBuildTaregtGroup(buildTarget);
            Debug.Log($"打包分组:{Enum.GetName(typeof(BuildTargetGroup), buildtargetgroup)}");
            if (buildtargetgroup != BuildTargetGroup.Unknown)
            {
                VersionConfigModuleManager.Singleton.initVerisonConfigData();
                var innerversioncode = VersionConfigModuleManager.Singleton.InnerGameVersionConfig.VersionCode;
                var innerresourceversioncode = VersionConfigModuleManager.Singleton.InnerGameVersionConfig.ResourceVersionCode;
                Debug.Log("打包版本信息:");
                Debug.Log($"版本号:{versionCode} 资源版本号:{resourceVersionCode}");
                Debug.Log($"包内VersionConfig信息:");
                Debug.Log($"版本号:{innerversioncode} 资源版本号:{innerresourceversioncode}");
                var prebundleversion = PlayerSettings.bundleVersion;
                PlayerSettings.bundleVersion = versionCode.ToString();
                Debug.Log($"打包修改版本号从:{prebundleversion}到{PlayerSettings.bundleVersion}");
                Debug.Log($"打包修改VersionConfig从:Version:{innerversioncode}到{versionCode} ResourceVersion:{innerresourceversioncode}到{resourceVersionCode}");
                VersionConfigModuleManager.Singleton.saveNewVersionCodeInnerConfig(versionCode);
                VersionConfigModuleManager.Singleton.saveNewResoueceCodeInnerConfig(resourceVersionCode);
                BuildPlayerOptions buildplayeroptions = new BuildPlayerOptions();
                var version_Code = versionCode.ToString().Replace('.', '-');
                buildplayeroptions.locationPathName = buildOutputPath + Path.DirectorySeparatorChar + PlayerSettings.productName + $"_{version_Code}_{resourceVersionCode}" + GetCorrespondingBuildFilePostfix(buildTarget);
                buildplayeroptions.scenes = GetBuildSceneArray();
                buildplayeroptions.target = buildTarget;
                buildplayeroptions.options = BuildOptions.StrictMode;
                if(isDevelopment)
                {
                    buildplayeroptions.options |= BuildOptions.Development;
                }
                Debug.Log($"打包平台:{Enum.GetName(typeof(BuildTarget), buildTarget)}");
                Debug.Log($"开发版本:{isDevelopment}");
                Debug.Log($"打包输出路径:{buildplayeroptions.locationPathName}");
                buildplayeroptions.targetGroup = buildtargetgroup;
                EditorUserBuildSettings.SwitchActiveBuildTarget(buildtargetgroup, buildTarget);
                BuildPipeline.BuildPlayer(buildplayeroptions);
            }
            else
            {
                Debug.LogError("不支持的打包平台选择,打包失败!");
            }
        }
        else
        {
            Debug.LogError("打包输出目录为空或不存在,打包失败!");
        }
    }

    /// <summary>
    /// 获取需要打包的场景数组
    /// </summary>
    /// <returns></returns>
    private static string[] GetBuildSceneArray()
    {
        //暂时默认BuildSetting里设置的场景才是要进包的场景
        List<string> editorscenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            editorscenes.Add(scene.path);
            Debug.Log($"需要打包的场景:{scene.path}");
        }
        return editorscenes.ToArray();
    }

    /// <summary>
    /// 获取对应的打包分组
    /// </summary>
    /// <param name="buildtarget"></param>
    /// <returns></returns>
    private static BuildTargetGroup GetCorrespondingBuildTaregtGroup(BuildTarget buildtarget)
    {
        switch (buildtarget)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return BuildTargetGroup.Standalone;
            case BuildTarget.Android:
                return BuildTargetGroup.Android;
            case BuildTarget.iOS:
                return BuildTargetGroup.iOS;
            default:
                return BuildTargetGroup.Unknown;
        }
    }

    /// <summary>
    /// 获取对应的打包分组的打包文件后缀
    /// </summary>
    /// <param name="buildtarget"></param>
    /// <returns></returns>
    private static string GetCorrespondingBuildFilePostfix(BuildTarget buildtarget)
    {
        switch (buildtarget)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return ".exe";
            case BuildTarget.Android:
                return ".apk";
            case BuildTarget.iOS:
                return "";
            default:
                return "";
        }
    }
}