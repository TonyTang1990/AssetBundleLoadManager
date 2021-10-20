/*
 * Description:             AssetBundleInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2018//09/28
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AssetBundleInfo.cs
/// AB加载信息的抽象，负责AB加载后的信息管理抽象(AB索引计数，Asset缓存都在这一层)
/// </summary>
public class AssetBundleInfo : AbstractResourceInfo, FactoryObj
{
    /// <summary> 当前加载对应的AB /// </summary>
    public AssetBundle Bundle
    {
        get;
        set;
    }
    
    /// <summary> 当前AB依赖AB的信息组 /// </summary>
    private HashSet<AssetBundleInfo> mDepAssetBundleInfoSets;

    public AssetBundleInfo()
    {
        Bundle = null;
        ResourcePath = string.Empty;
        LastUsedTime = 0.0f;
        mIsReady = false;
        mIsAllAssetLoaded = false;
        mDepAssetBundleInfoSets = new HashSet<AssetBundleInfo>();
        RefCount = 0;
        mReferenceOwnerList = new List<System.WeakReference>();
        mLoadedAssetMap = new Dictionary<string, UnityEngine.Object>();
        onResourceUnloadedCallback = null;
    }

    /// <summary>
    /// 添加依赖的AB信息
    /// </summary>
    /// <param name="depabi"></param>
    public void addDependency(AssetBundleInfo depabi)
    {
        if (depabi != null && mDepAssetBundleInfoSets.Add(depabi))
        {
            // 增加依赖AB的引用计数
            // 在当前AB引用计数归零时会一并返还
            depabi.retain();
        }
    }

    /// <summary>
    /// 实例化指定Asset(上层获取并绑定Asset实例化GameObject对象接口)
    /// </summary>
    /// <param name="assetname"></param>
    /// <returns></returns>
    public override GameObject instantiateAsset(string assetname)
    {
        var goasset = loadAsset<GameObject>(assetname);
        if (goasset != null)
        {
            var goinstance = GameObject.Instantiate<GameObject>(goasset);
            //不修改实例化后的名字，避免上层逻辑名字对不上
            //goinstance.name = goasset.name;
            // 绑定owner对象，用于判定是否还有有效对象引用AB资源
            retainOwner(goinstance);
            updateLastUsedTime();
            return goinstance;
        }
        else
        {
            ResourceLogger.logErr(string.Format("AB:{0}里加载GameObject Asset:{1}失败!", ResourcePath, assetname));
            return null;
        }
    }

    /// <summary>
    /// 获取并绑定指定Asste资源(上层获取并绑定特定类型Asset的接口)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="owner"></param>
    /// <param name="assetname"></param>
    /// <returns></returns>
    public override T getAsset<T>(UnityEngine.Object owner, string assetname)
    {
        if (owner != null)
        {
            var asset = loadAsset<T>(assetname);
            if (asset != null)
            {
                // 绑定owner对象，用于判定是否还有有效对象引用AB资源
                retainOwner(owner);
                updateLastUsedTime();
                return asset;
            }
            else
            {
                ResourceLogger.logWar(string.Format("AB : {0}里不存在Asset : {1}，获取Asset失败!", ResourcePath, assetname));
                return null;
            }
        }
        else
        {
            ResourceLogger.logErr(string.Format("不能绑定Asset到空对象上!加载AB:{0} Asset:{1}失败!", ResourcePath, assetname));
            return null;
        }
    }


    /// <summary>
    /// 获取并绑定指定Sub Asste资源(上层获取并绑定特定类型Sub Asset的接口)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="owner"></param>
    /// <param name="assemainAssetNametname"></param>
    /// <param name="assetname"></param>
    /// <returns></returns>
    public override T getSubAsset<T>(UnityEngine.Object owner, string mainAssetName, string assetname)
    {
        if (owner != null)
        {
            var asset = loadSubAsset<T>(mainAssetName, assetname);
            if (asset != null)
            {
                // 绑定owner对象，用于判定是否还有有效对象引用AB资源
                retainOwner(owner);
                updateLastUsedTime();
                return asset;
            }
            else
            {
                ResourceLogger.logWar(string.Format("AB : {0}里不存在Asset : {1}，获取Asset失败!", ResourcePath, assetname));
                return null;
            }
        }
        else
        {
            ResourceLogger.logErr(string.Format("不能绑定Asset到空对象上!加载AB:{0} Asset:{1}失败!", ResourcePath, assetname));
            return null;
        }
    }

    /// <summary>
    /// 释放AB
    /// </summary>
    public override void dispose()
    {
        unloadAssetBundle();

        LastUsedTime = 0.0f;

        var enu = mDepAssetBundleInfoSets.GetEnumerator();
        while (enu.MoveNext())
        {
            var dep = enu.Current;
            // 减少依赖的AB的引用计数
            dep.release();
        }
        mDepAssetBundleInfoSets.Clear();
        if (onResourceUnloadedCallback != null)
        {
            onResourceUnloadedCallback(this);
        }
        onResourceUnloadedCallback = null;
    }

    /*
    /// <summary>
    /// 更新最近使用时间
    /// </summary>
    public void updateLastUsedTime()
    {
        LastUsedTime = Time.time;
    }
    */

    #region Debug
    /// <summary>
    /// 获取AssetBundle使用的详细描述信息
    /// </summary>
    /// <returns></returns>
    public string getAssetBundleInfoDes()
    {
        var abides = string.Empty;
        abides += string.Format("AB Name: {0}\n", ResourcePath);
        abides += string.Format("Ref Count: {0}\n", RefCount);
        if (mReferenceOwnerList.Count == 0)
        {
            abides += "Owners Name : None\n";
        }
        else
        {
            abides += "Owners Name :\n";
            for (int i = 0, length = mReferenceOwnerList.Count; i < length; i++)
            {
                if (mReferenceOwnerList[i].Target != null)
                {
                    abides += string.Format("owner[{0}] : {1}\n", i, mReferenceOwnerList[i].Target.ToString());
                }
            }
        }
        abides += string.Format("Last Used Time: {0}\n", LastUsedTime);
        return abides;
    }
    #endregion

    /// <summary>
    /// 加载所有Asset(比如Shader预加载)
    /// Note:
    /// 加载图集使用loadAllAsset<Sprite>()或者loadAsset<Sprite>
    /// </summary>
    /// <param name="assetname"></param>
    /// <returns></returns>
    public override void loadAllAsset<T>()
    {
        if (mIsReady)
        {
            if (!mIsAllAssetLoaded)
            {
                var allassets = Bundle.LoadAllAssets<T>();
                foreach (var asset in allassets)
                {
                    var assetname = asset.name;
                    if (!mLoadedAssetMap.ContainsKey(assetname))
                    {
                        mLoadedAssetMap.Add(assetname, asset);
                    }
                    else
                    {
                        ResourceLogger.logErr(string.Format("资源名 : {0}里有同名资源！Asset资源 : {1}添加失败!", ResourcePath, assetname));
                    }
                }
                mIsAllAssetLoaded = true;
            }
        }
    }

    /// <summary>
    /// 加载指定Asset(可用上层访问Asset)
    /// Note:
    /// 因为没有绑定对象，仅用于临时访问Asset数据
    /// 访问过后无人引用的话会被回收
    /// </summary>
    /// <param name="assetname"></param>
    /// <returns></returns>
    public override T loadAsset<T>(string assetname)
    {
        if (mIsReady)
        {
            if (mLoadedAssetMap.ContainsKey(assetname))
            {
                return mLoadedAssetMap[assetname] as T;
            }
            else
            {
                if (Bundle == null)
                {
                    ResourceLogger.logErr(string.Format("AB : {0}资源丢失，不存在！", ResourcePath));
                    return null;
                }
                else
                {
                    var asset = Bundle.LoadAsset<T>(assetname);
                    if (asset != null)
                    {
                        mLoadedAssetMap.Add(assetname, asset);
                        return asset;
                    }
                    else
                    {
                        ResourceLogger.logErr(string.Format("AB:{0}里找不到Asset:{1}资源!", ResourcePath, assetname));
                        return null;
                    }
                }
            }
        }
        else
        {
            ResourceLogger.logErr(string.Format("异常状态，AB资源:{0}未就绪就请求Asset资源:{1}", ResourcePath, assetname));
            return null;
        }
    }

    /// <summary>
    /// 加载指定Sub Asset(可用上层访问Asset)
    /// Note:
    /// 因为没有绑定对象，仅用于临时访问Asset数据
    /// 访问过后无人引用的话会被回收
    /// </summary>
    /// <param name="mainAssetName"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public override T loadSubAsset<T>(string mainAssetName, string assetName)
    {
        if (mIsReady)
        {
            if (mLoadedAssetMap.ContainsKey(assetName))
            {
                return mLoadedAssetMap[assetName] as T;
            }
            else
            {
                if (Bundle == null)
                {
                    ResourceLogger.logErr(string.Format("AB : {0}资源丢失，不存在！", ResourcePath));
                    return null;
                }
                else
                {
                    var allSubAssets = Bundle.LoadAssetWithSubAssets<T>(mainAssetName);
                    if(allSubAssets.Length == 0)
                    {
                        ResourceLogger.logErr(string.Format("AB:{0}里找不到Asset:{1}资源!", ResourcePath, assetName));
                        return null;
                    }
                    T targetAsset = null;
                    for(int i = 0, length = allSubAssets.Length; i < length; i++)
                    {
                        if(allSubAssets[i].name.Equals(assetName) && allSubAssets[i] is T)
                        {
                            if (targetAsset != null)
                            {
                                ResourceLogger.logErr($"资源名: {ResourcePath}里有多个同类型:{typeof(T).GetType().Name}同名的SubAsset : {assetName}资源！");
                            }
                            targetAsset = allSubAssets[i];
                        }
                        if(!mLoadedAssetMap.ContainsKey(allSubAssets[i].name))
                        {
                            mLoadedAssetMap.Add(allSubAssets[i].name, allSubAssets[i]);
                        }
                    }
                    if(targetAsset != null)
                    {
                        return targetAsset;
                    }
                    else
                    {
                        ResourceLogger.logErr(string.Format("AB:{0}里找不到SubAsset:{1}资源!", ResourcePath, assetName));
                        return null;
                    }
                }
            }
        }
        else
        {
            ResourceLogger.logErr(string.Format("异常状态，AB资源:{0}未就绪就请求Asset资源:{1}", ResourcePath, assetName));
            return null;
        }
    }

    /// <summary>
    /// 卸载AB(AssetBundle.Unload(true)的形式)
    /// </summary>
    private void unloadAssetBundle()
    {
        ResourceLogger.log(string.Format("卸载AB:{0}", ResourcePath));
        if (Bundle != null)
        {
            // 索引计数为零时调用释放AB和Asset
            Bundle.Unload(true);
        }
        Bundle = null;
        mIsReady = false;
    }

    /// <summary>
    /// 回收重用
    /// </summary>
    public void recycle()
    {
        Bundle = null;
        ResourcePath = string.Empty;
        LastUsedTime = 0.0f;
        mIsReady = false;
        mIsAllAssetLoaded = false;
        mDepAssetBundleInfoSets.Clear();
        RefCount = 0;
        mReferenceOwnerList.Clear();
        mLoadedAssetMap.Clear();
        onResourceUnloadedCallback = null;
    }
}
