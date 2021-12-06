/*
 * Description:             AssetDatabaseLoader.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/13
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetDatabaseLoader.cs
    /// AssetDatabase模式下的Asset加载器
    /// </summary>
    public class AssetDatabaseLoader : AssetLoader
    {
        /// <summary>
        /// AssetDatabase异步加载请求模拟
        /// </summary>
        protected AssetDatabaseAsyncRequest mAssetDatabaseAsyncRequest;

        public AssetDatabaseLoader() : base()
        {
            mAssetInfo = null;
            mAssetDatabaseAsyncRequest = null;
        }

        public override void onCreate()
        {
            base.onCreate();
            mAssetInfo = null;
            mAssetDatabaseAsyncRequest = null;
        }

        public override void onDispose()
        {
            base.onDispose();
            mAssetInfo = null;
            mAssetDatabaseAsyncRequest = null;
        }

        /// <summary>
        /// 响应资源加载
        /// </summary>
        protected override void onLoad()
        {
            base.onLoad();
            if (LoadMethod == ResourceLoadMethod.Sync)
            {
                if (mAssetDatabaseAsyncRequest != null)
                {
                    // Asset还在异步加载的情况，取消Asset的异步加载回调，避免多次加载完成返回并触发再次加载Asset
                    ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}AssetDatabase:{ResourcePath}资源异步未加载完成,取消Asset异步加载完成回调注册!");
                    mAssetDatabaseAsyncRequest.completed -= onAssetAsyncLoadComplete;
                }
                var asset = AssetDatabase.LoadAssetAtPath(mAssetInfo.ResourcePath, mAssetInfo.AssetType);
                onAssetLoadComplete(asset);
            }
            else if (LoadMethod == ResourceLoadMethod.Async)
            {
                mAssetDatabaseAsyncRequest = new AssetDatabaseAsyncRequest(mAssetInfo.ResourcePath, mAssetInfo.AssetType);
                mAssetDatabaseAsyncRequest.completed += onAssetAsyncLoadComplete;
            }
            else
            {
                Debug.LogError($"不支持的加载方式:{LoadMethod}");
                failed();
            }
        }

        /// <summary>
        /// Asset异步加载完成
        /// </summary>
        /// <param name="assetDatabaseAsyncRequest"></param>
        protected void onAssetAsyncLoadComplete(AssetDatabaseAsyncRequest assetDatabaseAsyncRequest)
        {
            // Note:
            // 模拟的Asset异步加载做不到Asset同步加载时的异步返回空Asset，所以无法判定Asset == null
            if (IsDone)
            {
                Debug.LogError($"AssetDatabase Path:{ResourcePath}异步加载被同步打断，理论上已经取消回调监听，不应该进入这里!");
                return;
            }
            onAssetLoadComplete(mAssetDatabaseAsyncRequest.Asset);
        }

        /// <summary>
        /// 响应Asset加载完成
        /// </summary>
        /// <param name="asset"></param>
        protected void onAssetLoadComplete(Object asset)
        {
            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}AssetDatabase:{ResourcePath}加载完成!");
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
        /// 响应加载完成
        /// </summary>
        protected override void onComplete()
        {
            base.onComplete();
            mAssetDatabaseAsyncRequest = null;
        }
    }
}