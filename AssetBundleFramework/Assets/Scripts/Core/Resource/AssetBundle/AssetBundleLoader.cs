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
        /// 获取指定AssetBundle(加计数)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override AssetBundle GetAssetBundle()
        {
            if (!IsDone)
            {
                LoadImmediately();
            }
            var assetBundle = AssetBundleInfo.GetResource<AssetBundle>();
            AssetBundleInfo.Retain();
            return assetBundle;
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
                LoadImmediately();
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
                LoadImmediately();
            }
            var assetBundle = AssetBundleInfo.GetResource<AssetBundle>();
            AssetBundleInfo.RetainOwner(owner);
            return assetBundle;
        }
    }
}