/*
 * Description:             PlatformBuildPipeline.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/26
 */

using TResource;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// PlatformBuildPipeline.cs
/// 平台打包管线抽象类
/// Note:
/// 负责抽象打包流程，包含资源打包和APP打包
/// </summary>
public abstract class PlatformBuildPipeline
{
    /// <summary>
    /// 打包输出目录路径
    /// </summary>
    protected string mOutputFolderPath;

    /// <summary>
    /// 打包参数
    /// </summary>
    protected BuildParameters mBuildParameters;

    /// <summary>
    /// 对应的打包分组
    /// </summary>
    protected BuildTargetGroup mBuildTargetGroup;

    private PlatformBuildPipeline()
    {
        
    }

    /// <summary>
    /// BuildPipeline构造函数
    /// </summary>
    /// <param name="outputFolderPath"></param>
    /// <param name="buildParameters"></param>
    public PlatformBuildPipeline(string outputFolderPath, BuildParameters buildParameters)
    {
        mOutputFolderPath = outputFolderPath;
        mBuildParameters = buildParameters;
        mBuildTargetGroup = BuildEditorUtilities.GetCorrespondingBuildTaregtGroup(mBuildParameters.BuildTarget);
        InitPlatformChannelConfig();
    }

    /// <summary>
    /// 初始化平台渠道配置
    /// </summary>
    protected abstract void InitPlatformChannelConfig();

    /// <summary>
    /// 执行打包
    /// </summary>
    /// <returns></returns>
    public BuildResult DoBuild()
    {
        if(!CheckOutputPath())
        {
            return BuildResult.Invalide_Output_Path;
        }
        if(!CheckBuildParameters())
        {
            return BuildResult.Invalide_Build_Parameters;
        }
        if(mBuildTargetGroup == BuildTargetGroup.Unknown)
        {
            return BuildResult.Not_Supported_Build_Target_Group;
        }
        if(!CheckInnerVersionConfigExist())
        {
            return BuildResult.No_Inner_Version_Config;
        }
        if(!CheckPlatformChannelConfigExist())
        {
            return BuildResult.No_Platform_Channel_Config;
        }
        if(!CheckPlatformChannelConfigParams())
        {
            return BuildResult.Incorrect_Platform_Channel_Config;
        }
        var commonBuildPreProcessResult = DoCommonBuildPreProcess();
        if(!commonBuildPreProcessResult)
        {
            Debug.LogError("打包通用前处理失败，打包终止!");
            return BuildResult.Build_Common_PreProcess_Failed;
        }
        var buildResourceResult = DoBuildResource();
        if(!buildResourceResult)
        {
            Debug.LogError("资源打包失败，打包终止!");
            return BuildResult.Build_Resource_Failed;
        }
        var buildTargetCommonPreProcessResult = DoBuildTargetCommonPreProcess();
        if(!buildTargetCommonPreProcessResult)
        {
            Debug.LogError("打包目标通用前处理失败，打包终止!");
            return BuildResult.Build_Target_Common_PreProcess_Failed;
        }
        var buildTargetPreProcessResult = DoBuildTargetPreProcess();
        if(!buildTargetPreProcessResult)
        {
            Debug.LogError("打包预处理失败，打包终止!");
            return BuildResult.Build_Target_PreProcess_Failed;
        }
        var buildResult = DoRealAppBuild();
        if(buildResult != BuildResult.Success)
        {
            Debug.LogError("打包失败，打包终止!");
            return buildResult;
        }
        var buildTargetPostProcessResult = DoBuildTargetPostProcess();
        if(!buildTargetPostProcessResult)
        {
            Debug.LogError("打包目标后处理失败，打包终止!");
            return BuildResult.Build_Target_PostProcess_Failed;
        }
        return BuildResult.Success;
    }

    /// <summary>
    /// 执行打包前通用处理
    /// </summary>
    protected bool DoCommonBuildPreProcess()
    {
        Debug.Log("BuildPipeline.DoCommonBuildPreProcess()");
        EditorSceneManager.SaveOpenScenes();
        EditorUserBuildSettings.SwitchActiveBuildTarget(mBuildTargetGroup, mBuildParameters.BuildTarget);
        return true;
    }

    /// <summary>
    /// 执行资源打包
    /// </summary>
    protected virtual bool DoBuildResource()
    {
        Debug.Log("BuildPipeline.DoBuildResource()");
        var assetBundleBuilder = ResourceBuildTool.GetTargetAssetBundleBuilder(mBuildParameters.BuildTarget);
        assetBundleBuilder.AssetBundleBuildParams.AssetBundleBuildPurpose = AssetBundleBuildPurpose.BuildPlayerBaseLine;
        assetBundleBuilder.AssetBundleBuildParams.IsForceRebuild = mBuildParameters.ForceRebuildAB;
        return ResourceBuildTool.DoBuildAssetBundleByBuilder(assetBundleBuilder);
    }

