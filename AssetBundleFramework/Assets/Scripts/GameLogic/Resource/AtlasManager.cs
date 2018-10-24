/*
 * Description:             AtlasManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AtlasManager.cs
/// 图集管理单例类
/// </summary>
public class AtlasManager : SingletonTemplate<AtlasManager>, IModuleInterface
{
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