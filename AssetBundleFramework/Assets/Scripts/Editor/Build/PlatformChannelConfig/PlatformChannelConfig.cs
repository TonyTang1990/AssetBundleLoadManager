/*
 * Description:             PlatformChannelConfig.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/26
 */

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlatformChannelConfig.cs
/// 单个平台所有渠道配置
/// </summary>
[Serializable]
public abstract class PlatformChannelConfig<T> where T : ChannelConfig
{
    /// <summary>
    /// 打包平台
    /// </summary>
    public abstract BuildPlatform BuildPlatform
    {
        get;
    }

    /// <summary>
    /// 平台所有渠道配置
    /// </summary>
    [Header("平台所有渠道配置")]
    public List<T> ChannelCofigs = new List<T>();

    /// <summary>
    /// 获取指定渠道的配置
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public T GetChannelConfig(Channel channel)
    {
        if (ChannelCofigs == null)
        {
            Debug.LogError($"没有配置任何渠道的配置，无法获取{BuildPlatform}平台指定渠道的配置，channel={channel}");
            return null;
        }

        foreach (var config in ChannelCofigs)
        {
            if (config == null)
            {
                continue;
            }

            if (config.Channel == channel)
            {
                return config;
            }
        }

        Debug.LogError($"没有找到对应的渠道配置，channel={channel}");
        return null;
    }
}