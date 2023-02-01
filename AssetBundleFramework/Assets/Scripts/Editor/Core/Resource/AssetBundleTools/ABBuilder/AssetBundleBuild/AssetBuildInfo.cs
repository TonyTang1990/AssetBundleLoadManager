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
		/// Asset在AB里的访问名
		/// </summary>
		public string AddresableName
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
        /// <param name="assetPath">Asset路径</param>
		/// <param name="addresableName">Asset在AB里的访问名</param>
        public AssetBuildInfo(string assetPath, string addresableName)
		{
			AssetPath = assetPath;
			AddresableName = addresableName;
			IsCollectAsset = AssetBundleCollectSettingData.IsCollectAsset(assetPath);
			IsSceneAsset = AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(SceneAsset);
			IsVideoAsset = AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(UnityEngine.Video.VideoClip);
        }
	}
}