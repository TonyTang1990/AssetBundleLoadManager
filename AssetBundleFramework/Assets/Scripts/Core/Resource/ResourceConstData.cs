/*
 * Description:             ResourceConstData.cs
 * Author:                  TONYTANG
 * Create Date:             2021//04/17
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
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
        /// Asset打包信息Asset相对存储目录
        /// </summary>
        public const string AssetBuildInfoAssetRelativePath = "/Res/assetbuildinfo";

        /// <summary>
        /// 依赖文件Manifest的Asset名
        /// </summary>
        public const string AssetBundleManifestAssetName = "AssetBundleManifest";

        /// <summary>
        /// 包内AB的MD5信息记录文件名
        /// </summary>
        public const string AssetBundleMd5InfoFileName = "AssetBundleMd5.txt";

        /// <summary>
        /// AssetBundle信息分隔符
        /// </summary>
        public const char AssetBundlleInfoSeparater = '|';
    }
}