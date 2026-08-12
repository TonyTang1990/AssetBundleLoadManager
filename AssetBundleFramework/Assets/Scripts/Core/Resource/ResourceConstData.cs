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
        /// Shader变体搜集Asset名
        /// </summary>
        public const string ShaderVariantsAssetName = "DIYShaderVariantsCollection.shadervariants";

        /// <summary>
        /// Shader变体搜集Asset路径
        /// </summary>
        public const string ShaderVariantsAssetRelativePath = "Assets/Res/shadervariants/DIYShaderVariantsCollection.shadervariants";

        /// <summary>
        /// Asset打包信息Asset相对存储目录
        /// </summary>
        public const string AssetBuildInfoAssetRelativeFolderPath = "Res/assetBuildInfo";

        /// <summary>
        /// 依赖文件Manifest的Asset名
        /// </summary>
        public const string AssetBundleManifestAssetName = "AssetBundleManifest";

        /// <summary>
        /// 热更校验AB资源信息文件名(含热更AB资源信息记录文件名的大小和Hash值信息)
        /// </summary>
        public const string VerifyABInfoFileName = "VerifyABInfo.json";

        /// <summary>
        /// 热更AB资源信息记录文件名(含热更新文件的AB名，MD5名，AB+MD5名，文件大小，Hash值(Sha256)用于后续热更文件的完整性和正确性验证))
        /// </summary>
        public const string ABInfoFileName = "ABInfo.json";

        /// <summary>
        /// AssetBundle信息分隔符
        /// </summary>
        public const char AssetBundlleInfoSeparater = '|';
    }
}