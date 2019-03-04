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
public class AssetBundleInfo : FactoryObj
{

    /// <summary>
    /// AB卸载委托
    /// </summary>
    /// <param name="abi"></param>
    public delegate void OnUnloadedHandler(AssetBundleInfo abi);

    /// <summary> AB卸载回调(用于通知AB对应Loader切换状态) /// </summary>
    public OnUnloadedHandler onUnloadedCallback;

    /// <summary> 当前加载对应的AB /// </summary>
    public AssetBundle Bundle
    {
        get;
        set;
    }

    /// <summary> AB名字 /// </summary>
    public string AssetBundleName
    {
        get;
        set;
    }

    /// <summary>
    /// 上一次使用的有效时间(用于回收策略判定，比如越久未使用先回收，0.0f表示被回收或者未被使用)
    /// </summary>
    public float LastUsedTime
    {
        get;
        private set;
    }

    /// <summary> 是否不再有人使用 /// </summary>
    public bool IsUnsed
    {
        get { return mIsReady && mRefCount <= 0 && updateOwnerReference() == 0; }
    }

    /// <summary> AB是否已经加载完成 /// </summary>
    public bool mIsReady;

    /// <summary>
    /// AB里的所有Asset是否已经加载完(仅用于判定通过AssetBundle.LoadAllAssets()或者AssetBundle.LoadAllAssetsAsync()的加载方式)
    /// </summary>
    private bool mIsAllAssetLoaded;

    /// <summary> 当前AB依赖AB的信息组 /// </summary>
    private HashSet<AssetBundleInfo> mDepAssetBundleInfoSets;

    /// <summary> AB引用计数 /// </summary>
    public int RefCount
    {
        get
        {
            return mRefCount;
        }
    }
    private int mRefCount;

    /// <summary>
    /// 引用对象列表
    /// 用于判定引用AB的对象是否依然有效(还在使用未销毁)
    /// </summary>
    public List<System.WeakReference> ReferenceOwnerList
    {
        get
        {
            return mReferenceOwnerList;
        }
    }
    private List<System.WeakReference> mReferenceOwnerList;

    /// <summary>
    /// 已加载的Asset映射map
    /// Key为Asset名字，Value为对应Asset对象
    /// </summary>
    private Dictionary<string, UnityEngine.Object> mLoadedAssetMap;

