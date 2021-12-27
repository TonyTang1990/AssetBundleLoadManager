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
    /// Asset打包信息Asset名(用于存储Asset打包映射AB信息(从而做到根据Asset路径就能查到AB名字))
    /// </summary>
    public const string AssetBuildInfoAssetName = "AssetBuildInfo";

    /// <summary>
    /// Asset打包信息Asset相对存储目录
    /// </summary>
    public const string AssetBuildInfoAssetRelativePath = "/Res/assetbuildinfo";

    /// <summary>
    /// AssetBundle文件默认的后缀名
    /// </summary>
    public const string AssetBundleDefaultVariant = "bundle";

    /// <summary>
    /// AssetBundle文件默认的后缀名
    /// </summary>
    public const string AssetBundleDefaultVariantWithPoint = ".bundle";

    /// <summary>
    /// AssetBundle依赖信息文件默认的后缀名
    /// </summary>
    public const string AssetBundleDefaultManifestPostfixWithPoint = ".manifest";

    /// <summary>
    /// 包内AB的MD5信息记录文件名
    /// </summary>
    public const string AssetBundleMd5InfoFileName = "AssetBundleMd5.txt";

    /// <summary>
    /// 包外热更新AB的MD5信息记录文件名
    /// </summary>
    public const string HotUpdateAssetBundleMd5InfoFileName = "HotUpdateResourceMd5.txt";

    /// <summary>
    /// AssetBundle信息分隔符
    /// </summary>
    public const char AssetBundlleInfoSeparater = ':';
}