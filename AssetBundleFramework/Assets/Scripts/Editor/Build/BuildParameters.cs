/*
 * Description:             BuildParameters.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/20
 */

using UnityEditor;

/// <summary>
/// BuildParameters.cs
/// 打包参数类
/// </summary>
public class BuildParameters
{
    /// <summary>
    /// 打包平台
    /// </summary>
    public BuildTarget BuildTarget
    {
        get;
        private set;
    } = BuildTarget.Android;

    /// <summary>
    /// 打包渠道
    /// </summary>
    public Channel Channel
    {
        get;
        private set;
    } = Channel.None;

    /// <summary>
    /// 是否打开发版本
    /// </summary>
    public bool IsDevelopment
    {
        get;
        private set;
    } = false;

    /// <summary>
    /// 版本号
    /// </summary>
    public double VersionCode
    {
        get;
        private set;
    } = 1.0;

    /// <summary>
    /// 资源版本号
    /// </summary>
    public int ResourceVersionCode
    {
        get;
        private set;
    } = 1;

    /// <summary>
    /// 是否完全重新打包AB
    /// </summary>
    public bool ForceRebuildAB
    {
        get;
        private set;
    } = true;

    private BuildParameters()
    {
        
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="buildTarget"></param>
    /// <param name="developmentMode"></param>
    /// <param name="channel"></param>
    /// <param name="isDevelopment"></param>
    /// <param name="versionCode"></param>
    /// <param name="resourceVersionCode"></param>
    /// <param name="forceRebuildAB"></param>
    /// <param name="sdkType"></param>
    public BuildParameters(BuildTarget buildTarget, Channel channel, bool isDevelopment,
                           double versionCode, int resourceVersionCode, bool forceRebuildAB)
    {
        BuildTarget = buildTarget;
        Channel = channel;
        IsDevelopment = isDevelopment;
        VersionCode = versionCode;
        ResourceVersionCode = resourceVersionCode;
        ForceRebuildAB = forceRebuildAB;
    }

    /// <summary>
    /// 重写ToString方法
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"BuildParameters: BuildTarget={BuildTarget}, BuildChannel={Channel}, IsDevelopment={IsDevelopment}, VersionCode={VersionCode}, ResourceVersionCode={ResourceVersionCode}, ForceRebuildAB={ForceRebuildAB}";
    }
}