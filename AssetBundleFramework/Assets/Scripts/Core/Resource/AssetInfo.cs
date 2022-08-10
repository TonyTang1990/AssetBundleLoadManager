/*
 * Description:             AssetInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/13
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetInfo.cs
    /// </summary>
    public class AssetInfo : AbstractResourceInfo
    {
        /// <summary>
        /// Asset类型
        /// </summary>
        public Type AssetType
        {
            get;
            protected set;
        }

        /// <summary>
        /// Asset名
        /// </summary>
        public string AssetName
        {
            get;
            protected set;
        }

        /// <summary>
        /// 所属AB路径信息(仅在AB模式下有值)
        /// </summary>
        public string OwnerAsestBundlePath
        {
            get;
            protected set;
        }

        public AssetInfo()
        {
            AssetType = null;
            AssetName = null;
            OwnerAsestBundlePath = null;
        }

        public override void onCreate()
        {
            base.onCreate();
            AssetType = null;
            AssetName = null;
            OwnerAsestBundlePath = null;
        }

        public override void onDispose()
        {
            base.onDispose();
            AssetType = null;
            AssetName = null;
            OwnerAsestBundlePath = null;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="assetPath">Asset路径</param>
        /// <param name="assetType">Asset类型</param>
        /// <param name="assetBundlePath">所属AB路径</param>
        /// <param name="loadType">加载类型</param>
        public void init(string assetPath, Type assetType, string assetBundlePath = null, ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            ResourcePath = assetPath;
            AssetType = assetType;
            AssetName = Path.GetFileNameWithoutExtension(ResourcePath);
            OwnerAsestBundlePath = assetBundlePath;
            LoadType = loadType;
        }

        public override void dispose()
        {
            if (LoadType != ResourceLoadType.NormalLoad)
            {
                Debug.LogWarning($"正在卸载非NormalLoad的AssetPath:{ResourcePath}的AssetInfo信息!");
            }
            // AssetLoader和AssetInfo是一一对应，
            // 在AssetInfo回收时,AssetLoader也应该得到回收
            LoaderManager.Singleton.deleteLoaderByPath(ResourcePath);
            base.dispose();
        }
    }
}