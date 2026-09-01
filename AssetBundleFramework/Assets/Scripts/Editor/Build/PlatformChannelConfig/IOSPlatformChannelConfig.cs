/*
 * Description:             IOSPlatformChannelConfig.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/26
 */

using System;

/// <summary>
/// IOSPlatformChannelConfig.cs
/// </summary>
[Serializable]
public class IOSPlatformChannelConfig : PlatformChannelConfig<IOSChannelConfig>
{
    /// <summary>
    /// IOS平台
    /// </summary>
    public override BuildPlatform BuildPlatform
    {
        get { return BuildPlatform.IOS; }
    }
}