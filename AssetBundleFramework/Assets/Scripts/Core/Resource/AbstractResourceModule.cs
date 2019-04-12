/*
 * Description:             AbstractResourceModule.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/07
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AbstractResourceModule.cs
/// 资源加载模式模块抽象(用于区分不同的资源加载模式 e.g. AssetBundle || AssetDatabase)
/// </summary>
public abstract class AbstractResourceModule {

    /// <summary> 逻辑层资源加载完成委托 /// </summary>
    /// <param name="abinfo"></param>
    public delegate void LoadResourceCompleteHandler(AbstractResourceInfo abinfo);

    /// <summary>
    /// 资源加载模式
    /// </summary>
    public ResourceLoadMode ResLoadMode
    {
        get;
        protected set;
    }

    /// <summary>
    /// 是否开启资源回收检测(有些情况下不适合频繁回收创建，比如战斗场景)
    /// </summary>
    public bool EnableResourceRecyclingUnloadUnsed
    {
        get;
        set;
    }

    /// <summary>
    /// 资源加载模块初始化
    /// </summary>
    public abstract void init();

    /// <summary>
    /// 开启资源不再使用回收检测
    /// </summary>
    public abstract void startResourceRecyclingTask();

    /// <summary>
    /// 添加指定资源到白名单
    /// </summary>
    /// <param name="resname">资源名(既AB名)</param>
    public abstract void addToWhiteList(string resname);

    /// <summary>
    /// 请求资源
    /// 资源加载统一入口
    /// </summary>
    /// <param name="resname">资源AB名</param>
    /// <param name="completehandler">加载完成上层回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    public abstract void requstResource(string resname, LoadResourceCompleteHandler completehandler, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync);

    /// <summary>
    /// 更新入口
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// 释放可释放的预加载资源(递归判定，不限制回收数量)
    /// Note:
    /// 切场景前调用，确保所有预加载资源正确释放
    /// </summary>
    public abstract void unloadAllUnsedPreloadLoadedResources();

    /// <summary>
    /// 提供给外部的触发卸载所有正常加载不再使用的资源资源(递归判定，不限制回收数量)
    /// Note:
    /// 同步接口，回收数量会比较大，只建议切场景时场景卸载后调用一次
    /// </summary>
    public abstract void unloadAllUnsedNormalLoadedResources();

    #region 调试开发工具
    /// <summary>
    /// 打印当前资源所有使用者信息以及索引计数(开发用)
    /// </summary>
    public abstract void printAllLoadedResourceOwnersAndRefCount();
    #endregion
}