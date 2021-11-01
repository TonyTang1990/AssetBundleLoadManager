/*
 * Description:             AssetBundleInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/13
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetBundleInfo.cs
    /// AssetBundle加载信息
    /// </summary>
    public class AssetBundleInfo : AbstractResourceInfo
    {
        /// <summary>
        /// 当前AB依赖AB路径信息组
        /// </summary>
        private string[] mDepAssetBundlePaths;

        /// <summary>
        /// 所有已加载的Asset信息
        /// </summary>
        private Dictionary<string, AssetInfo> mAllLoadedAssetInfoMap;

        /// <summary>
        /// 是否不再有人使用
        /// </summary>
        public override bool IsUnsed
        {
            get 
            {
                return mIsReady && RefCount <= 0 && updateOwnerReference() == 0 && IsAllAssetsUnsed;
            }
        }

        /// <summary>
        /// 是否所有的子Asset没人使用
        /// </summary>
        protected bool IsAllAssetsUnsed
        {
            get
            {
                foreach(var assetInfo in mAllLoadedAssetInfoMap)
                {
                    if(!assetInfo.Value.IsUnsed)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public AssetBundleInfo()
        {
            mAllLoadedAssetInfoMap = new Dictionary<string, AssetInfo>();
        }

        public override void onCreate()
        {
            base.onCreate();
            mAllLoadedAssetInfoMap.Clear();
        }

        public override void onDispose()
        {
            base.onDispose();
            mAllLoadedAssetInfoMap.Clear();
        }

        public void init(string assetBundlePath, string[] depAssetBundlePaths)
        {
            ResourcePath = assetBundlePath;
            mDepAssetBundlePaths = depAssetBundlePaths;
        }

        /// <summary>
        /// 添加AssetBundle内的Asset信息
        /// </summary>
        /// <param name="assetInfo"></param>
        /// <returns></returns>
        public bool addAssetInfo(AssetInfo assetInfo)
        {
            if(assetInfo.OwnerAsestBundlePath == ResourcePath)
            {
                if(!mAllLoadedAssetInfoMap.ContainsKey(assetInfo.ResourcePath))
                {
                    mAllLoadedAssetInfoMap.Add(assetInfo.ResourcePath, assetInfo);
                    return true;
                }
                else
                {
                    Debug.LogError($"AssetBundlePath:{ResourcePath}不允许重复添加AssetPath:{assetInfo.ResourcePath}的Asset信息,添加失败,请检查代码!");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"AssetBundlePath:{ResourcePath}不允许添加属于其他AssetBundlePath:{assetInfo.OwnerAsestBundlePath}的Asset信息,添加失败,请检查代码!");
                return false;
            }
        }

        public override void dispose()
        {

        }
    }
}