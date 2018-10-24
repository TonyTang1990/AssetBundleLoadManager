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
/// AB加载任务状态
/// </summary>
public enum ABLoadState
{
    None = 1,             // 未加载状态
    Loading = 2,          // 加载中状态
    Complete = 3,         // 加载完成状态
    Error = 4             // 出错状态
}

/// <summary>
/// AssetBundleLoader.cs
/// AB资源加载器抽象(一个加载器实例对象负责一个AB的加载任务，AB加载任务完成后重用)
/// </summary>
public class AssetBundleLoader : FactoryObj {

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
                mUnloadedAssetBundleName = new List<string>();
            }
            else
            {
                mDepAssetBundleInfoList.Clear();
                mUnloadedAssetBundleName.Clear();
            }
            if(DepABNames != null)
            {
                mUnloadedAssetBundleName.AddRange(DepABNames);
            }
        }
    }
    private string[] mDepABNames;

    /// <summary>
    /// AB资源加载完成逻辑层回调
    /// </summary>
    public ResourceModuleManager.LoadABCompleteHandler LoadABCompleteCallBack
    {
        get;
        set;
    }

    /// <summary>
    /// AB加载完成资源管理通知回调
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

    /// <summary> AB资源加载任务状态 /// </summary>
    public ABLoadState LoadState
    {
        get;
        private set;
    }

    /// <summary> 依赖的AB数量 /// </summary>
    public int DepABCount
    {
        get
        {
            return mDepABCount;
        }
        private set
        {
            mDepABCount = value;
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
        private set
        {
            mABInfo = value;
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
        set
        {
            mDepAssetBundleInfoList = value;
        }
    }
    private List<AssetBundleInfo> mDepAssetBundleInfoList;

    /// <summary>
    /// 未加载的资源列表(测试用)
    /// </summary>
    public List<string> UnloadedAssetBundleName
    {
        get
        {
            return mUnloadedAssetBundleName;
        }
        private set
        {
            mUnloadedAssetBundleName = value;
        }
    }
    private List<string> mUnloadedAssetBundleName;

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
        if(LoadState == ABLoadState.None)
        {
            LoadState = ABLoadState.Loading;
            loadSelfAssetBundle();
        }
        else if(LoadState == ABLoadState.Complete)
        {
            Debug.LogError(string.Format("AB : {0}已经处于完成状态，不应该再被调用startLoad，请检查资源加载是否异常！", ABName));
        }
        else if(LoadState == ABLoadState.Error)
        {
            Debug.LogError(string.Format("AB:{0}处于Error状态，无法加载!", ABName));
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
                // 依赖AB统一采取同步加载方式
                ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource(dpab, onDepABLoadComplete, ABLoadType.NormalLoad, ABLoadMethod.Sync);
            }
        }
    }

    /// <summary>
    /// 加载自身AB
    /// </summary>
    private void loadSelfAssetBundle()
    {
        Debug.Log(string.Format("加载AB:{0}", ABName));
        if(LoadMethod == ABLoadMethod.Sync)
        {
            loadAssetBundleSync();
        }
        else
        {
            ModuleManager.Singleton.getModule<ResourceModuleManager>().StartCoroutine(loadAssetBundleAsync());
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
        if(System.IO.File.Exists(abpath))
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

    /// <summary>
    /// 异步加载AB
    /// </summary>
    /// <returns></returns>
    private IEnumerator loadAssetBundleAsync()
    {
        var abpath = AssetBundlePath.GetABPath() + ABName;
        var abrequest = AssetBundle.LoadFromFileAsync(abpath);
        yield return abrequest;
        onSelfABLoadComplete(abrequest.assetBundle);
    }

    /// <summary>
    /// 依赖AB加载完成回调
    /// </summary>
    /// <param name="abinfo">ab加载信息</param>
    private void onDepABLoadComplete(AssetBundleInfo abinfo)
    {
        Debug.Log(string.Format("依赖AB:{0}加载成功!", abinfo.AssetBundleName));
        mDepAssetBundleInfoList.Add(abinfo);
        // 移除已经加载过的，存储来测试查看用
        mUnloadedAssetBundleName.Remove(abinfo.AssetBundleName);
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
    private void onSelfABLoadComplete(AssetBundle ab = null)
    {
        Debug.Log(string.Format("AB:{0}自身加载完成!", ABName));
        
        mABInfo = ModuleManager.Singleton.getModule<ResourceModuleManager>().createAssetBundleInfo(ABName, ab);
        mABInfo.updateLastUsedTime();

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
        Debug.Log(string.Format("AB:{0}所有AB加载完成!", ABName));

        // 所有AB加载完添加依赖AB索引信息并通知上层可以使用了
        foreach (var dpabinfo in mDepAssetBundleInfoList)
        {
            mABInfo.addDependency(dpabinfo);
        }

        // 所有AB加载完成才算完成
        LoadState = ABLoadState.Complete;

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
