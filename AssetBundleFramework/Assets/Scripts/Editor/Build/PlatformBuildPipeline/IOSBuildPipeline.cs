/*
 * Description:             IOSBuildPipeline.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/26
 */

using UnityEngine;

/// <summary>
/// IOSBuildPipeline.cs
/// iOS平台的打包管线
/// </summary>
public class IOSBuildPipeline : PlatformBuildPipeline
{
    /// <summary>
    /// iOS平台的对应渠道配置对象
    /// </summary>
    protected IOSChannelConfig mTargetChannelConfig;

    /// <summary>
    /// IOSBuildPipeline构造函数
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="buildParameters"></param>
    public IOSBuildPipeline(string outputPath, BuildParameters buildParameters) : base(outputPath, buildParameters)
    {
        
    }

    /// <summary>
    /// 初始化IOS平台的渠道配置
    /// </summary>
    protected override void InitPlatformChannelConfig()
    {
        var allPlatformChannelConfig = AllPlatformChannelConfig.LoadAllPlatformChannelConfig();
        if(allPlatformChannelConfig != null)
        {
            mTargetChannelConfig = allPlatformChannelConfig.IOSPlatformChannelConfig.GetChannelConfig(mBuildParameters.Channel);
        }
    }

    /// <summary>
    /// 检查IOS平台的渠道配置是否存在
    /// </summary>
    /// <returns></returns>
    protected override bool CheckPlatformChannelConfigExist()
    {
        if(mTargetChannelConfig == null)
        {
            Debug.LogError($"IOS平台的渠道:{mBuildParameters.Channel}配置不存在,打包失败!");
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
    /// <exception cref="System.NotImplementedException"></exception>
    protected override bool DoBuildTargetPreProcess()
    {
        return true;
    }

    /// <summary>
    /// 执行打包App
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    protected override BuildResult DoRealAppBuild()
    {
        return BuildResult.Not_Supported_Build_Target;
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
}