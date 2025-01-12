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

            public void OnCreate()
            {
                RequestUid = 0;
                RequestCallBack = null;
            }

            public void OnDispose()
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
        /// 拥有资源是否不再使用
        /// </summary>
        public bool IsUnsed
        {
            get
            {
                return mAssetInfo != null ? mAssetInfo.IsUnsed : true;
            }
        }

        /// <summary>
        /// Asset信息
        /// </summary>
        protected AssetInfo mAssetInfo;

        /// <summary>
        /// Asset请求UID列表(为了确保逻辑层面的回调顺序一致性采用List)
        /// Note:
        /// 只添加不移除，是否有效看mRequestUidAndInfoMap里是否有对应请求信息
        /// 避免在回调过程中取消删除出问题
        /// </summary>
        protected List<int> mRequestUIDList;

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
            mRequestUIDList = new List<int>();
            mRequestUidAndInfoMap = new Dictionary<int, AssetRequestInfo>();
            mAssetAsyncRequest = null;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            AssetType = null;
            mAssetInfo = null;
            mRequestUIDList.Clear();
            mRequestUidAndInfoMap.Clear();
            mAssetAsyncRequest = null;
        }

        public override void OnDispose()
        {
            base.OnDispose();
            AssetType = null;
            mAssetInfo = null;
            mRequestUIDList.Clear();
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
        public void Init(string assetPath, Type assetType, AssetInfo assetInfo, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            ResourcePath = assetPath;
            AssetType = assetType;
            mAssetInfo = assetInfo;
            LoadType = loadType;
            LoadMethod = loadMethod;
            // 创建加载器时就添加相关Asset计数，确保资源加载管理正确
            // 后续加载取消时会返还对应计数
            mAssetInfo.Retain();
        }

        /// <summary>
        /// 获取指定Asset(会加索引计数)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAsset<T>() where T : UnityEngine.Object
        {
            if (!IsDone)
            {
                LoadImmediately();
            }
            var asset = mAssetInfo.GetResource<T>();
            mAssetInfo.Retain();
            return asset;
        }

        /// <summary>
        /// 获取指定Asset(不会加索引计数)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ObtainAsset<T>() where T : UnityEngine.Object
        {
            if (!IsDone)
            {
                LoadImmediately();
            }
            var asset = mAssetInfo.GetResource<T>();
            return asset;
        }

        /// <summary>
        /// 为Asset添加指定owner的引用并返回该Asset
        /// 所有owner都销毁且所属ab引用计数归零可回收
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="owner"></param>
        /// <returns></returns>
        public T BindAsset<T>(UnityEngine.Object owner) where T : UnityEngine.Object
        {
            if (!IsDone)
            {
                LoadImmediately();
            }
            T asset = mAssetInfo.GetResource<T>();
            mAssetInfo.RetainOwner(owner);
            return asset;
        }

        /// <summary>
        /// 添加资源引用，引用计数+1(用于不需要获取指定Asset直接添加计数的情况)
        /// </summary>
        public void RetainAsset()
        {
            mAssetInfo.Retain();
        }

        /// <summary>
        /// 减少Asset资源资源计数
        /// 所有owner都销毁且自身索引计数归零并且所属AssetBundle引用计数归零+绑定对象为空时可回收
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="owner"></param>
        /// <returns></returns>
        public void ReleaseAsset()
        {
            mAssetInfo.Release();
        }

        /// <summary>
        /// 解除Asset资源的指定Owner对象绑定
        /// 所有owner都销毁且所属ab引用计数归零可回收
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="owner"></param>
        /// <returns></returns>
        public bool ReleaseOwner(UnityEngine.Object owner)
        {
            return mAssetInfo.ReleaseOwner(owner);
        }

        /// <summary>
        /// 获取索引计数
        /// </summary>
        /// <returns></returns>
        public int GetReferenceCount()
        {
            return mAssetInfo != null ? mAssetInfo.RefCount : 0;
        }

        /// <summary>
        /// 获取索引计数
        /// </summary>
        /// <returns></returns>
        public int GetOwnerNumber()
        {
            return mAssetInfo != null ? mAssetInfo.ReferenceOwnerList.Count : 0;
        }

        /// <summary>
        /// 响应资源加载
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();
            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}开始加载Asset:{ResourcePath}!");
        }

        /// <summary>
        /// 响应资源加载取消(处理资源加载取消的情况)
        /// </summary>
        protected override void OnCancel()
        {
            base.OnCancel();
        }

        /// <summary>
        /// 响应加载完成
        /// </summary>
        protected override void OnComplete()
        {
            base.OnComplete();
            // 修改资源准备状态(表示资源准备完成，可以获取或者判定卸载)
            mAssetInfo.IsReady = true;

            mAssetAsyncRequest = null;
            // 通知上层Asset加载完成
            for(int i = 0; i < mRequestUIDList.Count; i++)
            {
                var requestUID = mRequestUIDList[i];
                AssetRequestInfo assetRequestInfo;
                if(mRequestUidAndInfoMap.TryGetValue(requestUID, out assetRequestInfo))
                {
                    assetRequestInfo.RequestCallBack?.Invoke(this, requestUID);
                    removeRequest(requestUID);
                }
            }
            mRequestUIDList.Clear();
            mRequestUidAndInfoMap.Clear();

            // 上层多个加载逻辑回调，在完成后根据调用getAsset或bindAsset情况去添加计数和绑定
            // 针对当前Asset的加载的基础计数已经在加载之前就完成计数的添加了，这里只需返回主Asset和主AB的提前计数即可
            mAssetInfo.Release();
        }

        /// <summary>
        /// 添加Asset加载请求完成逻辑回调
        /// </summary>
        /// <param name="requestUID"></param>
        /// <param name="loadAssetCompleteCallBack"></param>
        /// <returns></returns>
        public bool AddRequest(int requestUID, Action<AssetLoader, int> loadAssetCompleteCallBack)
        {
            if (!mRequestUidAndInfoMap.ContainsKey(requestUID))
            {
                ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}绑定Asset:{ResourcePath}加载请求UID:{requestUID}成功!");
                var assetRequestInfo = ObjectPool.Singleton.Pop<AssetRequestInfo>();
                assetRequestInfo.init(requestUID, loadAssetCompleteCallBack);
                mRequestUIDList.Add(requestUID);
                mRequestUidAndInfoMap.Add(requestUID, assetRequestInfo);
                LoaderManager.Singleton.AddAssetRequestUID(requestUID, ResourcePath);
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
        public override bool CancelRequest(int requestUID)
        {
            base.CancelRequest(requestUID);
            if(RemoveRequest(requestUID))
            {
                ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}Asset:{ResourcePath}取消请求UID:{requestUID}成功!");
                // 所有请求都取消表示没人再请求此Asset了
                if (mRequestUidAndInfoMap.Count == 0)
                {
                    Cancel();
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
        private bool RemoveRequest(int requestUID)
        {
            AssetRequestInfo assetRequestInfo;
            if (mRequestUidAndInfoMap.TryGetValue(requestUID, out assetRequestInfo))
            {
                ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}Asset:{ResourcePath}移除请求UID:{requestUID}成功!");
                mRequestUidAndInfoMap.Remove(requestUID);
                LoaderManager.Singleton.RemoveAssetRequestUID(requestUID);
                ObjectPool.Singleton.Push<AssetRequestInfo>(assetRequestInfo);
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