/*
 * Description:             BundleLoader.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/30
 */

using System;
using System.Collections;
using System.Collections.Generic;
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
        /// 依赖的AB路径数组
        /// </summary>
        public string[] DepABPaths
        {
            get;
            protected set;
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
                return mAllRequiredAssetBundleNumber == LoadCompleteAssetBundleNumber;
            }
        }

        /// <summary>
        /// 所有需要的AB总数量
        /// </summary>
        protected int mAllRequiredAssetBundleNumber;

        /// <summary>
        /// 加载完成的AB数量
        /// </summary>
        public int LoadCompleteAssetBundleNumber
        {
            get;
            protected set;
        }

        /// <summary>
        /// 所有AB资源加载完成逻辑层回调
        /// </summary>
        protected Action<BundleLoader> mLoadABCompleteCallBack;

        /// <summary>
        /// 所有AB资源加载完成逻辑回调Map<请求UID,逻辑回调>
        /// </summary>
        protected Dictionary<int, Action<BundleLoader>> mLoadABCompleteCallBackMap;

        /// <summary>
        /// AssetBundle异步请求
        /// </summary>
        protected AssetBundleCreateRequest mAssetBundleAsyncRequest;

        public BundleLoader() : base()
        {
            DepAssetBundleInfoList = new List<AssetBundleInfo>();
            mLoadABCompleteCallBackMap = new Dictionary<int, Action<BundleLoader>>();
        }

        public override void onCreate()
        {
            base.onCreate();
            DepABPaths = null;
            AssetBundleInfo = null;
            DepAssetBundleInfoList.Clear();
            mLoadABCompleteCallBack = null;
            mLoadABCompleteCallBackMap.Clear();
        }

        public override void onDispose()
        {
            base.onDispose();
            DepABPaths = null;
            AssetBundleInfo = null;
            DepAssetBundleInfoList.Clear();
            mLoadABCompleteCallBack = null;
            mLoadABCompleteCallBackMap.Clear();
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
            mAllRequiredAssetBundleNumber = DepABPaths != null ? DepABPaths.Length + 1 : 1;
            // 创建加载器时就添加相关AssetBundle计数，确保资源加载管理正确
            // 后续加载取消时会返还对应计数
            AssetBundleInfo.retain();
            // 依赖的AB的计数一开始也要添加，在加载完成后无需返还(作为依赖计数统计一次即可)
            AssetBundleInfo depAssetBundleInfo;
            for (int i = 0, length = DepABPaths.Length; i < length; i++)
            {
                depAssetBundleInfo = ResourceModuleManager.Singleton.CurrentResourceModule.getOrCreateAssetBundleInfo(DepABPaths[i], ResourceLoadType.NormalLoad);
                DepAssetBundleInfoList.Add(depAssetBundleInfo);
                depAssetBundleInfo.retain();
            }
        }

        /// <summary>
        /// 获取指定AssetBundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract AssetBundle getAssetBundle();

        /// <summary>
        /// 为AssetBundle添加指定owner的引用并返回该Asset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="owner"></param>
        /// <returns></returns>
        public abstract AssetBundle bindAssetBundle(UnityEngine.Object owner);

        /// <summary>
        /// 响应资源加载
        /// </summary>
        protected override void onLoad()
        {
            base.onLoad();
            // Note:
            // 依赖AB一律采取Normal加载方式
            if (LoadMethod == ResourceLoadMethod.Sync)
            {
                loadAssetBundleSync();
                BundleLoader assetBundleLoader;
                int assetBundleLoadUID;
                for (int i = 0, length = DepABPaths.Length; i < length; i++)
                {
                    assetBundleLoadUID = ResourceModuleManager.Singleton.requstAssetBundleSync(DepABPaths[i], out assetBundleLoader, onAssetBundleLoadComplete, ResourceLoadType.NormalLoad);
                }
            }
            else if (LoadMethod == ResourceLoadMethod.Async)
            {
                loadAssetBundleAsync();
                BundleLoader assetBundleLoader;
                int assetBundleLoadUID;
                for (int i = 0, length = DepABPaths.Length; i < length; i++)
                {
                    assetBundleLoadUID = ResourceModuleManager.Singleton.requstAssetBundleAsync(DepABPaths[i], out assetBundleLoader, onAssetBundleLoadComplete, ResourceLoadType.NormalLoad);
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
            var abpath = global::AssetBundlePath.GetABLoadFullPath(ResourcePath);
            AssetBundle ab = null;
#if UNITY_EDITOR
            //因为资源不全，很多资源丢失，导致直接报错
            //这里临时先在Editor模式下判定下文件是否存在，避免AssetBundle.LoadFromFile()直接报错
            if (System.IO.File.Exists(abpath))
            {
                Debug.Log(string.Format("开始同步加载AB:{0}", ResourcePath));
                ab = AssetBundle.LoadFromFile(abpath);
            }
#else
            ab = AssetBundle.LoadFromFile(abpath);
#endif
            // 加载完成后无论都要设置setResource确保后续的正常使用
            AssetBundleInfo.setResource(ab);
            onAssetBundleLoadComplete(this);
        }

        /// <summary>
        /// 异步加载自身AB
        /// </summary>
        protected virtual void loadAssetBundleAsync()
        {
            var abpath = global::AssetBundlePath.GetABLoadFullPath(ResourcePath);
#if UNITY_EDITOR
            //因为资源不全，很多资源丢失，导致直接报错
            //这里临时先在Editor模式下判定下文件是否存在，避免AssetBundle.LoadFromFile()直接报错
            if (System.IO.File.Exists(abpath))
            {
                Debug.Log(string.Format("开始异步加载AB:{0}", ResourcePath));
                mAssetBundleAsyncRequest = AssetBundle.LoadFromFileAsync(abpath);
            }
#else
            mAssetBundleAsyncRequest = AssetBundle.LoadFromFileAsync(abpath);
#endif
            mAssetBundleAsyncRequest.completed += onAssetBundleAsyncLoadComplete;
        }

        /// <summary>
        /// AssetBundle异步加载完成
        /// </summary>
        /// <param name="asyncOperation"></param>
        protected void onAssetBundleAsyncLoadComplete(AsyncOperation asyncOperation)
        {
            // 加载完成后无论都要设置setResource确保后续的正常使用
            AssetBundleInfo.setResource(mAssetBundleAsyncRequest.assetBundle);
            onAssetBundleLoadComplete(this);
        }

        /// <summary>
        /// 响应AB加载完成
        /// </summary>
        /// <param name="assetBundleLader"></param>
        protected void onAssetBundleLoadComplete(BundleLoader assetBundleLader)
        {
            ResourceLogger.log($"AssetBundle:{ResourcePath}的AssetBundle:{assetBundleLader.ResourcePath}加载完成!");
            assetBundleLader.AssetBundleInfo.updateLastUsedTime();
            LoadCompleteAssetBundleNumber++;
            //AB加载数据统计
            if (ResourceLoadAnalyse.Singleton.ResourceLoadAnalyseSwitch)
            {
                ResourceLoadAnalyse.Singleton.addResourceLoadedTime(ResourcePath);
            }
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
            ResourceLogger.log($"Asset:{ResourcePath}的所有AB数量:{mAllRequiredAssetBundleNumber}全部加载完成!");
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

            // 返还提前添加的AssetBundle计数，确保计数正确
            AssetBundleInfo.release();

            mAssetBundleAsyncRequest = null;
            mLoadABCompleteCallBackMap.Clear();

            // 通知上层ab加载完成
            mLoadABCompleteCallBack?.Invoke(this);
            mLoadABCompleteCallBack = null;

            // Loader在只允许在卸载后回收，上层用户都是面向Loader级别去访问AB和Asset
            // AB加载完成后，AssetBundleLoader的任务就完成了，回收重用
            //ObjectPool.Singleton.push<AssetBundleLoader>(this);
        }

        /// <summary>
        /// 添加AB加载完成逻辑回调
        /// </summary>
        /// <param name="requestUID"></param>
        /// <param name="loadABCompleteCallBack"></param>
        /// <returns></returns>
        public bool addLoadABCompleteCallBack(int requestUID, Action<BundleLoader> loadABCompleteCallBack)
        {
            if (!mLoadABCompleteCallBackMap.ContainsKey(requestUID))
            {
                ResourceLogger.log($"绑定AssetBundle:{ResourcePath}加载请求UID:{requestUID}成功!");
                mLoadABCompleteCallBackMap.Add(requestUID, loadABCompleteCallBack);
                if (loadABCompleteCallBack != null)
                {
                    mLoadABCompleteCallBack += loadABCompleteCallBack;
                }
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
            Action<BundleLoader> loadABCompleteCallBack;
            if (mLoadABCompleteCallBackMap.TryGetValue(requestUID, out loadABCompleteCallBack))
            {
                ResourceLogger.log($"AssetBundle:{ResourcePath}取消请求UID:{requestUID}成功!");
                removeRequest(requestUID);
                if (loadABCompleteCallBack != null)
                {
                    mLoadABCompleteCallBack -= loadABCompleteCallBack;
                }
                // 所有请求都取消表示没人再请求此AB了
                if (mLoadABCompleteCallBackMap.Count == 0)
                {
                    cancel();
                }
                return true;
            }
            else
            {
                Debug.LogError($"找不到请求UID:{requestUID}请求,取消AssetBundle:{ResourcePath}请求失败!");
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
            Action<BundleLoader> loadABCompleteCallBack;
            if (mLoadABCompleteCallBackMap.TryGetValue(requestUID, out loadABCompleteCallBack))
            {
                mLoadABCompleteCallBackMap.Remove(requestUID);
                LoaderManager.Singleton.removeAssetBundleRequestUID(requestUID);
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