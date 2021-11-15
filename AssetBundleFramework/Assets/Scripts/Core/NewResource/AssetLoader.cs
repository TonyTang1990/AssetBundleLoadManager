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
        /// Asset请求信息
        /// </summary>
        protected class AssetRequestInfo : IRecycle
        {
            /// <summary>
            /// 请求Uid
            /// </summary>
            public int RequestUid
            {
                get;
                protected set;
            }

            /// <summary>
            /// 请求回调
            /// </summary>
            public Action<AssetLoader, int> RequestCallBack
            {
                get;
                protected set;
            }

            /// <summary>
            /// 初始化
            /// </summary>
            /// <param name="requestUid"></param>
            /// <param name="requestCallBack"></param>
            public void init(int requestUid, Action<AssetLoader, int> requestCallBack)
            {
                RequestUid = requestUid;
                RequestCallBack = requestCallBack;
            }

            public void onCreate()
            {
                RequestUid = 0;
                RequestCallBack = null;
            }

            public void onDispose()
            {
                RequestUid = 0;
                RequestCallBack = null;
            }
        }

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
        /// Asset请求信息列表(为了确保逻辑层面的回调顺序一致性采用List)
        /// </summary>
        protected List<AssetRequestInfo> mRequestInfoList;

        /// <summary>
        /// Asset资源请求UID和请求信息Map<请求UID,请求信息>
        /// </summary>
        protected Dictionary<int, AssetRequestInfo> mRequestUidAndInfoMap;

        /// <summary>
        /// Asset异步请求
        /// </summary>
        protected AssetBundleRequest mAssetAsyncRequest;

        public AssetLoader() : base()
        {
            AssetType = null;
            mAssetInfo = null;
            mRequestInfoList = new List<AssetRequestInfo>();
            mRequestUidAndInfoMap = new Dictionary<int, AssetRequestInfo>();
            mAssetAsyncRequest = null;
        }

        public override void onCreate()
        {
            base.onCreate();
            AssetType = null;
            mAssetInfo = null;
            mRequestInfoList.Clear();
            mRequestUidAndInfoMap.Clear();
            mAssetAsyncRequest = null;
        }

        public override void onDispose()
        {
            base.onDispose();
            AssetType = null;
            mAssetInfo = null;
            mRequestInfoList.Clear();
            mRequestUidAndInfoMap.Clear();
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
        /// 获取索引计数
        /// </summary>
        /// <returns></returns>
        public int getReferenceCount()
        {
            return mAssetInfo != null ? mAssetInfo.RefCount : 0;
        }

        /// <summary>
        /// 获取索引计数
        /// </summary>
        /// <returns></returns>
        public int getOwnerNumber()
        {
            return mAssetInfo != null ? mAssetInfo.ReferenceOwnerList.Count : 0;
        }

        /// <summary>
        /// 响应资源加载
        /// </summary>
        protected override void onLoad()
        {
            base.onLoad();
            ResourceLogger.log($"开始加载Asset:{ResourcePath}!");
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

            mAssetAsyncRequest = null;
            // 通知上层Asset加载完成
            for(int i = 0; i < mRequestInfoList.Count; i++)
            {
                mRequestInfoList[i].RequestCallBack?.Invoke(this, mRequestInfoList[i].RequestUid);
                removeRequest(mRequestInfoList[i].RequestUid);
                i--;
            }
            mRequestInfoList.Clear();
            mRequestUidAndInfoMap.Clear();

            // 上层多个加载逻辑回调，在完成后根据调用getAsset或bindAsset情况去添加计数和绑定
            // 针对当前Asset的加载的基础计数已经在加载之前就完成计数的添加了，这里只需返回主Asset和主AB的提前计数即可
            mAssetInfo.release();
        }

        /// <summary>
        /// 添加Asset加载请求完成逻辑回调
        /// </summary>
        /// <param name="requestUID"></param>
        /// <param name="loadAssetCompleteCallBack"></param>
        /// <returns></returns>
        public bool addRequest(int requestUID, Action<AssetLoader, int> loadAssetCompleteCallBack)
        {
            if (!mRequestUidAndInfoMap.ContainsKey(requestUID))
            {
                ResourceLogger.log($"绑定Asset:{ResourcePath}加载请求UID:{requestUID}成功!");
                var assetRequestInfo = ObjectPool.Singleton.pop<AssetRequestInfo>();
                assetRequestInfo.init(requestUID, loadAssetCompleteCallBack);
                mRequestInfoList.Add(assetRequestInfo);
                mRequestUidAndInfoMap.Add(requestUID, assetRequestInfo);
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
            if(removeRequest(requestUID))
            {
                ResourceLogger.log($"Asset:{ResourcePath}取消请求UID:{requestUID}成功!");
                // 所有请求都取消表示没人再请求此Asset了
                if (mRequestUidAndInfoMap.Count == 0)
                {
                    cancel();
                }
                return true;
            }
            else
            {
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
            AssetRequestInfo assetRequestInfo;
            if (mRequestUidAndInfoMap.TryGetValue(requestUID, out assetRequestInfo))
            {
                ResourceLogger.log($"Asset:{ResourcePath}移除请求UID:{requestUID}成功!");
                mRequestInfoList.Remove(assetRequestInfo);
                mRequestUidAndInfoMap.Remove(requestUID);
                LoaderManager.Singleton.removeAssetRequestUID(requestUID);
                ObjectPool.Singleton.push<AssetRequestInfo>(assetRequestInfo);
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