/*
 * Description:             HotUpdateModuleManager.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/14
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HotUpdateModuleManager.cs
/// 资源热更模块
/// </summary>
public class HotUpdateModuleManager : SingletonMonoBehaviourTemplate<HotUpdateModuleManager>, IModuleInterface
{
    /// <summary>
    /// 真实模块名
    /// </summary>
    public string ModuleName
    {
        get
        {
            return this.GetType().ToString();
        }
    }


}