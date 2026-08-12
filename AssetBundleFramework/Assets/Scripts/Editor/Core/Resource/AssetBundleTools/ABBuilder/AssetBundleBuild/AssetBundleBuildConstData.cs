/*
 * Description:             AssetBundleBuildConstData.cs
 * Author:                  TonyTang
 * Create Date:             2021//04/11
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetBundleBuildConstData.cs
    /// AB打包常量数据
    /// </summary>
    public static class AssetBundleBuildConstData
    {
        /// <summary>
        /// 构建输出的说明文件
        /// </summary>
        public const string ReadmeFileName = "readme.txt";

        /// <summary>
        /// Asset AB打包详细说明文件
        /// </summary>
        public const string AssetBuildReadmeFileName = "assetBuildReadme.txt";

        /// <summary>
        /// AB打包步骤记录文件
        /// </summary>
        public const string BuildLogStepFileName = "buildlogtep.json";

        /// <summary>
        /// 缩进值
        /// </summary>
        public const float INDENTATION = 20f;

        /// <summary>
        /// 构建缓存目录相对Asset路径
        /// </summary>
        public const string BuildTempFolderRelativePath = "../BuildCache/";

        /// <summary>
        /// 构建AB缓存目录相对Asset路径
        /// </summary>
        public const string BuildABTempFolderRelativePath = BuildTempFolderRelativePath + "ABBuild/";

        /// <summary>
        /// 构建AB改名缓存目录相对Asset路径
        /// </summary>
        public const string BuildABRenameTempFolderRelativePath = BuildTempFolderRelativePath + "ABBuildRename/";

        /// <summary>
        /// 构建Resources缓存目录相对Asset路径
        /// </summary>
        public const string BuildResourcesTempFolderRelativePath = BuildTempFolderRelativePath + "ResourcesBuild/";
    }
}