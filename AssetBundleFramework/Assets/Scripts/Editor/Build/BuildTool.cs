/*
 * Description:             BuildTool.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/20
 */

using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BuildTool.cs
/// 打包工具静态类
/// </summary>
public static class BuildTool
{
    /// <summary>
    /// 执行Android打包
    /// </summary>
    [MenuItem("Build/Android/打Inner包(None Channel+No SDK)")]
    static void DoBuildInnerAndroid()
    {
        Debug.Log("BuildTool.DoBuildInnerAndroid()");
        var innerVersionConfig = VersionUtilities.ReadInnerVersionConfig();
        var versionCode = innerVersionConfig != null ? innerVersionConfig.VersionCode : 1;
        var resourceVersionCode = innerVersionConfig != null ? innerVersionConfig.ResourceVersionCode : 1;
        var buildParameters = new BuildParameters(BuildTarget.Android, Channel.None, true,
                                                  versionCode, resourceVersionCode, false);
        DoBuild(buildParameters);
    }

    /// <summary>
    /// 执行Android打包
    /// </summary>
    [MenuItem("Build/Android/打Production包(GooglePlay+All SDK)")]
    static void DoBuildProductionAndroid()
    {
        Debug.Log("BuildTool.DoBuildProductionAndroid()");
        var innerVersionConfig = VersionUtilities.ReadInnerVersionConfig();
        var versionCode = innerVersionConfig != null ? innerVersionConfig.VersionCode : 1;
        var resourceVersionCode = innerVersionConfig != null ? innerVersionConfig.ResourceVersionCode : 1;
        var buildParameters = new BuildParameters(BuildTarget.Android, Channel.GooglePlay, true,
                                                  versionCode, resourceVersionCode, false);
        DoBuild(buildParameters);
    }

    /// <summary>
    /// 执行Android打包
    /// </summary>
    [MenuItem("Build/Android/打ThirdpartyTest包(GooglePlay+All SDK)")]
    static void DoBuildThirdpartyTestAndroid()
    {
        Debug.Log("BuildTool.DoBuildThirdpartyTestAndroid()");
        var innerVersionConfig = VersionUtilities.ReadInnerVersionConfig();
        var versionCode = innerVersionConfig != null ? innerVersionConfig.VersionCode : 1;
        var resourceVersionCode = innerVersionConfig != null ? innerVersionConfig.ResourceVersionCode : 1;
        var buildParameters = new BuildParameters(BuildTarget.Android, Channel.GooglePlay, true,
                                                  versionCode, resourceVersionCode, false);
        DoBuild(buildParameters);
    }

    /// <summary>
    /// 执行IOS打包
    /// </summary>
    [MenuItem("Build/IOS/打Debug包")]
    static void DoBuildIOS()
    {
        Debug.Log("BuildTool.DoBuildIOS()");
    }

    #region 命令行打包
    /// <summary>
    /// 执行从命令行打包
    /// </summary>
    public static void DoBuildFromCommands()
    {
        Debug.Log("BuildTool.DoBuildFromCommands()");
        var commandArgs = Environment.GetCommandLineArgs();
        var buildParameters = BuildCommand.ParseCommandArgsToBuildParameters(commandArgs);
        try
        {
            var buildResult = DoBuild(buildParameters);
            EditorApplication.Exit((int)buildResult);
        }
        catch (Exception ex)
        {
            Debug.LogError($"BuildTool.DoBuildFromCommands() Exception:{ex}");
            EditorApplication.Exit((int)BuildResult.Build_Exception);
        }
    }
    #endregion

    /// <summary>
    /// 根据指定输出路径和打包参数创建打包管线对象
    /// </summary>
    /// <param name="outputFolderPath"></param>
    /// <param name="buildParameters"></param>
    /// <returns></returns>
    private static PlatformBuildPipeline CreateBuildPipeline(string outputFolderPath, BuildParameters buildParameters)
    {
        var buildTarget = buildParameters.BuildTarget;
        if(buildTarget == BuildTarget.Android)
        {
            return new AndroidBuildPipeline(outputFolderPath, buildParameters);
        }
        else if(buildTarget == BuildTarget.iOS)
        {
            return new IOSBuildPipeline(outputFolderPath, buildParameters);
        }
        else if(buildTarget == BuildTarget.StandaloneWindows || buildTarget == BuildTarget.StandaloneWindows64)
        {
            return new WindowsBuildPipeline(outputFolderPath, buildParameters);
        }
        else
        {
            Debug.LogError($"不支持的打包平台:{buildTarget}，创建打包管线对象失败！");
            return null;
        }
    }

    /// <summary>
    /// 执行打包(含资源打包)
    /// </summary>
    /// <param name="buildParameters">打包参数</param>
    public static BuildResult DoBuild(BuildParameters buildParameters)
    {
        Debug.Log("BuildTool.DoBuild()");
        if(buildParameters == null)
        {
            Debug.LogError("不允许传入空参数，打包失败！");
            return BuildResult.No_Build_Parameters;
        }
        Debug.Log($"打包参数:{buildParameters}");
        var outputFolderPath = BuildEditorUtilities.GetBuildOutputFolderPath(buildParameters.BuildTarget);
        var buildPipeline = CreateBuildPipeline(outputFolderPath, buildParameters);
        if(buildPipeline == null)
        {
            return BuildResult.Not_Supported_Build_Target;
        }
        var buildResult = buildPipeline.DoBuild();
        if(buildResult != BuildResult.Success)
        {
            Debug.LogError($"[BuildPatch] App打包失败，打包结果:{buildResult}，打包终止！");
            return buildResult;
        }
        Debug.Log($"[BuildPatch] 打包成功，打包结束！");
        return buildResult;
    }
}