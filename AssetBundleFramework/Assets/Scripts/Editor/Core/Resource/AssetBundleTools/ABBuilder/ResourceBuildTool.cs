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
        /// 获取指定本地存储Key和项目挂钩的Key值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetProjectPlayerPrefKey(string key)
        {
            return $"{ResourceBuildConst.ProjectPathHashValue}_{key}";
        }

        /// <summary>
        /// 存储AssetBundle打包配置
        /// </summary>
        public static void SaveSettingsToPlayerPrefs(AssetBundleBuilder abBuilder)
        {
            var assetBundleBuildParams = abBuilder.AssetBundleBuildParams;
            var compressOptionKey = GetProjectPlayerPrefKey(ResourceBuildConst.ABBuildSettingCompressOptionKey);
            PlayerPrefs.SetString(compressOptionKey, assetBundleBuildParams.CompressOption.ToString());
            var isForceRebuildKey = GetProjectPlayerPrefKey(ResourceBuildConst.ABBuildSettingIsForceRebuildKey);
            PlayerPrefs.SetInt(isForceRebuildKey, assetBundleBuildParams.IsForceRebuild ? 1 : 0);
            var isAppendHashKey = GetProjectPlayerPrefKey(ResourceBuildConst.ABBuildSettingIsAppendHashKey);
            PlayerPrefs.SetInt(isAppendHashKey, assetBundleBuildParams.IsAppendHash ? 1 : 0);
            var IsDisableWriteTypeTreeKey = GetProjectPlayerPrefKey(ResourceBuildConst.ABBuildSettingIsDisableWriteTypeTreeKey);
            PlayerPrefs.SetInt(IsDisableWriteTypeTreeKey, assetBundleBuildParams.IsDisableWriteTypeTree ? 1 : 0);
            var IsIgnoreTypeTreeChangesKey = GetProjectPlayerPrefKey(ResourceBuildConst.ABBuildSettingIsIgnoreTypeTreeChangesKey);
            PlayerPrefs.SetInt(IsIgnoreTypeTreeChangesKey, assetBundleBuildParams.IsIgnoreTypeTreeChanges ? 1 : 0);
        }

        /// <summary>
        /// 读取AssetBundle打包配置
        /// </summary>
        public static void LoadSettingsFromPlayerPrefs(AssetBundleBuilder abBuilder)
        {
            var assetBundleBuildParams = abBuilder.AssetBundleBuildParams;
            var compressOptionKey = GetProjectPlayerPrefKey(ResourceBuildConst.ABBuildSettingCompressOptionKey);
            var compressOptionValue = PlayerPrefs.GetString(compressOptionKey, ABCompressOption.ChunkBasedCompressionLZ4.ToString());
            assetBundleBuildParams.CompressOption = (ABCompressOption)Enum.Parse(typeof(ABCompressOption), compressOptionValue);
            var isForceRebuildKey = GetProjectPlayerPrefKey(ResourceBuildConst.ABBuildSettingIsForceRebuildKey);
            assetBundleBuildParams.IsForceRebuild = PlayerPrefs.GetInt(isForceRebuildKey, 0) != 0;
            var isAppendHashKey = GetProjectPlayerPrefKey(ResourceBuildConst.ABBuildSettingIsAppendHashKey);
            assetBundleBuildParams.IsAppendHash = PlayerPrefs.GetInt(isAppendHashKey, 0) != 0;
            var IsDisableWriteTypeTreeKey = GetProjectPlayerPrefKey(ResourceBuildConst.ABBuildSettingIsDisableWriteTypeTreeKey);
            assetBundleBuildParams.IsDisableWriteTypeTree = PlayerPrefs.GetInt(IsDisableWriteTypeTreeKey, 0) != 0;
            var IsIgnoreTypeTreeChangesKey = GetProjectPlayerPrefKey(ResourceBuildConst.ABBuildSettingIsIgnoreTypeTreeChangesKey);
            assetBundleBuildParams.IsIgnoreTypeTreeChanges = PlayerPrefs.GetInt(IsIgnoreTypeTreeChangesKey, 0) != 0;
        }

        /// <summary>
        /// 获取指定打包平台的AssetBundleBuilder实例
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public static AssetBundleBuilder GetTargetAssetBundleBuilder(BuildTarget buildTarget)
        {
            var assetBundleBuildParams = new AssetBundleBuildParams(buildTarget);
            var assetBundleBuilder = new AssetBundleBuilder(assetBundleBuildParams);
            LoadSettingsFromPlayerPrefs(assetBundleBuilder);
            return assetBundleBuilder;
        }

        /// <summary>
        /// 执行AssetBundle打包
        /// </summary>
        /// <param name="abBuildPurpose">AB打包用途</param>
        /// <param name="buildTarget">打包平台</param>
        /// <param name="isForceBuild">是否强制重新打包</param>
        public static bool DoBuildAssetBundle(AssetBundleBuildPurpose abBuildPurpose, BuildTarget buildTarget,
                                              ABCompressOption compressOption = ABCompressOption.ChunkBasedCompressionLZ4,
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
            assetBundleBuilder.AssetBundleBuildParams.PrintAllParams();
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