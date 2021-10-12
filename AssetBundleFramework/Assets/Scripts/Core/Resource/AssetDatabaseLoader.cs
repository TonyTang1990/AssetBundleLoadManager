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
/// AssetDatabase模式资源加载是面相Asset级别的，依赖加载和索引计数也是面向Asset级别的
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
    public string AssetPath
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
    public AssetDatabaseInfo AssetInfo
    {
        get
        {
            return mAssetInfo;
        }
    }
    private AssetDatabaseInfo mAssetInfo;

    /// <summary>
    /// 当前Asset依赖的Asset对应的Asset信息列表
    /// </summary>
    public List<AssetDatabaseInfo> DepAssetInfoList
    {
        get
        {
            return mDepAssetInfoList;
        }
    }
    private List<AssetDatabaseInfo> mDepAssetInfoList;

    /// <summary>
    /// 所有依赖的有效Asset路径列表
    /// </summary>
    private List<string> mAllValideDepAssetPathList;

    /// <summary>
    /// 已加载的依赖Asset数量
    /// </summary>
    private int mLoadedDepAssetCount;


    public AssetDatabaseLoader()
    {
        AssetPath = string.Empty;
        LoadResourceCompleteCallBack = null;
        LoadMethod = ResourceLoadMethod.Sync;
        LoadState = ResourceLoadState.None;
        mAssetInfo = null;
        mDepAssetInfoList = new List<AssetDatabaseInfo>();
        mLoadedDepAssetCount = 0;
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
            ResourceLogger.logErr(string.Format("Asset : {0}处于等待加载中状态，不应该再被调用startLoad，请检查资源加载是否异常！", AssetPath));
        }
        else if (LoadState == ResourceLoadState.Loading)
        {
            ResourceLogger.logErr(string.Format("Asset : {0}处于加载中状态，不应该再被调用startLoad，请检查资源加载是否异常！", AssetPath));
        }
        else if (LoadState == ResourceLoadState.SelfComplete)
        {
            ResourceLogger.logErr(string.Format("Asset : {0}已经处于自身加载完成状态，不应该再被调用startLoad，请检查资源加载是否异常！", AssetPath));
        }
        else if (LoadState == ResourceLoadState.AllComplete)
        {
            ResourceLogger.logErr(string.Format("Asset : {0}已经处于自身以及依赖AB加载完成状态，不应该再被调用startLoad，请检查资源加载是否异常！", AssetPath));
        }
        else if (LoadState == ResourceLoadState.Error)
        {
            ResourceLogger.logErr(string.Format("Asset:{0}处于Error状态，无法加载!", AssetPath));
        }
    }

    public void recycle()
    {
        AssetPath = string.Empty;
        LoadResourceCompleteCallBack = null;
        LoadMethod = ResourceLoadMethod.Sync;
        LoadState = ResourceLoadState.None;
        mAssetInfo = null;
        mDepAssetInfoList.Clear();
        mAllValideDepAssetPathList.Clear();
        mLoadedDepAssetCount = 0;
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    private void loadAsset()
    {
        ResourceLogger.log(string.Format("加载资源:{0}", AssetPath));
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
        if(string.IsNullOrEmpty(AssetPath))
        {
            ResourceLogger.logErr(string.Format("找不到资源名 : {0}的资源!", AssetPath));
        }
        else
        {
            mAssetInfo = createAssetDatabaseInfo(AssetPath);
            mAssetInfo.updateLastUsedTime();
        }

        ///AssetDatabase模式下没有依赖资源的概念，
        ///所以一口气回调资源加载任务完成和上层逻辑回调
        LoadState = ResourceLoadState.SelfComplete;
        LoadSelfResourceCompleteNotifier(this);
        LoadSelfResourceCompleteNotifier = null;

        loadDepAssets();

        LoadState = ResourceLoadState.AllComplete;
        mAssetInfo.mIsReady = true;
        LoadResourceCompleteCallBack(mAssetInfo);
        LoadResourceCompleteCallBack = null;

        // 资源加载完成后，AssetDatabaseLoader的任务就完成了，回收重用
        AssetDatabaseLoaderFactory.recycle(this);
    }
    
    /// <summary>
    /// 加载依赖Asset
    /// </summary>
    private void loadDepAssets()
    {
        // AssetDatabase模式下的依赖按Asset依赖来计算
        // 通过AssetDatabase.GetDependencies()方法获得
        var allDependentAssets = AssetDatabase.GetDependencies(AssetPath, true);
        mAllValideDepAssetPathList = new List<string>();
        foreach(var dependentAssetPath in allDependentAssets)
        {
            if(ResourceHelper.IsAssetPathHasValideAssetPostFix(dependentAssetPath) && !string.Equals(AssetPath, dependentAssetPath))
            {
                mAllValideDepAssetPathList.Add(dependentAssetPath);
            }
        }

        // 如果没有依赖Asset信息，直接当做Asset加载完成返回
        if (mAllValideDepAssetPathList.Count == 0)
        {
            allAssetLoadedComplete();
        }
        else
        {
            for(int i = 0, length = mAllValideDepAssetPathList.Count; i < length; i++)
            {
                // 依赖Asset统一采用ResourceLoadType.NormalLoad方式，不采用被依赖资源Asset的ResourceLoadType
                // 只要被依赖Asset不被卸载就不会导致依赖Asset被卸载
                ResourceModuleManager.Singleton.requstResource(mAllValideDepAssetPathList[i], onDependentAssetLoadComplete, ResourceLoadType.NormalLoad, LoadMethod);
            }
        }
    }

    /// <summary>
    /// 依赖Asset加载完成回调
    /// </summary>
    /// <param name="resInfo"></param>
    private void onDependentAssetLoadComplete(AbstractResourceInfo resInfo)
    {
        var assetInfo = resInfo as AssetDatabaseInfo;
        ResourceLogger.log($"依赖Asset:{assetInfo.ResourcePath}加载成功!");
        mDepAssetInfoList.Add(assetInfo);
        // 每完成一个依赖Asset加载，添加依赖Asset索引信息
        mAssetInfo.addDependency(assetInfo);

        mLoadedDepAssetCount++;

        // 作为依赖Asset时并不会触发getAsset || instantiateAsset之类的接口。
        // 依赖与Unity加载依赖Asset自动还原的机制，所以这里我们需要手动更新Asset资源的最近使用时间
        assetInfo.updateLastUsedTime();
        if(mLoadedDepAssetCount == mAllValideDepAssetPathList.Count)
        {
            allAssetLoadedComplete();
        }
    }

    /// <summary>
    /// 所有Asset加载完成
    /// </summary>
    private void allAssetLoadedComplete()
    {
        ResourceLogger.log($"Asset:{AssetPath}所有Asset加载完成!");

        // 依赖Asset的索引计数添加在每一个依赖Asset加载完成时，
        // 所有完成再添加计数如果存在加载打断取消就没法正确恢复依赖Asset计数了

        LoadState = ResourceLoadState.AllComplete;

        // 所有Asset加载完成才算Asset Ready可以使用
        mAssetInfo.mIsReady = true;

        // 通知上层asset加载完成，可以开始加载具体的asset
        LoadResourceCompleteCallBack(mAssetInfo);
        LoadResourceCompleteCallBack = null;

        // 自身Asset以及依赖Asset加载完成后，AssetDatabaseLoader的任务就完成了，回收重用
        AssetDatabaseLoaderFactory.recycle(this);
    }

    /// <summary>
    /// 创建AssetDatabaseInfo对象信息
    /// </summary>
    /// <param name="assetBundlePath">AB路径</param>
    /// <param name="respath">资源路径</param>
    /// <returns></returns>
    private AssetDatabaseInfo createAssetDatabaseInfo(string respath)
    {
        var adi = AssetDatabaseInfoFactory.create();
        adi.ResourcePath = respath;
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
            ResourceLoadAnalyse.Singleton.addResourceUnloadedTime(adi.ResourcePath);
        }
        // 资源卸载时资源AssetDatabaseInfo回收时回收重用
        AssetDatabaseInfoFactory.recycle(adi);
    }
}
#endif