/*
 * Description:             WindowManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/25
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WindowManager.cs
/// 窗口管理单例类
/// </summary>
public class WindowManager : SingletonTemplate<WindowManager>, IModuleInterface
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