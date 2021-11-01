/*
 * Description:             BundleAssetLoader.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/30
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// BundleAssetLoader.cs
    /// AB模式下的Asset加载器
    /// </summary>
    public class BundleAssetLoader : AssetLoader
    {
        /// <summary>
        /// 主AssetBundle路径
        /// </summary>
        public string MainAssetBundlePath
        {
            get;
            protected set;
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
        /// 当前AB加载信息
        /// </summary>
        protected AssetBundleInfo mABInfo;

        /// <summary>
        /// 当前AB依赖的AB对应的AB信息列表(用于构建当前AssetBundleInfo)
        /// </summary>
        protected List<AssetBundleInfo> mDepAssetBundleInfoList;

        /// <summary>
        /// 是否所有的AB都加载完成
        /// </summary>
        protected bool IsAllABLoaded
        {
            get
            {
                return mAllRequiredAssetBundleNumber == mLoadCompleteAssetBundleNumber;
            }
        }

        /// <summary>
        /// 所有需要的AB总数量
        /// </summary>
        protected int mAllRequiredAssetBundleNumber;

        /// <summary>
        /// 加载完成的AB数量
        /// </summary>
        protected int mLoadCompleteAssetBundleNumber;

        /// <summary>
        /// 主Bundle请求UID
        /// </summary>
        protected int mMainBundleLoaderUID;

        /// <summary>
        /// 主Bundle加载器
        /// </summary>
        protected BundleLoader mMainBundleLoader;

        public BundleAssetLoader() : base()
        {
            MainAssetBundlePath = null;
            DepABPaths = null;
            mABInfo = null;
            mDepAssetBundleInfoList = new List<AssetBundleInfo>();
            mLoadCompleteAssetBundleNumber = 0;
            mMainBundleLoader = null;
        }

        public override void onCreate()
        {
            base.onCreate();
            MainAssetBundlePath = null;
            DepABPaths = null;
            mABInfo = null;
            mDepAssetBundleInfoList.Clear();
            mLoadCompleteAssetBundleNumber = 0;
            mMainBundleLoader = null;
        }

        public override void onDispose()
        {
            base.onDispose();
            MainAssetBundlePath = null;
            DepABPaths = null;
            mABInfo = null;
            mDepAssetBundleInfoList.Clear();
            mLoadCompleteAssetBundleNumber = 0;
            mMainBundleLoader = null;
        }

        /// <summary>
        /// 初始化Bundle路径信息
        /// </summary>
        /// <param name="ownerAssetBundlePath"></param>
        /// <param name="depAssetBundlePaths"></param>
        public void initBundleInfo(string ownerAssetBundlePath, string[] depAssetBundlePaths)
        {
            MainAssetBundlePath = ownerAssetBundlePath;
            DepABPaths = depAssetBundlePaths;
            mAllRequiredAssetBundleNumber = DepABPaths != null ? DepABPaths.Length + 1 : 1;
            // 创建加载器时就添加相关AssetBundle计数，确保资源加载管理正确
            // 后续加载取消时会返还对应计数，AB的计数会在AB加载完成后返还(因为AB的计数会在AB加载器创建时添加计数)
            // 仅主AB采取和Asset加载方式一致的方式，依赖AB采用NormalLoad方式
            mABInfo = ResourceModuleManager.Singleton.CurrentResourceModule.getOrCreateAssetBundleInfo(MainAssetBundlePath, LoadType);
            mABInfo.retainSelf();
            // 关联AssetInfo和AssetBundleInfo
            mABInfo.addAssetInfo(mAssetInfo);
            AssetBundleInfo depAssetBundleInfo;
            for(int i = 0, length = DepABPaths.Length; i < length; i++)
            {
                depAssetBundleInfo = ResourceModuleManager.Singleton.CurrentResourceModule.getOrCreateAssetBundleInfo(DepABPaths[i], ResourceLoadType.NormalLoad);
                mDepAssetBundleInfoList.Add(depAssetBundleInfo);
                depAssetBundleInfo.retainSelf();
            }
        }

        /// <summary>
        /// 响应资源加载
        /// </summary>
        protected override void onLoad()
        {
            base.onLoad();
            // Note:
            // 只有主AB采用Asset的加载方式，依赖AB一律采取Normal加载方式
            if(LoadMethod == ResourceLoadMethod.Sync)
            {
                // BundlerLoader会负责加载自身AB和依赖AB，这里只需触发主AB加载即可
                mMainBundleLoaderUID = ResourceModuleManager.Singleton.requstAssetBundleSync(MainAssetBundlePath, out mMainBundleLoader, onAssetBundleLoadComplete, LoadType);
            }
            else if(LoadMethod == ResourceLoadMethod.Async)
            {
                mMainBundleLoaderUID = ResourceModuleManager.Singleton.requstAssetBundleAsync(MainAssetBundlePath, out mMainBundleLoader, onAssetBundleLoadComplete, LoadType);
            }
            else
            {
                Debug.LogError($"不支持的加载方式:{LoadMethod}");
                failed();
            }
        }

        /// <summary>
        /// 响应AB加载完成
        /// </summary>
        /// <param name="assetBundleLader"></param>
        protected void onAssetBundleLoadComplete(BundleLoader assetBundleLader)
        {
            mLoadCompleteAssetBundleNumber++;
            if(IsAllABLoaded)
            {
                onAllAssetBundleLoadComplete();
            }
        }

        /// <summary>
        /// 响应所有AB加载完成
        /// </summary>
        protected void onAllAssetBundleLoadComplete()
        {
            ResourceLogger.log($"Asset:{ResourcePath}的所有依赖AB数量:{mAllRequiredAssetBundleNumber}全部加载完成!");
            if(LoadMethod == ResourceLoadMethod.Sync)
            {
                var asset = mMainBundleLoader.getAssetBundle().LoadAsset(mAssetInfo.AssetName, mAssetInfo.AssetType);
                onAssetLoadComplete(asset);
            }
            else if(LoadMethod == ResourceLoadMethod.Async)
            {
                mAssetAsyncRequest = mMainBundleLoader.getAssetBundle().LoadAssetAsync(mAssetInfo.AssetName, mAssetInfo.AssetType);
                mAssetAsyncRequest.completed += onAssetAsyncLoadComplete;
            }
        }

        /// <summary>
        /// Asset异步加载完成
        /// </summary>
        /// <param name="asyncOperation"></param>
        protected void onAssetAsyncLoadComplete(AsyncOperation asyncOperation)
        {
            onAssetLoadComplete(mAssetAsyncRequest.asset);
        }

        /// <summary>
        /// 响应Asset加载完成
        /// </summary>
        /// <param name="asset"></param>
        protected void onAssetLoadComplete(Object asset)
        {
            ResourceLogger.log($"Asset:{ResourcePath}加载完成!");
            // 加载完成后无论都要设置setResource确保后续的正常使用
            mAssetInfo.setResource(asset);
            if (asset != null)
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
            // 上层多个加载逻辑回调，在完成后根据调用getAsset或bindAsset情况去添加计数和绑定
            // 返还提前添加的Asset以及AssetBundle计数信息，确保正确的资源管理
            // 依赖AB的真正计数添加由BundleLoader去负责(确保单个AB的依赖AB计数只添加一次)
            mABInfo.releaseSelf();
            for (int i = 0, length = mDepAssetBundleInfoList.Count; i < length; i++)
            {
                mDepAssetBundleInfoList[i].releaseSelf();
            }
            base.onComplete();
        }
    }
}