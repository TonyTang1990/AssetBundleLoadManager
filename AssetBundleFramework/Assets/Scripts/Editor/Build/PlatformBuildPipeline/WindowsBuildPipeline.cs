/*
 * Description:             WindowsBuildPipeline.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/26
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WindowsBuildPipeline.cs
/// </summary>
public class WindowsBuildPipeline : PlatformBuildPipeline
{
    /// <summary>
    /// Windows平台的对应渠道配置对象
    /// </summary>
    protected WindowsChannelConfig mTargetChannelConfig;

    /// <summary>
    /// WindowsBuildPipeline构造函数
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="buildParameters"></param>
    public WindowsBuildPipeline(string outputPath, BuildParameters buildParameters) : base(outputPath, buildParameters)
    {
        
    }


    /// <summary>
    /// 初始化Windows平台的渠道配置
    /// </summary>
    protected override void InitPlatformChannelConfig()
    {
        var allPlatformChannelConfig = AllPlatformChannelConfig.LoadAllPlatformChannelConfig();
        if(allPlatformChannelConfig != null)
        {
            mTargetChannelConfig = allPlatformChannelConfig.WindowsPlatformChannelConfig.GetChannelConfig(mBuildParameters.Channel);
        }
    }

    /// <summary>
    /// 检查Windows平台的渠道配置是否存在
    /// </summary>
    /// <returns></returns>
    protected override bool CheckPlatformChannelConfigExist()
    {
        if(mTargetChannelConfig == null)
        {
            Debug.LogError($"Windows平台的渠道:{mBuildParameters.Channel}配置不存在,打包失败!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 检查Windows平台的渠道配置参数是否正确
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
        return true;
    }

    /// <summary>
    /// 执行打包App
    /// </summary>
    /// <returns></returns>
    protected override BuildResult DoRealAppBuild()
    {
        return BuildResult.Not_Supported_Build_Target;
    }

    /// <summary>
    /// 执行打包App后处理
    /// </summary>
    /// <returns></returns>
    protected override bool DoBuildTargetPostProcess()
    {
        return true;
    }
}