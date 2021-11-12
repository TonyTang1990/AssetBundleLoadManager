/*
 * Description:             AssetBundleLoader.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/13
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetBundleLoader.cs
    /// AssetBundle加载器
    /// </summary>
    public class AssetBundleLoader : BundleLoader
    {
        /// <summary>
        /// 获取指定AssetBundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override AssetBundle getAssetBundle()
        {
            if (!IsDone)
            {
                loadImmediately();
            }
            var assetBundle = AssetBundleInfo.getResource<AssetBundle>();
            AssetBundleInfo.retain();
            return assetBundle;
        }

        /// <summary>
        /// 为AssetBundle添加指定owner的引用并返回该Asset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="owner"></param>
        /// <returns></returns>
        public override AssetBundle bindAssetBundle(UnityEngine.Object owner)
        {
            if (!IsDone)
            {
                loadImmediately();
            }
            var assetBundle = AssetBundleInfo.getResource<AssetBundle>();
            AssetBundleInfo.retainOwner(owner);
            return assetBundle;
        }
    }
}