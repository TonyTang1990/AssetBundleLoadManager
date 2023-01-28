/*
 * Description:             AssetBuildInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2023//01/23
 */
using System.Collections.Generic;
using UnityEditor;

namespace TResource
{
	/// <summary>
	/// Asset打包信息类
	/// </summary>
	public class AssetBuildInfo
	{
		/// <summary>
        /// Asset路径
        /// </summary>
		public string AssetPath
		{
			private set;
			get;
		}

		/// <summary>
        /// 是否是可收集Asset
        /// </summary>
		public bool IsCollectAsset
		{
			private set;
			get;
		}

        /// <summary>
        /// 是否是场景Asset
        /// </summary>
		public bool IsSceneAsset
		{
			private set;
			get;
		}

        /// <summary>
        /// 是否是视频Asset
        /// </summary>
		public bool IsVideoAsset
		{
			private set;
			get;
		}

		/// <summary>
		/// 被依赖次数
        /// Note:
        /// 1. 包含参与和未参与打包搜集的Asset依赖次数
		/// </summary>
		public int DependCount = 0;

		/// <summary>
		/// AssetBundle标签
		/// </summary>
		public string AssetBundleLabel = null;

		/// <summary>
		/// AssetBundle变体
		/// </summary>
		public string AssetBundleVariant = null;

		/// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="assetPath"></param>
        public AssetBuildInfo(string assetPath)
		{
			AssetPath = assetPath;
			IsCollectAsset = AssetBundleCollectSettingData.IsCollectAsset(assetPath);
			IsSceneAsset = AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(SceneAsset);
			IsVideoAsset = AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(UnityEngine.Video.VideoClip);
        }
	}
}