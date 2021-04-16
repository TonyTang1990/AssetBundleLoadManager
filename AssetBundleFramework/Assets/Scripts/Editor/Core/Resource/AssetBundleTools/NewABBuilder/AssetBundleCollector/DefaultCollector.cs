//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using UnityEngine;

namespace MotionFramework.Editor
{
	public class LabelNone : IAssetCollector
	{
		string IAssetCollector.GetAssetBundleLabel(string assetPath, Collector collector = null)
		{
			// 注意：如果依赖资源来自于忽略文件夹，那么会触发这个异常
			throw new Exception($"{nameof(AssetBundleCollectSetting)} has depend asset in ignore folder : {assetPath}");
		}
	}

	/// <summary>
	/// 以文件路径作为标签名
	/// </summary>
	public class LabelByFolderPath : IAssetCollector
	{
		string IAssetCollector.GetAssetBundleLabel(string assetPath, Collector collector = null)
		{
			// 例如："Assets/Config/test.txt" --> "Assets/Config"
			return Path.GetDirectoryName(assetPath);
		}
	}

	/// <summary>
	/// 以文件夹路径作为标签名
	/// 注意：该文件夹下所有资源被打到一个AssetBundle文件里
	/// </summary>
	public class LabelByFilePath : IAssetCollector
	{
		string IAssetCollector.GetAssetBundleLabel(string assetPath, Collector collector = null)
		{
			// 例如："Assets/Config/test.txt" --> "Assets/Config/test"
			return assetPath.Remove(assetPath.LastIndexOf("."));
		}
	}

    /// <summary>
    /// 以固定名字作为标签名
    /// 注意: 该AB没有路径，但Asset含路径
    /// </summary>
    public class LableByConstName : IAssetCollector
    {
        string IAssetCollector.GetAssetBundleLabel(string assetPath, Collector collector = null)
        {
            Debug.Assert(collector != null, "固定名字加载策略不允许传空Collector!");
            // 例如："Assets/Config/test.txt" --> "ConstName"
            return collector.ConstName;
        }
    }
}