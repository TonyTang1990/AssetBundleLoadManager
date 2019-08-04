/*
 * Description:             HotUpdateConfig.cs
 * Author:                  TONYTANG
 * Create Date:             2019//08/04
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HotUpdateConfig.cs
/// 游戏热更新地址抽象类
/// </summary>
[Serializable]
public class HotUpdateConfig {

    /// <summary>
    /// 热更APK名字
    /// </summary>
    public string APKName;

    /// <summary>
    /// 热更新本地测试地址
    /// </summary>
    public string HotUpdateLocalURL;

    /// <summary>
    /// 热更新正式地址
    /// </summary>
    public string HotUpdateURL;
}