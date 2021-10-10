/*
 * Description:             AssetDatabaseInfo.cs
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
/// AssetDatabaseInfo.cs
/// AssetDatabase模式下资源加载信息抽象，负责资源加载后的信息管理抽象
/// Note:
/// 索引计数在这一层，但AssetDatabase模式暂时只负责计数不负责卸载
/// </summary>
public class AssetDatabaseInfo : AbstractResourceInfo, FactoryObj
{
    /// <summary>
    /// 加载的Asset路径
    /// </summary>
    public string AssetPath
    {
        get;
        set;
    }

    public AssetDatabaseInfo()
    {
        AssetBundlePath = string.Empty;
        LastUsedTime = 0.0f;
        mIsReady = false;
        mIsAllAssetLoaded = false;
        RefCount = 0;
        mReferenceOwnerList = new List<System.WeakReference>();
        mLoadedAssetMap = new Dictionary<string, UnityEngine.Object>();
        AssetPath = string.Empty;
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
            ResourceLogger.logErr(string.Format("AB:{0}里加载GameObject Asset:{1}失败!", AssetBundlePath, assetname));
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
                ResourceLogger.logWar(string.Format("AB : {0}里不存在Asset : {1}，获取Asset失败!", AssetBundlePath, assetname));
                return null;
            }
        }
        else
        {
            ResourceLogger.logErr(string.Format("不能绑定Asset到空对象上!加载AB:{0} Asset:{1}失败!", AssetBundlePath, assetname));
            return null;
        }
    }

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
                // AB打包更新后，编辑器没有加载所有Asset一说
                mIsAllAssetLoaded = true;
            }
        }
    }

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
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetPath);
                if (asset != null)
                {
                    mLoadedAssetMap.Add(assetname, asset);
                    return asset;
                }
                else
                {
                    ResourceLogger.logErr(string.Format("找不到符合类型 : {0}，资源名: {1}，Asset : {2}资源！", typeof(T).GetType(), AssetBundlePath, assetname));
                    return null;
                }
            }
        }
        else
        {
            ResourceLogger.logErr(string.Format("异常状态，AB资源:{0}未就绪就请求Asset资源:{1}", AssetBundlePath, assetname));
            return null;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public override void dispose()
    {
        // Assetdatabase模式下为了开发方便没有正确添加依赖计数，
        // 所以AssetDatabase不允许资源回收，避免错误的资源回收
        Debug.LogError("Assetdatabase模式不允许资源卸载!");

        unloadResource();

        LastUsedTime = 0.0f;

        if (onResourceUnloadedCallback != null)
        {
            onResourceUnloadedCallback(this);
        }
        onResourceUnloadedCallback = null;
    }

    /// <summary>
    /// 卸载资源(Resources.UnloadAsset遍历卸载)
    /// </summary>
    private void unloadResource()
    {
        ResourceLogger.log(string.Format("卸载资源:{0}", AssetBundlePath));
        foreach(var loadedasset in mLoadedAssetMap)
        {
            var asset = loadedasset.Value;
            if (asset is GameObject)
            {
                ResourceLogger.log(string.Format("无法通过Resources.UnloadAsset卸载GameObject : {0}资源，后续会清空后通过Resources.UnloadUnsedAsset卸载!", asset.name));
            }
            else if(asset is Component)
            {
                ResourceLogger.log(string.Format("无法通过Resources.UnloadAsset卸载Component : {0}资源，后续会清空后通过Resources.UnloadUnsedAsset卸载!", asset.name));
            }
            else
            {
                //AssetDatabase模式不支持卸载资源，
                //因为并非真实的模拟AssetBundle资源加载行为，只是单纯的把所需资源自身加载进来
                //Resources.UnloadAsset(loadedasset.Value);
                ResourceLogger.log(string.Format("假卸载资源:{0}的Asset : {1}", AssetBundlePath, loadedasset.Value.name));
            }
        }
        mLoadedAssetMap.Clear();
        mIsReady = false;
    }

    /// <summary>
    /// 回收重用
    /// </summary>
    public void recycle()
    {
        AssetBundlePath = string.Empty;
        LastUsedTime = 0.0f;
        mIsReady = false;
        mIsAllAssetLoaded = false;
        RefCount = 0;
        mReferenceOwnerList.Clear();
        mLoadedAssetMap.Clear();
        AssetPath = string.Empty;
    }
}
#endif