﻿/*
 * Description:             AssetBundleModule.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/24
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Build.Pipeline;

namespace TResource
{
    /// <summary>
    /// AssetBundleModule.cs
    /// AB模式的资源模块管理类
    /// </summary>
    public class AssetBundleModule : AbstractResourceModule
    {

        #region AB依赖信息部分
        /// <summary>
        /// Asset打包信息
        /// </summary>
        public AssetBuildInfoAsset AssetBuildInfo
        {
            get
            {
                return mAssetBuildInfo;
            }
        }
        protected AssetBuildInfoAsset mAssetBuildInfo;

        /// <summary>
        /// AssetBundle依赖信息Map
        /// Ket为AssetBundle的路径(不带后缀),Value为依赖的AssetBundle路径列表(不带后缀)
        /// Note:
        /// 不带后缀是为了上层加载AssetBundle对后缀名无感知
        /// </summary>
        public Dictionary<string, string[]> AssetBundleDependencyMap
        {
            get;
            private set;
        }

        /// <summary>
        /// 加载AssetBundle依赖信息
        /// </summary>
        private void LoadAssetBuildManifest()
        {
            if(AssetBundleDependencyMap == null)
            {
                AssetBundleDependencyMap = new Dictionary<string, string[]>();
            }
            AssetBundleDependencyMap.Clear();
            // Note:
            // 依赖AB不带后缀
            var abPath = AssetBundlePath.GetABLoadFullPathNoPostFix(AssetBundlePath.DependencyFileName);
            AssetBundle ab = AssetBundle.LoadFromFile(abPath);
            if (ab != null)
            {
#if SCRIPTABLE_ASSET_BUILD_PIPELINE
                var assetBundleManifest = ab.LoadAsset<CompatibilityAssetBundleManifest>(ResourceConstData.AssetBundleManifestAssetName);
#else
                var assetBundleManifest = ab.LoadAsset<AssetBundleManifest>(ResourceConstData.AssetBundleManifestAssetName);
#endif
                var allAssetBundlePath = assetBundleManifest.GetAllAssetBundles();
                for (int i = 0, length = allAssetBundlePath.Length; i < length; i++)
                {
                    var assetBundlePath = allAssetBundlePath[i];
                    var dependenciesPathes = assetBundleManifest.GetAllDependencies(assetBundlePath);
                    assetBundlePath = PathUtilities.GetPathWithoutPostFix(assetBundlePath);
                    for (int j = 0, length2 = dependenciesPathes.Length; j < length2; j++)
                    {
                        var dependenciesPath = dependenciesPathes[j];
                        dependenciesPathes[j] = PathUtilities.GetPathWithoutPostFix(dependenciesPath);
                    }
                    AssetBundleDependencyMap.Add(assetBundlePath, dependenciesPathes);
                }
                ab.Unload(true);
                Debug.Log($"AssetBundle依赖信息文件:{abPath}加载成功!");
            }
            else
            {
                Debug.LogError($"找不到AssetBundle依赖信息文件:{AssetBundlePath.DependencyFileName}");
            }
        }

        /// <summary>
        /// 加载Asset打包信息
        /// </summary>
        private void LoadAssetBuildInfo()
        {
            // 确保之前加载的AB打包信息卸载彻底
            if (mAssetBuildInfo != null)
            {
                Resources.UnloadAsset(mAssetBuildInfo);
                mAssetBuildInfo = null;
            }
            // AssetBundle打包信息没有依赖信息，直接加载即可
            var assetBuildInfoAssetRelativePath = AssetBundlePath.GetAssetBuildInfoFileRelativePath();
#if !SCRIPTABLE_ASSET_BUILD_PIPELINE
            // 老版BuildPipeline.BuildAssetBundles打包指定AssetBundleBuild.assetNames为含大写
            // 但不知道为什么打包出来的AB里面的加载路径依然是全小写，这里老版AB统一成全小写加载
            // Note:
            // 1. 经测试打包时设置全小写，老版AB加载依然可以用大写路径加载
            assetBuildInfoAssetRelativePath = assetBuildInfoAssetRelativePath.ToLower();
#endif
            var assetBuildInfoABPath = AssetBundlePath.ChangeAssetPathToABPath(assetBuildInfoAssetRelativePath);
            var abPath = AssetBundlePath.GetABLoadFullPath(assetBuildInfoABPath.ToLower());
            AssetBundle ab = null;
            ab = AssetBundle.LoadFromFile(abPath);
            if (ab != null)
            {
                mAssetBuildInfo = ab.LoadAsset<AssetBuildInfoAsset>(assetBuildInfoAssetRelativePath);
                mAssetBuildInfo.Init();
                ab.Unload(false);
                Debug.Log("Asset打包信息文件加载成功!");
            }
            else
            {
                Debug.LogError($"找不到Asset打包信息文件:{assetBuildInfoAssetRelativePath}");
            }
        }

        /// <summary>
        /// 获取AssetBundle所依赖的AB信息
        /// </summary>
        /// <param name="abPath"></param>
        /// <returns></returns>
        private string[] GetAssetBundleDpInfo(string abPath)
        {
            if (string.IsNullOrEmpty(abPath))
            {
                return null;
            }
            if (AssetBundleDependencyMap.ContainsKey(abPath))
            {
                return AssetBundleDependencyMap[abPath];
            }
            else
            {
                Debug.LogError($"找不到AssetBundle:{abPath}的AssetBundle依赖信息,请检查是否传递了正确的AssetBundle路径!");
                return null;
            }
        }

        /// <summary>
        /// 获取指定Asset路径的AB名
        /// </summary>
        /// <param name="assetpath"></param>
        /// <returns></returns>
        private string GetAssetPathAssetBundleName(string assetpath)
        {
            var abpath = mAssetBuildInfo.GetAssetABPath(assetpath);
            if (!string.IsNullOrEmpty(abpath))
            {
                return abpath;
            }
            else
            {
                Debug.LogError($"找不到Asset路径:{assetpath}的AB路径信息!");
                return null;
            }
        }

        /// <summary>
        /// 打印所有AB依赖信息
        /// </summary>
        public void PrintAllResourceDpInfo()
        {
            foreach (var abinfo in AssetBundleDependencyMap)
            {
                ResourceLogger.log(string.Format("AB Path:{0}", abinfo.Key));
                foreach (var dpfile in abinfo.Value)
                {
                    ResourceLogger.log(string.Format("       DP AB Path:{0}", dpfile));
                }
            }
        }
#endregion

        /// <summary>
        /// 已加载AB里不再有有效引用的AB信息列表
        /// </summary>
        protected List<AssetBundleInfo> mUnsedAssetBundleInfoList;

        /// <summary>
        /// 资源加载模块初始化
        /// </summary>
        public override void Init()
        {
            base.Init();

            mUnsedAssetBundleInfoList = new List<AssetBundleInfo>();

            ResLoadMode = ResourceLoadMode.AssetBundle;
            // 加载AssetBundle依赖信息
            LoadAssetBuildManifest();
            // 加载Asset打包信息
            LoadAssetBuildInfo();
            // 延迟绑定AB相关信息获取委托
            GetAssetPathBundleNameDelegate = GetAssetPathAssetBundleName;
            GetAssetBundleDpInfoDelegate = GetAssetBundleDpInfo;
        }

        /// <summary>
        /// 重新加载数据(针对热更流程后需要重新加载部分数据的情况)
        /// </summary>
        public override void ReloadData()
        {
            base.ReloadData();
            // 重新加载AssetBundle依赖信息
            LoadAssetBuildManifest();
            // 重新加载Asset打包信息
            LoadAssetBuildInfo();
        }

        /// <summary>
        /// 真正的请求Asset资源
        /// </summary>
        /// <param name="assetPath">Asset资源路径(带后缀)</param>
        /// <param name="assetLoader">Asset资源加载器</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        /// <returns>请求UID</returns>
        protected override int RealRequestAsset<T>(string assetPath, out AssetLoader assetLoader, Action<AssetLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            var requestUID = LoaderManager.Singleton.GetNextRequestUID();
            var ownerAssetBundlePath = GetAssetPathBundleNameDelegate(assetPath);
            var depABPaths = GetAssetBundleDpInfoDelegate(ownerAssetBundlePath);
            var bundleAssetLoader = LoaderManager.Singleton.createBundleAssetLoader<T>(assetPath, ownerAssetBundlePath, loadType, loadMethod) as BundleAssetLoader;
            bundleAssetLoader.InitBundleInfo(ownerAssetBundlePath, depABPaths);
            bundleAssetLoader.AddRequest(requestUID, completeHandler);
            assetLoader = bundleAssetLoader as AssetLoader;
            bundleAssetLoader.Load();
            return requestUID;
        }

        /// <summary>
        /// 真正的请求AssetBundle资源
        /// </summary>
        /// <param name="abPath">AssetBundle资源路径</param>
        /// <param name="bundleLoader">AB资源加载器</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        /// <returns>请求UID</returns>
        protected override int RealRequestAssetBundle(string abPath, out BundleLoader bundleLoader, Action<BundleLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            // TODO: 支持动态AB资源下载
            var requestUID = LoaderManager.Singleton.GetNextRequestUID();
            var depABPaths = GetAssetBundleDpInfoDelegate(abPath);
            bundleLoader = LoaderManager.Singleton.CreateAssetBundleLoader<AssetBundleLoader>(abPath, depABPaths, loadType, loadMethod);
            bundleLoader.AddRequest(requestUID, completeHandler);
            bundleLoader.Load();
            return requestUID;
        }

        /// <summary>
        /// 真正执行资源卸载指定类型不再使用的资源接口
        /// </summary>
        /// <param name="resourceloadtype"></param>
        protected override void DoUnloadSpecificLoadTypeUnsedResource(ResourceLoadType resourceloadtype)
        {
            // 递归判定卸载所有不再可用的正常加载AB
            bool hasunsedAssetBundle = true;
            while (hasunsedAssetBundle)
            {
                // 检查回收不再使用正常已加载的AB
                CheckUnsedAssetBundleResources();

                if (mUnsedAssetBundleInfoList.Count == 0)
                {
                    //不再有可卸载的AB
                    hasunsedAssetBundle = false;
                }
                else
                {
                    // 有可卸载的AB
                    DoUnloadUnsedAssetBundleWithLimit(true);
                }
            }
        }

        /// <summary>
        /// 执行不再使用资源监察
        /// </summary>
        protected override void DoCheckUnusedResource()
        {
            base.DoCheckUnusedResource();
            CheckUnsedAssetBundleResources();
            DoUnloadUnsedAssetBundleWithLimit(true);
        }

        /// <summary>
        /// 执行卸载所有不再使用的资源
        /// </summary>
        protected override void DoUnloadAllUnusedResources()
        {
            base.DoUnloadAllUnusedResources();
            CheckUnsedAssetBundleResources();
            DoUnloadUnsedAssetBundleWithLimit(false);
        }

        /// <summary>
        /// 检查未使用AssetBundle
        /// </summary>
        protected void CheckUnsedAssetBundleResources()
        {
            mUnsedAssetBundleInfoList.Clear();
            var time = Time.time;
            // 检查正常加载的资源AssetBundle，回收不再使用的AssetBundle
            foreach (var loadedAssetBundleInfo in mAllLoadedNormalAssetBundleInfoMap)
            {
                if (loadedAssetBundleInfo.Value.IsUnsed)
                {
                    // 强制卸载不需要判定有效资源生命时长
                    //if ((time - loadedAssetBundleInfo.Value.LastUsedTime) > ResourceMinimumLifeTime)
                    mUnsedAssetBundleInfoList.Add(loadedAssetBundleInfo.Value);
                }
            }

            if (mUnsedAssetBundleInfoList.Count > 0)
            {
                // 根据最近使用时间升序排列
                mUnsedAssetBundleInfoList.Sort(ABILastUsedTimeSort);
            }
        }

        /// <summary>
        /// 资源信息根据最近使用时间排序
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int ABILastUsedTimeSort(AbstractResourceInfo a, AbstractResourceInfo b)
        {
            return a.LastUsedTime.CompareTo(b.LastUsedTime);
        }

        /// <summary>
        /// 卸载未使用的AssetBundle
        /// </summary>
        /// <param name="withLimitNumber">是否限制卸载数量</param>
        protected void DoUnloadUnsedAssetBundleWithLimit(bool withLimitNumber = false)
        {
            for (int i = 0; i < mUnsedAssetBundleInfoList.Count; i++)
            {
                if (withLimitNumber == false || (withLimitNumber && i < MaxUnloadABNumberPerFrame))
                {
                    DeleteAssetBundleInfo(mUnsedAssetBundleInfoList[i].ResourcePath);
                }
                else
                {
                    break;
                }
            }
            mUnsedAssetBundleInfoList.Clear();
        }

        /// <summary>
        /// 强制卸载所有资源(只在特定情况下用 e.g. 热更后卸载所有已加载资源后重新初始化加载AB资源)***慎用***
        /// </summary>
        public override void ForceUnloadAllResources()
        {
            // 强制清除待卸载AssetBundleInfo信息，避免强制清除后依然触发清理报错
            mUnsedAssetBundleInfoList.Clear();
            var assetBundlePathList = new List<string>(mAllLoadedNormalAssetBundleInfoMap.Keys);
            assetBundlePathList.AddRange(mAllLoadedPermanentAssetBundleInfoMap.Keys);
            foreach(var assetBundlePath in assetBundlePathList)
            {
                if(mAllLoadedNormalAssetBundleInfoMap.ContainsKey(assetBundlePath) ||
                    mAllLoadedPermanentAssetBundleInfoMap.ContainsKey(assetBundlePath))
                {
                    DeleteAssetBundleInfo(assetBundlePath);
                }
            }
        }

#region 资源调试辅助功能
        /// <summary>
        /// 强制卸载指定AB(只支持NormalLoad的AB资源强制卸载)
        /// </summary>
        /// <param name="assetBundelPath"></param>
        public void ForceUnloadSpecificAssetBundle(string assetBundelPath)
        {
            AssetBundleInfo assetBundleInfo = GetAssetBundleInfo(assetBundelPath);
            if(assetBundleInfo != null)
            {
                if (mAllLoadedNormalAssetBundleInfoMap.TryGetValue(assetBundelPath, out assetBundleInfo))
                {
                    DeleteAssetBundleInfo(assetBundelPath);
                }
                else
                {
                    ResourceLogger.logErr(string.Format("AssetBundle资源 : {0}属于非NormalLoad资源，不允许强制卸载!", assetBundelPath));
                }
            }
            else
            {
                ResourceLogger.logErr(string.Format("AssetBundle资源 : {0}未被加载，无法强制卸载!", assetBundelPath));
            }
        }
#endregion
    }
}