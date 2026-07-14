/*
 * Description:             DownloadAssetBundleLoader.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/30
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// DownloadAssetBundleLoader.cs
    /// 动态热更的AssetBundle下载器
    /// </summary>
    public class DownloadAssetBundleLoader : BundleLoader
    {
        // TODO: 支持边玩边下的设计

        /// <summary>
        /// 获取指定AssetBundle(加计数)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override AssetBundle GetAssetBundle()
        {
            if (!IsDone)
            {
                Debug.LogError($"动态资源:{ResourcePath}不支持立即获取AssetBundle!");
                return null;
            }
            return AssetBundleInfo.GetResource<AssetBundle>();
        }

        /// <summary>
        /// 获取指定AssetBundle(不加计数)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override AssetBundle ObtainAssetBundle()
        {
            if (!IsDone)
            {
                Debug.LogError($"动态资源:{ResourcePath}不支持立即获取AssetBundle!");
                return null;
            }
            var assetBundle = AssetBundleInfo.GetResource<AssetBundle>();
            return assetBundle;
        }

        /// <summary>
        /// 为AssetBundle添加指定owner的引用并返回该Asset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="owner"></param>
        /// <returns></returns>
        public override AssetBundle BindAssetBundle(UnityEngine.Object owner)
        {
            if (!IsDone)
            {
                Debug.LogError($"动态资源:{ResourcePath}不支持立即获取AssetBundle并绑定对象!");
                return null;
            }
            var assetBundle = AssetBundleInfo.GetResource<AssetBundle>();
            AssetBundleInfo.RetainOwner(owner);
            return assetBundle;
        }
    }
}