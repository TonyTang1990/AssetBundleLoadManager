/*
 * Description:             ResourceBuildTool.cs
 * Author:                  TONYTANG
 * Create Date:             2021//12/19
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// ResourceBuildTool.cs
    /// 资源打包工具
    /// </summary>
    public static class ResourceBuildTool
    {
        /// <summary>
        /// 执行AssetBundle打包
        /// </summary>
        /// <param name="abBuildPurpose">AB打包用途</param>
        /// <param name="buildTarget">打包平台</param>
        /// <param name="isForceBuild">是否强制重新打包</param>
        public static bool DoBuildAssetBundle(AssetBundleBuildPurpose abBuildPurpose, BuildTarget buildTarget,
                                              ABCompressOption compressOption = ABCompressOption.Uncompressed,
                                              bool isForceBuild = false, bool isAppendHash = false,
                                              bool isDisableWriteTypeTree = false, bool isIgnoreTypeTreeChanges = false,
                                              double versionCode = 0, int resourceVersionCode = 0)
        {
            var assetBundleBuildParams = new AssetBundleBuildParams(AssetBundleBuildPurpose.BuildPlayerBaseLine,
                                                                    buildTarget, compressOption, isForceBuild,
                                                                    isAppendHash, isDisableWriteTypeTree,
                                                                    isIgnoreTypeTreeChanges, versionCode, resourceVersionCode);
            var assetBundleBuilder = new AssetBundleBuilder(assetBundleBuildParams);
            var abBuildResult = ExecuteAssetBundleBuild(assetBundleBuilder);
            return abBuildResult;
        }

        /// <summary>
        /// 使用指定AssetBundleBuilder执行AssetBundle打包
        /// </summary>
        /// <param name="assetBundleBuilder">AssetBundle打包</param>
        public static bool DoBuildAssetBundleByBuilder(AssetBundleBuilder assetBundleBuilder)
        {
            var abBuildResult = ExecuteAssetBundleBuild(assetBundleBuilder);
            return abBuildResult;
        }

        /// <summary>
        /// 执行构建
        /// </summary>
        /// <param name="assetBundleBuilder"></param>
        private static bool ExecuteAssetBundleBuild(AssetBundleBuilder assetBundleBuilder)
        {
            var timecounter = new TimeCounter();
            timecounter.Start("AssetBundleBuild");
            var preAssetBuildResult = assetBundleBuilder.PreAssetBuild();
            if(!preAssetBuildResult)
            {
                timecounter.End();
                Debug.LogError($"[BuildPatch] 资源打包前置处理失败，打包终止！");
                return false;
            }
            var postAssetBuildResult = assetBundleBuilder.PostAssetBuild();
            if(!postAssetBuildResult)
            {
                timecounter.End();
                Debug.LogError($"[BuildPatch] 资源打包后置处理失败，打包终止！");
                return false;
            }
            timecounter.End();
            return true;
        }
    }
}