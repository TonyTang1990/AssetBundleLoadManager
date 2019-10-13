/*
 * Description:             AbstractResourceInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/04
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AbstractResourceInfo.cs
/// 资源加载信息抽象类
/// </summary>
public abstract class AbstractResourceInfo {
    
    /// <summary>
    /// 资源卸载委托
    /// </summary>
    /// <param name="abi"></param>
    public delegate void OnResourceUnloadedHandler(AbstractResourceInfo abi);

    /// <summary> 资源卸载回调(用于通知资源对应Loader切换状态) /// </summary>
    public OnResourceUnloadedHandler onResourceUnloadedCallback;

    /// <summary> 加载任务对应的资源名字 /// </summary>
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
        protected set;
    }

    /// <summary> 是否不再有人使用 /// </summary>
    public bool IsUnsed
    {
        get { return mIsReady && RefCount <= 0 && updateOwnerReference() == 0; }
    }

    /// <summary> 资源是否已经加载完成 /// </summary>
    public bool mIsReady
    {
        get;
        set;
    }

    /// <summary> 引用计数 /// </summary>
    public int RefCount
    {
        get;
        protected set;
    }

    /// <summary>
    /// 资源里的所有Asset是否已经加载完(仅用于判定通过AssetBundle.LoadAllAssets()或者AssetBundle.LoadAllAssetsAsync()或者AssetDatabase.LoadAllAssetsAtPath的加载方式)
    /// </summary>
    protected bool mIsAllAssetLoaded;
    
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
    protected List<System.WeakReference> mReferenceOwnerList;

    /// <summary>
    /// 已加载的Asset映射map
    /// Key为Asset名字，Value为对应Asset对象
    /// </summary>
    protected Dictionary<string, UnityEngine.Object> mLoadedAssetMap;

    /// <summary>
    /// 加载所有Asset(比如Shader预加载)
    /// Note:
    /// 加载图集使用loadAllAsset<Sprite>()或者loadAsset<Sprite>
    /// </summary>
    /// <param name="assetname"></param>
    /// <returns></returns>
    public abstract void loadAllAsset<T>() where T : UnityEngine.Object;

    /// <summary>
    /// 加载指定Asset(可用上层访问Asset)
    /// Note:
    /// 因为没有绑定对象，仅用于临时访问Asset数据
    /// 访问过后无人引用的话会被回收
    /// </summary>
    /// <param name="assetname"></param>
    /// <returns></returns>
    public abstract T loadAsset<T>(string assetname) where T : UnityEngine.Object;

    /// <summary>
    /// 实例化指定Asset(上层获取并绑定Asset实例化GameObject对象接口)
    /// </summary>
    /// <param name="assetname"></param>
    /// <returns></returns>
    public abstract GameObject instantiateAsset(string assetname);

    /// <summary>
    /// 获取并绑定指定Asste资源(上层获取并绑定特定类型Asset的接口)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="owner"></param>
    /// <param name="assetname"></param>
    /// <returns></returns>
    public abstract T getAsset<T>(UnityEngine.Object owner, string assetname) where T : UnityEngine.Object;

    /// <summary>
    /// 添加引用，引用计数+1
    /// </summary>
    public void retain()
    {
        RefCount++;
    }

    /// <summary>
    /// 释放引用，引用计数-1
    /// </summary>
    public void release()
    {
        RefCount = Mathf.Max(0, RefCount - 1);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public abstract void dispose();

    /// <summary>
    /// 更新最近使用时间
    /// </summary>
    public void updateLastUsedTime()
    {
        LastUsedTime = Time.time;
    }

    /// <summary>
    /// 移除指定拥有者绑定(用于解决上层绑定对象一直存在导致资源无法释放的问题)
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public bool releaseOwner(UnityEngine.Object owner)
    {
        if (owner == null)
        {
            ResourceLogger.logErr(string.Format("引用对象不能为空!无法为资源:{0}解除绑定!", AssetBundleName));
            return false;
        }

        var ownerindex = mReferenceOwnerList.FindIndex((ow) => ow.Target.Equals(owner));
        if (ownerindex != -1)
        {
            ResourceLogger.log(string.Format("资源:{0}找到指定绑定对象:{1},解除绑定并减少资源索引计数!", AssetBundleName, owner));
            mReferenceOwnerList.RemoveAt(ownerindex);
            release();
            return true;
        }
        else
        {
            ResourceLogger.log(string.Format("资源:{0}找不到指定绑定对象:{1},解除绑定失败!", AssetBundleName, owner));
            return false;
        }
    }

    /// <summary>
    /// 为AB添加指定owner的引用
    /// 所有owner都销毁则ab引用计数归零可回收
    /// </summary>
    /// <param name="owner"></param>
    protected void retainOwner(UnityEngine.Object owner)
    {
        if (owner == null)
        {
            ResourceLogger.logErr(string.Format("引用对象不能为空!无法为资源:{0}添加引用!", AssetBundleName));
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
    protected int updateOwnerReference()
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

    #region 辅助调试工具
    /// <summary>
    /// 打印当前AB所有使用者信息以及索引计数(开发用)
    /// </summary>
    public void printAllOwnersNameAndRefCount()
    {
        ResourceLogger.log(string.Format("AB Name: {0}", AssetBundleName));
        ResourceLogger.log(string.Format("Ref Count: {0}", RefCount));
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
    #endregion
}