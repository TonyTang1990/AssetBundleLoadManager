/*
 * Description:             AssetDatabaseModule.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/07
 */

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AssetDatabaseModule.cs
/// AssetDatabase资源加载模块
/// </summary>
public class AssetDatabaseModule : AbstractResourceModule
{
    /// <summary>
    /// 资源加载模块初始化
    /// </summary>
    public override void init()
    {
        AssetDatabaseLoaderFactory.initialize(20);             // 考虑到大部分都是采用同步加载，所以AssetDatabaseLoader并不需要初始化太多
        AssetDatabaseInfoFactory.initialize(200);
    }

    /// <summary>
    /// 开启资源不再使用回收检测
    /// </summary>
    public override void startResourceRecyclingTask()
    {

    }

    /// <summary>
    /// 添加指定资源到白名单
    /// </summary>
    /// <param name="resname">资源名(既AB名)</param>
    public override void addToWhiteList(string resname)
    {

    }

    /// <summary>
    /// 请求资源
    /// 资源加载统一入口
    /// </summary>
    /// <param name="resname">资源AB名</param>
    /// <param name="assetname">asset名</param>
    /// <param name="completehandler">加载完成上层回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    public override void requstResource(string resname, string assetname, LoadResourceCompleteHandler completehandler, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        AssetDatabaseLoader adloader = createADLoader(resname, assetname);
        //暂时默认都当同步加载，不支持异步模拟
        adloader.LoadMethod = loadmethod;
        adloader.LoadType = loadtype;
        adloader.LoadResourceCompleteCallBack = completehandler;
        adloader.startLoad();
    }

    /// <summary>
    /// 更新入口
    /// </summary>
    public override void Update()
    {

    }


    /// <summary>
    /// 释放可释放的预加载资源(递归判定，不限制回收数量)
    /// Note:
    /// 切场景前调用，确保所有预加载资源正确释放
    /// </summary>
    public override void unloadAllUnsedPreloadLoadedResources()
    {

    }

    /// <summary>
    /// 提供给外部的触发卸载所有正常加载不再使用的资源(递归判定，不限制回收数量)
    /// Note:
    /// 同步接口，回收数量会比较大，只建议切场景时场景卸载后调用一次
    /// </summary>
    public override void unloadAllUnsedNormalLoadedResources()
    {

    }

    /// <summary>
    /// 创建AssetDatabase资源加载对象
    /// </summary>
    /// <param name="resname">资源名</param>
    /// <param name="assetname">asset名</param>
    /// <returns></returns>
    private AssetDatabaseLoader createADLoader(string resname, string assetname)
    {
        var loader = AssetDatabaseLoaderFactory.create();
        loader.AssetBundleName = resname;
        loader.AssetName = assetname;
        return loader;
    }
}
#endif