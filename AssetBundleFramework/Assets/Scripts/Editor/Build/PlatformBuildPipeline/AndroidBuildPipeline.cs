/*
 * Description:             AndroidBuildPipeline.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/26
 */

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// AndroidBuildPipeline.cs
/// </summary>
public class AndroidBuildPipeline : PlatformBuildPipeline
{
    /// <summary>
    /// Android平台的对应渠道配置
    /// </summary>
    protected AndroidChannelConfig mTargetChannelConfig;

    /// <summary>
    /// AndroidBuildPipeline构造函数
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="buildParameters"></param>
    public AndroidBuildPipeline(string outputPath, BuildParameters buildParameters) : base(outputPath, buildParameters)
    {
    }

    /// <summary>
    /// 初始化Android平台的渠道配置
    /// </summary>
    protected override void InitPlatformChannelConfig()
    {
        var allPlatformChannelConfig = AllPlatformChannelConfig.LoadAllPlatformChannelConfig();
        if(allPlatformChannelConfig != null)
        {
            mTargetChannelConfig = allPlatformChannelConfig.AndroidPlatformChannelConfig.GetChannelConfig(mBuildParameters.Channel);
        }
    }

    /// <summary>
    /// 检查Android平台的渠道配置是否存在
    /// </summary>
    /// <returns></returns>
    protected override bool CheckPlatformChannelConfigExist()
    {
        if(mTargetChannelConfig == null)
        {
            Debug.LogError($"Android平台的渠道:{mBuildParameters.Channel}配置不存在,打包失败!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 检查Android平台的渠道配置参数是否正确
    /// </summary>
    /// <returns></returns>
    protected override bool CheckPlatformChannelConfigParams()
    {
        return mTargetChannelConfig.IsContentValide();
    }
    
    /// <summary>
    /// 执行打包App前处理
    /// </summary>
    /// <returns></returns>
    protected override bool DoBuildTargetPreProcess()
    {
        MakeSureBuildOutputFolderExists();

        // 设置包名和公司名称
        var packageName = mTargetChannelConfig.ApplicationIdentifier;
        var companyName = mTargetChannelConfig.CompanyName;
        var productName = mTargetChannelConfig.ProductName;
        PlayerSettings.applicationIdentifier = packageName;
        PlayerSettings.companyName = companyName;
        PlayerSettings.productName = productName;
        Debug.Log($"打包设置包名:{packageName} 公司名称:{companyName} 产品名称:{productName}");

        // 生成 AAB；false 则生成 APK
        EditorUserBuildSettings.buildAppBundle = true;
        Debug.Log($"打包设置生成AAB为:{EditorUserBuildSettings.buildAppBundle}");
        // Google Play 发布通常至少需要 ARM64，ARMv7主要是为了兼容老机器
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        Debug.Log($"打包设置目标架构为:{PlayerSettings.Android.targetArchitectures}");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        Debug.Log($"打包设置最小SDK版本为:{PlayerSettings.Android.minSdkVersion}");
        // 使用 Unity 安装的最高 Target API
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
        Debug.Log($"打包设置目标SDK版本为:{PlayerSettings.Android.targetSdkVersion}");
        // Release 包开启代码压缩/混淆
        PlayerSettings.Android.minifyRelease = true;
        Debug.Log($"打包设置Release包代码压缩为:{PlayerSettings.Android.minifyRelease}");
        PlayerSettings.Android.minifyDebug = false;
        Debug.Log($"打包设置Debug包代码压缩为:{PlayerSettings.Android.minifyDebug}");
        // 正式包使用自定义签名文件
        PlayerSettings.Android.useCustomKeystore = true;
        Debug.Log($"打包设置使用自定义签名文件为:{PlayerSettings.Android.useCustomKeystore}");

        // 设置签名文件路径
        PlayerSettings.Android.keystoreName = mTargetChannelConfig.KeystoreFileRelativePath;
        PlayerSettings.Android.keystorePass = mTargetChannelConfig.KeystorePassword;
        PlayerSettings.Android.keyaliasName = mTargetChannelConfig.KeyAlias;
        PlayerSettings.Android.keyaliasPass = mTargetChannelConfig.KeyAliasPassword;
        Debug.Log($"打包设置签名文件路径为:{PlayerSettings.Android.keystoreName}");
        Debug.Log($"打包设置签名文件密码为:{PlayerSettings.Android.keystorePass}");
        Debug.Log($"打包设置密钥别名为:{PlayerSettings.Android.keyaliasName}");
        Debug.Log($"打包设置密钥别名密码为:{PlayerSettings.Android.keyaliasPass}");

        // 设置Android平台打包Icons图标信息
        var iconSet = mTargetChannelConfig.IconSet;
        var configLegacyIcons = iconSet.LegacyIcons;
        PlatformIcon[] legacyPlatformIcons = PlayerSettings.GetPlatformIcons(mBuildTargetGroup, AndroidPlatformIconKind.Legacy);
        for(int index = 0, length = legacyPlatformIcons.Length; index < length; index++)
        {
            var legacyPlatformIcon = legacyPlatformIcons[index];
            legacyPlatformIcon.SetTexture(configLegacyIcons[index]);
        }
        PlayerSettings.SetPlatformIcons(mBuildTargetGroup, AndroidPlatformIconKind.Legacy, legacyPlatformIcons);

        PlatformIcon[] adaptivePlatformIcons = PlayerSettings.GetPlatformIcons(mBuildTargetGroup, AndroidPlatformIconKind.Adaptive);
        var configAdaptiveBackgroundIcons = iconSet.AdaptiveBackgroundIcons;
        var configAdaptiveForegroundIcons = iconSet.AdaptiveForegroundIcons;
        for(int index = 0, length = adaptivePlatformIcons.Length; index < length; index++)
        {
            var adaptivePlatformIcon = adaptivePlatformIcons[index];
            var configBackgroundIcon = configAdaptiveBackgroundIcons[index];
            var configForegroundIcon = configAdaptiveForegroundIcons[index];
            adaptivePlatformIcon.SetTexture(configBackgroundIcon, 0);
            adaptivePlatformIcon.SetTexture(configForegroundIcon, 1);
        }
        PlayerSettings.SetPlatformIcons(mBuildTargetGroup, AndroidPlatformIconKind.Adaptive, adaptivePlatformIcons);

        PlatformIcon[] roundPlatformIcons = PlayerSettings.GetPlatformIcons(mBuildTargetGroup, AndroidPlatformIconKind.Round);
        var configRoundIcons = iconSet.RoundIcons;
        for(int index = 0, length = roundPlatformIcons.Length; index < length; index++)
        {
            var roundPlatformIcon = roundPlatformIcons[index];
            roundPlatformIcon.SetTexture(configRoundIcons[index]);
        }
        PlayerSettings.SetPlatformIcons(mBuildTargetGroup, AndroidPlatformIconKind.Round, roundPlatformIcons);
        return true;
    }

    /// <summary>
    /// 执行打包App
    /// </summary>
    /// <returns></returns>
    protected override BuildResult DoRealAppBuild()
    {
        var buildTarget = mBuildParameters.BuildTarget;
        var targetVersionCode = mBuildParameters.VersionCode;
        var targetResourceVersionCode = mBuildParameters.ResourceVersionCode;
        var isDevelopment = mBuildParameters.IsDevelopment;
        Debug.Log($"打包修改版本号从:{PlayerSettings.bundleVersion}到{targetVersionCode}");
        PlayerSettings.bundleVersion = targetVersionCode.ToString();
        var buildNumer = BuildEditorUtilities.CalculateBuildNumber(targetVersionCode, targetResourceVersionCode);
        Debug.Log($"打包修改Android构建版本号从:{PlayerSettings.Android.bundleVersionCode}到{buildNumer}");
        PlayerSettings.Android.bundleVersionCode = buildNumer;
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        var now = DateTime.Now;
        var timeStamp = $"{now.Year}_{now.Month}_{now.Day}_{now.Hour}_{now.Minute}_{now.Second}";
        var buildPostFix = BuildEditorUtilities.GetCorrespondingBuildFilePostfix(buildTarget);
        var outputFileName = $"{PlayerSettings.productName}_{timeStamp}{buildPostFix}";
        // 输出目录结构:Build/版本号/资源版本号/时间戳/包名.apk
        var buildOutputPath = Path.Combine(mOutputFolderPath, $"{targetVersionCode}/{targetResourceVersionCode}/");
        var locationPathName = Path.Combine(buildOutputPath, outputFileName);
        Debug.Log($"打包输出路径:{locationPathName}");
        buildPlayerOptions.locationPathName = locationPathName;
        buildPlayerOptions.scenes = BuildEditorUtilities.GetBuildSceneArray();
        buildPlayerOptions.target = buildTarget;
        buildPlayerOptions.options = BuildOptions.StrictMode;
        if(isDevelopment)
        {
            buildPlayerOptions.options |= BuildOptions.Development;
        }
        Debug.Log($"打包平台:{Enum.GetName(typeof(BuildTarget), buildTarget)}");
        Debug.Log($"开发版本:{isDevelopment}");
        Debug.Log($"打包输出路径:{buildPlayerOptions.locationPathName}");
        buildPlayerOptions.targetGroup = mBuildTargetGroup;
        // 确保设置成打包APK而非Gradle工程和AAB包
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false; 
        EditorUserBuildSettings.buildAppBundle = false;
        var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var buildResult = buildReport.summary.result;
        var isBuildSuccess = buildResult == UnityEditor.Build.Reporting.BuildResult.Succeeded;
        if(!isBuildSuccess)
        {
            Debug.LogError($"打包失败,打包结果:{buildResult}!");
            return BuildResult.Build_Failed;
        }
        return BuildResult.Success;
    }

    /// <summary>
    /// 执行打包App后处理
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    protected override bool DoBuildTargetPostProcess()
    {
        return true;
    }

    /// <summary>
    /// 确保构建输出文件夹存在
    /// </summary>
    private void MakeSureBuildOutputFolderExists()
    {
        var buildOutputFolderPath = GetBuildOutputFolderPath();
        Debug.Log($"buildOutputFolderPath:{buildOutputFolderPath}");
        if (!Directory.Exists(buildOutputFolderPath))
        {
            Directory.CreateDirectory(buildOutputFolderPath);
        }
    }

    /// <summary>
    /// 获取打包输出文件夹路径
    /// </summary>
    /// <returns></returns>
    private string GetBuildOutputFolderPath()
    {
        return Path.Combine(mOutputFolderPath, $"{mBuildParameters.VersionCode}/{mBuildParameters.ResourceVersionCode}/");
    }
}