    public AssetBundleInfo()
    {
        Bundle = null;
        AssetBundleName = string.Empty;
        LastUsedTime = 0.0f;
        mIsReady = false;
        mIsAllAssetLoaded = false;
        mDepAssetBundleInfoSets = new HashSet<AssetBundleInfo>();
        mRefCount = 0;
        mReferenceOwnerList = new List<System.WeakReference>();
        mLoadedAssetMap = new Dictionary<string, UnityEngine.Object>();
        onUnloadedCallback = null;
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
    public GameObject instantiateAsset(string assetname)
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
            ResourceLogger.logErr(string.Format("AB:{0}里加载GameObject Asset:{1}失败!", AssetBundleName, assetname));
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
    public T getAsset<T>(UnityEngine.Object owner, string assetname) where T : UnityEngine.Object
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
                ResourceLogger.logWar(string.Format("AB : {0}里不存在Asset : {1}，获取Asset失败!", AssetBundleName, assetname));
                return null;
            }
        }
        else
        {
            ResourceLogger.logErr(string.Format("不能绑定Asset到空对象上!加载AB:{0} Asset:{1}失败!", AssetBundleName, assetname));
            return null;
        }
    }

    /// <summary>
    /// 添加引用，引用计数+1
    /// </summary>
    public void retain()
    {
        mRefCount++;
    }

    /// <summary>
    /// 释放引用，引用计数-1
    /// </summary>
    public void release()
    {
        mRefCount = Mathf.Max(0, mRefCount - 1);
    }

    /// <summary>
    /// 释放AB
    /// </summary>
    public void dispose()
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
        if (onUnloadedCallback != null)
        {
            onUnloadedCallback(this);
        }
        onUnloadedCallback = null;
    }

    /// <summary>
    /// 更新最近使用时间
    /// </summary>
    public void updateLastUsedTime()
    {
        LastUsedTime = Time.time;
    }

    #region Debug
    /// <summary>
    /// 打印当前AB所有使用者信息以及索引计数(开发用)
    /// </summary>
    public void printAllOwnersNameAndRefCount()
    {
        ResourceLogger.log(string.Format("AB Name: {0}", AssetBundleName));
        ResourceLogger.log(string.Format("Ref Count: {0}", mRefCount));
        if (mReferenceOwnerList.Count == 0)
        {
            ResourceLogger.log("Owners Name : None");
        }
        else
        {
            ResourceLogger.log("Owners Name :");
            for (int i = 0, length = mReferenceOwnerList.Count; i < length; i++)
            {
                if (mReferenceOwnerList[i].Target != null)
                {
                    ResourceLogger.log(string.Format("owner[{0}] : {1}", i, mReferenceOwnerList[i].Target.ToString()));
                }
            }
        }
        ResourceLogger.log(string.Format("Last Used Time: {0}", LastUsedTime));
    }

    /// <summary>
    /// 获取AssetBundle使用的详细描述信息
    /// </summary>
    /// <returns></returns>
    public string getAssetBundleInfoDes()
    {
        var abides = string.Empty;
        abides += string.Format("AB Name: {0}\n", AssetBundleName);
        abides += string.Format("Ref Count: {0}\n", mRefCount);
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
    public void loadAllAsset<T>() where T : UnityEngine.Object
    {
        if (mIsReady)
        {
            if (!mIsAllAssetLoaded)
            {
                var allassets = Bundle.LoadAllAssets<T>();
                foreach (var asset in allassets)
                {
                    // mLoadedAssetMap.Add(asset.name.ToLower(), asset);
                    mLoadedAssetMap[asset.name.ToLower()] = asset;

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
    public T loadAsset<T>(string assetname) where T : UnityEngine.Object
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
                    ResourceLogger.logErr(string.Format("AB : {0}资源丢失，不存在！", AssetBundleName));
                    return null;
                }
                else
                {
                    // 图集需要全部加载才能加载到指定Sprite
                    if (typeof(T) == typeof(Sprite))
                    {
                        if (!mIsAllAssetLoaded)
                        {
                            var allassets = Bundle.LoadAllAssets();
                            // foreach (var asset in allassets)
                            var count = allassets.Length;
                            for (int i = count - 1; i >= 0; i--)
                            {
                                //Note:
                                //只存储图集的Sprite Asset，不存储Texture2D
                                //现发现Sprite Asset有和Texture2D同名的情况存在
                                //TODO：解决Sprite Asset和Texture2D同名问题，满足可加载使用图集的Texture2D Asset
                                //暂时未存储Texture2D，要访问图集Texture2D可以通过Sprite.texture的形式访问
                                var asset = allassets[i];
                                if (mLoadedAssetMap.ContainsKey(asset.name.ToLower()))
                                {
                                    continue;
                                }
                                if (asset is Sprite)
                                {
                                    mLoadedAssetMap.Add(asset.name.ToLower(), asset);
                                }
                                /*
                                else if (asset is Texture2D) //修改为同时存储 Sprite 和存储Texture2D对应的Sprite
                                {
                                    mLoadedAssetMap.Add(asset.name.ToLower(), (asset as Texture2D).toSprite());
                                }
                                */
                            }
                            mIsAllAssetLoaded = true;
                        }

                        if (mLoadedAssetMap.ContainsKey(assetname))
                        {
                            return mLoadedAssetMap[assetname] as T;
                        }
                        else
                        {
                            return null;
                        }
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
                            ResourceLogger.logErr(string.Format("AB:{0}里找不到Asset:{1}资源!", AssetBundleName, assetname));
                            return null;
                        }
                    }
                }
            }
        }
        else
        {
            ResourceLogger.logErr(string.Format("异常状态，AB资源:{0}未就绪就请求Asset资源:{1}", AssetBundleName, assetname));
            return null;
        }
    }

    /// <summary>
    /// 为AB添加指定owner的引用
    /// 所有owner都销毁则ab引用计数归零可回收
    /// </summary>
    /// <param name="owner"></param>
    private void retainOwner(UnityEngine.Object owner)
    {
        if (owner == null)
        {
            ResourceLogger.logErr(string.Format("引用对象不能为空!无法为AB:{0}添加引用!", AssetBundleName));
            return;
        }

        foreach (var referenceowner in mReferenceOwnerList)
        {
            if (owner.Equals(referenceowner))
            {
                return;
            }
        }

        System.WeakReference wr = new System.WeakReference(owner);
        mReferenceOwnerList.Add(wr);
    }

    /// <summary>
    /// 获取AB有效的引用对象计数
    /// </summary>
    /// <returns></returns>
    private int updateOwnerReference()
    {
        for (int i = 0; i < mReferenceOwnerList.Count; i++)
        {
            UnityEngine.Object o = (UnityEngine.Object)mReferenceOwnerList[i].Target;
            if (!o)
            {
                mReferenceOwnerList.RemoveAt(i);
                i--;
            }
        }
        return mReferenceOwnerList.Count;
    }

    /// <summary>
    /// 卸载AB(AssetBundle.Unload(true)的形式)
    /// </summary>
    private void unloadAssetBundle()
    {
        ResourceLogger.log(string.Format("卸载AB:{0}", AssetBundleName));
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
        AssetBundleName = string.Empty;
        LastUsedTime = 0.0f;
        mIsReady = false;
        mIsAllAssetLoaded = false;
        mDepAssetBundleInfoSets.Clear();
        mRefCount = 0;
        mReferenceOwnerList.Clear();
        mLoadedAssetMap.Clear();
        onUnloadedCallback = null;
    }
}
