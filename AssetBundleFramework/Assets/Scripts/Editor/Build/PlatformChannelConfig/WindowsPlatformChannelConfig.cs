/*
 * Description:             WindowsPlatformChannelConfig.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/26
 */

using System;

/// <summary>
/// WindowsPlatformChannelConfig.cs
/// Windows平台渠道配置
/// </summary>
[Serializable]
public class WindowsPlatformChannelConfig : PlatformChannelConfig<WindowsChannelConfig>
{
    /// <summary>
    /// Windows平台
    /// </summary>
    public override BuildPlatform BuildPlatform
    {
        get { return BuildPlatform.Windows; }
    }
}