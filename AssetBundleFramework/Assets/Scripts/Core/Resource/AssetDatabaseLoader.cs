/*
 * Description:             AssetDatabaseLoader.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/04
 */

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AssetDatabaseLoader.cs
/// AssetDatabase模式资源加载器抽象
/// Note:
/// AssetDatabase模式没有资源回收，没有依赖加载的概念，只负责加载和引用计数相关，资源回收统一在切场景的时候
/// AssetDatabase模式主要是为了Editor开发模式下不走AB，快速迭代
/// </summary>
public class AssetDatabaseLoader : FactoryObj
{
    /// <summary> 资源管理层资源加载完成委托 /// </summary>
    /// <param name="adl">资源加载任务信息</param>
    public delegate void LoadResourceCompleteNotifier(AssetDatabaseLoader adl);

    /// <summary>
    /// 加载任务对应的资源路径
    /// </summary>
    public string AssetBundlePath
    {
        get;
        set;
    }

    /// <summary>
    /// 资源加载完成逻辑层回调
    /// </summary>
    public AbstractResourceModule.LoadResourceCompleteHandler LoadResourceCompleteCallBack
    {
        get;
        set;
    }

    /// <summary>
    /// 资源自身加载完成资源管理通知回调
    /// </summary>
    public LoadResourceCompleteNotifier LoadSelfResourceCompleteNotifier
    {
        get;
        set;
    }

    /// <summary>
    /// 资源加载方式
    /// </summary>
    public ResourceLoadMethod LoadMethod
    {
        get;
        set;
    }

    /// <summary>
    /// 资源加载类型
    /// </summary>
    public ResourceLoadType LoadType
    {
        get;
        set;
    }

    /// <summary> 资源自身加载任务状态 /// </summary>
    public ResourceLoadState LoadState
    {
        get;
        set;
    }

    /// <summary> 当前资源加载信息 /// </summary>
    public AssetDatabaseInfo ResourceInfo
    {
        get
        {
            return mResourceInfo;
        }
    }
    private AssetDatabaseInfo mResourceInfo;

    public AssetDatabaseLoader()
    {
        AssetBundlePath = string.Empty;
        LoadResourceCompleteCallBack = null;
        LoadMethod = ResourceLoadMethod.Sync;
        LoadState = ResourceLoadState.None;
        mResourceInfo = null;
    }

    /// <summary>
    /// 开始资源加载任务
    /// </summary>
    public void startLoad()
    {
        if (LoadState == ResourceLoadState.None)
        {
            LoadState = ResourceLoadState.Waiting;
            loadAsset();
        }
        else if (LoadState == ResourceLoadState.Waiting)
        {
            ResourceLogger.logErr(string.Format("AB : {0}处于等待加载中状态，不应该再被调用startLoad，请检查资源加载是否异常！", AssetBundlePath));
        }
        else if (LoadState == ResourceLoadState.Loading)
        {
            ResourceLogger.logErr(string.Format("AB : {0}处于加载中状态，不应该再被调用startLoad，请检查资源加载是否异常！", AssetBundlePath));
        }
        else if (LoadState == ResourceLoadState.SelfComplete)
        {
            ResourceLogger.logErr(string.Format("AB : {0}已经处于自身加载完成状态，不应该再被调用startLoad，请检查资源加载是否异常！", AssetBundlePath));
        }
        else if (LoadState == ResourceLoadState.AllComplete)
        {
            ResourceLogger.logErr(string.Format("AB : {0}已经处于自身以及依赖AB加载完成状态，不应该再被调用startLoad，请检查资源加载是否异常！", AssetBundlePath));
        }
        else if (LoadState == ResourceLoadState.Error)
        {
            ResourceLogger.logErr(string.Format("AB:{0}处于Error状态，无法加载!", AssetBundlePath));
        }
    }

    public void recycle()
    {
        AssetBundlePath = string.Empty;
        LoadResourceCompleteCallBack = null;
        LoadMethod = ResourceLoadMethod.Sync;
        LoadState = ResourceLoadState.None;
        mResourceInfo = null;
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    private void loadAsset()
    {
        ResourceLogger.log(string.Format("加载资源:{0}", AssetBundlePath));
        //暂时默认都当同步处理
        if (LoadMethod == ResourceLoadMethod.Sync)
        {
            LoadState = ResourceLoadState.Loading;
            loadAssetSync();
        }
        else
        {
            LoadState = ResourceLoadState.Loading;
            loadAssetSync();
        }
    }

    /// <summary>
    /// 同步加载资源
    /// </summary>
    private void loadAssetSync()
    {
        if(string.IsNullOrEmpty(AssetBundlePath))
        {
            ResourceLogger.logErr(string.Format("找不到资源名 : {0}的资源!", AssetBundlePath));
        }
        else
        {
            mResourceInfo = createAssetDatabaseInfo(AssetBundlePath);
            mResourceInfo.updateLastUsedTime();
        }

        ///AssetDatabase模式下没有依赖资源的概念，
        ///所以一口气回调资源加载任务完成和上层逻辑回调
        LoadState = ResourceLoadState.SelfComplete;
        LoadSelfResourceCompleteNotifier(this);
        LoadSelfResourceCompleteNotifier = null;

        LoadState = ResourceLoadState.AllComplete;
        mResourceInfo.mIsReady = true;
        LoadResourceCompleteCallBack(mResourceInfo);
        LoadResourceCompleteCallBack = null;

        // 资源加载完成后，AssetDatabaseLoader的任务就完成了，回收重用
        AssetDatabaseLoaderFactory.recycle(this);
    }

    /// <summary>
    /// 创建AssetDatabaseInfo对象信息
    /// </summary>
    /// <param name="respath">资源路径</param>
    /// <returns></returns>
    private AssetDatabaseInfo createAssetDatabaseInfo(string respath)
    {
        var adi = AssetDatabaseInfoFactory.create();
        adi.AssetBundlePath = respath;
        adi.onResourceUnloadedCallback = onResourceUnloaded;
        return adi;
    }

    /// <summary>
    /// 对应资源卸载回调
    /// </summary>
    /// <param name="ari"></param>
    private static void onResourceUnloaded(AbstractResourceInfo ari)
    {
        var adi = ari as AssetDatabaseInfo;
        //资源卸载数据统计
        if (ResourceLoadAnalyse.Singleton.ResourceLoadAnalyseSwitch)
        {
            ResourceLoadAnalyse.Singleton.addResourceUnloadedTime(adi.AssetBundlePath);
        }
        // 资源卸载时资源AssetDatabaseInfo回收时回收重用
        AssetDatabaseInfoFactory.recycle(adi);
    }
}
#endif