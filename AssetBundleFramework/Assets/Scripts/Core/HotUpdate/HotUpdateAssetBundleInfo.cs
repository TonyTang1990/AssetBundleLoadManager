/*
 * Description:             HotUpdateAssetBundleInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2022//01/03
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HotUpdateAssetBundleInfo.cs
/// 热更新AssetBundle信息
/// </summary>
public class HotUpdateAssetBundleInfo 
{
    /// <summary>
    /// AssetBundle路径
    /// </summary>
    public string AssetBundlePath
    {
        get;
        private set;
    }

    /// <summary>
    /// AssetBundle的MD5
    /// </summary>
    public string AssetBundleMD5
    {
        get;
        set;
    }

    private HotUpdateAssetBundleInfo()
    {

    }

    public HotUpdateAssetBundleInfo(string assetBundlePath, string assetBundleMd5)
    {
        AssetBundlePath = assetBundlePath;
        AssetBundleMD5 = assetBundleMd5;
    }
}