/*
 * Description:             AndroidPlatformChannelConfig.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/26
 */

using System;

/// <summary>
/// AndroidPlatformChannelConfig.cs
/// Android平台渠道配置
/// </summary>
[Serializable]
public class AndroidPlatformChannelConfig : PlatformChannelConfig<AndroidChannelConfig>
{
    /// <summary>
    /// Android平台
    /// </summary>
    public override BuildPlatform BuildPlatform
    {
        get { return BuildPlatform.Android; }
    }
}