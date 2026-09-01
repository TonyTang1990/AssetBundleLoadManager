/*
 * Description:             ChannelConfig.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/26
 */

using System;
using UnityEngine;

/// <summary>
/// ChannelConfig.cs
/// 单个渠道配置
/// </summary>
[Serializable]
public abstract class ChannelConfig
{
    /// <summary>
    /// 渠道
    /// </summary>
    [Header("渠道")]
    public Channel Channel;

    /// <summary>
    /// 应用程序标识符
    /// </summary>
    [Header("应用程序标识符")]
    public string ApplicationIdentifier;

    /// <summary>
    /// 公司名称
    /// </summary>
    [Header("公司名称")]
    public string CompanyName;

    /// <summary>
    /// 应用程序名称
    /// </summary>
    [Header("应用程序名称")]
    public string ProductName;

    /// <summary>
    /// 内容是否有效
    /// </summary>
    /// <returns></returns>
    public virtual bool IsContentValide()
    {
        if(string.IsNullOrEmpty(ApplicationIdentifier))
        {
            Debug.LogError($"渠道:{Channel}配置参数ApplicationIdentifier为空,打包失败!");
            return false;
        }
        else if(string.IsNullOrEmpty(ProductName))
        {
            Debug.LogError($"渠道:{Channel}配置参数ProductName为空,打包失败!");
            return false;
        }
        else if(string.IsNullOrEmpty(CompanyName))
        {
            Debug.LogError($"渠道:{Channel}配置参数CompanyName为空,打包失败!");
            return false;
        }
        return true;
    }
}