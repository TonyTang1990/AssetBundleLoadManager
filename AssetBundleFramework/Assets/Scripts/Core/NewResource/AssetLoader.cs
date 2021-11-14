/*
 * Description:             AssetLoader.cs
 * Author:                  TONYTANG
 * Create Date:             2021/10/13
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetLoader.cs
    /// Asset加载器基类
    /// </summary>
    public abstract class AssetLoader : Loadable
    {
        /// <summary>
        /// Asset类型
        /// </summary>
        public Type AssetType
        {
            get;
            protected set;
        }

        /// <summary>
        /// Asset信息
        /// </summary>
        protected AssetInfo mAssetInfo;

        /// <summary>
        /// Asset资源加载完成逻辑层回调
        /// </summary>
        protected Action<AssetLoader> mLoadAssetCompleteCallBack;

        /// <summary>
        /// Asset资源加载完成逻辑回调Map<请求UID,逻辑回调>
        /// </summary>
        protected Dictionary<int, Action<AssetLoader>> mLoadAssetCompleteCallBackMap;

        /// <summary>
        /// Asset异步请求
        /// </summary>
        protected AssetBundleRequest mAssetAsyncRequest;

        public AssetLoader() : base()
        {
            AssetType = null;
            mAssetInfo = null;
            mLoadAssetCompleteCallBackMap = new Dictionary<int, Action<AssetLoader>>();
            mAssetAsyncRequest = null;
        }

        public override void onCreate()
        {
            base.onCreate();
            AssetType = null;
            mAssetInfo = null;
            mLoadAssetCompleteCallBack = null;
            mLoadAssetCompleteCallBackMap.Clear();
            mAssetAsyncRequest = null;
        }

        public override void onDispose()
        {
            base.onDispose();
            AssetType = null;
            mAssetInfo = null;
            mLoadAssetCompleteCallBack = null;
            mLoadAssetCompleteCallBackMap.Clear();
            mAssetAsyncRequest = null;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="assetPath">Asset路径</param>
        /// <param name="assetType">Asset类型</param>
        /// <param name="assetInfo">Asset信息</param>
        /// <param name="loadType">加载类型</param>
        /// <param name="loadMethod">加载方法</param>
        public void init(string assetPath, Type assetType, AssetInfo assetInfo, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            ResourcePath = assetPath;
            AssetType = assetType;
            mAssetInfo = assetInfo;
            LoadType = loadType;
            LoadMethod = loadMethod;
            // 创建加载器时就添加相关Asset计数，确保资源加载管理正确
            // 后续加载取消时会返还对应计数
            mAssetInfo.retain();
        }

        /// <summary>
        /// 获取指定Asset(会加索引计数)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T getAsset<T>() where T : UnityEngine.Object
        {
            if (!IsDone)
            {
                loadImmediately();
            }
            var asset = mAssetInfo.getResource<T>();
            mAssetInfo.retain();
            return asset;
        }

        /// <summary>
        /// 获取指定Asset(不会加索引计数)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T obtainAsset<T>() where T : UnityEngine.Object
        {
            if (!IsDone)
            {
                loadImmediately();
            }
            var asset = mAssetInfo.getResource<T>();
            return asset;
        }

        /// <summary>
        /// 为Asset添加指定owner的引用并返回该Asset
        /// 所有owner都销毁且所属ab引用计数归零可回收
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="owner"></param>
        /// <returns></returns>
        public T bindAsset<T>(UnityEngine.Object owner) where T : UnityEngine.Object
        {
            if (!IsDone)
            {
                loadImmediately();
            }
            T asset = mAssetInfo.getResource<T>();
            mAssetInfo.retainOwner(owner);
            return asset;
        }

        /// <summary>
        /// 为Asset添加指定owner的引用并返回该Asset
        /// 所有owner都销毁且所属ab引用计数归零可回收
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="owner"></param>
        /// <returns></returns>
        public T releaseAsset<T>(UnityEngine.Object owner) where T : UnityEngine.Object
        {
            if (!IsDone)
            {
                loadImmediately();
            }
            T asset = mAssetInfo.getResource<T>();
            mAssetInfo.releaseOwner(owner);
            return asset;
        }

        /// <summary>
        /// 响应资源加载取消(处理资源加载取消的情况)
        /// </summary>
        protected override void onCancel()
        {
            base.onCancel();
        }

        /// <summary>
        /// 响应加载完成
        /// </summary>
        protected override void onComplete()
        {
            base.onComplete();
            // 修改资源准备状态(表示资源准备完成，可以获取或者判定卸载)
            mAssetInfo.IsReady = true;

            // 上层多个加载逻辑回调，在完成后根据调用getAsset或bindAsset情况去添加计数和绑定
            // 针对当前Asset的加载的基础计数已经在加载之前就完成计数的添加了，这里只需返回主Asset和主AB的提前计数即可
            mAssetInfo.release();

            mAssetAsyncRequest = null;
            mLoadAssetCompleteCallBackMap.Clear();

            // 通知上层Asset加载完成
            mLoadAssetCompleteCallBack?.Invoke(this);
            mLoadAssetCompleteCallBack = null;

            // Loader在只允许在卸载后回收，上层用户都是面向Loader级别去访问AB和Asset
            // AB加载完成后，AssetBundleLoader的任务就完成了，回收重用
            //ObjectPool.Singleton.push<AssetBundleLoader>(this);
        }

        /// <summary>
        /// 添加Asset加载完成逻辑回调
        /// </summary>
        /// <param name="requestUID"></param>
        /// <param name="loadAssetCompleteCallBack"></param>
        /// <returns></returns>
        public bool addLoadAssetCompleteCallBack(int requestUID, Action<AssetLoader> loadAssetCompleteCallBack)
        {
            if (!mLoadAssetCompleteCallBackMap.ContainsKey(requestUID))
            {
                ResourceLogger.log($"绑定Asset:{ResourcePath}加载请求UID:{requestUID}成功!");
                mLoadAssetCompleteCallBackMap.Add(requestUID, loadAssetCompleteCallBack);
                if (loadAssetCompleteCallBack != null)
                {
                    mLoadAssetCompleteCallBack += loadAssetCompleteCallBack;
                }
                LoaderManager.Singleton.addAssetRequestUID(requestUID, ResourcePath);
                return true;
            }
            else
            {
                Debug.LogError($"重复绑定相同请求UID:{requestUID}回调,绑定Asset:{ResourcePath}请求回调失败!");
                return false;
            }
        }

        /// <summary>
        /// 取消指定请求UID请求
        /// </summary>
        /// <param name="requestUID"></param>
        /// <returns></returns>
        public override bool cancelRequest(int requestUID)
        {
            base.cancelRequest(requestUID);
            Action<AssetLoader> loadAssetCompleteCallBack;
            if (mLoadAssetCompleteCallBackMap.TryGetValue(requestUID, out loadAssetCompleteCallBack))
            {
                ResourceLogger.log($"Asset:{ResourcePath}取消请求UID:{requestUID}成功!");
                removeRequest(requestUID);
                if (loadAssetCompleteCallBack != null)
                {
                    mLoadAssetCompleteCallBack -= loadAssetCompleteCallBack;
                }
                // 所有请求都取消表示没人再请求此Asset了
                if (mLoadAssetCompleteCallBackMap.Count == 0)
                {
                    cancel();
                }
                return true;
            }
            else
            {
                Debug.LogError($"找不到请求UID:{requestUID}请求,取消Asset:{ResourcePath}请求失败!");
                return false;
            }
        }

        /// <summary>
        /// 清除指定UID请求
        /// </summary>
        /// <param name="requestUID"></param>
        /// <returns></returns>
        private bool removeRequest(int requestUID)
        {
            Action<AssetLoader> loadAssetCompleteCallBack;
            if (mLoadAssetCompleteCallBackMap.TryGetValue(requestUID, out loadAssetCompleteCallBack))
            {
                mLoadAssetCompleteCallBackMap.Remove(requestUID);
                LoaderManager.Singleton.removeAssetRequestUID(requestUID);
                return true;
            }
            else
            {
                Debug.LogError($"找不到请求UID:{requestUID}回调,移除Asset:{ResourcePath}请求失败!");
                return false;
            }
        }
    }
}