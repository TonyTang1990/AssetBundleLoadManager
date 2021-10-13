/*
 * Description:             AssetBundleLoader.cs
 * Author:                  TONYTANG
 * Create Date:             2018//09/28
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AssetBundleLoader.cs
/// AB资源加载器抽象(一个加载器实例对象负责一个AB的加载任务，AB加载任务完成后重用)
/// </summary>
public class AssetBundleLoader : FactoryObj
{
    /// <summary> 资源管理层AB加载完成委托 /// </summary>
    /// <param name="abl">AB加载任务信息</param>
    public delegate void LoadABCompleteNotifier(AssetBundleLoader abl);

    /// <summary>
    /// 加载任务对应的AB资源路径
    /// </summary>
    public string AssetBundlePath
    {
        get;
        set;
    }

    /// <summary>
    /// 依赖的AB路径数组
    /// </summary>
    public string[] DepABPaths
    {
        get
        {
            return mDepABPaths;
        }
        set
        {
            mDepABPaths = value;
            mDepABCount = mDepABPaths == null ? 0 : mDepABPaths.Length;
            if (mDepAssetBundleInfoList == null)
            {
                mDepAssetBundleInfoList = new List<AssetBundleInfo>();
#if UNITY_EDITOR
                //Editor模式下的调试功能
                mUnloadedAssetBundleName = new List<string>();
#endif
            }
            else
            {
                mDepAssetBundleInfoList.Clear();
#if UNITY_EDITOR
                //Editor模式下的调试功能
                mUnloadedAssetBundleName.Clear();
#endif
            }
#if UNITY_EDITOR
            //Editor模式下的调试功能
            if (DepABPaths != null)
            {
                mUnloadedAssetBundleName.AddRange(DepABPaths);
            }
#endif
        }
    }
    private string[] mDepABPaths;

    /// <summary>
    /// 所有AB资源加载完成逻辑层回调
    /// </summary>
    public AbstractResourceModule.LoadResourceCompleteHandler LoadABCompleteCallBack
    {
        get;
        set;
    }

    /// <summary>
    /// 自身AB加载完成资源管理通知回调
    /// </summary>
    public LoadABCompleteNotifier LoadSelfABCompleteNotifier
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

    /// <summary> AB资源自身加载任务状态 /// </summary>
    public ResourceLoadState LoadState
    {
        get;
        set;
    }
    
    /// <summary> 依赖的AB数量 /// </summary>
    public int DepABCount
    {
        get
        {
            return mDepABCount;
        }
    }
    private int mDepABCount;

    /// <summary 已加载的依赖AB数量 /// </summary>
    private int mLoadedDepABCount;

    /// <summary> 当前AB加载信息 /// </summary>
    public AssetBundleInfo ABInfo
    {
        get
        {
            return mABInfo;
        }
    }
    private AssetBundleInfo mABInfo;

    /// <summary> 当前AB依赖的AB对应的AB信息列表(用于构建当前AssetBundleInfo) /// </summary>
    public List<AssetBundleInfo> DepAssetBundleInfoList
    {
        get
        {
            return mDepAssetBundleInfoList;
        }
    }
    private List<AssetBundleInfo> mDepAssetBundleInfoList;

#if UNITY_EDITOR
    /// <summary>
    /// 未加载的资源列表(测试用)
    /// </summary>
    public List<string> UnloadedAssetBundleName
    {
        get
        {
            return mUnloadedAssetBundleName;
        }
    }
    private List<string> mUnloadedAssetBundleName;
#endif

    public AssetBundleLoader()
    {
        AssetBundlePath = string.Empty;
        DepABPaths = null;
        LoadABCompleteCallBack = null;
        LoadSelfABCompleteNotifier = null;
        LoadMethod = ResourceLoadMethod.Sync;
        LoadState = ResourceLoadState.None;
        mLoadedDepABCount = 0;
        mABInfo = null;
    }

    public AssetBundleLoader(string abpath, string[] depnames)
    {
        AssetBundlePath = abpath;
        DepABPaths = depnames;
        LoadABCompleteCallBack = null;
        LoadSelfABCompleteNotifier = null;
        LoadMethod = ResourceLoadMethod.Sync;
        LoadState = ResourceLoadState.None;
        mLoadedDepABCount = 0;
        mABInfo = null;
    }

    /// <summary>
    /// 开始资源加载任务
    /// </summary>
    public void startLoad()
    {
        if (LoadState == ResourceLoadState.None)
        {
            LoadState = ResourceLoadState.Waiting;
            loadSelfAssetBundle();
        }
        else if(LoadState == ResourceLoadState.Waiting)
        {
            ResourceLogger.logErr(string.Format("AB : {0}处于等待加载中状态，不应该再被调用startLoad，请检查资源加载是否异常！", AssetBundlePath));
        }
        else if(LoadState == ResourceLoadState.Loading)
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

    /// <summary>
    /// 加载自身AB
    /// </summary>
    private void loadSelfAssetBundle()
    {
        if (LoadMethod == ResourceLoadMethod.Sync)
        {
            LoadState = ResourceLoadState.Loading;
            loadAssetBundleSync();
        }
        else
        {
            //异步加载AB修改成限制携程数量，入队列的形式
            //ModuleManager.Singleton.getModule<ResourceModuleManager>().StartCoroutine(loadAssetBundleAsync());
            AssetBundleAsyncQueue.enqueue(this);
        }
    }

    /// <summary>
    /// 同步加载AB
    /// </summary>
    /// <returns></returns>
    private void loadAssetBundleSync()
    {
        var abpath = global::AssetBundlePath.GetABLoadFullPath(AssetBundlePath);
        AssetBundle ab = null;
#if UNITY_EDITOR
        //因为资源不全，很多资源丢失，导致直接报错
        //这里临时先在Editor模式下判定下文件是否存在，避免AssetBundle.LoadFromFile()直接报错
        if (System.IO.File.Exists(abpath))
        {
            Debug.Log(string.Format("开始同步加载AB:{0}", AssetBundlePath));
            ab = AssetBundle.LoadFromFile(abpath);
        }
#else
        ab = AssetBundle.LoadFromFile(abpath);
#endif
        onSelfABLoadComplete(ab);
    }

    /// <summary>
    /// AB资源加载完成
    /// </summary>
    public void onSelfABLoadComplete(AssetBundle ab = null)
    {
        ResourceLogger.log(string.Format("AB:{0}自身加载完成!", AssetBundlePath));
        mABInfo = AssetBundleModule.createAssetBundleInfo(AssetBundlePath, ab);
        mABInfo.updateLastUsedTime();
        LoadState = ResourceLoadState.SelfComplete;
        // 通知上层自身AB加载完成，移除加载任务
        LoadSelfABCompleteNotifier(this);
        LoadSelfABCompleteNotifier = null;
        if (ab == null)
        {
            Debug.LogError(string.Format("AB : {0}文件不存在！", AssetBundlePath));
            failed();
            return;
        }

        loadDepAssetBundle();
    }

    /// <summary>
    /// 加载依赖AB
    /// </summary>
    private void loadDepAssetBundle()
    {
        // 如果没有依赖AB信息，直接当做AB加载完成返回
        // 否则直接当做AB加载完成处理
        if (DepABCount == 0)
        {
            onAllABLoadedComplete();
        }
        else
        {
            foreach (var dpabpath in DepABPaths)
            {
                // 依赖AB统一采用ResourceLoadType.NormalLoad方式，不采用被依赖资源AB的ResourceLoadType
                // 只要被依赖AB不被卸载就不会导致依赖AB被卸载
                ResourceModuleManager.Singleton.requstResource(dpabpath, onDepABLoadComplete, ResourceLoadType.NormalLoad, LoadMethod);
            }
        }
    }

    /// <summary>
    /// 依赖AB加载完成回调
    /// </summary>
    /// <param name="resinfo">ab加载信息</param>
    private void onDepABLoadComplete(AbstractResourceInfo resinfo)
    {
        var abinfo = resinfo as AssetBundleInfo;
        ResourceLogger.log($"AB:{AssetBundlePath}的依赖AB:{abinfo.ResourcePath}加载成功!");
        mDepAssetBundleInfoList.Add(abinfo);
        // 每完成一个依赖AB加载，添加依赖AB索引信息
        mABInfo.addDependency(abinfo);

#if UNITY_EDITOR
        //Editor模式下的调试功能
        // 移除已经加载过的，存储来测试查看用
        mUnloadedAssetBundleName.Remove(abinfo.ResourcePath);
#endif
        mLoadedDepABCount++;
        // 作为依赖AB时并不会触发getAsset || instantiateAsset之类的接口，
        // 依赖于Unity加载依赖AB自动还原的机制，所以这里我们需要手动更新AB资源的最近使用时间
        abinfo.updateLastUsedTime();
        if (mLoadedDepABCount == mDepABCount)
        {
            onAllABLoadedComplete();
        }
    }

    /// <summary>
    /// 所有AB加载完成(自身和依赖AB)
    /// </summary>
    private void onAllABLoadedComplete()
    {
        ResourceLogger.log(string.Format("AB:{0}所有AB加载完成!", AssetBundlePath));

        // 依赖AB的索引计数添加在每一个依赖AB加载完成时，
        // 所有完成再添加计数如果存在加载打断取消就没法正确恢复依赖AB计数了

        complete();
    }

    /// <summary>
    /// 加载失败
    /// </summary>
    private void failed()
    {
        ResourceLogger.logErr($"加载AB:{AssetBundlePath}失败!");
        LoadState = ResourceLoadState.Error;
        onComplete();
    }

    /// <summary>
    /// 完成加载
    /// </summary>
    private void complete()
    {
        ResourceLogger.log($"加载AB:{AssetBundlePath}完成!");
        LoadState = ResourceLoadState.AllComplete;
        onComplete();
    }

    /// <summary>
    /// 响应加载完成
    /// </summary>
    private void onComplete()
    {
        // 所有AB加载完成才算AB Ready可以使用
        mABInfo.mIsReady = true;

        // 通知上层ab加载完成，可以开始加载具体的asset
        LoadABCompleteCallBack(mABInfo);
        LoadABCompleteCallBack = null;

        // 自身AB以及依赖AB加载完成后，AssetBundleLoader的任务就完成了，回收重用
        AssetBundleLoaderFactory.recycle(this);
    }

    /// <summary>
    /// 回收重用
    /// </summary>
    public void recycle()
    {
        AssetBundlePath = string.Empty;
        mDepABPaths = null;
        LoadABCompleteCallBack = null;
        LoadSelfABCompleteNotifier = null;
        LoadMethod = ResourceLoadMethod.Sync;
        LoadState = ResourceLoadState.None;
        mDepABCount = 0;
        mLoadedDepABCount = 0;
        mABInfo = null;
        mDepAssetBundleInfoList.Clear();
    }
}
