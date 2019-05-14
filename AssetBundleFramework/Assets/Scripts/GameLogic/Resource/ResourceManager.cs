/*
 * Description:             ResourceManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ResourceManager.cs
/// 上层资源请求单例管理类
/// </summary>
public class ResourceManager : SingletonTemplate<ResourceManager> {

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

    /// <summary>
    /// 获取窗口实例对象
    /// </summary>
    /// <param name="wndname"></param>
    /// <returns></returns>
    public GameObject GetWindowInstance(string wndname)    
    {
        GameObject wndinstance = null;
        ResourceModuleManager.Singleton.requstResource(
        wndname,
        (abi) =>
        {
            wndinstance = abi.instantiateAsset(wndname);
        });
        return wndinstance;
    }
}