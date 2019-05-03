/*
 * Description:             ModuleManager.cs
 * Author:                  tanghuan
 * Create Date:             2018/09/27
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ModuleManager.cs
/// 模块单例类，用于解耦各模块之间耦合度
/// 获取各模块的中介类
/// </summary>
public sealed class ModuleManager : SingletonTemplate<ModuleManager> {

    /// <summary>
    /// 注册模块映射Map
    /// Key为模块类型名，Value为模块实例对象
    /// </summary>
    private Dictionary<string, IModuleInterface> mRegisteredModuleMap;

    public ModuleManager()
    {
        mRegisteredModuleMap = new Dictionary<string, IModuleInterface>();
    }

    /// <summary>
    /// 注册模块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="module"></param>
    public void registerModule<T>(T module) where T : IModuleInterface
    {
        if(!mRegisteredModuleMap.ContainsKey(module.ModuleName))
        {
            mRegisteredModuleMap.Add(module.ModuleName, module);
        }
        else
        {
            Debug.LogError(string.Format("重复注册:{0}模块，同一模块只能注册一个!", module.ModuleName));
        }
    }

    /// <summary>
    /// 取消注册模块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="module"></param>
    public void unregisterModule<T>() where T : IModuleInterface
    {
        var modulename = typeof(T).Name;
        if (mRegisteredModuleMap.ContainsKey(modulename))
        {
            mRegisteredModuleMap.Remove(modulename);
        }
        else
        {
            Debug.LogError(string.Format("未注册:{0}的模块，无法取消注册!", modulename));
        }
    }

    /// <summary>
    /// 取消所有已注册模块
    /// </summary>
    public void unregisterAllModule()
    {
        mRegisteredModuleMap.Clear();
    }

    /// <summary>
    /// 获取指定注册模块
    /// </summary>
    /// <param name="mt"></param>
    /// <returns></returns>
    public T getModule<T>() where T : IModuleInterface, new()
    {
        var modulename = typeof(T).Name;
        if (mRegisteredModuleMap.ContainsKey(modulename))
        {
            return (T)mRegisteredModuleMap[modulename];
        }
        else
        {
            Debug.LogError(string.Format("未注册:{0}的模块，无法正确获取该模块!", modulename));
            return default(T);
        }
    }
}
