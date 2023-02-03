/*
 * Description:             OldAssetBundleBuilder.cs
 * Author:                  TONYTANG
 * Create Date:             2023//02/01
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// OldAssetBundleBuilder.cs
    /// 老版AB打包工具
    /// </summary>
    public static class OldAssetBundleBuilder
    {
        /// <summary>
        /// 执行老版自定义AB打包
        /// </summary>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="buildTarget">打包平台</param>
        /// <param name="options">打包选项设置</param>
        /// <param name="allAssetBundleBuildList">AB打包列表</param>
        /// <returns></returns>
        public static AssetBundleManifest BuildAllAssetBundles(string outputDirectory, BuildTarget buildTarget, BuildAssetBundleOptions options, List<AssetBundleBuild> allAssetBundleBuildList, out bool buildSuccess)
        {
            var unityManifest = BuildPipeline.BuildAssetBundles(outputDirectory, allAssetBundleBuildList.ToArray(), options, buildTarget);
            buildSuccess = unityManifest != null;
            if (unityManifest == null)
            {
                Debug.LogError("[BuildPatch] 构建过程中发生错误！");
                return null;
            }
            // 检测循环依赖
            CheckCycleDepend(unityManifest);
            return unityManifest;
        }

        /// <summary>
        /// 检测循环依赖
        /// </summary>
        private static void CheckCycleDepend(AssetBundleManifest unityManifest)
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
                if (CheckCycle(unityManifest, element, visited, stack))
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
        /// 检查循环依赖
        /// </summary>
        /// <param name="unityManifest"></param>
        /// <param name="element"></param>
        /// <param name="visited"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        private static bool CheckCycle(AssetBundleManifest unityManifest, string element, List<string> visited, List<string> stack)
        {
            if (visited.Contains(element) == false)
            {
                visited.Add(element);
                stack.Add(element);

                string[] depends = unityManifest.GetDirectDependencies(element);
                foreach (var dp in depends)
                {
                    if (visited.Contains(dp) == false && CheckCycle(unityManifest, dp, visited, stack))
                        return true;
                    else if (stack.Contains(dp))
                        return true;
                }
            }

            stack.Remove(element);
            return false;
        }
    }
}