/*
 * Description:             AssetDatabaseModule.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/24
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetDatabaseModule.cs
    /// Editor模式资源加载模块管理类
    /// </summary>
    public class AssetDatabaseModule : AbstractResourceModule
    {
        /// <summary>
        /// 真正的请求Asset资源(由不同的资源模块去实现)
        /// </summary>
        /// <param name="assetPath">Asset资源路径(带后缀)</param>
        /// <param name="assetLoader">Asset资源加载器</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        /// <returns>请求UID</returns>
        protected override int realRequestAsset<T>(string assetPath, out AssetLoader assetLoader, LoadResourceCompleteHandler completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            assetLoader = null;
            return 0;
        }

        /// <summary>
        /// 真正的请求AssetBundle资源(由不同的资源模块去实现)
        /// </summary>
        /// <param name="abPath">AssetBundle资源路径</param>
        /// <param name="abLoader">AB资源加载器</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        /// <returns>请求UID</returns>
        protected override int realRequestAssetBundle(string abPath, out AssetBundleLoader abLoader, LoadResourceCompleteHandler completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            abLoader = null;
            return 0;
        }
    }
}