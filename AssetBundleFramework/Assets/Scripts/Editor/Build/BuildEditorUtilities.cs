/*
 * Description:             BuildEditorUtilities.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/21
 */

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BuildEditorUtilities.cs
/// 打包编辑器静态工具类
/// </summary>
public static class BuildEditorUtilities
{
    /// <summary>
    /// 获取打包输出目录路径
    /// </summary>
    /// <param name="buildTarget"></param>
    /// <returns></returns>
    public static string GetBuildOutputFolderPath(BuildTarget buildTarget)
    {
        switch (buildTarget)
        {
            case BuildTarget.Android:
                return GetAndroidBuildOutputFolderPath();
            case BuildTarget.iOS:
                return GetIOSBuildOutputFolderPath();
            default:
                Debug.LogError($"不支持的打包平台: {buildTarget}");
                return string.Empty;
        }
    }

    /// <summary>
    /// 获取Android打包输出目录路径
    /// </summary>
    /// <returns></returns>
    private static string GetAndroidBuildOutputFolderPath()
    {
        return Path.Combine(Application.dataPath, "../Build/Android");
    }

    /// <summary>
    /// 获取IOS打包输出目录路径
    /// </summary>
    /// <returns></returns>
    private static string GetIOSBuildOutputFolderPath()
    {
        return Path.Combine(Application.dataPath, "../Build/IOS");
    }
    /// <summary>
    /// 获取对应的打包分组
    /// </summary>
    /// <param name="buildtarget"></param>
    /// <returns></returns>
    public static BuildTargetGroup GetCorrespondingBuildTaregtGroup(BuildTarget buildtarget)
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
    public static string GetCorrespondingBuildFilePostfix(BuildTarget buildtarget)
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

    /// <summary>
    /// 获取需要打包的场景数组
    /// </summary>
    /// <returns></returns>
    public static string[] GetBuildSceneArray()
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
    /// 计算打包版本号
    /// </summary>
    /// <param name="versionCode"></param>
    /// <param name="resourceVersionCode"></param>
    /// <returns></returns>
    public static int CalculateBuildNumber(double versionCode, int resourceVersionCode)
    {
        // 利用版本号和资源版本号在提交商店时肯定会提升的原理
        // 商店构建版本号 = 版本号(*.**) * 1000000 + 资源版本号(1-9999)
        return (int)(versionCode * 1000000) + resourceVersionCode;
    }
}