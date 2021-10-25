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
        /// 所有已加载的Asset信息
        /// </summary>
        private Dictionary<string, AssetInfo> mAllLoadedAssetInfoMap;

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

        public void init(string assetBundlePath)
        {
            ResourcePath = assetBundlePath;
        }

        /// <summary>
        /// 添加AssetBundle内已加载的Asset信息
        /// </summary>
        /// <param name="assetInfo"></param>
        /// <returns></returns>
        public bool addLoadedAssetInfo(AssetInfo assetInfo)
        {
            if(assetInfo.OwnerAsestBundleInfo.ResourcePath.Equals(ResourcePath))
            {
                if(!mAllLoadedAssetInfoMap.ContainsKey(assetInfo.ResourcePath))
                {
                    mAllLoadedAssetInfoMap.Add(assetInfo.ResourcePath, assetInfo);
                    return true;
                }
                else
                {
                    Debug.LogError($"AssetBundlePath:{ResourcePath}不允许重复添加AssetPath:{assetInfo.ResourcePath}的Asset信息,添加失败!");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"AssetBundlePath:{ResourcePath}不允许添加属于其他AssetBundlePath:{assetInfo.OwnerAsestBundleInfo.ResourcePath}的Asset信息,添加失败!");
                return false;
            }
        }


        public override void dispose()
        {

        }
    }
}