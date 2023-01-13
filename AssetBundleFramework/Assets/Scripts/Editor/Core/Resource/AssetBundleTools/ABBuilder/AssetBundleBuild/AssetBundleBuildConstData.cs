/*
 * Description:             AssetBundleBuildConstData.cs
 * Author:                  TONYTANG
 * Create Date:             2021//04/11
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AssetBundleBuildConstData.cs
/// AB打包常量数据
/// </summary>
public static class AssetBundleBuildConstData
{
    /// <summary>
    /// 构建输出的Unity清单文件名称
    /// </summary>
    public const string UnityManifestFileName = "UnityManifest";
    
    /// <summary>
    /// 构建输出的说明文件
    /// </summary>
    public const string ReadmeFileName = "readme.txt";

    /// <summary>
    /// Asset AB打包详细说明文件
    /// </summary>
    public const string AssetBuildReadmeFileName = "assetBuildReadme.txt";

    /// <summary>
    /// 缩进值
    /// </summary>
    public const float INDENTATION = 20f;
}