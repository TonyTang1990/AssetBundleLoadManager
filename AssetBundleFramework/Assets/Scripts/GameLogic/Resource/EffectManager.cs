/*
 * Description:             EffectManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EffectManager.cs
/// 特效管理单例类
/// </summary>
public class EffectManager : SingletonTemplate<EffectManager>, IModuleInterface {

    /// <summary>
    /// 模块名
    /// </summary>
    public string ModuleName
    {
        get
        {
            return this.GetType().ToString();
        }
    }


}