    /// <summary>
    /// 执行打包目标通用前处理
    /// </summary>
    /// <returns></returns>
    protected bool DoBuildTargetCommonPreProcess()
    {
        Debug.Log("BuildPipeline.DoBuildTargetCommonPreProcess()");
        var saveInnerConfigVersionResult = SaveNewInnerConfigVersion();
        if(!saveInnerConfigVersionResult)
        {
            Debug.LogError("保存新的内部版本配置文件失败，打包终止!");
            return false;
        }
        // 脚本后端：IL2CPP
        PlayerSettings.SetScriptingBackend(mBuildTargetGroup, ScriptingImplementation.IL2CPP);
        Debug.Log($"打包设置脚本后端为:{ScriptingImplementation.IL2CPP}");
        // Player Settings 中的 Api Compatibility Level：
        // 这里的“.NET Framework”不是脚本后端，而是可使用的 .NET API 范围
        PlayerSettings.SetApiCompatibilityLevel(mBuildTargetGroup, ApiCompatibilityLevel.NET_Unity_4_8);
        Debug.Log($"打包设置Api兼容性级别为:{ApiCompatibilityLevel.NET_Unity_4_8}");
        // IL2CPP C++ 编译配置
        PlayerSettings.SetIl2CppCompilerConfiguration(mBuildTargetGroup, Il2CppCompilerConfiguration.Release);
        Debug.Log($"打包设置IL2CPP C++ 编译配置为:{Il2CppCompilerConfiguration.Release}");
        // 打开增量GC
        PlayerSettings.gcIncremental = true;
        Debug.Log("打包设置增量GC为:true");
        // IL2CPP 代码生成方式
        NamedBuildTarget nameBuildTarget = NamedBuildTarget.FromBuildTargetGroup(mBuildTargetGroup);
        PlayerSettings.SetIl2CppCodeGeneration(nameBuildTarget, Il2CppCodeGeneration.OptimizeSpeed);
        Debug.Log($"打包设置IL2CPP代码生成方式为:{Il2CppCodeGeneration.OptimizeSpeed}");
        // 托管代码裁剪等级
        PlayerSettings.SetManagedStrippingLevel(mBuildTargetGroup, ManagedStrippingLevel.Medium);
        Debug.Log($"打包设置托管代码裁剪等级为:{ManagedStrippingLevel.Medium}");
        return true;
    }

    /// <summary>
    /// 执行打包App前预处理
    /// </summary>
    protected abstract bool DoBuildTargetPreProcess(); 

    /// <summary>
    /// 执行真正App打包方法
    /// </summary>
    /// <returns></returns>
    protected abstract BuildResult DoRealAppBuild();

    /// <summary>
    /// 执行打包App后处理
    /// </summary>
    protected abstract bool DoBuildTargetPostProcess();

    /// <summary>
    /// 检查打包输出路径是否有效
    /// </summary>
    /// <returns></returns>
    protected bool CheckOutputPath()
    {
        if(string.IsNullOrEmpty(mOutputFolderPath))
        {
            Debug.LogError("打包输出目录为空,打包失败!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 检查打包参数是否有效
    /// </summary>
    /// <returns></returns>
    protected virtual bool CheckBuildParameters()
    {
        if(mBuildParameters == null)
        {
            Debug.LogError("不允许传入空的打包参数，执行打包失败！");
            return false;
        }
        if(!VersionEditorUtilities.IsValideVersionCode(mBuildParameters.VersionCode))
        {
            Debug.LogError($"不支持的版本号:{mBuildParameters.VersionCode},要求格式:*.**且大于等于1，请传入输入有效版本号值，执行打包失败!");
            return false;
        }
        if(!VersionEditorUtilities.IsValideResourceVersionCode(mBuildParameters.ResourceVersionCode))
        {
            Debug.LogError($"不支持的资源版本号:{mBuildParameters.ResourceVersionCode},要求范围:0~{VersionEditorUtilities.MaxResourceVersionCode}，请传入输入有效资源版本号值，执行打包失败!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 检查内部版本配置文件是否存在
    /// </summary>
    /// <returns></returns>
    protected bool CheckInnerVersionConfigExist()
    {
        var innerVersionConfig = VersionUtilities.ReadInnerVersionConfig();
        if(innerVersionConfig == null)
        {
            Debug.LogError("包内VersionConfig读取失败,打包失败!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 检查平台渠道配置是否存在
    /// </summary>
    /// <returns></returns>
    protected abstract bool CheckPlatformChannelConfigExist();

    /// <summary>
    /// 检查平台渠道配置参数是否正确
    /// </summary>
    /// <returns></returns>
    protected abstract bool CheckPlatformChannelConfigParams();

    /// <summary>
    /// 保存新的内部版本配置文件
    /// </summary>
    /// <returns></returns>
    protected bool SaveNewInnerConfigVersion()
    {
        var innerVersionConfig = VersionUtilities.ReadInnerVersionConfig();
        var innerVersionCode = innerVersionConfig.VersionCode;
        var innerResourceVersionCode = innerVersionConfig.ResourceVersionCode;
        var targetVersionCode = mBuildParameters.VersionCode;
        var targetResourceVersionCode = mBuildParameters.ResourceVersionCode;
        Debug.Log($"打包修改包内VersionConfig从:Version:{innerVersionCode}到{targetVersionCode} ResourceVersion:{innerResourceVersionCode}到{targetResourceVersionCode}");
        VersionEditorUtilities.SaveInnerVersionConfig(targetVersionCode, targetResourceVersionCode);
        return true;
    }
}