/*
 * Description:             AndroidChannelConfig.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/26
 */

using System;
using UnityEngine;

/// <summary>
/// AndroidChannelConfig.cs
/// Android平台渠道配置
/// </summary>
[Serializable]
public class AndroidChannelConfig : ChannelConfig
{
    /// <summary>
    /// Keystore文件相对路径
    /// </summary>
    [Header("Keystore文件相对路径")]
    public string KeystoreFileRelativePath;

    /// <summary>
    /// Keystore文件密码
    /// </summary>
    [Header("Keystore文件密码")]
    public string KeystorePassword;
    
    /// <summary>
    /// Keystore别名
    /// </summary>
    [Header("Keystore别名")]
    public string KeyAlias;

    /// <summary>
    /// Keystore别名密码
    /// </summary>
    [Header("Keystore别名密码")]
    public string KeyAliasPassword;

    /// <summary>
    /// Android图标集合
    /// </summary>
    [Header("Android图标集合")]
    public AndroidIconSet IconSet = new AndroidIconSet();

    /// <summary>
    /// 内容是否有效
    /// </summary>
    /// <returns></returns>
    public override bool IsContentValide()
    {
        var baseResult = base.IsContentValide();
        if(!baseResult)
        {
            return false;
        }
        if(string.IsNullOrEmpty(KeystoreFileRelativePath))
        {
            Debug.LogError($"渠道:{Channel}配置参数KeystoreFileRelativePath为空,打包失败!");
            return false;
        }
        else if(string.IsNullOrEmpty(KeystorePassword))
        {
            Debug.LogError($"渠道:{Channel}配置参数KeystorePassword为空,打包失败!");
            return false;
        }
        else if(string.IsNullOrEmpty(KeyAlias))
        {
            Debug.LogError($"渠道:{Channel}配置参数KeyAlias为空,打包失败!");
            return false;
        }
        else if(string.IsNullOrEmpty(KeyAliasPassword))
        {
            Debug.LogError($"渠道:{Channel}配置参数KeyAliasPassword为空,打包失败!");
            return false;
        }
        else if(IconSet == null || !IconSet.IsContentValide())
        {
            Debug.LogError($"渠道:{Channel}配置参数IconSet无效,打包失败!");
            return false;
        }
        return true;
    }
}