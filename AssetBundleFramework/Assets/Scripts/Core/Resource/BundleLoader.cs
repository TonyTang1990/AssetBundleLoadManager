/*
 * Description:             BundleLoader.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/30
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// BundleLoader.cs
    /// AssetBundle加载器基类(负责自身AB和依赖AB的加载)
    /// </summary>
    public abstract class BundleLoader : Loadable
    {
        /// <summary>
        /// Bundle请求信息
        /// </summary>
        protected class BundleRequestInfo : IRecycle
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
            public Action<BundleLoader, int> RequestCallBack
            {
                get;
                protected set;
            }

            /// <summary>
            /// 初始化
            /// </summary>
            /// <param name="requestUid"></param>
            /// <param name="requestCallBack"></param>
            public void init(int requestUid, Action<BundleLoader, int> requestCallBack)
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
        /// 依赖的AB路径数组
        /// </summary>
        public string[] DepABPaths
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
                return AssetBundleInfo != null ? AssetBundleInfo.IsUnsed : true;
            }
        }

        /// <summary>
        /// AssetBundle信息
        /// </summary>
        public AssetBundleInfo AssetBundleInfo
        {
            get;
            protected set;
        }

        /// <summary>
        /// 当前AB依赖的AB对应的AB信息列表(用于构建当前AssetBundleInfo)
        /// </summary>
        public List<AssetBundleInfo> DepAssetBundleInfoList
        {
            get;
            protected set;
        }

        /// <summary>
        /// 是否所有的AB都加载完成
        /// </summary>
        protected bool IsAllABLoaded
        {
            get
            {
                return mLoadUnCompleteAssetBundlePathMap.Count == 0;
            }
        }

        /// <summary>
        /// 加载已完成数量
        /// </summary>
        public int LoadCompletedAssetBundleNumer
        {
            get
            {
                return mAllRequiredAssetBundleNumber - mLoadUnCompleteAssetBundlePathMap.Count;
            }
        }

        /// <summary>
        /// 所有需要的AB总数量
        /// </summary>
        protected int mAllRequiredAssetBundleNumber;

        /// <summary>
        /// 加载完成的AssetBundle路径Map(Key为加载完成AssetBundle路径,Value为true)
        /// Note:
        /// 同步加载的AssetBundle的加载器为null(因为加载在获取AssetBundle加载器之前就完成了)
        /// </summary>
        protected Dictionary<string, bool> mLoadUnCompleteAssetBundlePathMap;

        /// <summary>
        /// AssetBundle路径和加载器Map(Key为加载完成AssetBundle路径,Value为请求AssetBundle加载器)
        /// </summary>
        protected Dictionary<string, BundleLoader> mAssetBundlePathAndBundleLoaderMap;

        /// <summary>
        /// Bundle请求信息列表(为了确保逻辑层面的回调顺序一致性采用List)
        /// </summary>
        protected List<BundleRequestInfo> mRequestInfoList;

        /// <summary>
        /// AB资源请求Uid和请求信息Map<请求UID,请求信息>
        /// </summary>
        protected Dictionary<int, BundleRequestInfo> mRequestUidAndInfoMap;

        /// <summary>
        /// AssetBundle异步请求
        /// </summary>
        protected AssetBundleCreateRequest mAssetBundleAsyncRequest;

        public BundleLoader() : base()
        {
            DepAssetBundleInfoList = new List<AssetBundleInfo>();
            mLoadUnCompleteAssetBundlePathMap = new Dictionary<string, bool>();
            mAssetBundlePathAndBundleLoaderMap = new Dictionary<string, BundleLoader>();
            mRequestInfoList = new List<BundleRequestInfo>();
            mRequestUidAndInfoMap = new Dictionary<int, BundleRequestInfo>();
        }

        public override void onCreate()
        {
            base.onCreate();
            DepABPaths = null;
            AssetBundleInfo = null;
            DepAssetBundleInfoList.Clear();
            mAllRequiredAssetBundleNumber = 0;
            mLoadUnCompleteAssetBundlePathMap.Clear();
            mAssetBundlePathAndBundleLoaderMap.Clear();
            mRequestInfoList.Clear();
            mRequestUidAndInfoMap.Clear();
        }

        public override void onDispose()
        {
            base.onDispose();
            DepABPaths = null;
            AssetBundleInfo = null;
            DepAssetBundleInfoList.Clear();
            mAllRequiredAssetBundleNumber = 0;
            mLoadUnCompleteAssetBundlePathMap.Clear();
            mAssetBundlePathAndBundleLoaderMap.Clear();
            mRequestInfoList.Clear();
            mRequestUidAndInfoMap.Clear();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="assetBundlePath">AB路径</param>
        /// <param name="loadType">加载类型</param>
        /// <param name="depABPaths">依赖AB路径组</param>
        /// <param name="loadMethod">加载方法</param>
        public void init(string assetBundlePath, AssetBundleInfo assetBundleInfo, string[] depABPaths, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            ResourcePath = assetBundlePath;
            AssetBundleInfo = assetBundleInfo;
            DepABPaths = depABPaths;
            LoadType = loadType;
            LoadMethod = loadMethod;
            mLoadUnCompleteAssetBundlePathMap.Add(ResourcePath, true);
            mAllRequiredAssetBundleNumber = DepABPaths != null ? DepABPaths.Length + 1 : 1;
            // 创建加载器时就添加相关AssetBundle计数，确保资源加载管理正确
            // 后续加载取消时会返还对应计数
            AssetBundleInfo.retain();
            // 依赖的AB的计数一开始也要添加，在加载完成后无需返还(作为依赖计数统计一次即可)
            AssetBundleInfo depAssetBundleInfo;
            // 等于null的话意味着上层已经出问题了，这里只是避免阻断
            if (DepABPaths != null)
            {
                for (int i = 0, length = DepABPaths.Length; i < length; i++)
                {
                    mLoadUnCompleteAssetBundlePathMap.Add(DepABPaths[i], true);
                    depAssetBundleInfo = ResourceModuleManager.Singleton.CurrentResourceModule.getOrCreateAssetBundleInfo(DepABPaths[i], ResourceLoadType.NormalLoad);
                    DepAssetBundleInfoList.Add(depAssetBundleInfo);
                    depAssetBundleInfo.retain();
                }
            }
        }

        /// <summary>
        /// 获取指定AssetBundle(加计数)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract AssetBundle getAssetBundle();

        /// <summary>
        /// 获取指定AssetBundle(不加计数)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract AssetBundle obtainAssetBundle();

        /// <summary>
        /// 为AssetBundle添加指定owner的引用并返回该Asset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="owner"></param>
        /// <returns></returns>
        public abstract AssetBundle bindAssetBundle(UnityEngine.Object owner);

        /// <summary>
        /// 添加资源引用，引用计数+1(用于不需要获取指定AssetBundle直接添加计数的情况)
        /// </summary>
        public void retainAssetBundle()
        {
            AssetBundleInfo.retain();
        }

        /// <summary>
        /// 减少AssetBundle资源资源计数
        /// 所有owner都销毁且自身索引计数归零并且所属所有Asset都引用计数归零+绑定对象为空时可回收
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="owner"></param>
        /// <returns></returns>
        public void releaseAssetBundle()
        {
            AssetBundleInfo.release();
        }

        /// <summary>
        /// 获取索引计数
        /// </summary>
        /// <returns></returns>
        public int getReferenceCount()
        {
            return AssetBundleInfo != null ? AssetBundleInfo.RefCount : 0;
        }

        /// <summary>
        /// 获取索引计数
        /// </summary>
        /// <returns></returns>
        public int getOwnerNumber()
        {
            return AssetBundleInfo != null ? AssetBundleInfo.ReferenceOwnerList.Count : 0;
        }

        /// <summary>
        /// 响应资源加载
        /// </summary>
        protected override void onLoad()
        {
            base.onLoad();
            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}开始加载AssetBundle:{ResourcePath}");
            // Note:
            // 依赖AB一律采取Normal加载方式
            if (LoadMethod == ResourceLoadMethod.Sync)
            {
                if (mAssetBundlePathAndBundleLoaderMap.Count == 0)
                {
                    loadAssetBundleSync();
                    BundleLoader assetBundleLoader;
                    int assetBundleLoadUID;
                    // 等于null的话意味着上层已经出问题了，这里只是避免阻断
                    if (DepABPaths != null)
                    {
                        for (int i = 0, length = DepABPaths.Length; i < length; i++)
                        {
                            assetBundleLoadUID = ResourceModuleManager.Singleton.requstAssetBundleSync(DepABPaths[i], out assetBundleLoader, onAssetBundleLoadComplete, ResourceLoadType.NormalLoad);
                            mAssetBundlePathAndBundleLoaderMap.Add(DepABPaths[i], assetBundleLoader);
                        }
                    }
                }
                else
                {
                    var unCompleteAssetBundlePathList = mLoadUnCompleteAssetBundlePathMap.Keys.ToList();
                    // 有异步或者动态下载未加载完的AssetBundle
                    // 此时异步加载的需要转同步加载
                    foreach (var uncompleteABPath in unCompleteAssetBundlePathList)
                    {
                        ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}AssetBundle:{uncompleteABPath}异步或动态下载未加载完成，转同步加载!");
                        // 如果是当前资源的异步加载正在进行，我们需要取消异步加载完成回调，避免触发多次加载完成回调
                        if (uncompleteABPath.Equals(ResourcePath))
                        {
                            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}取消AssetBundle:{uncompleteABPath}的异步加载完成回调注册!");
                            loadAssetBundleSync();
                        }
                        else
                        {
                            mAssetBundlePathAndBundleLoaderMap[uncompleteABPath].loadImmediately();
                        }
                    }
                }
            }
            else if (LoadMethod == ResourceLoadMethod.Async)
            {
                loadAssetBundleAsync();
                BundleLoader assetBundleLoader;
                int assetBundleLoadUID;
                // 等于null的话意味着上层已经出问题了，这里只是避免阻断
                if (DepABPaths != null)
                {
                    for (int i = 0, length = DepABPaths.Length; i < length; i++)
                    {
                        assetBundleLoadUID = ResourceModuleManager.Singleton.requstAssetBundleAsync(DepABPaths[i], out assetBundleLoader, onAssetBundleLoadComplete, ResourceLoadType.NormalLoad);
                        mAssetBundlePathAndBundleLoaderMap.Add(DepABPaths[i], assetBundleLoader);
                    }
                }
            }
            else
            {
                Debug.LogError($"不支持的加载方式:{LoadMethod}");
                failed();
            }
        }

        /// <summary>
        /// 同步加载自身AB
        /// </summary>
        protected virtual void loadAssetBundleSync()
        {
            var abPath = AssetBundlePath.GetABLoadFullPath(ResourcePath);
            AssetBundle ab = null;
            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}开始同步加载AssetBundle:{ResourcePath}");
            // Note:
            // 先异步LoadFromFileAsyn后同步LoadFromFile的情况下，LoadFromFile返回的ab为null，且Unity会提示***.AB已经被加载
            // 要想同步加载时异步能正确返回，我们需要调用AssetBundleCreateRequeust.assetBundle触发同步加载
            if(mAssetBundleAsyncRequest == null)
            {
                ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}同步加载AssetBundle:{ResourcePath}");
#if UNITY_EDITOR
                //因为资源不全，很多资源丢失，导致直接报错
                //这里临时先在Editor模式下判定下文件是否存在，避免AssetBundle.LoadFromFile()直接报错
                if (System.IO.File.Exists(abPath))
                {
                    ab = AssetBundle.LoadFromFile(abPath);
                }
#else
                ab = AssetBundle.LoadFromFile(abPath);
#endif
            }
            else
            {
                ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}异步转同步加载AssetBundle:{ResourcePath}");
                // 异步转同步时移除异步的回调监听，避免相同AB加载触发加载完成多次
                mAssetBundleAsyncRequest.completed -= onAssetBundleAsyncLoadComplete;
                // 异步加载时改成同步加载触发立刻加载完成
                ab = mAssetBundleAsyncRequest.assetBundle;
            }
            // 加载完成后无论都要设置setResource确保后续的正常使用
            AssetBundleInfo.setResource(ab);
            onAssetBundleLoadComplete(this);
        }

        /// <summary>
        /// 异步加载自身AB
        /// </summary>
        protected virtual void loadAssetBundleAsync()
        {
            var abPath = AssetBundlePath.GetABLoadFullPath(ResourcePath);
            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}开始异步加载AssetBundle:{ResourcePath}");
#if UNITY_EDITOR
            //因为资源不全，很多资源丢失，导致直接报错
            //这里临时先在Editor模式下判定下文件是否存在，避免AssetBundle.LoadFromFile()直接报错
            if (System.IO.File.Exists(abPath))
            {
                mAssetBundleAsyncRequest = AssetBundle.LoadFromFileAsync(abPath);
            }
#else
            mAssetBundleAsyncRequest = AssetBundle.LoadFromFileAsync(abPath);
#endif
            mAssetBundleAsyncRequest.completed += onAssetBundleAsyncLoadComplete;
        }

        /// <summary>
        /// AssetBundle异步加载完成
        /// </summary>
        /// <param name="asyncOperation"></param>
        protected void onAssetBundleAsyncLoadComplete(AsyncOperation asyncOperation)
        {
            if (mAssetBundleAsyncRequest.assetBundle == null || IsDone)
            {
                Debug.LogError($"AssetBundle Path:{ResourcePath}异步加载被同步打断，理论上已经取消回调监听，不应该进入这里!");
                return;
            }
            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}AssetBundle:{ResourcePath}异步加载完成!");
            // 加载完成后无论都要设置setResource确保后续的正常使用
            AssetBundleInfo.setResource(mAssetBundleAsyncRequest.assetBundle);
            onAssetBundleLoadComplete(this);
        }

        /// <summary>
        /// 响应AB加载完成
        /// </summary>
        /// <param name="assetBundleLader"></param>
        /// <param name="requestUid">请求Uid</param>
        protected void onAssetBundleLoadComplete(BundleLoader assetBundleLader, int requestUid = 0)
        {
            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}AssetBundle:{ResourcePath}的AssetBundle:{assetBundleLader.ResourcePath}加载完成!");
            assetBundleLader.AssetBundleInfo.updateLastUsedTime();
            mLoadUnCompleteAssetBundlePathMap.Remove(assetBundleLader.ResourcePath);
#if UNITY_EDITOR
            //AB加载数据统计
            if (ResourceLoadAnalyse.Singleton.ResourceLoadAnalyseSwitch)
            {
                ResourceLoadAnalyse.Singleton.addResourceLoadedTime(ResourcePath);
            }
#endif
            if (IsAllABLoaded)
            {
                onAllAssetBundleLoadComplete();
            }
        }

        /// <summary>
        /// 响应所有AB加载完成
        /// </summary>
        protected void onAllAssetBundleLoadComplete()
        {
            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}Asset:{ResourcePath}的所有AB数量:{mAllRequiredAssetBundleNumber}全部加载完成!");
            if(AssetBundleInfo.getResource<AssetBundle>() != null)
            {
                complete();
            }
            else
            {
                failed();
            }
        }

        /// <summary>
        /// 响应资源加载取消
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
            // 标识资源已经准备完成可以使用和卸载
            AssetBundleInfo.IsReady = true;

            mAssetBundleAsyncRequest = null;

            // 通知上层ab加载完成
            for(int i = 0; i < mRequestInfoList.Count; i++)
            {
                mRequestInfoList[i].RequestCallBack?.Invoke(this, mRequestInfoList[i].RequestUid);
                removeRequest(mRequestInfoList[i].RequestUid);
                i--;
            }
            mRequestUidAndInfoMap.Clear();
            mRequestInfoList.Clear();

            // 返还提前添加的AssetBundle计数，确保计数正确
            AssetBundleInfo.release();
        }

        /// <summary>
        /// 添加AB加载请求完成逻辑回调
        /// </summary>
        /// <param name="requestUID"></param>
        /// <param name="loadABCompleteCallBack"></param>
        /// <returns></returns>
        public bool addRequest(int requestUID, Action<BundleLoader, int> loadABCompleteCallBack)
        {
            if (!mRequestUidAndInfoMap.ContainsKey(requestUID))
            {
                ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}绑定AssetBundle:{ResourcePath}加载请求UID:{requestUID}成功!");
                var bundleRequestInfo = ObjectPool.Singleton.pop<BundleRequestInfo>();
                bundleRequestInfo.init(requestUID, loadABCompleteCallBack);
                mRequestInfoList.Add(bundleRequestInfo);
                mRequestUidAndInfoMap.Add(requestUID, bundleRequestInfo);
                LoaderManager.Singleton.addAssetBundleRequestUID(requestUID, ResourcePath);
                return true;
            }
            else
            {
                Debug.LogError($"重复绑定相同请求UID:{requestUID}回调,绑定AssetBundle:{ResourcePath}请求回调失败!");
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
            if (removeRequest(requestUID))
            {
                ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}AssetBundle:{ResourcePath}取消请求UID:{requestUID}成功!");
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
            BundleRequestInfo bundleRequestInfo;
            if (mRequestUidAndInfoMap.TryGetValue(requestUID, out bundleRequestInfo))
            {
                ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}AssetBundle:{ResourcePath}移除请求UID:{requestUID}成功!");
                mRequestInfoList.Remove(bundleRequestInfo);
                mRequestUidAndInfoMap.Remove(requestUID);
                LoaderManager.Singleton.removeAssetBundleRequestUID(requestUID);
                ObjectPool.Singleton.push<BundleRequestInfo>(bundleRequestInfo);
                return true;
            }
            else
            {
                Debug.LogError($"找不到请求UID:{requestUID}回调,移除AssetBundle:{ResourcePath}请求失败!");
                return false;
            }
        }
    }
}