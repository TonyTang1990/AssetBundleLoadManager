/*
 * Description:             VersionConfig.cs
 * Author:                  TONYTANG
 * Create Date:             2018/08/12
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VersionConfig.cs
/// 游戏版本抽象类
/// </summary>
[Serializable]
public class VersionConfig
{
    /// <summary>
    /// 版本号
    /// </summary>
    public double VersionCode;

    /// <summary>
    /// 资源版本号
    /// </summary>
    public int ResourceVersionCode;

    public VersionConfig()
    {
        
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="versionCode"></param>
    /// <param name="resourceVersionCode"></param>
    public VersionConfig(double versionCode, int resourceVersionCode)
    {
        VersionCode = versionCode;
        ResourceVersionCode = resourceVersionCode;
    }
}