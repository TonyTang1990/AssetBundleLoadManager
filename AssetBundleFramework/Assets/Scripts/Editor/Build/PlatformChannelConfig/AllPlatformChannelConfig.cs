/*
 * Description:             AllPlatformChannelConfig.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/26
 */

using UnityEditor;
using UnityEngine;

/// <summary>
/// AllPlatformChannelConfig.cs
/// 所有平台渠道配置(打包相关比如程序Icon、SplashScreen、签名文件等)
/// </summary>
[CreateAssetMenu(fileName = "AllPlatformChannelConfig", menuName = "Build/Config/AllPlatformChannelConfig")]
public sealed class AllPlatformChannelConfig : ScriptableObject
{
    /// <summary>
    /// 加载所有平台的渠道配置
    /// </summary>
    /// <returns></returns>
    public static AllPlatformChannelConfig LoadAllPlatformChannelConfig()
    {
        var allPlatformChannelConfig = AssetDatabase.LoadAssetAtPath<AllPlatformChannelConfig>(PlatformChannelPath.AllPlatformChannelConfigAssetPath);
        if(allPlatformChannelConfig == null)
        {
            Debug.LogError($"未找到AllPlatformChannelConfig，路径为:{PlatformChannelPath.AllPlatformChannelConfigAssetPath}，请自行创建一个并配置相关数据用于打包！");
        }
        return allPlatformChannelConfig;
    }

    /// <summary>
    /// Android平台渠道配置
    /// </summary>
    [Header("Android平台渠道配置")]
    public AndroidPlatformChannelConfig AndroidPlatformChannelConfig = new AndroidPlatformChannelConfig();

    /// <summary>
    /// IOS平台渠道配置
    /// </summary>
    [Header("IOS平台渠道配置")]
    public IOSPlatformChannelConfig IOSPlatformChannelConfig = new IOSPlatformChannelConfig();

    /// <summary>
    /// Windows平台渠道配置
    /// </summary>
    [Header("Windows平台渠道配置")]
    public WindowsPlatformChannelConfig WindowsPlatformChannelConfig = new WindowsPlatformChannelConfig();

    /// <summary>
    /// 获取指定平台和渠道的配置，如果未找到则返回null
    /// </summary>
    /// <param name="buildPlatform"></param>
    /// <param name="channel"></param>
    /// <returns>指定平台和渠道的配置，如果未找到则返回null</returns>
    public T GetPlatformChannelConfig<T>(BuildPlatform buildPlatform, Channel channel) where T : ChannelConfig
    {
        if(buildPlatform == BuildPlatform.Android)
        {
            return AndroidPlatformChannelConfig.GetChannelConfig(channel) as T;
        }
        else if(buildPlatform == BuildPlatform.IOS)
        {
            return IOSPlatformChannelConfig.GetChannelConfig(channel) as T;
        }
        else if(buildPlatform == BuildPlatform.Windows)
        {
            return WindowsPlatformChannelConfig.GetChannelConfig(channel) as T;
        }
        Debug.LogError($"未配置的打包平台:{buildPlatform}，获取渠道:{channel}的配置失败");
        return null;
    }
}