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

        /// <summary>
        /// 添加引用，引用计数+1
        /// </summary>
        public override void retain()
        {
            base.retain();
            if(OwnerAsestBundlePath != null)
            {
                // 常规Asset引用计数添加需要添加所属AB的使用计数
                var assetBundleInfo = ResourceModuleManager.Singleton.CurrentResourceModule.getAssetBundleInfo(OwnerAsestBundlePath);
                assetBundleInfo.retain();
            }
        }

        /// <summary>
        /// 释放引用，引用计数-1
        /// </summary>
        public override void release()
        {
            base.release();
            if (OwnerAsestBundlePath != null)
            {
                // 常规Asset引用计数减少需要减少所属AB的使用计数
                var assetBundleInfo = ResourceModuleManager.Singleton.CurrentResourceModule.getAssetBundleInfo(OwnerAsestBundlePath);
                assetBundleInfo.release();
            }
        }

        public override void dispose()
        {

        }
    }
}