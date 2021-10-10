/*
 * Description:             ResourceModuleManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018/08/12
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

// AssetBundle资源模式：
// 资源加载方案1：
// 1. 加载AB
// 2. 判定AB是否有依赖AB，有依赖AB先加载依赖AB，没有则直接加载自身AB
// 3. 有依赖AB等依赖AB加载完再加载自身AB
// 4. 自身AB加载完回调上层通知

// 方案1问题:
// 当依赖AB互相依赖时，会导致死锁

// 解决方案:
// 资源加载方案2:
// 修改成先加载自己，自己加载完成再加载依赖AB
// 然后等所有依赖AB加载完成再往上回调

// AB加载管理相关概念：
// 1. 依赖AB与被依赖者采用同样的加载方式(ResourceLoadMethod)，但加载方式依赖AB统一采用ResourceLoadType.NormalLoad
// 2. 依赖AB通过索引计数管理，只要原始AB不被卸载，依赖AB就不会被卸载
// 3. 已加载的AB资源加载类型只允许从低往高变(NormalLoad -> Preload -> PermanentLoad)，不允许从高往低(PermanentLoad -> Preload -> NormalLoad)

/// <summary>
/// ResourceModuleManager.cs
/// 资源加载管理类
/// </summary>
public class ResourceModuleManager : SingletonTemplate<ResourceModuleManager>
{
    /// <summary>
    /// 当前资源加载模块
    /// </summary>
    public AbstractResourceModule CurrentResourceModule
    {
        get;
        set;
    }

    /// <summary>
    /// 资源加载模式
    /// </summary>
    public ResourceLoadMode ResLoadMode
    {
        get
        {
            return mResLoadMode;
        }
        set
        {
#if UNITY_EDITOR
            mResLoadMode = value;
            PlayerPrefs.SetInt(ResLoadModeKey, (int)mResLoadMode);
            Debug.Log(string.Format("切换资源加载模式到 : {0},重新运行Editor后生效!", mResLoadMode));
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
#else
            //非编辑器只支持AssetBundle模式
            mResLoadMode = ResourceLoadMode.AssetBundle;
            Debug.Log("真机模式只支持AssetBundle模式，不允许切换!");
#endif
        }
    }
    private ResourceLoadMode mResLoadMode;

#if UNITY_EDITOR
    /// <summary>
    /// 资源加载模式Key
    /// </summary>
    private const string ResLoadModeKey = "ResLoadModeKey";
#endif

    /// <summary>
    /// 初始化资源加载相关的
    /// </summary>
    public void init()
    {
#if UNITY_EDITOR
        mResLoadMode = (ResourceLoadMode)PlayerPrefs.GetInt(ResLoadModeKey, (int)ResourceLoadMode.AssetBundle);
        if (mResLoadMode == ResourceLoadMode.AssetBundle)
        {
            CurrentResourceModule = new AssetBundleModule();
        }
        else if (mResLoadMode == ResourceLoadMode.AssetDatabase)
        {
            CurrentResourceModule = new AssetDatabaseModule();
        }
#else
        //非编辑器只支持AssetBundle模式
        mResLoadMode = ResourceLoadMode.AssetBundle;
        CurrentResourceModule = new AssetBundleModule();
#endif
        Debug.Log(string.Format("当前资源加载模式 : {0}", mResLoadMode));
        CurrentResourceModule.init();
        CurrentResourceModule.loadAssetBundleBuildInfo();
    }

    /// <summary>
    /// 开启资源不再使用回收检测
    /// </summary>
    public void startResourceRecyclingTask()
    {
        CurrentResourceModule.startResourceRecyclingTask();
    }

    /// <summary>
    /// 添加指定资源到白名单
    /// </summary>
    /// <param name="respath">资源路径</param>
    public void addToWhiteList(string respath)
    {
        CurrentResourceModule.addToWhiteList(respath);
    }

    /// <summary>
    /// 请求资源
    /// 资源加载统一入口
    /// </summary>
    /// <param name="respath">资源AB路径</param>
    /// <param name="completehandler">加载完成上层回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    public void requstResource(string respath, AbstractResourceModule.LoadResourceCompleteHandler completehandler, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        CurrentResourceModule.requstResource(respath, completehandler, loadtype, loadmethod);
    }

    /// <summary>
    /// 获取指定AB名字的资源加载信息
    /// </summary>
    /// <param name="abname"></param>
    /// <returns></returns>
    public AbstractResourceInfo getSpecificARI(string abname)
    {
        return CurrentResourceModule.getSpecificARI(abname);
    }

    public void Update()
    {
        CurrentResourceModule.Update();
    }

    /// <summary>
    /// 释放可释放的预加载资源(递归判定，不限制回收数量)
    /// Note:
    /// 切场景前调用，确保所有预加载资源正确释放
    /// </summary>
    public void unloadAllUnsedPreloadLoadedResources()
    {
        CurrentResourceModule.unloadAllUnsedPreloadLoadedResources();
    }

    /// <summary>
    /// 提供给外部的触发卸载所有正常加载不再使用的资源资源(递归判定，不限制回收数量)
    /// Note:
    /// 同步接口，回收数量会比较大，只建议切场景时场景卸载后调用一次
    /// </summary>
    public void unloadAllUnsedNormalLoadedResources()
    {
        CurrentResourceModule.unloadAllUnsedNormalLoadedResources();
    }
}