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
        protected List<AssetBundleInfo> mDepABInfoList;

        /// <summary>
        /// 所在AB是否加载完成
        /// </summary>
        protected bool mIsABLoaded;

        /// <summary>
        /// 主Bundle请求UID
        /// </summary>
        protected AssetBundleRequestHandle mMainBundleLoadRequest;

        /// <summary>
        /// 主Bundle加载器
        /// </summary>
        protected BundleLoader mMainBundleLoader;

        public BundleAssetLoader() : base()
        {
            MainAssetBundlePath = null;
            DepABPaths = null;
            mABInfo = null;
            mDepABInfoList = new List<AssetBundleInfo>();
            mIsABLoaded = false;
            mMainBundleLoader = null;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            MainAssetBundlePath = null;
            DepABPaths = null;
            mABInfo = null;
            mDepABInfoList.Clear();
            mIsABLoaded = false;
            mMainBundleLoadRequest = null;
            mMainBundleLoader = null;
        }

        public override void OnDispose()
        {
            base.OnDispose();
            MainAssetBundlePath = null;
            DepABPaths = null;
            mABInfo = null;
            mDepABInfoList.Clear();
            mIsABLoaded = false;
            mMainBundleLoadRequest = null;
            mMainBundleLoader = null;
        }

        /// <summary>
        /// 初始化Bundle路径信息
        /// </summary>
        /// <param name="ownerABPath"></param>
        /// <param name="depABPaths"></param>
        public void InitBundleInfo(string ownerABPath, string[] depABPaths)
        {
            MainAssetBundlePath = ownerABPath;
            DepABPaths = depABPaths;
            mIsABLoaded = false;
            // 创建加载器时就添加相关AssetBundle计数，确保资源加载管理正确
            // 后续加载取消时会返还对应计数，AB的计数会在AB加载完成后返还(因为AB的计数会在AB加载器创建时添加计数)
            // 仅主AB采取和Asset加载方式一致的方式，依赖AB采用NormalLoad方式
            mABInfo = ResourceModuleManager.Singleton.CurrentResourceModule.GetOrCreateAssetBundleInfo(MainAssetBundlePath, LoadType);
            mABInfo?.Retain();
            // 关联AssetInfo和AssetBundleInfo
            mABInfo?.AddAssetInfo(mAssetInfo);
            if(DepABPaths != null)
            {
                AssetBundleInfo depAssetBundleInfo;
                for (int i = 0, length = DepABPaths.Length; i < length; i++)
                {
                    depAssetBundleInfo = ResourceModuleManager.Singleton.CurrentResourceModule.GetOrCreateAssetBundleInfo(DepABPaths[i], ResourceLoadType.NormalLoad);
                    mDepABInfoList.Add(depAssetBundleInfo);
                    depAssetBundleInfo.Retain();
                }
            }
        }

        /// <summary>
        /// 响应资源加载
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();
            // Note:
            // 只有主AB采用Asset的加载方式，依赖AB一律采取Normal加载方式
            if(LoadMethod == ResourceLoadMethod.Sync)
            {
                if(mMainBundleLoader == null && !mIsABLoaded)
                {
                    // BundlerLoader会负责加载自身AB和依赖AB，这里只需触发主AB加载即可
                    mMainBundleLoadRequest = ResourceModuleManager.Singleton.RequstABSync(MainAssetBundlePath, out mMainBundleLoader, OnABLoadComplete, LoadType);
                }
                else if(mMainBundleLoader != null && !mIsABLoaded)
                {
                    // 已经在加载但没有完成的下(比如之前异步加载),立刻转成同步加载
                    // Note:
                    // 如果依赖AB里有动态下载的AB资源则不会立马完整所有加载需要等待
                    ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}BundleAsset:{ResourcePath}打断异步加载,触发同步加载!");
                    mMainBundleLoader.LoadImmediately();
                }
                else if(mIsABLoaded && mAssetAsyncRequest != null)
                {
                    // AB加载完成但是Asset还在异步加载的情况
                    // 取消Asset的异步加载回调，避免多次加载完成返回并触发再次加载Asset
                    ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}BundleAsset:{ResourcePath}所有AssetBundle加载完成,取消Asset异步加载完成回调注册!");
                    mAssetAsyncRequest.completed -= OnAssetAsyncLoadComplete;
                    DoLoadAsset();
                }
            }
            else if(LoadMethod == ResourceLoadMethod.Async)
            {
                mMainBundleLoadRequest = ResourceModuleManager.Singleton.RequstABAsync(MainAssetBundlePath, out mMainBundleLoader, OnABLoadComplete, LoadType);
            }
            else
            {
                Debug.LogError($"不支持的加载方式:{LoadMethod}");
                Failed();
            }
        }

        /// <summary>
        /// 响应AB加载完成
        /// </summary>
        /// <param name="assetBundleLoader"></param>
        protected void OnABLoadComplete(BundleLoader assetBundleLoader, AssetBundleRequestHandle requestHandle)
        {
            mIsABLoaded = true;
            OnABLoadComplete();
        }

        /// <summary>
        /// 响应所属AB加载完成
        /// </summary>
        protected void OnABLoadComplete()
        {
            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}Asset:{ResourcePath}的所在AB:{MainAssetBundlePath}加载完成!");
            DoLoadAsset();
        }

        /// <summary>
        /// 触发加载Asset
        /// </summary>
        protected void DoLoadAsset()
        {
            if (LoadMethod == ResourceLoadMethod.Sync)
            {
                var assetBundle = mMainBundleLoader != null ? mMainBundleLoader.ObtainAssetBundle() : null;
                if(assetBundle == null)
                {
                    OnAssetLoadComplete(null);
                    return;
                }
                var asset = assetBundle.LoadAsset(mAssetInfo.ResourcePath, mAssetInfo.AssetType);
                OnAssetLoadComplete(asset);
            }
            else if (LoadMethod == ResourceLoadMethod.Async)
            {
                if(mMainBundleLoader != null)
                {
                    var assetBundle = mMainBundleLoader != null ? mMainBundleLoader.ObtainAssetBundle() : null;
                    if(assetBundle == null)
                    {
                        OnAssetLoadComplete(null);
                        return;
                    }
                    mAssetAsyncRequest = assetBundle.LoadAssetAsync(mAssetInfo.ResourcePath, mAssetInfo.AssetType);
                    mAssetAsyncRequest.completed += OnAssetAsyncLoadComplete;
                }
                else
                {
                    OnAssetAsyncLoadComplete(null);
                }
            }
        }

        /// <summary>
        /// Asset异步加载完成
        /// </summary>
        /// <param name="asyncOperation"></param>
        protected void OnAssetAsyncLoadComplete(AsyncOperation asyncOperation)
        {
            if (mAssetAsyncRequest.asset == null || IsDone)
            {
                Debug.LogError($"Asset Path:{ResourcePath}异步加载被同步打断，理论上已经取消回调监听，不应该进入这里!");
                return;
            }
            OnAssetLoadComplete(mAssetAsyncRequest.asset);
        }

        /// <summary>
        /// 响应Asset加载完成
        /// </summary>
        /// <param name="asset"></param>
        protected void OnAssetLoadComplete(Object asset)
        {
            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}Asset:{ResourcePath}加载完成!");
            // 加载完成后无论都要设置setResource确保后续的正常使用
            mAssetInfo.SetResource(asset);
            if (asset != null)
            {
                Complete();
            }
            else
            {
                Failed();
            }
        }

        /// <summary>
        /// 响应资源加载取消
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
            // 上层多个加载逻辑回调，在完成后根据调用getAsset或bindAsset情况去添加计数和绑定
            // 返还提前添加的Asset以及AssetBundle计数信息，确保正确的资源管理
            // 依赖AB的真正计数添加由BundleLoader去负责(确保单个AB的依赖AB计数只添加一次)
            mABInfo?.Release();
            for (int i = 0, length = mDepABInfoList.Count; i < length; i++)
            {
                mDepABInfoList[i].Release();
            }
        }

        /// <summary>
        /// AssetBundle模式加载指定SubAsset
        /// Unity接口会返回主Asset下指定类型的所有Asset和SubAsset，因此全部写入缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subAssetName"></param>
        /// <returns></returns>
        protected override T DoLoadSubAsset<T>(string subAssetName)
        {
            var assetBundle = mMainBundleLoader?.ObtainAssetBundle();
            if (assetBundle == null)
            {
                return null;
            }

            var subAssets = assetBundle.LoadAssetWithSubAssets<T>(mAssetInfo.ResourcePath);
            mAssetInfo.CacheSubAssets(subAssets);

            T subAsset;
            if (mAssetInfo.TryGetSubAsset<T>(subAssetName, out subAsset))
            {
                return subAsset;
            }
            return null;
        }
    }
}
