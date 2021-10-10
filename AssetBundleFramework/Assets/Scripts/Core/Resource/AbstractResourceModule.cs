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
    /// 资源白名单映射Map(用Dictionary只是为了快速访问)
    /// Key为资源名，Value也为资源名
    /// 在白名单里的资源，只要上层加载该资源，一律以PreLoad形式加载资源
    /// 这里主要是为了避免因为上层逻辑代码导致错误的资源被频繁加载卸载
    /// 比如: Res.cs    public bool SetImageSprite(UnityEngine.UI.Image image, string spName)
    /// </summary>
    protected Dictionary<string, string> mResourceWhileListMap;

    /// <summary>
    /// 所有已加载的资源的信息映射Map
    /// Key为AB加载类型，Value为该加载类型已经加载的资源信息映射Map(Key为AB名字，Value为资源加载信息)
    /// </summary>
    protected Dictionary<ResourceLoadType, Dictionary<string, AbstractResourceInfo>> mAllLoadedResourceInfoMap;

    #region Asset打包信息
    /// <summary>
    /// Asset打包信息
    /// </summary>
    public AssetBuildInfoAsset AssetBuildInfo
    {
        get
        {
            return mAssetBuildInfo;
        }
    }
    protected AssetBuildInfoAsset mAssetBuildInfo;

    #endregion

    #region FSP计算部分
    /// <summary>
    /// 当前FPS
    /// </summary>
    public int CurrentFPS
    {
        get
        {
            return mCurrentFPS;
        }
    }
    protected int mCurrentFPS;

    /// <summary>
    /// 经历的时间
    /// </summary>
    private float mTotalDeltaTime;

    /// <summary>
    /// 经历的帧数
    /// </summary>
    private int mFrameCount;

    /// <summary>
    /// FPS更新间隔频率
    /// </summary>
    private float mFPSUpdateInterval;
    #endregion

    /// <summary>
    /// 资源加载模块初始化
    /// </summary>
    public virtual void init()
    {
        ResLoadMode = ResourceLoadMode.Invalide;
        EnableResourceRecyclingUnloadUnsed = true;
        mResourceWhileListMap = new Dictionary<string, string>();
        mAllLoadedResourceInfoMap = new Dictionary<ResourceLoadType, Dictionary<string, AbstractResourceInfo>>();
        mAllLoadedResourceInfoMap.Add(ResourceLoadType.NormalLoad, new Dictionary<string, AbstractResourceInfo>());
        mAllLoadedResourceInfoMap.Add(ResourceLoadType.Preload, new Dictionary<string, AbstractResourceInfo>());
        mAllLoadedResourceInfoMap.Add(ResourceLoadType.PermanentLoad, new Dictionary<string, AbstractResourceInfo>());
        mFPSUpdateInterval = 1.0f;
    }

    /// <summary>
    /// 加载AssetBundle打包信息
    /// </summary>
    public abstract void loadAssetBundleBuildInfo();

    /// <summary>
    /// 开启资源不再使用回收检测
    /// </summary>
    public void startResourceRecyclingTask()
    {
        CoroutineManager.Singleton.startCoroutine(checkUnsedResource());
    }

    /// <summary>
    /// 队列里不再有资源需要加载时检查不再使用的资源
    /// </summary>
    protected abstract IEnumerator checkUnsedResource();

    /// <summary>
    /// 添加指定资源到白名单
    /// Note:
    /// 默认白名单里的资源都以ResourceLoadType.Preload方式加载
    /// </summary>
    /// <param name="respath">资源路径</param>
    public void addToWhiteList(string respath)
    {
        if (!mResourceWhileListMap.ContainsKey(respath))
        {
            mResourceWhileListMap.Add(respath, respath);
        }
        else
        {
            ResourceLogger.logErr(string.Format("资源 : {0}重复添加白名单!", respath));
        }
    }

    /// <summary>
    /// 请求资源
    /// 上层资源加载统一入口
    /// </summary>
    /// <param name="respath">资源AB路径</param>
    /// <param name="completehandler">加载完成上层回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    public void requstResource(string respath, LoadResourceCompleteHandler completehandler, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        // 在白名单里的资源一律以预加载形式加载，
        // 避免因为上层逻辑错误加载后被频繁加载卸载
        if (mResourceWhileListMap.ContainsKey(respath))
        {
            loadtype = ResourceLoadType.Preload;
        }
        realRequestResource(respath, completehandler, loadtype, loadmethod);
    }

    /// <summary>
    /// 真正的请求资源(由不同的资源模块去实现)
    /// </summary>
    /// <param name="respath">资源AB路径</param>
    /// <param name="completehandler">加载完成上层回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    protected abstract void realRequestResource(string respath, LoadResourceCompleteHandler completehandler, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync);

    /// <summary>
    /// 更新入口
    /// </summary>
    public virtual void Update()
    {
        mTotalDeltaTime += Time.deltaTime;
        mFrameCount++;
        if (mTotalDeltaTime >= mFPSUpdateInterval)
        {
            mCurrentFPS = (int)(mFrameCount / mTotalDeltaTime);
            mTotalDeltaTime = 0f;
            mFrameCount = 0;
        }
    }

    /// <summary>
    /// 获取指定AB名字的资源加载信息
    /// </summary>
    /// <param name="abname"></param>
    /// <returns></returns>
    public AbstractResourceInfo getSpecificARI(string abname)
    {
        if(!string.IsNullOrEmpty(abname))
        {
            foreach (var loadedresourceinfomap in mAllLoadedResourceInfoMap)
            {
                AbstractResourceInfo ari;
                if (loadedresourceinfomap.Value.TryGetValue(abname, out ari))
                {
                    return ari;
                }
            }
            DIYLog.LogError($"找不到AB名:{abname}的资源加载信息!");
            return null;
        }
        else
        {
            DIYLog.LogError("不允许传空AB名字,获取对应资源加载信息失败!");
            return null;
        }
    }

    /// <summary>
    /// 释放可释放的预加载资源(递归判定，不限制回收数量)
    /// Note:
    /// 切场景前调用，确保所有预加载资源正确释放
    /// </summary>
    public void unloadAllUnsedPreloadLoadedResources()
    {
        unloadSpecificLoadTypeUnsedResource(ResourceLoadType.Preload);
    }

    /// <summary>
    /// 提供给外部的触发卸载所有正常加载不再使用的资源(递归判定，不限制回收数量)
    /// Note:
    /// 同步接口，回收数量会比较大，只建议切场景时场景卸载后调用一次
    /// </summary>
    public void unloadAllUnsedNormalLoadedResources()
    {
        unloadSpecificLoadTypeUnsedResource(ResourceLoadType.NormalLoad);
    }

    /// <summary>
    /// 卸载指定类型不再使用的资源(Note:不支持卸载常驻资源类型)
    /// </summary>
    /// <param name="resourceloadtype">资源加载类型</param>
    protected void unloadSpecificLoadTypeUnsedResource(ResourceLoadType resourceloadtype)
    {
        if (resourceloadtype == ResourceLoadType.PermanentLoad)
        {
            ResourceLogger.logErr("不允许卸载常驻AB资源!");
            return;
        }
        doUnloadSpecificLoadTypeUnsedResource(resourceloadtype);
    }

    /// <summary>
    /// 真正执行资源卸载指定类型不再使用的资源接口
    /// </summary>
    /// <param name="resourceloadtype">资源加载类型</param>
    protected abstract void doUnloadSpecificLoadTypeUnsedResource(ResourceLoadType resourceloadtype);

    /// <summary>
    /// 更新已加载AB的加载类型
    /// </summary>
    /// <param name="resname">资源名</param>
    /// <param name="oldloadtype">旧的加载类型</param>
    /// <param name="newloadtype">新的家在类型</param>
    protected void updateLoadedResourceInfoLoadType(string resname, ResourceLoadType oldloadtype, ResourceLoadType newloadtype)
    {
        if (mAllLoadedResourceInfoMap[oldloadtype].ContainsKey(resname))
        {
            var abi = mAllLoadedResourceInfoMap[oldloadtype][resname];
            mAllLoadedResourceInfoMap[newloadtype].Add(resname, abi);
            mAllLoadedResourceInfoMap[oldloadtype].Remove(resname);
            ResourceLogger.log(string.Format("已加载的资源 : {0}从资源类型 : {1}更新到资源类型 : {2}！", resname, oldloadtype, newloadtype));
        }
        else
        {
            ResourceLogger.logErr(string.Format("资源类型 : {0}里找不到已加载的资源 : {1}，无法更新该资源的加载类型！", oldloadtype, resname));
        }
    }

    /// <summary>
    /// 获取指定加载类型的已加载资源信息映射Map
    /// </summary>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public Dictionary<string, AbstractResourceInfo> getSpecificLoadTypeARIMap(ResourceLoadType loadtype)
    {
        if (mAllLoadedResourceInfoMap.ContainsKey(loadtype))
        {
            return mAllLoadedResourceInfoMap[loadtype];
        }
        else
        {
            ResourceLogger.logErr(string.Format("找不到资源类型 : {0}的已加载AB信息!", loadtype));
            return null;
        }
    }

    #region 调试开发工具
    /// <summary>
    /// 获取正常已加载不可用的AB数量(不包含常驻AB)
    /// </summary>
    public int getNormalUnsedABNumber()
    {
        var unsednumber = 0;
        // 检查回收不再使用的AB
        foreach (var loadedab in mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad])
        {
            if (loadedab.Value.IsUnsed)
            {
                unsednumber++;
            }
        }
        return unsednumber;
    }

    /// <summary>
    /// 获取预加载已加载不可用的AB数量(不包含常驻AB)
    /// </summary>
    public int getPreloadUnsedABNumber()
    {
        var unsednumber = 0;
        // 检查回收不再使用的AB
        foreach (var loadedab in mAllLoadedResourceInfoMap[ResourceLoadType.Preload])
        {
            if (loadedab.Value.IsUnsed)
            {
                unsednumber++;
            }
        }
        return unsednumber;
    }

    /// <summary>
    /// 打印当前资源所有使用者信息以及索引计数(开发用)
    /// </summary>
    public void printAllLoadedResourceOwnersAndRefCount()
    {
        ResourceLogger.log("Normal Loaded AssetDatabase Info:");
        foreach (var adi in mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad])
        {
            adi.Value.printAllOwnersNameAndRefCount();
        }

        ResourceLogger.log("Preload Loaded AssetDatabase Info:");
        foreach (var adi in mAllLoadedResourceInfoMap[ResourceLoadType.Preload])
        {
            adi.Value.printAllOwnersNameAndRefCount();
        }

        ResourceLogger.log("Permanent Loaded AssetDatabase Info:");
        foreach (var adi in mAllLoadedResourceInfoMap[ResourceLoadType.PermanentLoad])
        {
            adi.Value.printAllOwnersNameAndRefCount();
        }
    }
    #endregion
}