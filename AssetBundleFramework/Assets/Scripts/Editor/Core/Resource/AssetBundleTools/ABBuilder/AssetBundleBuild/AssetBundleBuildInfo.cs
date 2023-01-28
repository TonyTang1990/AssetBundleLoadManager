/*
 * Description:             AssetBundleBuildInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2023//01/23
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetBundle打包信息
    /// </summary>
    public class AssetBundleBuildInfo
    {
        /// <summary>
		/// AssetBundle标签
		/// </summary>
		public string AssetBundleName
        {
            get;
            private set;
        }

        /// <summary>
        /// AssetBundle变体
        /// </summary>
        public string AssetBundleVariant
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前AB打包信息里所属的Asset打包信息Map<Asset路径，Asset打包信息>
        /// </summary>
        private Dictionary<string, AssetBuildInfo> mAssetBuildInfoMap;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="assetBundleVariant"></param>
        public AssetBundleBuildInfo(string assetBundleName, string assetBundleVariant)
        {
            AssetBundleName = assetBundleName;
            AssetBundleVariant = assetBundleVariant;
            mAssetBuildInfoMap = new Dictionary<string, AssetBuildInfo>();
        }

        /// <summary>
        /// 添加所属Asset打包信息
        /// </summary>
        /// <param name="assetBuildInfo"></param>
        /// <returns></returns>
        public bool AddAssetBuildInfo(AssetBuildInfo assetBuildInfo)
        {
            if(mAssetBuildInfoMap.ContainsKey(assetBuildInfo.AssetPath))
            {
                return false;
            }
            mAssetBuildInfoMap.Add(assetBuildInfo.AssetPath, assetBuildInfo);
            return true;
        }

        /// <summary>
        /// 获取当前AB打包信息里的Asset数量
        /// </summary>
        /// <returns></returns>
        public int GetTotalAssetBuildNum()
        {
            return mAssetBuildInfoMap.Count;
        }

        /// <summary>
        /// 获取当前AB打包信息里的所有Asset打包Asset名
        /// </summary>
        /// <returns></returns>
        public string[] GetAllAssetNames()
        {
            string[] allAssetNames = new string[mAssetBuildInfoMap.Count];
            var assetIndex = 0;
            foreach(var assetBuildInfo in mAssetBuildInfoMap)
            {
                allAssetNames[assetIndex] = assetBuildInfo.Value.AssetPath;
                assetIndex++;
            }
            return allAssetNames;
        }
    }
}
