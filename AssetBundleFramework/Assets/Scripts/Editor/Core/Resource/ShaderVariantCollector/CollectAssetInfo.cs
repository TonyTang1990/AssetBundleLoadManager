/*
 * Description:             CollectAssetInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2026/01/19
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CollectAssetInfo.cs
/// 搜集Asset信息
/// </summary>
public class CollectAssetInfo
{
    /// <summary>
    /// Asset的GUID
    /// </summary>
    public string AssetGuid
    {
        get;
        private set;
    }

    /// <summary>
    /// Asset路径
    /// </summary>
    public string AssetPath
    {
        get;
        private set;
    }

    /// <summary>
    /// Asset类型
    /// </summary>
    public Type AssetType
    {
        get;
        private set;
    }

    /// <summary>
    /// 依赖的Asset路径数组
    /// </summary>
    public string[] DependencyAssetPaths
    {
        get;
        private set;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="assetGuid"></param>
    /// <param name="assetPath"></param>
    /// <param name="assetType"></param>
    /// <param name="dependencyAssetPaths"></param>
    public CollectAssetInfo(string assetGuid, string assetPath, Type assetType, string[] dependencyAssetPaths)
    {
        AssetGuid = assetGuid;
        AssetPath = assetPath;
        AssetType = assetType;
        DependencyAssetPaths = dependencyAssetPaths;
    }
}
