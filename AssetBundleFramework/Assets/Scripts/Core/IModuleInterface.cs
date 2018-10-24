/*
 * Description:             IModuleInterface.cs
 * Author:                  tanghuan
 * Create Date:             2018/09/27
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IModuleInterface.cs
/// 模块接口
/// </summary>
public interface IModuleInterface
{
    /// <summary>
    /// 真实的模块名
    /// </summary>
    string ModuleName
    {
        get;
    }
}
