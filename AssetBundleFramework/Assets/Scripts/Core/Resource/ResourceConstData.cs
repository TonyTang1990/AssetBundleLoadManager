/*
 * Description:             ResourceConstData.cs
 * Author:                  TONYTANG
 * Create Date:             2021//04/17
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ResourceConstData.cs
/// 资源常量数据
/// </summary>
public static class ResourceConstData
{
    /// <summary>
    /// Shader AB名字
    /// </summary>
    public const string ShaderABName = "shaderlist";

    /// <summary>
    /// Shader变体搜集文件名
    /// </summary>
    public const string ShaderVariantsAssetName = "DIYShaderVariantsCollection";

    /// <summary>
    /// AssetBundle打包信息Asset名(用于存储Asset打包映射AB信息(从而做到根据Asset路径就能查到AB名字))
    /// </summary>
    public const string AssetBundleBuildInfoAssetName = "AssetBundleBuildInfo";

    /// <summary>
    /// AssetBundle打包信息Asset相对存储目录
    /// </summary>
    public const string AssetBundleBuildInfoAssetRelativePath = "/Res/assetbundlebuildinfo";
}