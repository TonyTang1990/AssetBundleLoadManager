/*
 * Description:             SBPAssetBundleBuilder.cs
 * Author:                  TONYTANG
 * Create Date:             2023//02/01
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;
using UnityEngine.Build.Pipeline;

namespace TResource
{
    /// <summary>
    /// SBPAssetBundleBuilder.cs
    /// 新版Scriptable Build Pipeline AB打包工具
    /// </summary>
    public static class SBPAssetBundleBuilder
    {
        /// <summary>
        /// 执行新版Scriptable Build Pipeline自定义AB打包
        /// </summary>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="buildTarget">打包平台</param>
        /// <param name="options">打包选项设置</param>
        /// <param name="allAssetBundleBuildList">AB打包列表</param>
        /// <param name="buildSuccess">打包是否成功</param>
        /// <returns></returns>
        public static CompatibilityAssetBundleManifest DoSBPAssetBundleBuild(string outputDirectory, BuildTarget buildTarget, CustomBuildParameters buildParams, List<AssetBundleBuild> allAssetBundleBuildList, out bool buildSuccess)
        {
            ScriptableBuildPipeline.slimWriteResults = true;
            ScriptableBuildPipeline.useDetailedBuildLog = false;
            ScriptableBuildPipeline.threadedArchiving = true;
            var buildContent = new BundleBuildContent(allAssetBundleBuildList);
            IBundleBuildResults results;
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out results);
            buildSuccess = exitCode >= ReturnCode.Success;
            if (exitCode < ReturnCode.Success)
            {
                Debug.LogError($"[BuildPatch] 构建过程中发生错误exitCode:{exitCode}！");
                return null;
            }
            CompatibilityAssetBundleManifest unityManifest = CreateAndBuildAssetBundleManifest(outputDirectory, results, out buildSuccess);
            CheckCycleDependSBP(unityManifest);
            // 创建说明文件
            CreateSBPReadmeFile(outputDirectory, results);
            return unityManifest;
        }

        /// <summary>
        /// 创建并打包AssetBundleManifest
        /// Note:
        /// 1. 新版Scriptable Build Pipeline打包没有AssetBundleManifest文件，需要自己创建并打包CompatibilityAssetBundleManifest兼容文件
        /// </summary>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="results">打包结果</param>
        /// <param name="buildSuccess">打包是否成功</param>
        /// <returns></returns>
        private static CompatibilityAssetBundleManifest CreateAndBuildAssetBundleManifest(string outputDirectory, IBundleBuildResults results, out bool buildSuccess)
        {
            var outputDirectoryFullPath = Path.GetFullPath(outputDirectory);
            var outputDirectoryInfo = new DirectoryInfo(outputDirectoryFullPath);
            var outputFolderName = outputDirectoryInfo.Name;
            var unityManifestAssetRelativePath = Path.Combine(outputDirectory, outputFolderName);
            // TODO: 添加Manifest创建和打包相关代码
            buildSuccess = true;
            return null;
        }

        /// <summary>
        /// 创建ScriptableBuildPipeline Readme文件到输出目录
        /// </summary>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="bundleBuildResults">打包结果</param>
        private static void CreateSBPReadmeFile(string outputDirectory, IBundleBuildResults bundleBuildResults)
        {
            // 删除旧文件
            string filePath = $"{outputDirectory}/{AssetBundleBuildConstData.ReadmeFileName}";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            Debug.Log($"创建说明文件：{filePath}");

            StringBuilder content = new StringBuilder();
            AssetBundleBuilder.AppendBuildTargetAndTimeContent(content);
            AssetBundleBuilder.AppendCollectorContent(content);
            AssetBundleBuilder.AppendBuildParametersContent(content);
            AssetBundleBuilder.AppendData(content, $"--构建清单--");
            foreach (var bundleBuildInfos in bundleBuildResults.BundleInfos)
            {
                var bundleBuildInfo = bundleBuildInfos.Value;
                AssetBundleBuilder.AppendData(content, bundleBuildInfo.FileName);
            }

            // 创建新文件
            File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Scriptable Build Pipeline检测循环依赖
        /// </summary>
        private static void CheckCycleDependSBP(CompatibilityAssetBundleManifest unityManifest)
        {
            List<string> visited = new List<string>(100);
            List<string> stack = new List<string>(100);
            string[] allAssetBundles = unityManifest.GetAllAssetBundles();
            for (int i = 0; i < allAssetBundles.Length; i++)
            {
                var element = allAssetBundles[i];
                visited.Clear();
                stack.Clear();

                // 深度优先搜索检测有向图有无环路算法
                if (CheckCycleSBP(unityManifest, element, visited, stack))
                {
                    foreach (var ele in stack)
                    {
                        UnityEngine.Debug.LogWarning(ele);
                    }
                    throw new Exception($"Found cycle assetbundle : {element}");
                }
            }
        }

        /// <summary>
        /// Scriptable Build Pipeline检查循环依赖
        /// </summary>
        /// <param name="unityManifest"></param>
        /// <param name="element"></param>
        /// <param name="visited"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        private static bool CheckCycleSBP(CompatibilityAssetBundleManifest unityManifest, string element, List<string> visited, List<string> stack)
        {
            if (visited.Contains(element) == false)
            {
                visited.Add(element);
                stack.Add(element);

                string[] depends = unityManifest.GetDirectDependencies(element);
                foreach (var dp in depends)
                {
                    if (visited.Contains(dp) == false && CheckCycleSBP(unityManifest, dp, visited, stack))
                    {
                        return true;
                    }
                    else if (stack.Contains(dp))
                    {
                        return true;
                    }
                }
            }

            stack.Remove(element);
            return false;
        }
    }
}