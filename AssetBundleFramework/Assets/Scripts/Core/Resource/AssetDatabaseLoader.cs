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

        public override void OnCreate()
        {
            base.OnCreate();
            mAssetInfo = null;
            mAssetDatabaseAsyncRequest = null;
        }

        public override void OnDispose()
        {
            base.OnDispose();
            mAssetInfo = null;
            mAssetDatabaseAsyncRequest = null;
        }

        /// <summary>
        /// 响应资源加载
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();
            if (LoadMethod == ResourceLoadMethod.Sync)
            {
                if (mAssetDatabaseAsyncRequest != null)
                {
                    // Asset还在异步加载的情况，取消Asset的异步加载回调，避免多次加载完成返回并触发再次加载Asset
                    ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}AssetDatabase:{ResourcePath}资源异步未加载完成,取消Asset异步加载完成回调注册!");
                    mAssetDatabaseAsyncRequest.completed -= OnAssetAsyncLoadComplete;
                }
#if UNITY_EDITOR
                var asset = AssetDatabase.LoadAssetAtPath(mAssetInfo.ResourcePath, mAssetInfo.AssetType);
#else
                Object asset = null;
#endif
                OnAssetLoadComplete(asset);
            }
            else if (LoadMethod == ResourceLoadMethod.Async)
            {
                mAssetDatabaseAsyncRequest = new AssetDatabaseAsyncRequest(mAssetInfo.ResourcePath, mAssetInfo.AssetType);
                mAssetDatabaseAsyncRequest.completed += OnAssetAsyncLoadComplete;
            }
            else
            {
                Debug.LogError($"不支持的加载方式:{LoadMethod}");
                Failed();
            }
        }

        /// <summary>
        /// Asset异步加载完成
        /// </summary>
        /// <param name="assetDatabaseAsyncRequest"></param>
        protected void OnAssetAsyncLoadComplete(AssetDatabaseAsyncRequest assetDatabaseAsyncRequest)
        {
            // Note:
            // 模拟的Asset异步加载做不到Asset同步加载时的异步返回空Asset，所以无法判定Asset == null
            if (IsDone)
            {
                Debug.LogError($"AssetDatabase Path:{ResourcePath}异步加载被同步打断，理论上已经取消回调监听，不应该进入这里!");
                return;
            }
            OnAssetLoadComplete(mAssetDatabaseAsyncRequest.Asset);
        }

        /// <summary>
        /// 响应Asset加载完成
        /// </summary>
        /// <param name="asset"></param>
        protected void OnAssetLoadComplete(Object asset)
        {
            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}AssetDatabase:{ResourcePath}加载完成!");
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
        /// 响应加载完成
        /// </summary>
        protected override void OnComplete()
        {
            base.OnComplete();
            mAssetDatabaseAsyncRequest = null;
        }
    }
}