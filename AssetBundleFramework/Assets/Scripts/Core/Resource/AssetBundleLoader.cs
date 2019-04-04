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

    /// <summary>
    /// 加载任务对应的AB资源名字
    /// </summary>
    public string ABName
    {
        get;
        set;
    }

    /// <summary>
    /// 依赖的AB名字数组
    /// </summary>
    public string[] DepABNames
    {
        get
        {
            return mDepABNames;
        }
        set
        {
            mDepABNames = value;
            mDepABCount = mDepABNames == null ? 0 : mDepABNames.Length;
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
            if (DepABNames != null)
            {
                mUnloadedAssetBundleName.AddRange(DepABNames);
            }
#endif
        }
    }
    private string[] mDepABNames;

    /// <summary>
    /// 所有AB资源加载完成逻辑层回调
    /// </summary>
    public ResourceModuleManager.LoadABCompleteHandler LoadABCompleteCallBack
    {
        get;
        set;
    }

    /// <summary>
    /// 自身AB加载完成资源管理通知回调
    /// </summary>
    public ResourceModuleManager.LoadABCompleteNotifier LoadABCompleteNotifier
    {
        get;
        set;
    }

    /// <summary>
    /// 资源加载方式
    /// </summary>
    public ABLoadMethod LoadMethod
    {
        get;
        set;
    }

    /// <summary>
    /// 资源加载类型
    /// </summary>
    public ABLoadType LoadType
    {
        get;
        set;
    }

    /// <summary> AB资源自身加载任务状态 /// </summary>
    public ABLoadState LoadState
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
        ABName = string.Empty;
        DepABNames = null;
        LoadABCompleteCallBack = null;
        LoadABCompleteNotifier = null;
        LoadMethod = ABLoadMethod.Sync;
        LoadState = ABLoadState.None;
        mLoadedDepABCount = 0;
        mABInfo = null;
    }

    public AssetBundleLoader(string abname, string[] depnames)
    {
        ABName = abname;
        DepABNames = depnames;
        LoadABCompleteCallBack = null;
        LoadABCompleteNotifier = null;
        LoadMethod = ABLoadMethod.Sync;
        LoadState = ABLoadState.None;
        mLoadedDepABCount = 0;
        mABInfo = null;
    }

    /// <summary>
    /// 开始资源加载任务
    /// </summary>
    public void startLoad()
    {
        if (LoadState == ABLoadState.None)
        {
            LoadState = ABLoadState.Waiting;
            loadSelfAssetBundle();
        }
        else if(LoadState == ABLoadState.Waiting)
        {
            ResourceLogger.logErr(string.Format("AB : {0}处于等待加载中状态，不应该再被调用startLoad，请检查资源加载是否异常！", ABName));
        }
        else if(LoadState == ABLoadState.Loading)
        {
            ResourceLogger.logErr(string.Format("AB : {0}处于加载中状态，不应该再被调用startLoad，请检查资源加载是否异常！", ABName));
        }
        else if (LoadState == ABLoadState.SelfComplete)
        {
            ResourceLogger.logErr(string.Format("AB : {0}已经处于自身加载完成状态，不应该再被调用startLoad，请检查资源加载是否异常！", ABName));
        }
        else if (LoadState == ABLoadState.AllComplete)
        {
            ResourceLogger.logErr(string.Format("AB : {0}已经处于自身以及依赖AB加载完成状态，不应该再被调用startLoad，请检查资源加载是否异常！", ABName));
        }
        else if (LoadState == ABLoadState.Error)
        {
            ResourceLogger.logErr(string.Format("AB:{0}处于Error状态，无法加载!", ABName));
        }
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
            allABLoadedComplete();
        }
        else
        {
            foreach (var dpab in DepABNames)
            {
                // 依赖AB统一采用ABLoadType.NormalLoad方式，不采用被依赖资源AB的ABLoadType
                // 只要被依赖AB不被卸载就不会导致依赖AB被卸载
                ResourceModuleManager.getInstance().requstResource(dpab, onDepABLoadComplete, ABLoadType.NormalLoad, LoadMethod);
            }
        }
    }

    /// <summary>
    /// 加载自身AB
    /// </summary>
    private void loadSelfAssetBundle()
    {
        ResourceLogger.log(string.Format("加载AB:{0}", ABName));
        if (LoadMethod == ABLoadMethod.Sync)
        {
            LoadState = ABLoadState.Loading;
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
        var abpath = AssetBundlePath.GetABPath() + ABName;
        AssetBundle ab = null;
#if UNITY_EDITOR
        //因为资源不全，很多资源丢失，导致直接报错
        //这里临时先在Editor模式下判定下文件是否存在，避免AssetBundle.LoadFromFile()直接报错
        if (System.IO.File.Exists(abpath))
        {
            ab = AssetBundle.LoadFromFile(abpath);
        }
        else
        {
            Debug.LogError(string.Format("AB : {0}文件不存在！", ABName));
        }
#else
        ab = AssetBundle.LoadFromFile(abpath);
#endif
        onSelfABLoadComplete(ab);
    }

    /*
     * 异步加载AB走AssetBundleAsyncQueue的队列模式
    /// <summary>
    /// 异步加载AB
    /// </summary>
    /// <returns></returns>
    public IEnumerator loadAssetBundleAsync()
    {
        var abpath = AssetBundlePath.GetABPath() + ABName;
        AssetBundleCreateRequest abrequest = null;
#if UNITY_EDITOR
        //因为资源不全，很多资源丢失，导致直接报错
        //这里临时先在Editor模式下判定下文件是否存在，避免AssetBundle.LoadFromFile()直接报错
        if (System.IO.File.Exists(abpath))
        {
            abrequest = AssetBundle.LoadFromFileAsync(abpath);
        }
        else
        {
            Debug.LogError(string.Format("AB : {0}文件不存在！", ABName));
        }
#else
        abrequest = AssetBundle.LoadFromFileAsync(abpath);
#endif
        yield return abrequest;
        var assetbundle = abrequest.assetBundle;
        if (assetbundle == null)
        {
            ResourceLogger.logErr(string.Format("Failed to load AssetBundle : {0}!", ABName));
        }

        onSelfABLoadComplete(assetbundle);
    }
    */

    /// <summary>
    /// 依赖AB加载完成回调
    /// </summary>
    /// <param name="abinfo">ab加载信息</param>
    private void onDepABLoadComplete(AssetBundleInfo abinfo)
    {
        ResourceLogger.log(string.Format("依赖AB:{0}加载成功!", abinfo.AssetBundleName));
        mDepAssetBundleInfoList.Add(abinfo);
#if UNITY_EDITOR
        //Editor模式下的调试功能
        // 移除已经加载过的，存储来测试查看用
        mUnloadedAssetBundleName.Remove(abinfo.AssetBundleName);
#endif
        mLoadedDepABCount++;
        // 作为依赖AB时并不会触发getAsset || instantiateAsset之类的接口，
        // 依赖于Unity加载依赖AB自动还原的机制，所以这里我们需要手动更新AB资源的最近使用时间
        abinfo.updateLastUsedTime();
        if (mLoadedDepABCount == mDepABCount)
        {
            allABLoadedComplete();
        }
    }

    /// <summary>
    /// AB资源加载完成
    /// </summary>
    public void onSelfABLoadComplete(AssetBundle ab = null)
    {
        ResourceLogger.log(string.Format("AB:{0}自身加载完成!", ABName));

        mABInfo = ResourceModuleManager.getInstance().createAssetBundleInfo(ABName, ab);
        mABInfo.updateLastUsedTime();
        
        LoadState = ABLoadState.SelfComplete;

        // 通知上层自身AB加载完成，移除加载任务
        LoadABCompleteNotifier(this);
        LoadABCompleteNotifier = null;

        loadDepAssetBundle();
    }

    /// <summary>
    /// 所有AB加载完成(自身和依赖AB)
    /// </summary>
    private void allABLoadedComplete()
    {
        ResourceLogger.log(string.Format("AB:{0}所有AB加载完成!", ABName));

        // 所有AB加载完添加依赖AB索引信息并通知上层可以使用了
        foreach (var dploader in mDepAssetBundleInfoList)
        {
            mABInfo.addDependency(dploader);
        }

        LoadState = ABLoadState.AllComplete;

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
        ABName = string.Empty;
        mDepABNames = null;
        LoadABCompleteCallBack = null;
        LoadABCompleteNotifier = null;
        LoadMethod = ABLoadMethod.Sync;
        LoadState = ABLoadState.None;
        mDepABCount = 0;
        mLoadedDepABCount = 0;
        mABInfo = null;
        mDepAssetBundleInfoList.Clear();
    }
}
